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
    public class Scanner
    {
        public Parser parser;

        public string filename;
        public SourceLocation caller;
        String source;

        int srcpos;
        List<SourceNote> sourceMap;
        int sourceMapPos;
        bool saveSpaces;
        StringBuilder spstr;

        public Scanner(Parser _parser, String _filename, SourceLocation _caller)
        {
            parser = _parser;
            filename = _filename;
            caller = _caller;

            try
            {
                source = File.ReadAllText(filename);        //read entire file as single string
            }
            catch (Exception e)
            {
                parser.fatal("error reading source file " + filename + " : " + e.Message);
            }
            transformSource();

            srcpos = 0;
            sourceMapPos = 0;
            saveSpaces = parser.options.saveSpaces;
            spstr = new StringBuilder();
        }

        //- source transformation ---------------------------------------------

        /*(5.1.1.2) 
            translation phase 1 : replace trigraphs with their eqivalent chars 
            translation phase 2 : splice lines ending with line continuation chars together
         */

        Dictionary<char, char> trigraphs = new Dictionary<char, char>() { 
                        { '=', '#' },  { ')', ']' }, { '!', '|' }, 
                        { '(', '[' },  { '\'','^' }, { '>', '}' },
                        { '/', '\\' }, { '<', '{' }, { '-', '~' }};

        //translate source & build source line map
        public void transformSource()
        {
            if (!source.EndsWith("\n"))
            {
                source = source + '\n';                 //add eoln at end of file if not already there
            }
            source = source + '\0';                     //mark end of file

            StringBuilder result = new StringBuilder();
            sourceMap = new List<SourceNote>();
            int linenum = 1;
            int linepos = 0;
            sourceMap.Add(new SourceNote(1, 0, 0));

            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == '?' && (source[i + 1] == '?') && trigraphs.ContainsKey(source[i + 2]))
                {
                    result.Append(trigraphs[source[i + 2]]);
                    linepos += 3;
                    sourceMap.Add(new SourceNote(linenum, linepos, result.Length));
                    i += 2;
                }
                else if ((source[i] == '\\') && (source[i + 1] == '\n'))
                {
                    i += 1;
                    linenum++;
                    linepos = 1;
                    sourceMap.Add(new SourceNote(linenum, linepos, result.Length + 1));
                }
                else if ((source[i] == '\\') && ((source[i + 1] == '\r') && (source[i + 2] == '\n')))
                {
                    i += 2;
                    linenum++;
                    linepos = 1;
                    sourceMap.Add(new SourceNote(linenum, linepos, result.Length + 1));
                }
                else if (source[i] == '\n')
                {
                    linenum++;
                    linepos = 1;
                    sourceMap.Add(new SourceNote(linenum, linepos, result.Length + 1));
                    result.Append(source[i]);
                }
                else
                {
                    result.Append(source[i]);
                    linepos++;
                }
            }
            source = result.ToString();
        }

        //- skipping whitespace & comments  -----------------------------------

        public bool isSpace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\v' || ch == '\f' || ch == '\r');
        }

        public void skipWhitespace()
        {
            bool done = false;
            char ch = source[srcpos];
            while (!done)
            {
                //skip any whitespace
                if ((ch == ' ') || (ch == '\t') || (ch == '\f') || (ch == '\v') || (ch == '\r'))
                {
                    if ((ch == ' ') || (ch == '\t'))        //keep tabs on spaces
                    {
                        spstr.Append(ch);
                    }
                    ch = source[++srcpos];
                    continue;
                }

                //skip any following comments, if we found a comment, then we're not done yet
                if ((ch == '/') && (source[srcpos + 1] == '/'))
                {
                    skipLineComment();
                    ch = source[++srcpos];
                    continue;
                }

                if ((ch == '/') && (source[srcpos + 1] == '*'))
                {
                    skipBlockComment();
                    ch = source[++srcpos];
                    continue;
                }

                //if we've gotten to here, then we not at a space, eoln or comment & we're done
                done = true;
            };
        }

        //skip remainder of current line up to (but not including) the eoln char
        //since nothing follows this but the eoln, don't include this spaces in the preprocessor's fragment
        public void skipLineComment()
        {
            srcpos += 2;
            char ch = source[srcpos];
            while (ch != '\n' && ch != '\0')
            {
                ch = source[++srcpos];
            }
        }

        //skip source characters until reach next '*/' or eof
        public void skipBlockComment()
        {
            srcpos += 2;
            int mark = spstr.Length;
            spstr.Append("  ");
            char ch = source[srcpos];
            while (!((ch == '\0') || ((ch == '*') && (source[srcpos + 1] == '/'))))
            {
                if (ch == '\n')
                {
                    spstr = spstr.Remove(mark,spstr.Length - mark);       //discard all the comment chars in this line
                    spstr.Append('\n');
                    mark++;
                }
                else
                {
                    spstr.Append(' ');
                }
                ch = source[++srcpos];
            }
            if (ch != '\0')         //if we haven't reached eof, then skip '*/' chars
            {
                srcpos += 2;
                spstr.Append("  ");
            }
        }

        //- source scanning ------------------------------------------------

        public bool isAlpha(char ch)
        {
            return ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_');
        }

        public bool isAlphaNum(char ch)
        {
            return isAlpha(ch) || isDigit(ch);
        }

        /*(6.4.2.1) 
         identifier:
            identifier-nondigit
            identifier identifier-nondigit
            identifier digit

          (6.4.2.1) 
         identifier-nondigit:
            nondigit

          (6.4.2.1) 
         nondigit: one of
            _ 
            a ... z
            A ... Z

          (6.4.2.1) 
         digit: one of
            0 ... 9
         */
        public string scanIdentifier()
        {
            String idstr = "";
            char ch = source[srcpos];
            while (isAlphaNum(ch))
            {
                idstr = idstr + ch;
                ch = source[++srcpos];
            }
            return idstr;
        }

        public bool isDigit(char ch)
        {
            return (ch >= '0' && ch <= '9');
        }

        /*
          (6.4.4.2) 
         floating-constant:
            decimal-floating-constant

          (6.4.4.2) 
         decimal-floating-constant:
            fractional-constant exponent-part[opt] floating-suffix[opt]
            digit-sequence exponent-part floating-suffix[opt]

          (6.4.4.2) 
         fractional-constant:
            digit-sequence[opt] . digit-sequence
            digit-sequence .

          (6.4.4.2) 
         exponent-part:
            e sign[opt] digit-sequence
            E sign[opt] digit-sequence

          (6.4.4.2) 
         sign: one of
            + -

          (6.4.4.2) 
         digit-sequence:
            digit
            digit-sequence digit

          (6.4.4.2) 
         floating-suffix: one of
            f l F L
        */
        public string scanFloatConst(String fstr)
        {
            if (fstr.EndsWith("."))      //get optional decimal part
            {
                char c1 = source[srcpos];
                while (c1 >= '0' && c1 <= '9')
                {
                    fstr = fstr + c1;
                    c1 = source[++srcpos];
                }
                if (fstr.EndsWith("."))      //if we didn't have a decimal part above, we add one anyway (123. --> 123.0)
                {
                    fstr = fstr + '0';
                }
                if ((c1 == 'e') || (c1 == 'E'))     //then check for exponent part
                {
                    fstr = fstr + 'E';
                    srcpos++;
                }
            }

            if (fstr.EndsWith("E"))      //get optional exponent part
            {
                char s1 = source[srcpos];
                if ((s1 == '+') || (s1 == '-'))     //exponent sign is optional
                {
                    fstr = fstr + s1;
                    s1 = source[++srcpos];
                }
                while (s1 >= '0' && s1 <= '9')
                {
                    fstr = fstr + s1;
                    s1 = source[++srcpos];
                }
            }

            //check for float const suffixes
            char f1 = source[srcpos];
            if ((f1 == 'f') || (f1 == 'F') || (f1 == 'l') || (f1 == 'L'))
            {
                fstr = fstr + Char.ToUpper(f1);
                srcpos++;
            }

            return fstr;
        }

        /*(6.4.4.1) 
         integer-constant:
            decimal-constant integer-suffix[opt]
            octal-constant integer-suffix[opt]
            hexadecimal-constant integer-suffix[opt]
         
          (6.4.4.1) 
         decimal-constant:
            nonzero-digit
            decimal-constant digit

          (6.4.4.1) 
         octal-constant:
            0
            octal-constant octal-digit
         
          (6.4.4.1) 
         hexadecimal-constant:
            hexadecimal-prefix hexadecimal-digit
            hexadecimal-constant hexadecimal-digit
         
          (6.4.4.1) 
         hexadecimal-prefix: one of
            0x 0X
         
          (6.4.4.1) 
         nonzero-digit: one of
            1 ... 9
        
          (6.4.4.1) 
         octal-digit: one of
            0 ... 7
        
          (6.4.4.1) 
         hexadecimal-digit: one of
            0 ... 9
            a ... f
            A ... F
        
          (6.4.4.1) 
         integer-suffix:
            unsigned-suffix long-suffix[opt]
            unsigned-suffix long-long-suffix
            long-suffix unsigned-suffix[opt]
            long-long-suffix unsigned-suffix[opt]
        
          (6.4.4.1) 
         unsigned-suffix: one of
            u U
        
          (6.4.4.1) 
         long-suffix: one of
            l L
        
          (6.4.4.1) 
         long-long-suffix: one of
            ll LL
        */
        public string scanNumber()     //either int or float const
        {
            int bass = 10;      //default number base
            char ch = source[srcpos];
            String numstr = "" + ch;

            if (ch != '.')       //get mantissa
            {
                if (ch == '0')             //set base
                {
                    if ((source[srcpos + 1] == 'X' || source[srcpos + 1] == 'x'))
                    {
                        bass = 16;
                        numstr += source[++srcpos];
                        srcpos++;
                    }
                    else
                    {
                        bass = 8;
                    }
                }
                ch = source[++srcpos];
                while (((bass == 10) && (ch >= '0' && ch <= '9')) ||
                        ((bass == 8) && (ch >= '0' && ch <= '7')) ||
                        ((bass == 16) && ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f'))))
                {
                    numstr = numstr + ch;
                    ch = source[++srcpos];
                }
            }
            else
            {
                numstr = "0.";                         //add the leading 0 to a float const str '.1234'
                return scanFloatConst(numstr);         //get floating point constant string
            }

            //got the mantissa, if the next char is decimal point or exponent, then it's a float const
            if ((ch == '.') || (ch == 'E') || (ch == 'e'))
            {
                srcpos++;                         //skip '.' or 'E' or 'e'

                if (ch == 'E' || ch == 'e')
                {
                    numstr = numstr + ".0";            //add decimal part if missing (123E10 --> 123.0E10)
                }
                numstr = numstr + Char.ToUpper(ch);
                return scanFloatConst(numstr);         //get floating point constant token
            }

            //not a float, check for int const suffixes, can be in any order
            bool usuffix = false;
            int lsuffix = 0;
            string intstr = numstr;
            for (int i = 0; i < 2; i++)     //check for int const suffixes
            {
                if ((!usuffix) && ((ch == 'u') || (ch == 'U')))
                {
                    usuffix = true;
                    numstr = numstr + "U";
                    ch = source[++srcpos];
                }

                if ((lsuffix == 0) && ((ch == 'l') || (ch == 'L')))
                {
                    lsuffix++;
                    numstr = numstr + "L";
                    ch = source[++srcpos];
                    if ((ch == 'l') || (ch == 'L'))
                    {
                        lsuffix++;
                        numstr = numstr + "L";
                        ch = source[++srcpos];
                    }
                }
            }

            return numstr;
        }

        /*(6.4.4.4) 
         character-constant:
            ' c-char-sequence '
            L' c-char-sequence '

          (6.4.4.4) 
         c-char-sequence:
            c-char
            c-char-sequence c-char
         
          (6.4.4.4)
         c-char:
            any member of the source character set except the single-quote ', backslash \, or new-line character
            escape-sequence
         
          (6.4.4.4) 
         escape-sequence:
            simple-escape-sequence
            octal-escape-sequence
            hexadecimal-escape-sequence

          (6.4.4.4) 
         simple-escape-sequence: one of
                \' \" \? \\ \a \b \f \n \r \t \v
         
          (6.4.4.4) 
         octal-escape-sequence:
            \ octal-digit
            \ octal-digit octal-digit
            \ octal-digit octal-digit octal-digit
         
          (6.4.4.4) 
         hexadecimal-escape-sequence:
            \x hexadecimal-digit
            hexadecimal-escape-sequence hexadecimal-digit
          */
        public string scanCharLiteral(bool isLong)
        {
            string cstr = (isLong) ? "L\'" : "\'";
            char ch = source[++srcpos];
            while ((ch != '\'') && (ch != '\n') && (ch != '\0'))
            {
                if ((ch == '\\') && (source[srcpos + 1] == '\''))
                {
                    cstr = cstr + "\\\'";
                    srcpos++;                    //skip over escaped single quotes
                }
                else
                {
                    cstr = cstr + ch;
                }
                ch = source[++srcpos];
            }
            if (ch == '\'')         //add the closing quote if not at eoln or eof
            {
                cstr = cstr + '\'';
                srcpos++;
            }
            return cstr;
        }

        /*(6.4.5) 
         string-literal:
            " s-char-sequenceopt "
            L" s-char-sequenceopt "

          (6.4.5) 
         s-char-sequence:
            s-char
            s-char-sequence s-char

          (6.4.5) 
         s-char:
            any member of the source character set except the double-quote ", backslash \, or new-line character
            escape-sequence
         */
        public string scanString(bool isLong)
        {
            string sstr = (isLong) ? "L\"" : "\"";
            char ch = source[++srcpos];
            while ((ch != '\"') && (ch != '\n') && (ch != '\0'))
            {
                if ((ch == '\\') && (source[srcpos + 1] == '\"'))
                {
                    sstr = sstr + "\\\"";
                    srcpos++;                    //skip over escaped double quotes
                }
                else
                {
                    sstr = sstr + ch;
                }
                ch = source[++srcpos];
            }
            if (ch == '\"')                     //skip the closing quote if not at eoln or eof
            {
                sstr = sstr + '\"';
                srcpos++;
            }
            return sstr;
        }

        //- main scanning method ----------------------------------------------

        public char TranslateDiagraphs()
        {
            char ch = source[srcpos];
            if (ch == '<' && (source[srcpos + 1] == ':'))
            {
                ch = '[';
                srcpos++;
            }
            if (ch == ':' && (source[srcpos + 1] == '>'))
            {
                ch = ']';
                srcpos++;
            }
            if (ch == '<' && (source[srcpos + 1] == '%'))
            {
                ch = '{';
                srcpos++;
            }
            if (ch == '%' && (source[srcpos + 1] == '>'))
            {
                ch = '}';
                srcpos++;
            }
            if (ch == '%' && (source[srcpos + 1] == ':'))
            {
                ch = '#';
                srcpos++;
            }
            //we handle '%:%:' --> '##' when we handle '#' the 2nd time

            return ch;
        }

        public SourceLocation getFragLocation()
        {
            while ((sourceMapPos < sourceMap.Count - 1) && (srcpos >= sourceMap[sourceMapPos + 1].newpos))
            {
                sourceMapPos++;
            }
            int linenum = sourceMap[sourceMapPos].linenum;
            int linepos = (srcpos - sourceMap[sourceMapPos].newpos) + sourceMap[sourceMapPos].linepos;
            return new SourceLocation(filename, linenum, linepos, caller);
        }

        //(5.1.1.2) translation phase 3 : scan source line into preprocessing tokens
        public Fragment getFrag()
        {
            Fragment frag = null;
            SourceLocation loc = getFragLocation();
            spstr.Clear();

            char ch = source[srcpos];
            while (true)
            {
                if (isSpace(ch))
                {
                    skipWhitespace();
                    frag = new Fragment(FragType.SPACE, (saveSpaces ? spstr.ToString() : " "));
                    break;
                }

                //line comment
                if (ch == '/' && (source[srcpos + 1] == '/'))
                {
                    skipLineComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //block comment
                if (ch == '/' && (source[srcpos + 1] == '*'))
                {
                    skipBlockComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //L is a special case since it can start long char constants or long string constants, as well as identifiers
                if (ch == 'L')
                {
                    srcpos++;                     //skip initial 'L'
                    if ((source[srcpos + 1]) == '\'')
                    {
                        string chstr = scanCharLiteral(true);
                        frag = new Fragment(FragType.CHAR, chstr);
                        break;
                    }
                    else if ((source[srcpos + 1]) == '"')
                    {
                        string sstr = scanString(true);
                        frag = new Fragment(FragType.STRING, sstr);
                        break;
                    }
                }

                //identifier
                if (isAlpha(ch))
                {
                    string idstr = scanIdentifier();
                    frag = new Fragment(FragType.WORD, idstr);
                    break;
                }

                //numeric constant
                //'.' can start a float const
                if ((isDigit(ch)) || (ch == '.' && isDigit(source[srcpos + 1])))
                {
                    string numstr = scanNumber();
                    frag = new Fragment(FragType.NUMBER, numstr);
                    break;
                }

                //char constant
                if (ch == '\'')
                {
                    string chstr = scanCharLiteral(false);
                    frag = new Fragment(FragType.CHAR, chstr);
                    break;
                }

                //string constant
                if (ch == '"')
                {
                    string sstr = scanString(false);
                    frag = new Fragment(FragType.STRING, sstr);
                    break;
                }

                //end of line - does not include eolns in block comments or spliced lines
                if (ch == '\n')
                {
                    frag = new Fragment(FragType.EOLN, "<eoln>");
                    srcpos++;
                    break;
                }

                //end of file - check if this isn't a stray 0x0 char in file, if so pass it on as punctuation
                if ((ch == '\0') && (srcpos == (source.Length - 1)))
                {
                    frag = new Fragment(FragType.EOF, "<eof>");
                    break;
                }

                //translate chars before handling punctuation
                ch = TranslateDiagraphs();

                //anything else is punctuation
                frag = new Fragment(FragType.PUNCT, "" + ch);
                srcpos++;
                break;
            }

            frag.loc = loc;
            return frag;
        }
    }

    //- fragment class ----------------------------------------------

    public enum FragType
    {
        WORD,
        NUMBER,
        STRING,
        CHAR,
        PUNCT,
        SPACE,
        EOLN,
        EOF
    }

    public class Fragment
    {
        public String str;
        public FragType type;
        public SourceLocation loc;
        public int spCount;
        public int spLines;

        public Fragment(FragType _type, String _str)
        {
            str = _str;
            type = _type;
            loc = null;
            spCount = 0;
            spLines = 0;
        }

        public override string ToString()
        {
            //return str + " at " + loc.ToString();
            return str;
        }
    }

    //- source file position tracking -----------------------------------------

    public class SourceNote
    {
        public int linenum;
        public int linepos;
        public int newpos;

        public SourceNote(int _linenum, int _linepos, int _newpos)
        {
            linenum = _linenum;
            linepos = _linepos;
            newpos = _newpos;
        }
    }

    public class SourceLocation
    {
        public string filename;
        public int linenum;
        public int linepos;
        public SourceLocation caller;

        public SourceLocation(string fname, int lnum, int lpos, SourceLocation _caller)
        {
            filename = fname;
            linenum = lnum;
            linepos = lpos;
            caller = _caller;
        }

        public override string ToString()
        {
            string result = filename + '(' + linenum + ':' + linepos + ')';
            if (caller != null)
            {
                result += (" included from " + caller.ToString());
            }

            return result;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");