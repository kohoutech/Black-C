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
        string filename;
        String source;
        int srcpos;

        //old vars
        //SourceBuffer buffer;
        //string[] lines;
        //int linenum;
        //SourceLocation tokenLoc;
        //string curline;
        //bool atLineStart;            //token was at start of line
        //bool leadingWS;                 //token was preceeded by whitespace (including comments)

        public Scanner(String _filename)
        {
            filename = _filename;
            try
            {
                source = File.ReadAllText(filename);        //read entire file as single string
                source = source + '\0';                     //mark end of file
            }
            catch (Exception e)
            {
                Console.WriteLine("error reading source file " + filename + " : " + e.Message);
            }
        }

        ////- source file mgmt --------------------------------------------------

        //public void saveSource()
        //{
        //    //buffer.curline = curline;
        //    //buffer.linenum = linenum;
        //    //buffer.linepos = pos;
        //    //buffer.atBOL = atBOL;
        //    //buffer.eolnCount = eolnCount;
        //}

        //public void setSource(SourceBuffer srcbuf)
        //{
        //    buffer = srcbuf;
        //    //lines = srcbuf.lines;
        //    //linenum = srcbuf.linenum;
        //    //pos = srcbuf.linepos;
        //    //atBOL = srcbuf.atBOL;            

        //    //getCurrentLine();
        //    //atEOF = false;
        //}

        //- source transformation ---------------------------------------------

        Dictionary<char, char> trigraphs = new Dictionary<char, char>() { 
                        { '=', '#' },  { ')', ']' }, { '!', '|' }, 
                        { '(', '[' },  { '\'','^' }, { '>', '}' },
                        { '/', '\\' }, { '<', '{' }, { '-', '~' }};

        public char translateTrigraphs(char ch, out int offset)
        {
            offset = 0;
            if (ch == '?' && (source[srcpos + 1] == '?'))
            {
                char ch2 = source[srcpos + 2];
                if (trigraphs.ContainsKey(ch2))
                {
                    ch = trigraphs[ch2];
                    offset = 2;
                }
            }
            return ch;
        }

        //line continuation - either (\\ + \n) or (\\ + \r\n)
        public char spliceContinuedLines(char ch, ref int offset)
        {
            if ((ch == '\\') && (source[srcpos + 1 + offset] == '\n'))
            {
                offset += 2;
                ch = source[srcpos + offset];
            }
            if ((ch == '\\') && ((source[srcpos + 1 + offset] == '\r') && (source[srcpos + 2 + offset] == '\n')))
            {
                offset += 3;
                ch = source[srcpos + offset];
            }
            return ch;
        }

        //translation phase 1 : replace trigraphs with their eqivalent chars
        //translation phase 2 : splice lines ending with line continuation chars together
        public char getChar()
        {
            char ch = source[srcpos];
            int offset;
            ch = translateTrigraphs(ch, out offset);
            ch = spliceContinuedLines(ch, ref offset);
            return ch;
        }

        public void gotoNextChar()
        {
            char ch = source[srcpos];
            int offset;
            ch = translateTrigraphs(ch, out offset);
            ch = spliceContinuedLines(ch, ref offset);
            srcpos = srcpos + offset + 1;
        }

        public char getNextChar()
        {
            gotoNextChar();
            char ch = getChar();
            return ch;
        }

        public char getCharAt(int pos)
        {
            int curpos = srcpos;
            while (pos > 0)
            {
                    gotoNextChar();
                    pos--;
            }
            char ch = getChar();
            srcpos = curpos;
            return ch;
        }

        //- skipping whitespace & comments  -----------------------------------

        public bool isSpace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\v' || ch == '\f' || ch == '\r' || ch == '\n');
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
                    gotoNextChar();
                    ch = getChar();
                    continue;
                }

                //skip any eolns - any new line handling that needs to be done goes here
                if ((ch == '\n'))
                {
                    ch = getNextChar();
                    continue;
                }

                //skip any following comments, if we found a comment, then we're not done yet
                if ((ch == '/') && (getCharAt(1) == '/'))
                {
                    skipLineComment();
                    continue;
                }

                if ((ch == '/') && (getCharAt(1) == '*'))
                {
                    skipBlockComment();
                    continue;
                }

                //if we've gotten to here, then we not at a space, eoln or comment & we're done
                done = true;
            };
        }

        //skip remainder of current line & eoln chars
        public void skipLineComment()
        {
            char ch = getChar();
            while (ch != '\n' && ch != '\0')
            {
                ch = getNextChar();
            }
            if (ch != '\0')     //if not eof, then skip the eoln char
            {
                gotoNextChar();
            }
        }

        //skip source characters until reach next '*/' or eof
        public void skipBlockComment()
        {
            char ch = getChar();
            while ((ch != '\0') && ((ch != '*') && (getCharAt(1) != '/')))
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
                char ch2 = getNextChar();                   //get '.' or 'E' or 'e'

                if (ch == 'E' || ch == 'e')
                {
                    numstr = numstr + ".0";            //add decimal part if missing (123E10 --> 123.0E10)
                }
                numstr = numstr + Char.ToUpper(ch2);
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
        public string scanCharLiteral()
        {
            string chstr = "";
            //    String cstr = (c == 'L') ? "L\'" : "";

            //    while ((pos < curline.Length) && (curline[pos] != '\''))
            //    {
            //        if ((curline[pos] == '\\') && (pos < curline.Length - 1) && (curline[pos + 1] == '\''))
            //        {
            //            cstr = cstr + "\\\'";
            //            pos += 2;                   //skip over escaped single quotes
            //        }
            //        else
            //        {
            //            cstr = cstr + curline[pos];
            //            pos++;
            //        }
            //    }
            //    if ((pos < curline.Length))         //skip the closing quote if not at eoln
            //    {
            //        pos++;
            //    }
            //    Token token = new Token(TokenType.tCHARCONST, cstr, tokenLoc);
            //    setTokenFlags(token);
            return chstr;
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
        public string scanString()
        {
            String str = "";
            //    String str = (c == 'L') ? "L\"" : "";

            //    while ((pos < curline.Length) && (curline[pos] != '\"'))
            //    {
            //        if ((curline[pos] == '\\') && (pos < curline.Length - 1) && (curline[pos + 1] == '\"'))
            //        {
            //            str = str + "\\\"";
            //            pos += 2;                   //skip over escaped single quotes
            //        }
            //        else
            //        {
            //            str = str + curline[pos];
            //            pos++;
            //        }
            //    }
            //    if ((pos < curline.Length))         //skip the closing quote if not at eoln
            //    {
            //        pos++;
            //    }
            //    Token token = new Token(TokenType.tSTRINGCONST, str, tokenLoc);
            //    setTokenFlags(token);
            return str;
        }

        public char TranslateChars()
        {
            char ch = getChar();


            //diagraphs
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
            if (ch == '%' && (getCharAt(1) == '>'))
            {
                ch = '#';
                gotoNextChar();
            }
            //we handle '%:%:' --> '##' when we handle '#'

            return ch;
        }

        //- main scanning method ----------------------------------------------

        //translation phase 3 : scan source line into preprocessing tokens
        public Fragment getFrag()
        {
            Fragment frag = null;

            char ch = getChar();
            while (true)
            {
                if (isSpace(ch))
                {
                    skipWhitespace();
                    frag = new Fragment(FragType.SPACE, " ");
                    break;
                }

                //line comment
                if (ch == '/' && (getCharAt(1) == '/'))
                {
                    skipLineComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //block comment
                if (ch == '/' && (getCharAt(1) == '*'))
                {
                    skipBlockComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //L is a special case since it can start long char constants or long string constants, as well as identifiers
                if (ch == 'L')
                {
                    if ((getCharAt(1)) == '\'')
                    {
                        string chstr = scanCharLiteral();
                        frag = new Fragment(FragType.CHAR, chstr);
                        break;
                    }
                    else if ((getCharAt(1)) == '"')
                    {
                        string sstr = scanString();
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
                    string chstr = scanCharLiteral();
                    frag = new Fragment(FragType.CHAR, chstr);
                    break;
                }

                //string constant
                if (ch == '"')
                {
                    string sstr = scanString();
                    frag = new Fragment(FragType.STRING, sstr);
                    break;
                }

                //end of file
                if (ch == '\0')
                {
                    frag = new Fragment(FragType.EOF, "");
                    break;
                }

                //translate chars before handling punctuation
                ch = TranslateChars();

                //proprocessing
                //        case '#':
                //            if (buffer.ch == '#')
                //            {
                //                buffer.gotoNextChar();
                //                ttype = TokenType.tDOUBLEHASH;     
                //                tokenStr = "##";
                //            }
                //            else
                //            {
                //                ttype = TokenType.tHASH;
                //                tokenStr = "#";
                //            }
                //            break;

                //anything else is punctuation
                frag = new Fragment(FragType.PUNCT, "" + ch);
                gotoNextChar();
                break;
            }

            return frag;
        }

        //private void setTokenFlags(Token token)
        //{
        //    token.leadingSpace = leadingWS;
        //    token.startsLine = atLineStart;
        //    atLineStart = false;                //if we've read a token, then we're not at the beginning of the line anymore
        //}        
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
        EOF
    }

    public class Fragment
    {
        public String str;
        public FragType type;

        public Fragment(FragType _type, String _str)
        {
            str = _str;
            type = _type;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");