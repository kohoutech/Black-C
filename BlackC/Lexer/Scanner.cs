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

        string filename;
        String source;
        int srcpos;
        int linenum;
        int linestart;

        public Scanner(Parser _parser, String _filename)
        {
            parser = _parser;
            filename = _filename;
            try
            {
                source = File.ReadAllText(filename);        //read entire file as single string
                if (!source.EndsWith("\n"))
                {
                    source = source + '\n';                 //add eoln at end of file if not already there
                }
                source = source + '\0';                     //mark end of file
            }
            catch (Exception e)
            {
                parser.error("error reading source file " + filename + " : " + e.Message);
            }

            srcpos = 0;
            linenum = 1;
            linestart = 0;
        }

        //update location when we go to a new line
        public void handleEoln()
        {
            linenum++;
            linestart = srcpos;
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

        //(5.2.1.1)
        //translate char if trigraph & return offset pointing to where char would be in source file
        public int translateTrigraphs(ref char ch)
        {
            int offset = 0;
            if (ch == '?' && (source[srcpos + 1] == '?'))
            {
                char ch2 = source[srcpos + 2];
                if (trigraphs.ContainsKey(ch2))
                {
                    ch = trigraphs[ch2];
                    offset = 2;
                }
            }
            return offset;
        }

        //line continuation - return char following either (\\ + \n) or (\\ + \r\n) & true if line was spliced
        public bool spliceContinuedLines(ref char ch, ref int offset)
        {
            bool spliced = false;
            if ((ch == '\\') && (source[srcpos + 1 + offset] == '\n'))
            {
                offset += 2;
                spliced = true;
                ch = source[srcpos + offset];
            }
            if ((ch == '\\') && ((source[srcpos + 1 + offset] == '\r') && (source[srcpos + 2 + offset] == '\n')))
            {
                offset += 3;
                spliced = true;
                ch = source[srcpos + offset];
            }
            return spliced;
        }

        //get the char at the current source pos, possibly translating chars & joining lines
        public char getChar()
        {
            char ch = source[srcpos];
            int offset = translateTrigraphs(ref ch);
            spliceContinuedLines(ref ch, ref offset);
            return ch;
        }

        //goto the next source pos, possibly translating chars & joining lines
        //if we've crossed the end of line
        public void gotoNextChar()
        {
            char ch = source[srcpos];
            int offset = translateTrigraphs(ref ch);
            bool spliced = spliceContinuedLines(ref ch, ref offset);
            srcpos = srcpos + offset + 1;
            if ((ch == '\n') || spliced)
            {
                handleEoln();
            }
        }

        //goto next sourc epos & return the char there
        public char getNextChar()
        {
            gotoNextChar();
            char ch = getChar();
            return ch;
        }

        //char lookahead, returns the char at (source pos + lookahead pos) but do not advance current source pos;
        public char getCharAt(int lookahead)
        {
            int curpos = srcpos;
            while (lookahead > 0)
            {
                //goto the next char w/o triggering eoln handling
                char chpos = source[srcpos];
                int offset = translateTrigraphs(ref chpos);
                spliceContinuedLines(ref chpos, ref offset);
                srcpos = srcpos + offset + 1;
                lookahead--;
            }
            char ch = getChar();        //get char at offset
            srcpos = curpos;            //and return to current source pos
            return ch;
        }

        //- skipping whitespace & comments  -----------------------------------

        public bool isSpace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\v' || ch == '\f' || ch == '\r');
        }

        public void skipWhitespace()
        {
            bool done = false;
            char ch = getChar();
            while (!done)
            {
                //skip any whitespace
                if ((ch == ' ') || (ch == '\t') || (ch == '\f') || (ch == '\v') || (ch == '\r'))
                {
                    ch = getNextChar();
                    continue;
                }

                //skip any following comments, if we found a comment, then we're not done yet
                if ((ch == '/') && (getCharAt(1) == '/'))
                {
                    gotoNextChar();
                    gotoNextChar();
                    skipLineComment();
                    ch = getNextChar();
                    continue;
                }

                if ((ch == '/') && (getCharAt(1) == '*'))
                {
                    gotoNextChar();
                    gotoNextChar();
                    skipBlockComment();
                    ch = getNextChar(); 
                    continue;
                }

                //if we've gotten to here, then we not at a space, eoln or comment & we're done
                done = true;
            };
        }

        //skip remainder of current line up to eoln char
        public void skipLineComment()
        {
            char ch = getChar();
            while (ch != '\n' && ch != '\0')
            {
                ch = getNextChar();
            }
        }

        //skip source characters until reach next '*/' or eof
        public void skipBlockComment()
        {
            char ch = getChar();
            while (!((ch == '\0') || ((ch == '*') && (getCharAt(1) == '/'))))
            {
                ch = getNextChar();
            }
            if (ch != '\0')         //if we haven't reached eof, then skip '*/' chars
            {
                gotoNextChar();
                gotoNextChar();
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
            char ch = getChar();
            while (isAlphaNum(ch))
            {
                idstr = idstr + ch;
                ch = getNextChar();
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
                char c1 = getChar();
                while (c1 >= '0' && c1 <= '9')
                {
                    fstr = fstr + c1;
                    c1 = getNextChar();
                }
                if (fstr.EndsWith("."))      //if we didn't have a decimal part above, we add one anyway (123. --> 123.0)
                {
                    fstr = fstr + '0';
                }
                if ((c1 == 'e') || (c1 == 'E'))     //then check for exponent part
                {
                    fstr = fstr + 'E';
                    gotoNextChar();
                }
            }

            if (fstr.EndsWith("E"))      //get optional exponent part
            {
                char s1 = getChar();
                if ((s1 == '+') || (s1 == '-'))     //exponent sign is optional
                {
                    fstr = fstr + s1;
                    s1 = getNextChar();
                }
                while (s1 >= '0' && s1 <= '9')
                {
                    fstr = fstr + s1;
                    s1 = getNextChar();
                }
            }

            //check for float const suffixes
            char f1 = getChar();
            if ((f1 == 'f') || (f1 == 'F') || (f1 == 'l') || (f1 == 'L'))
            {
                fstr = fstr + Char.ToUpper(f1);
                gotoNextChar();
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
            char ch = getChar();
            String numstr = "" + ch;

            if (ch != '.')       //get mantissa
            {
                if (ch == '0')             //set base
                {
                    if ((getCharAt(1) == 'X' || getCharAt(1) == 'x'))
                    {
                        bass = 16;
                        gotoNextChar();             
                        numstr += getChar();
                        gotoNextChar();
                    }
                    else
                    {
                        bass = 8;
                    }
                }
                ch = getNextChar();
                while (((bass == 10) && (ch >= '0' && ch <= '9')) ||
                        ((bass == 8) && (ch >= '0' && ch <= '7')) ||
                        ((bass == 16) && ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f'))))
                {
                    numstr = numstr + ch;
                    ch = getNextChar();
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
                gotoNextChar();                         //skip '.' or 'E' or 'e'

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
                    ch = getNextChar();
                }

                if ((lsuffix == 0) && ((ch == 'l') || (ch == 'L')))
                {
                    lsuffix++;
                    numstr = numstr + "L";
                    ch = getNextChar();
                    if ((ch == 'l') || (ch == 'L'))
                    {
                        lsuffix++;
                        numstr = numstr + "L";
                        ch = getNextChar();
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
            char ch = getNextChar();
            while ((ch != '\'') && (ch != '\n') && (ch != '\0'))
            {
                if ((ch == '\\') && (getCharAt(1) == '\''))
                {
                    cstr = cstr + "\\\'";
                    gotoNextChar();                    //skip over escaped single quotes
                }
                else
                {
                    cstr = cstr + ch;                    
                }
                ch = getNextChar();
            }
            if (ch == '\'')         //add the closing quote if not at eoln or eof
            {
                cstr = cstr + '\'';
                gotoNextChar();
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
            char ch = getNextChar();
            while ((ch != '\"') && (ch != '\n') && (ch != '\0'))
            {
                if ((ch == '\\') && (getCharAt(1) == '\"'))
                {
                    sstr = sstr + "\\\"";
                    gotoNextChar();                    //skip over escaped double quotes
                }
                else
                {
                    sstr = sstr + ch;                    
                }
                ch = getNextChar();
            }
            if (ch == '\"')                     //skip the closing quote if not at eoln or eof
            {
                sstr = sstr + '\"';
                gotoNextChar();
            }
            return sstr;
        }

        public char TranslateDiagraphs()
        {
            char ch = getChar();
            if (ch == '<' && (getCharAt(1) == ':'))
            {
                ch = '[';
                gotoNextChar();
            }
            if (ch == ':' && (getCharAt(1) == '>'))
            {
                ch = ']';
                gotoNextChar();
            }
            if (ch == '<' && (getCharAt(1) == '%'))
            {
                ch = '{';
                gotoNextChar();
            }
            if (ch == '%' && (getCharAt(1) == '>'))
            {
                ch = '}';
                gotoNextChar();
            }
            if (ch == '%' && (getCharAt(1) == ':'))
            {
                ch = '#';
                gotoNextChar();
            }
            //we handle '%:%:' --> '##' when we handle '#' the 2nd time

            return ch;
        }

        //- main scanning method ----------------------------------------------

        //(5.1.1.2) translation phase 3 : scan source line into preprocessing tokens
        public Fragment getFrag()
        {
            Fragment frag = null;
            SourceLocation loc = new SourceLocation(filename, linenum, (srcpos - linestart + 1));

            char ch = getChar();
            while (true)
            {
                if (isSpace(ch))
                {
                    skipWhitespace();
                    frag = new Fragment(FragType.SPACE, "<space>");
                    break;
                }

                //line comment
                if (ch == '/' && (getCharAt(1) == '/'))
                {
                    gotoNextChar();
                    gotoNextChar();             //skip opening //
                    skipLineComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //block comment
                if (ch == '/' && (getCharAt(1) == '*'))
                {
                    gotoNextChar();
                    gotoNextChar();             //skip opening /*
                    skipBlockComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //L is a special case since it can start long char constants or long string constants, as well as identifiers
                if (ch == 'L')
                {
                    gotoNextChar();                     //skip initial 'L'
                    if ((getCharAt(1)) == '\'')
                    {
                        string chstr = scanCharLiteral(true);
                        frag = new Fragment(FragType.CHAR, chstr);
                        break;
                    }
                    else if ((getCharAt(1)) == '"')
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
                if ((isDigit(ch)) || (ch == '.' && isDigit(getCharAt(1))))
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
                    gotoNextChar();
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
                gotoNextChar();
                break;
            }

            frag.loc = loc;
            return frag;
        }
    }

    //---------------------------------------------------------------

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

        public Fragment(FragType _type, String _str)
        {
            str = _str;
            type = _type;
            loc = null;
        }

        public override string ToString()
        {
            return str + " at " + loc.ToString();
        }
    }

    //-------------------------------------------------------------------------

    public class SourceLocation
    {
        public string filename;
        public int linenum;
        public int linepos;

        public SourceLocation(string fname, int lnum, int lpos)
        {
            filename = fname;
            linenum = lnum;
            linepos = lpos;
        }

        public override string ToString()
        {
            return filename + '(' + linenum + ':' + linepos + ')';
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");