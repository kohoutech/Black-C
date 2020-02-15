/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2020  George E Greaney

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
using System.IO;

namespace BlackC.Lexer
{
    public class Preprocessor
    {
        public Parser parser;
        public Scanner scan;

        public Fragment curFrag;
        bool atLineStart;

        //public List<SourceBuffer> sourceStack;
        //public bool inMacro;
        //public Macro currentMacro;

        //public HashSet<String> oncelerList;

        public Preprocessor(Parser _parser, string filename)
        {
            parser = _parser;
            scan = new Scanner(parser, filename);
            curFrag = null;
            //    sourceStack = new List<SourceBuffer>();

            atLineStart = true;

            //    Macro.initMacros();
            //    inMacro = false;
            //    currentMacro = null;

            //    oncelerList = new HashSet<string>();        
        }

        public void setMainSourceFile(string filename)
        {
            //    SourceBuffer srcbuf = new SourceBuffer(".", filename);
            //    sourceStack.Add(srcbuf);
            //    scanner.setSource(srcbuf);
        }

        public void preprocessFile(String filename)
        {
            using (StreamWriter prepFile = new StreamWriter(filename))
            {
                Fragment frag = scan.getFrag();
                while (frag.type != FragType.EOF)
                {
                    if (frag.type == FragType.EOLN)
                    {
                        prepFile.WriteLine();
                    }
                    else
                    {
                        prepFile.Write(frag.ToString());
                    }
                    frag = scan.getFrag();
                }
            }
        }

        //- fragment stream handling ------------------------------------------

        public Fragment getFrag()
        {
            Fragment frag = getScannerFrag();
            return frag;
        }

        //handle directives in the scanner's fragment stream, will be sent to tokenizer as EOLN fragments
        public Fragment getScannerFrag()
        {
            curFrag = scan.getFrag();
            //Console.WriteLine("fragment = " + curFrag.ToString());

            //check for directive as first non-space frag at start of line
            if (atLineStart)
            {
                if ((curFrag.type == FragType.PUNCT) && (curFrag.str[0] == '#'))
                {
                    handleDirective();      //cur fragment will be left as the EOLN at end of directive line
                }
                else
                {
                    atLineStart = (curFrag.type == FragType.SPACE);
                }
            }
            if (curFrag.type == FragType.EOLN)
            {
                atLineStart = true;
            }

            return curFrag;
        }

        //public Token getToken()
        //{

        //    Token token = null;
        //    //bool done = true;
        //    //do
        //    //{
        //    //    if (inMacro)
        //    //    {
        //    //        token = currentMacro.getToken();
        //    //        inMacro = currentMacro.atEnd();
        //    //    }
        //    //    else
        //    //    {
        //            token = scanner.scanToken();
        //        //}
        //        //done = true;

        //        ////check for a directive
        //        //if ((token.type == TokenType.tHASH) && (token.atBOL))
        //        //{
        //        //    handleDirective();
        //        //    done = false;        //get token following directive's eoln
        //        //}

        //        ////check for a macro
        //        //else if (token.type == TokenType.tIDENTIFIER)
        //        //{
        //        //    Macro macro = Macro.lookupMacro(token);
        //        //    if (macro != null)
        //        //    {
        //        //        inMacro = true;
        //        //        currentMacro = macro;
        //        //        macro.invokeMacro(scanner);         //start macro running
        //        //        done = false;                       //and get first token from macro definition
        //        //    }
        //        //}

        //    //    //we've hit the end of file. if this is an include file, pull it off the stack 
        //    //    //and resume scanning at the point we stopped in the including file
        //    //    //we return the EOF token from the main file only
        //    //    else if ((token.type == TokenType.tEOF) && (sourceStack.Count > 1))
        //    //    {
        //    //        Console.WriteLine("closing include file " + sourceStack[sourceStack.Count - 1].fullname);
        //    //        sourceStack.RemoveAt(sourceStack.Count - 1);
        //    //        scanner.setSource(sourceStack[sourceStack.Count - 1]);
        //    //        done = false;                                           //get next token from including source
        //    //    }

        //    //} while (!done);

        //    return token;
        //}

        //- directive handling ------------------------------------------------

        //(6.10) Preprocessing directives

