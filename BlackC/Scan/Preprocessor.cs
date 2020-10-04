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

using BlackC.Parse;

namespace BlackC.Scan
{
    public class Preprocessor
    {
        public Parser parser;
        public PPTokenSource source;

        bool atLineStart;
        int skipLevel;

        public List<PPTokenSource> sourceStack;
        public List<PPTokenSource> macroStack;

        public Preprocessor(Parser _parser, string filename)
        {
            parser = _parser;


            sourceStack = new List<PPTokenSource>();
            source = new Scanner(parser, filename);           //open main source file
            sourceStack.Add(source);                          //and add it to bottom of include stack

            Macro.initMacros();
            macroStack = new List<PPTokenSource>();

            atLineStart = true;
            skipLevel = 0;
        }

        //- preprocessing only ------------------------------------------------

        public void preprocessFile(String outname)
        {
            List<String> lines = new List<string>();
            StringBuilder line = new StringBuilder();

            PPToken tok = getPPToken();
            while (tok.type != PPTokenType.EOF)
            {
                Console.WriteLine(tok.ToString());
                tok = getPPToken();
            }
            Console.WriteLine(tok.ToString());

            //while (frag.type != PPTokenType.EOF)
            //{
            //    if (frag.type == PPTokenType.EOLN)
            //    {
            //        lines.Add(line.ToString());
            //        line.Clear();
            //    }
            //    else
            //    {
            //        line.Append(frag.ToString());
            //    }
            //    frag = getPPToken();                
            //}
            //if (line.Length > 0)
            //{
            //    lines.Add(line.ToString());
            //}

            //if not saving spaces in output, compress multiple blank lines into one blank line
            //if (!parser.saveSpaces)
            //{
            //    lines = removeBlankLines(lines);
            //}

            //File.WriteAllLines(outname, lines);
        }

        public List<string> removeBlankLines(List<string> lines)
        {
            List<string> newLines = new List<string>();
            bool lastLineWasBlank = false;
            foreach (string line in lines)
            {
                bool thisLineIsBlank = lineIsBlank(line);
                if (!lastLineWasBlank || !thisLineIsBlank)
                {
                    newLines.Add(line);
                }
                lastLineWasBlank = thisLineIsBlank;
            }
            return newLines;
        }

        public bool lineIsBlank(string line)
        {
            foreach (char ch in line)
            {
                if (ch != ' ')
                {
                    return false;
                }
            }
            return true;
        }

        //- pp token stream handling ------------------------------------------

        //handles macro expansion & eof in include files
        //handle directives in the scanner's fragment stream, will be sent to tokenizer as EOLN fragments
        public PPToken getPPToken()
        {
            PPToken tok = null;

            while (true)
            {
                tok = source.getPPToken();
                //Console.WriteLine("pp token = " + tok.ToString());

                //check for directive as first non-space frag at start of line
                if (atLineStart)
                {
                    if ((tok.type == PPTokenType.PUNCT) && (tok.str[0] == '#'))
                    {
                        handleDirective();      
                        tok = new PPToken(PPTokenType.EOLN, "<eoln>");        //cur pp token will be left as the EOLN at end of directive line
                    }
                    else
                    {
                        atLineStart = (tok.type == PPTokenType.SPACE || tok.type == PPTokenType.COMMENT);
                    }
                }
                if ((tok.type == PPTokenType.EOLN) || (tok.type == PPTokenType.EOF))
                {
                    atLineStart = true;
                }

                //check for a macro if not skipping tokens
                if (tok.type == PPTokenType.WORD && skipLevel == 0)
                {
                        Macro macro = Macro.lookupMacro(tok.str);
                        if (macro != null)
                        {
//                          invokeMacro(macro);                 //start macro running
                            continue;                           //and loop around to get first macro token (if not empty)
                        }
                }

                //check if we've hit the end of macro. if this is a macro, pull it off the stack 
                //and resume scanning at the point we stopped in the previous source
                //note: check for macro end first because a file can contain a macro, but a macro can't include a file
                if ((tok.type == PPTokenType.EOF) && (macroStack.Count > 0))
                {
                    macroStack.RemoveAt(macroStack.Count - 1);
                    if (macroStack.Count > 0)
                    {
                        source = macroStack[macroStack.Count - 1];
                    }
                    else
                    {
                        source = sourceStack[sourceStack.Count - 1];
                    }
                    continue;                                           //get next token from including source
                }

                //check if we've hit the end of file. if this is an include file, pull it off the stack 
                //and resume scanning at the point we stopped in the including file
                //we return the EOF token from the main file only
                if ((tok.type == PPTokenType.EOF) && (sourceStack.Count > 1))
                {
                    //Console.WriteLine("closing include file " + sourceStack[sourceStack.Count - 1].filename);
                    sourceStack.RemoveAt(sourceStack.Count - 1);
                    source = sourceStack[sourceStack.Count - 1];
                    continue;                                           //get next token from including source if not at main file
                }

                if (skipLevel == 0)
                {
                    break;
                }
            }

            return tok;
        }

