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
        public List<SourceBuffer> bufferStack;

        Token lookahead;
        List<Token> replay;
        int recpos;

        public Preprocessor(Parser _parser)
        {
            parser = _parser;
            scanner = new Scanner();
            bufferStack = new List<SourceBuffer>();

            lookahead = null;
            replay = new List<Token>();
            recpos = 0;
        }

        public void setMainSourceFile(string filename)
        {
            SourceBuffer srcbuf = new SourceBuffer(filename);
            bufferStack.Add(srcbuf);
            scanner.setSource(srcbuf);
        }

        //- token stream handling ------------------------------------------

        public Token getToken()
        {
            Token token = null;

            if (recpos < replay.Count)
            {
                token = replay[recpos++];
            }
            else if (lookahead != null)
            {
                token = lookahead;
            }
            else
            {
                token = scanner.scanToken();
                if ((token.type == TokenType.tHASH) && (token.atBOL))
                {
                    token = scanner.scanToken();        //get directive name
                    switch (token.chars)
                    {
                        case "include" :
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

                        case "pragma ":
                            handlePragmaDirective();
                            break;

                        default:
                            handleUnknownDirective();
                            break;
                    }
                    token = scanner.scanToken();        //get token following directive's eoln
                }
                lookahead = token;
                replay.Add(token);
                recpos++;
            }

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
            recpos = 0;
        }

        public bool isNextToken(TokenType ttype)
        {
            return (getToken().type == ttype);
        }

        //- directive handling ------------------------------------------------

        //(6.10) Preprocessing directives

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
           SourceBuffer includeBuf = SourceBuffer.getIncludeFile(filename, parser.includePaths);
        }

        public void handleDefineDirective()
        {
            Console.WriteLine("saw #define");
        }

        public void handleUndefDirective()
        {
            Console.WriteLine("saw #undef");
        }

        public void handleIfDirective()
        {
            Console.WriteLine("saw #if");
        }

        public void handleIfdefDirective()
        {
            Console.WriteLine("saw #ifdef");
        }

        public void handleIfndefDirective()
        {
            Console.WriteLine("saw #ifndef");
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
            Console.WriteLine("saw #pragma");
        }

        public void handleUnknownDirective()
        {
            Console.WriteLine("saw unknown directive");
        }
    }

    //-------------------------------------------------------------------------

    public class Macro
    {
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");