        //handle directive, this will read all the fragments to the eoln in the directive line
        public void handleDirective()
        {
            curFrag = scan.getFrag();
            while (curFrag.type == FragType.SPACE)  //skip space(s) & get directive name
            {
                curFrag = scan.getFrag();
            }

            if (curFrag.type == FragType.EOLN)          //skip empty directives, ie "#  <eoln>"
            {
                return;
            }

            if (curFrag.type == FragType.WORD)
            {
                switch (curFrag.str)
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
                        parser.error("saw unknown directive #" + curFrag.str + " at " + curFrag.loc.ToString());
                        skipRestOfLine();
                        break;
                }
            }
            else
            {
                parser.error("invalid directive #" + curFrag.str + " at " + curFrag.loc.ToString());
                skipRestOfLine();
            }
        }

        public void skipRestOfLine()
        {
            while (curFrag.type != FragType.EOLN)
            {
                curFrag = scan.getFrag();        //skip remaining fragments in source line
            }
        }

        public void handleIncludeDirective()
        {
            //    Token token = scanner.scanToken();
            //    String filename = "";

            //    if (token.type == TokenType.tSTRINGCONST)
            //    {
            //        filename = token.chars;
            //    }
            //    else
            //    {
            //        token = scanner.scanToken();
            //        while (token.type != TokenType.tGTRTHAN)
            //        {
            //            filename += token.chars;
            //            token = scanner.scanToken();
            //        }
            //    }
            Console.WriteLine("saw #include");
            curFrag = scan.getFrag();
            skipRestOfLine();

            //    if (!oncelerList.Contains(filename))        //if file hasn't been marked with "pragma once"
            //    {
            //        SourceBuffer includeBuf = SourceBuffer.getIncludeFile(filename, parser.includePaths);
            //        sourceStack.Add(includeBuf);
            //        scanner.saveSource();
            //        scanner.setSource(includeBuf);
            //    }
        }

        public void handleDefineDirective()
        {
            Console.WriteLine("saw #define");
            curFrag = scan.getFrag();
            skipRestOfLine();

            //    Token token = scanner.scanToken();
            //    Macro macro = Macro.defineMacro(token);
            //    macro.scanMacroDefinition(scanner);
        }

        public void handleUndefDirective()
        {
            Console.WriteLine("saw #undef");
            curFrag = scan.getFrag();
            skipRestOfLine();

            //    Token token = scanner.scanToken();
            //    Macro.undefineMacro(token);
        }

        public void handleIfDirective()
        {
            Console.WriteLine("saw #if");
            curFrag = scan.getFrag();
            skipRestOfLine();

        }

        public void handleIfdefDirective()
        {
            Console.WriteLine("saw #ifdef");
            curFrag = scan.getFrag();
            skipRestOfLine();
            //    Token token = scanner.scanToken();
            //    Macro macro = Macro.lookupMacro(token);
        }

        public void handleIfndefDirective()
        {
            Console.WriteLine("saw #ifndef");
            curFrag = scan.getFrag();
            skipRestOfLine();

            //    Token token = scanner.scanToken();
            //    Macro macro = Macro.lookupMacro(token);
        }

        public void handleElifDirective()
        {
            Console.WriteLine("saw #elif");
            curFrag = scan.getFrag();
            skipRestOfLine();
        }

        public void handleElseDirective()
        {
            Console.WriteLine("saw #else");
            curFrag = scan.getFrag();
            skipRestOfLine();
        }

        public void handleEndifDirective()
        {
            Console.WriteLine("saw #endif");
            curFrag = scan.getFrag();
            skipRestOfLine();
        }

        public void handleLineDirective()
        {
            Console.WriteLine("saw #line");
            curFrag = scan.getFrag();
            skipRestOfLine();
        }

        public void handleErrorDirective()
        {
            Console.WriteLine("saw #error");
            curFrag = scan.getFrag();
            skipRestOfLine();
        }

        public void handlePragmaDirective()
        {
            Console.WriteLine("saw #pragma");
            curFrag = scan.getFrag();
            skipRestOfLine();
            //    Token token = scanner.scanToken();        //get pragma name

            //    switch (token.chars)
            //    {
            //        case "once" :
            //            String filename = sourceStack[sourceStack.Count - 1].filename;
            //            oncelerList.Add(filename);
            //            break;
            //    }
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");