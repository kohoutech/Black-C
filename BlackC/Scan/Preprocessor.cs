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
        public Scanner scan;

        public PPToken pptok;
        bool atLineStart;

        public List<Scanner> sourceStack;

        public Preprocessor(Parser _parser, string filename)
        {
            parser = _parser;

            Macro.initMacros();

            sourceStack = new List<Scanner>();
            scan = new Scanner(parser, filename);           //open main source file
            sourceStack.Add(scan);                          //and add it to bottom of include stack

            pptok = null;
            atLineStart = true;
        }

        //- preprocessing only ------------------------------------------------

        public void preprocessFile(String filename)
        {
            List<String> lines = new List<string>();
            StringBuilder line = new StringBuilder();

            PPToken frag = getPPToken();
            while (frag.type != PPTokenType.EOF)
            {
                if (frag.type == PPTokenType.EOLN)
                {
                    lines.Add(line.ToString());
                    line.Clear();
                }
                else
                {
                    line.Append(frag.ToString());
                }
                frag = getPPToken();                
            }
            if (line.Length > 0)
            {
                lines.Add(line.ToString());
            }

            //if not saving spaces in output, compress multiple blank lines into one blank line
            if (!parser.saveSpaces)
            {
                lines = removeBlankLines(lines);
            }

            File.WriteAllLines(filename, lines);
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

        //- fragment stream handling ------------------------------------------

        //handles macro expansion & eof in include files
        public PPToken getPPToken()
        {
            PPToken tok = null;
            bool done = false;

            //while (!done)
            //{
            //    done = true;
            //    if (Macro.inMacro())
            //    {
            //        frag = Macro.getfrag();
            //    }
            //    else
            //    {
                    tok = getScannerToken();
            //    }

            //    //check for a macro
            //    if (frag.type == PPTokenType.WORD)
            //    {
            //        Macro macro = Macro.lookupMacro(frag.str);
            //        if (macro != null)
            //        {
            //            invokeMacro(macro);                 //start macro running
            //            done = false;                       //and loop around to get first macro fragment (if not empty)
            //        }
            //    }

            //    //check if we've hit the end of file. if this is an include file, pull it off the stack 
            //    //and resume scanning at the point we stopped in the including file
            //    //we return the EOF token from the main file only
            //    if ((frag.type == PPTokenType.EOF) && (sourceStack.Count > 1))
            //    {
            //        Console.WriteLine("closing include file " + sourceStack[sourceStack.Count - 1].filename);
            //        sourceStack.RemoveAt(sourceStack.Count - 1);
            //        scan = sourceStack[sourceStack.Count - 1];
            //        done = false;                                           //get next token from including source if not at main file
            //    }
            //}

            return tok;
        }

        //not handling function-like macros yet
        public void invokeMacro(Macro macro)
        {
            Macro.setCurrentMacro(macro, null);
        }

        //handle directives in the scanner's fragment stream, will be sent to tokenizer as EOLN fragments
        public PPToken getScannerToken()
        {
            pptok = scan.getPPToken();
            //Console.WriteLine("fragment = " + pptok.ToString());

            ////check for directive as first non-space frag at start of line
            //if (atLineStart)
            //{
            //    if ((pptok.type == PPTokenType.PUNCT) && (pptok.str[0] == '#'))
            //    {
            //        handleDirective();      //cur fragment will be left as the EOLN at end of directive line
            //    }
            //    else
            //    {
            //        atLineStart = (pptok.type == PPTokenType.SPACE);
            //    }
            //}
            //if ((pptok.type == PPTokenType.EOLN) || (pptok.type == PPTokenType.EOF))
            //{
            //    atLineStart = true;
            //}

            return pptok;
        }

        //- directive handling ------------------------------------------------

        //(6.10) Preprocessing directives

        //handle directive, this will read all the fragments to the eoln in the directive line
        public void handleDirective()
        {
            //pptok = scan.getPPToken();
            //while (pptok.type == PPTokenType.SPACE)  //skip space(s) & get directive name
            //{
            //    pptok = scan.getPPToken();
            //}

            //if (pptok.type == PPTokenType.EOLN)          //skip empty directives, ie "#  <eoln>"
            //{
            //    return;
            //}

            //if (pptok.type == PPTokenType.WORD)
            //{
            //    switch (pptok.str)
            //    {
            //        case "include":
            //            handleIncludeDirective();
            //            break;

            //        case "define":
            //            handleDefineDirective();
            //            break;

            //        case "undef":
            //            handleUndefDirective();
            //            break;

            //        case "if":
            //            handleIfDirective();
            //            break;

            //        case "ifdef":
            //            handleIfdefDirective();
            //            break;

            //        case "ifndef":
            //            handleIfndefDirective();
            //            break;

            //        case "elif":
            //            handleElifDirective();
            //            break;

            //        case "else":
            //            handleElseDirective();
            //            break;

            //        case "endif":
            //            handleEndifDirective();
            //            break;

            //        case "line":
            //            handleLineDirective();
            //            break;

            //        case "error":
            //            handleErrorDirective();
            //            break;

            //        case "pragma":
            //            handlePragmaDirective();
            //            break;

            //        default:
            //            parser.error("saw unknown directive #" + pptok.str + " at " + pptok.loc.ToString());
            //            readRestOfDirective(false);
            //            break;
            //    }
            //}
            //else
            //{
            //    parser.error("invalid directive #" + pptok.str + " at " + pptok.loc.ToString());
            //    readRestOfDirective(false);
            //}
        }

        public List<PPToken> readRestOfDirective(bool keepSpaces)
        {
            List<PPToken> frags = new List<PPToken>();
            //pptok = scan.getFrag();
            //while (pptok.type != PPTokenType.EOLN)
            //{
            //    if ((pptok.type != PPTokenType.SPACE) || keepSpaces)
            //    {
            //        frags.Add(pptok);
            //    }
            //    pptok = scan.getFrag();
            //}
            return frags;
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
            //Console.WriteLine("saw #define");
            List<PPToken> frags = readRestOfDirective(true);
            Macro.defineMacro(frags);
        }

        public void handleUndefDirective()
        {
            Console.WriteLine("saw #undef");
            List<PPToken> frags = readRestOfDirective(false);

            //    Token token = scanner.scanToken();
            //    Macro.undefineMacro(token);
        }

        //- conditional compilation -----------------------------------

        public void handleIfDirective()
        {
            Console.WriteLine("saw #if");
            List<PPToken> frags = readRestOfDirective(false);

        }

        public void handleIfdefDirective()
        {
            Console.WriteLine("saw #ifdef");
            List<PPToken> frags = readRestOfDirective(false);
            //    Token token = scanner.scanToken();
            //    Macro macro = Macro.lookupMacro(token);
        }

        public void handleIfndefDirective()
        {
            Console.WriteLine("saw #ifndef");
            List<PPToken> frags = readRestOfDirective(false);

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

    public class PPTokenSource
    {
        virtual public PPToken getPPToken()
        {
            return null;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");