        //not handling function-like macros yet
        public void invokeMacro(Macro macro)
        {
            Macro.setCurrentMacro(macro, null);
        }


        //- directive handling ------------------------------------------------

        //(6.10) Preprocessing directives

        //handle directive, this will read all the pp tokens to the eoln in the directive line
        public void handleDirective()
        {
            PPToken tok = source.getPPToken();
            while (tok.type == PPTokenType.SPACE)       //skip space(s) & get directive name
            {
                tok = source.getPPToken();
            }

            if (tok.type == PPTokenType.EOLN)          //skip empty directives, ie "#  <eoln>"
            {
                return;
            }

            if (tok.type == PPTokenType.WORD)
            {
                switch (tok.str)
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
                        //parser.error("saw unknown directive #" + tok.str + " at " + tok.loc.ToString());
                        readRestOfDirective(false);
                        break;
                }
            }
            else
            {
                //parser.error("invalid directive #" + pptok.str + " at " + pptok.loc.ToString());
                readRestOfDirective(false);
            }
        }

        public List<PPToken> readRestOfDirective(bool keepSpaces)
        {
            List<PPToken> tokens = new List<PPToken>();
            PPToken tok = source.getPPToken();
            while (tok.type != PPTokenType.EOLN)
            {
                if ((tok.type != PPTokenType.SPACE) || keepSpaces)
                {
                    tokens.Add(tok);
                }
                tok = source.getPPToken();
            }
            return tokens;
        }

        //- source inclusion --------------------------------------------------

        public void handleIncludeDirective()
        {
            ////Console.WriteLine("saw #include");
            //SourceLocation includeLoc = pptok.loc;
            //List<PPToken> frags = readRestOfDirective(false);

            //String filename = "";
            //bool quoted = false;

            //if (frags[0].type == PPTokenType.STRING)
            //{
            //    filename = frags[0].str;
            //    int ofs = filename[0] == 'L' ? 2 : 1;
            //    filename = filename.Substring(ofs, filename.Length - (ofs + 1));
            //    quoted = true;
            //}
            //else
            //{
            //    int i = 1;
            //    while ((frags[i].type != PPTokenType.PUNCT) || (frags[i].str[0] != '>'))
            //    {
            //        filename += frags[i++].str;
            //    }
            //}

            //String pathname = findSourceFile(filename, quoted);
            //scan = new Scanner(parser, pathname, includeLoc);           //open include source file & make it current scanner
            //sourceStack.Add(scan);
        }

        //this will search a list of include paths at some point
        public string findSourceFile(string filename, bool quoted)
        {
            return filename;
        }

        //- macro definition --------------------------------------------------

        public void handleDefineDirective()
        {
            Console.WriteLine("saw #define");
            List<PPToken> frags = readRestOfDirective(true);
            Macro.defineMacro(frags);
        }

        public void handleUndefDirective()
        {
            Console.WriteLine("saw #undef");
            List<PPToken> tokens = readRestOfDirective(false);
            PPToken tok = tokens[0];

            Macro.undefineMacro(tok.str);
        }

        //- conditional compilation -----------------------------------

        public void handleIfDirective()
        {
            Console.WriteLine("saw #if");
            List<PPToken> tokens = readRestOfDirective(false);
        }

        public void handleIfdefDirective()
        {
            Console.WriteLine("saw #ifdef");
            List<PPToken> tokens = readRestOfDirective(false);
            //    Token token = scanner.scanToken();
            //    Macro macro = Macro.lookupMacro(token);
        }

        public void handleIfndefDirective()
        {
            Console.WriteLine("saw #ifndef");
            List<PPToken> tokens = readRestOfDirective(false);

            //    Token token = scanner.scanToken();
            //    Macro macro = Macro.lookupMacro(token);
        }

        public void handleElifDirective()
        {
            Console.WriteLine("saw #elif");
            List<PPToken> frags = readRestOfDirective(false);
        }

        public void handleElseDirective()
        {
            Console.WriteLine("saw #else");
            List<PPToken> frags = readRestOfDirective(false);
        }

        public void handleEndifDirective()
        {
            Console.WriteLine("saw #endif");
            List<PPToken> frags = readRestOfDirective(false);
        }

        //- miscellaneous -----------------------------------------------------

        public void handleLineDirective()
        {
            Console.WriteLine("saw #line");
            List<PPToken> frags = readRestOfDirective(false);
        }

        public void handleErrorDirective()
        {
            Console.WriteLine("saw #error");
            List<PPToken> frags = readRestOfDirective(false);
        }

        public void handlePragmaDirective()
        {
            //Console.WriteLine("saw #pragma");
            List<PPToken> frags = readRestOfDirective(true);
            parser.handlePragma(frags);
        }
    }

    //- pptoken source class --------------------------------------------------

    public abstract class PPTokenSource
    {
        abstract public PPToken getPPToken();
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");