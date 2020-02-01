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
        SourceBuffer buffer;
        string[] lines;
        int linenum;
        SourceLocation tokenLoc;
        string curline;
        bool atLineStart;            //token was at start of line
        bool leadingWS;                 //token was preceeded by whitespace (including comments)

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

        //- skipping whitespace & comments  -----------------------------------

        public bool isSpace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\v' || ch == '\f' || ch == '\r' || ch == '\n');
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
                    ch = source[++srcpos];
                    continue;
                }

                //skip any eolns - any new line handling that needs to be done goes here
                if ((ch == '\n'))
                {
                    ch = source[++srcpos];
                    continue;
                }

                //skip any following comments, if we found a comment, then we're not done yet
                if ((ch == '/') && (source[srcpos + 1] == '/'))
                {
                    skipLineComment();
                    continue;
                }

                if ((ch == '/') && (source[srcpos + 1] == '*'))
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
            //    while ((buffer.ch == '\n') ||
            //            (buffer.ch == '\r') && (buffer.peekNextChar() == '\n'))
            //    {
            //        buffer.gotoNextChar();
            //    }
            //    if (buffer.ch == '\r')
            //        buffer.gotoNextChar();
            //    buffer.onNewLine();
            //    buffer.gotoNextChar();
            //    atLineStart = true;
        }

        //skip source characters until reach next '*/'
        public void skipBlockComment()
        {
            //    while ((buffer.ch == '*') && (buffer.peekNextChar() == '/'))
            //    {
            //        if ((buffer.ch == '\n') ||
            //            (buffer.ch == '\r') && (buffer.peekNextChar() == '\n'))
            //        {
            //            if (buffer.ch == '\r')
            //                buffer.gotoNextChar();
            //            buffer.onNewLine();
            //            atLineStart = true;
            //        }

            //        buffer.gotoNextChar();
            //    }
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
        public string scanFloatConst()
        {
            string fstr = "";
            //    bool atend;
            //    if (num.EndsWith("."))      //get optional decimal part
            //    {
            //        atend = !(pos < curline.Length);
            //        while (!atend)
            //        {
            //            char c1 = curline[pos++];
            //            if (c1 >= '0' && c1 <= '9')
            //            {
            //                num = num + c1;
            //                atend = !(pos < curline.Length);
            //            }
            //            else
            //            {
            //                pos--;
            //                atend = true;
            //            }
            //        }
            //        if (num.EndsWith("."))      //if we didn't have a decimal part above, we add one anyway
            //        {
            //            num = num + '0';
            //        }
            //        if ((pos < curline.Length) && ((curline[pos] == 'e') || (curline[pos] == 'E')))     //then check for exponent part
            //        {
            //            num = num + 'E';
            //            pos++;
            //        }
            //    }
            //    if (num.EndsWith("E"))      //get optional decimal part
            //    {
            //        if ((pos < curline.Length) && ((curline[pos] == '+') || (curline[pos] == '-')))
            //        {
            //            char s1 = curline[pos++];
            //            num = num + s1;
            //        }
            //        atend = !(pos < curline.Length);
            //        while (!atend)
            //        {
            //            char c1 = curline[pos++];
            //            if (c1 >= '0' && c1 <= '9')
            //            {
            //                num = num + c1;
            //                atend = !(pos < curline.Length);
            //            }
            //            else
            //            {
            //                pos--;
            //                atend = true;
            //            }
            //        }
            //    }
            //    bool fsuffix = false;
            //    if ((pos < curline.Length) && ((curline[pos] == 'f') || (curline[pos] == 'F')))
            //    {
            //        fsuffix = true;
            //        num = num + "F";
            //        pos++;
            //    }
            //    if ((!fsuffix) && (pos < curline.Length) && ((curline[pos] == 'l') || (curline[pos] == 'L')))
            //    {
            //        num = num + "L";
            //        pos++;
            //    }

            //    Token token = new Token(TokenType.tFLOATCONST, num, tokenLoc);
            //    setTokenFlags(token);
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
            //        num = "0.";                         //add the leading 0 to a float const str '.1234'
            //        return scanFloatConst(num);         //get floating point constant token
                }

            //    //got the mantissa, if the next char is decimal point or exponent, then it's a float const
            //    if ((buffer.ch == '.') || (buffer.ch == 'E') || (buffer.ch == 'e'))
            //    {
            //        char c2 = buffer.gotoNextChar();           //get '.' or 'E' or 'e'
            //        num = num + Char.ToUpper(c2);
            //        return scanFloatConst(num);                 //get floating point constant token
            //    }

            //    //not a float, check for int const suffixes
            //    bool usuffix = false;
            //    int lsuffix = 0;
            //    string intstr = num;
            //    for (int i = 0; i < 2; i++)     //check for int const suffixes
            //    {
            //        if ((!usuffix) && ((buffer.ch == 'u') || (buffer.ch == 'U')))
            //        {
            //            usuffix = true;
            //            num = num + "U";
            //            buffer.gotoNextChar();
            //        }

            //        if ((lsuffix == 0) && ((buffer.ch == 'l') || (buffer.ch == 'L')))
            //        {
            //            lsuffix++;
            //            num = num + "L";
            //            buffer.gotoNextChar();
            //            if ((buffer.ch == 'l') || (buffer.ch == 'L'))
            //            {
            //                lsuffix++;
            //                num = num + "L";
            //                buffer.gotoNextChar();
            //            }
            //        }
            //    }

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

        Dictionary<char, char> trigraphs = new Dictionary<char, char>() { 
                        { '=', '#' },  { ')', ']' }, { '!', '|' }, 
                        { '(', '[' },  { '\'','^' }, { '>', '}' },
                        { '/', '\\' }, { '<', '{' }, { '-', '~' }};

        public char TranslateChars()
        {
            char ch = source[srcpos];

            //trigraphs
            if (ch == '?' && (source[srcpos + 1] == '?'))
            {
                char ch2 = source[srcpos + 2];
                if (trigraphs.ContainsKey(ch2))
                {
                    ch = trigraphs[ch2];
                    srcpos += 2;
                }
            }

            //diagraphs
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
            if (ch == '%' && (source[srcpos + 1] == '>'))
            {
                ch = '#';
                srcpos++;
            }
            //we handle '%:%:' --> '##' when we handle '#'

            return ch;
        }

        //- main scanning method ----------------------------------------------

        //translation phase 3 : scan source line into preprocessing tokens
        public Fragment getFrag()
        {
            Fragment frag = null;

            char ch = source[srcpos];
            while (true)
            {
                if (isSpace(ch))
                {
                    skipWhitespace();
                    frag = new Fragment(FragType.SPACE, " ");
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
                    if ((source[srcpos + 1]) == '\'')
                    {
                        string chstr = scanCharLiteral();
                        frag = new Fragment(FragType.CHAR, chstr);
                        break;
                    }
                    else if ((source[srcpos + 1]) == '"')
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
                if ((isDigit(ch)) || (ch == '.' && isDigit(source[srcpos + 1])))
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
                srcpos++;
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