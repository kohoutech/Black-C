/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2018  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackC
{
    public class Preprocessor
    {
        public Parser parser;
        public Scanner scanner;
        public List<SourceBuffer> sourceStack;
        public List<Macro> macroStack;
        public bool inMacro;
        public Macro currentMacro;

        public HashSet<String> oncelerList;

        Token lookahead;
        List<Token> replay;
        int recpos;

        public Preprocessor(Parser _parser)
        {
            parser = _parser;
            scanner = new Scanner();
            sourceStack = new List<SourceBuffer>();
            Macro.initMacros();
            macroStack = new List<Macro>();
            inMacro = false;
            currentMacro = null;

            oncelerList = new HashSet<string>();

            lookahead = null;
            replay = new List<Token>();
            recpos = 0;
        }

        public void setMainSourceFile(string filename)
        {
            SourceBuffer srcbuf = new SourceBuffer(".", filename);
            sourceStack.Add(srcbuf);
            scanner.setSource(srcbuf);
        }

        //- token stream handling ------------------------------------------

        public Token getToken()
        {
            //return any stored token first
            if (recpos < replay.Count)
            {
                return replay[recpos];                
            }

            if (lookahead != null)
            {
                return lookahead;
            }

            //don't have a stored token, so get a new one
            Token token = null;
            bool done = true;
            do
            {
                if (inMacro)
                {
                    token = currentMacro.getToken();
                    if (token == null)
                    {
                        macroStack.RemoveAt(macroStack.Count - 1);
                        inMacro = (macroStack.Count > 0);
                        token = scanner.scanToken();
                    }
                }
                else
                {
                    token = scanner.scanToken();
                }
                done = true;

                //check for a directive
                if ((token.type == TokenType.tHASH) && (token.atBOL))
                {
                    handleDirective();
                    done = false;        //get token following directive's eoln
                }

                //check for a macro
                else if (token.type == TokenType.tIDENTIFIER)
                {
                    Macro macro = Macro.lookupMacro(token);
                    if (macro != null)
                    {
                        inMacro = true;
                        macroStack.Add(macro);
                        currentMacro = macro;
                        macro.invokeMacro(scanner);         //start macro running
                        done = false;                       //and get first token from macro definition
                    }
                }

                //we've hit the end of file. if this is an include file, pull it off the stack 
                //and resume scanning at the point we stopped in the including file
                //we return the EOF token from the main file only
                if ((token.type == TokenType.tEOF) && (sourceStack.Count > 1))
                {
                    Console.WriteLine("closing include file " + sourceStack[sourceStack.Count - 1].fullname);
                    sourceStack.RemoveAt(sourceStack.Count - 1);
                    scanner.setSource(sourceStack[sourceStack.Count - 1]);
                    done = false;                                           //get next token from including source
                }

            } while (!done);

            lookahead = token;
            replay.Add(token);
            return token;
        }

        public void next()
        {
            lookahead = null;
            recpos++;
        }

        public int record()
        {
            return recpos;
        }

        //rewind one token
        public void rewind()
        {
            if (recpos > 0)
            {
                recpos--;
            }
        }

        //rewind tokens to cuepoint
        public void rewind(int cuepoint)
        {
            recpos = cuepoint;
        }

        public void reset()
        {
            replay.Clear();
            recpos = 0;
        }

        public bool isNextToken(TokenType ttype)
        {
            return (getToken().type == ttype);
        }

        //- directive handling ------------------------------------------------

        //(6.10) Preprocessing directives

        //handle directive, this will read all the tokens to the eoln in the directive line
        public void handleDirective()
        {
            Token token = scanner.scanToken();        //get directive name

            if (token.type != TokenType.tEOLN)          //skip empty directives, ie "#  <eoln>"
            {
                switch (token.chars)
                {
                    case "include":
                        handleIncludeDirective();
                        break;

                    case "define":
                        handleDefineDirective();
                        break;

                    case "undef":
                        handleUndefDirective();
                        break;

                    case "if":
                        handleIfDirective();
                        break;

                    case "ifdef":
                        handleIfdefDirective();
                        break;

                    case "ifndef":
                        handleIfndefDirective();
                        break;

                    case "elif":
                        handleElifDirective();
                        break;

                    case "else":
                        handleElseDirective();
                        break;

                    case "endif":
                        handleEndifDirective();
                        break;

                    case "line":
                        handleLineDirective();
                        break;

                    case "error":
                        handleErrorDirective();
                        break;

                    case "pragma":
                        handlePragmaDirective();
                        break;

                    default:
                        handleUnknownDirective(token.chars);
                        break;
                }
            }
        }

        public void skipRestOfLine(Token token)
        {
            while (token.type != TokenType.tEOLN)
            {
                token = scanner.scanToken();        //skip remaining chars in source line
            }
        }

        public void handleIncludeDirective()
        {
            Token token = scanner.scanToken();
            String filename = "";

            if (token.type == TokenType.tSTRINGCONST)
            {
                filename = token.chars;
            }
            else
            {
                token = scanner.scanToken();
                while (token.type != TokenType.tGTRTHAN)
                {
                    filename += token.chars;
                    token = scanner.scanToken();
                }
            }
            skipRestOfLine(token);

            if (!oncelerList.Contains(filename))        //if file hasn't been marked with "pragma once"
            {
                SourceBuffer includeBuf = SourceBuffer.getIncludeFile(filename, parser.includePaths);
                sourceStack.Add(includeBuf);
                scanner.saveSource();
                scanner.setSource(includeBuf);
            }
        }

        public void handleDefineDirective()
        {
            Token token = scanner.scanToken();
            Macro macro = Macro.defineMacro(token);
            macro.scanMacroDefinition(scanner);
        }

        public void handleUndefDirective()
        {
            Token token = scanner.scanToken();
            Macro.undefineMacro(token);
            skipRestOfLine(token);
        }

        public void handleIfDirective()
        {
            Console.WriteLine("saw #if");
        }

        public void handleIfdefDirective()
        {
            Console.Write("saw #ifdef ");
            Token token = scanner.scanToken();
            Macro macro = Macro.lookupMacro(token);
        }

        public void handleIfndefDirective()
        {
            Console.Write("saw #ifndef ");
            Token token = scanner.scanToken();
            Macro macro = Macro.lookupMacro(token);
        }

        public void handleElifDirective()
        {
            Console.WriteLine("saw #elif");
        }

        public void handleElseDirective()
        {
            Console.WriteLine("saw #else");
        }

        public void handleEndifDirective()
        {
            Console.WriteLine("saw #endif");
        }

        public void handleLineDirective()
        {
            Console.WriteLine("saw #line");
        }

        public void handleErrorDirective()
        {
            Console.WriteLine("saw #error");
        }

        public void handlePragmaDirective()
        {
            Token token = scanner.scanToken();        //get pragma name

            switch (token.chars)
            {
                case "once" :
                    String filename = sourceStack[sourceStack.Count - 1].filename;
                    oncelerList.Add(filename);
                    break;
            }
        }

        public void handleUnknownDirective(String direct)
        {
            Console.WriteLine("saw unknown directive: " + direct);
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");