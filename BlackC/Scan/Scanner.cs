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
    public class Scanner : PPTokenSource
    {
        public Parser parser;

        public string filename;
        string[] source;

        int linepos;
        int linenum;
        bool atEof;
        bool atEoln;
        bool inTrigraph;
        StringBuilder tokstr;

        public Scanner(Parser _parser, String _filename)
        {
            parser = _parser;
            filename = _filename;

            try
            {
                source = File.ReadAllLines(filename);        //read entire file as array of strings
            }
            catch (Exception e)
            {
                parser.fatal("error reading source file " + filename + " : " + e.Message);
            }

            linenum = 0;
            startLine();
            tokstr = new StringBuilder();
        }

        //- reading source chars ---------------------------------------------

        /*(5.1.1.2) 
            translation phase 1 : replace trigraphs with their eqivalent chars 
            translation phase 2 : splice lines ending with line continuation chars together
         */

        Dictionary<char, char> trigraphs = new Dictionary<char, char>() {
                        { '=', '#' },  { ')', ']' }, { '!', '|' },
                        { '(', '[' },  { '\'','^' }, { '>', '}' },
                        { '/', '\\' }, { '<', '{' }, { '-', '~' }};

        public void startLine()
        {
            if (linenum >= source.Length)
            {
                atEof = true;
                return;
            }

            //check for a trigraph (??/) at the end of the line which would become a continuation char
            if (source[linenum].EndsWith("??/"))
            {
                String s = source[linenum].Remove(source[linenum].Length - 3);
                s = s + "\\";
                source[linenum] = s;
            }
            linepos = 0;
            atEoln = source[linenum].Length == 0;
            inTrigraph = false;
        }

        public char getChar()
        {
            if (atEof)
            {
                return '\0';
            }

            if (atEoln)
            {
                return '\n';
            }

            if ((linepos < source[linenum].Length - 2) && (source[linenum][linepos] == '?') && (source[linenum][linepos + 1] == '?'))

            {
                char tc = source[linenum][linepos + 2];
                if (trigraphs.ContainsKey(tc))
                {
                    inTrigraph = true;
                    return trigraphs[tc];
                }
            }

            return source[linenum][linepos];
        }

        public void nextChar()
        {
            if (atEof)      //don't read past eof
            {
                return;
            }

            if (atEoln)
            {
                linenum++;
                startLine();
                return;
            }

            //still on the current line
            linepos += inTrigraph ? 3 : 1;
            inTrigraph = false;

            //handle line countinuations
            while ((linepos == source[linenum].Length - 1) && (source[linenum][linepos] == '\\'))
            {
                linenum++;
                startLine();                
            }

            //check for eoln on current line
            if (!atEof && (linepos >= source[linenum].Length))
            {
                atEoln = true;
            }
        }

        //- skipping whitespace & comments  -----------------------------------

        public bool isSpace(char ch)
        {
            return (ch == ' ' || ch == '\t' || ch == '\v' || ch == '\f' || ch == '\r');
        }

        public void skipWhitespace()
        {
            char ch = getChar();
            while (isSpace(ch))
            {
                nextChar();
                ch = getChar();
                continue;
            }
        }

        //skip remainder of current line up to (but not including) the eoln char
        //since nothing follows this but the eoln, don't include this spaces in the preprocessor's fragment
        public void skipLineComment()
        {
            nextChar();                     //skip 2nd / char
            char ch = getChar();
            while (ch != '\n' && !atEof)
            {
                nextChar();
                ch = getChar();
            }
        }

        //skip source characters until reach next '*/' or eof
        public void skipBlockComment()
        {
            bool done = false;
            do
            {
                nextChar();
                char ch = getChar();
                if (atEof)
                {
                    done = true;
                }
                else if (ch == '*')
                {
                    nextChar();
                    if (getChar() == '/')
                    {
                        nextChar();
                        done = true;
                    }
                }

            } while (!done);
        }

        //- source scanning ------------------------------------------------

        public bool isAlpha(char ch)
        {
            return ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || ch == '_');
        }

        public bool isDigit(char ch)
        {
            return (ch >= '0' && ch <= '9');
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
        public string scanIdentifier(char ch)
        {
            tokstr.Clear();
            tokstr.Append(ch);
            ch = getChar();
            while (isAlphaNum(ch))
            {
                tokstr.Append(ch);
                nextChar();
                ch = getChar();
            }
            return tokstr.ToString();
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

        //token string either ends with a '.' or a 'E' on entry
        public string scanFloatConst()
        {
            if (tokstr.ToString().EndsWith("."))      //get optional decimal part
            {
                char c1 = getChar();
                while (c1 >= '0' && c1 <= '9')
                {
                    tokstr.Append(c1);
                    nextChar();
                    c1 = getChar();
                }
                if (tokstr.ToString().EndsWith("."))      //if we didn't have a decimal part above, we add one anyway (123. --> 123.0)
                {
                    tokstr.Append('0');
                }
                if ((c1 == 'e') || (c1 == 'E'))     //then check for exponent part
                {
                    tokstr.Append('E');
                    nextChar();
                }
            }

            if (tokstr.ToString().EndsWith("E"))      //get optional exponent part
            {
                char s1 = getChar();
                if ((s1 == '+') || (s1 == '-'))     //exponent sign is optional
                {
                    tokstr.Append(s1);
                    nextChar();
                    s1 = getChar();
                }
                while (s1 >= '0' && s1 <= '9')
                {
                    tokstr.Append(s1);
                    nextChar();
                    s1 = getChar();
                }
            }

            //check for float const suffixes
            char f1 = getChar();
            if ((f1 == 'f') || (f1 == 'F') || (f1 == 'l') || (f1 == 'L'))
            {
                tokstr.Append(Char.ToUpper(f1));
                nextChar();
            }

            return tokstr.ToString();
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
        public string scanNumber(char ch, out bool isInt)     //either int or float const
        {
            isInt = true;       //assume int
            int bass = 10;      //default number base
            tokstr.Clear();
            tokstr.Append(ch);

            if (ch != '.')       //get mantissa
            {
                if (ch == '0')             //set base
                {
                    char ch2 = getChar();
                    if (ch2 == 'X' || ch2 == 'x')
                    {
                        bass = 16;
                        tokstr.Append(ch2);
                        nextChar();
                    }
                    else
                    {
                        bass = 8;
                    }
                }
                ch = getChar();
                while (((bass == 10) && (ch >= '0' && ch <= '9')) ||
                        ((bass == 8) && (ch >= '0' && ch <= '7')) ||
                        ((bass == 16) && ((ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f'))))
                {
                    tokstr.Append(ch);
                    nextChar();
                    ch = getChar();
                }
            }
            else
            {
                tokstr.Insert(0, '0');               //add the leading 0 to a float const str '.1234'
                isInt = false;                       //not an int
                return scanFloatConst();             //get floating point constant string
            }

            //got the mantissa, if the next char is decimal point or exponent, then it's a float const
            if ((ch == '.') || (ch == 'E') || (ch == 'e'))
            {
                nextChar();                         //skip '.' or 'E' or 'e'

                if (ch == 'E' || ch == 'e')
                {
                    tokstr.Append(".0");            //add decimal part if missing (123E10 --> 123.0E10)
                }
                tokstr.Append(Char.ToUpper(ch));
                isInt = false;                      //not an int
                return scanFloatConst();            //get floating point constant token
            }

            //not a float, check for int const suffixes, can be in any order
            bool usuffix = false;
            bool lsuffix = false;
            for (int i = 0; i < 2; i++)     //check for int const suffixes
            {
                if ((!usuffix) && ((ch == 'u') || (ch == 'U')))
                {
                    usuffix = true;
                    tokstr.Append("U");
                    nextChar();
                    ch = getChar();
                }

                if ((!lsuffix) && ((ch == 'l') || (ch == 'L')))
                {
                    lsuffix = true;
                    tokstr.Append("L");
                    nextChar();
                    ch = getChar();
                    if ((ch == 'l') || (ch == 'L'))     //check for LL or ll
                    {
                        tokstr.Append("L");
                        nextChar();
                        ch = getChar();
                    }
                }
            }

            return tokstr.ToString();
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
            tokstr.Append(isLong ? "L\'" : "\'");

            char ch = getChar();
            while ((ch != '\'') && (ch != '\n') && !atEof)
            {
                nextChar();
                if ((ch == '\\') && (getChar() == '\''))
                {
                    tokstr.Append("\\\'");
                    nextChar();                    //skip over escaped single quotes
                }
                else
                {
                    tokstr.Append(ch);
                }
                ch = getChar();
            }

            if (ch == '\'')         //add the closing quote if not at eoln or eof
            {
                tokstr.Append('\'');
                nextChar();
            }
            return tokstr.ToString();
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
            tokstr.Append(isLong ? "L\"" : "\"");

            char ch = getChar();
            while ((ch != '\"') && (ch != '\n') && (ch != '\0'))
            {
                nextChar();
                if ((ch == '\\') && (getChar() == '\"'))
                {
                    tokstr.Append("\\\"");
                    nextChar();                    //skip over escaped double quotes
                }
                else
                {
                    tokstr.Append(ch);
                }
                ch = getChar();
            }
            if (ch == '\"')                     //skip the closing quote if not at eoln or eof
            {
                tokstr.Append('\"');
                nextChar();
            }
            return tokstr.ToString();
        }

        //- main scanning method ----------------------------------------------

        public char TranslateDiagraphs(char ch1)
        {
            char ch2 = getChar();
            if (ch1 == '<' && ch2 == ':')
            {
                ch1 = '[';
                nextChar();
            }
            if (ch1 == ':' && ch2 == '>')
            {
                ch1 = ']';
                nextChar();
            }
            if (ch1 == '<' && ch2 == '%')
            {
                ch1 = '{';
                nextChar();
            }
            if (ch1 == '%' && ch2 == '>')
            {
                ch1 = '}';
                nextChar();
            }
            if (ch1 == '%' && ch2 == ':')
            {
                ch1 = '#';
                nextChar();
            }
            //we handle '%:%:' --> '##' when we handle '#' the 2nd time

            return ch1;
        }

        //(5.1.1.2) translation phase 3 : scan source line into preprocessing tokens
        override public PPToken getPPToken()
        {
            PPToken tok = null;
            int tokpos = linepos;
            int tokline = linenum;
            tokstr.Clear();

            char ch = getChar();
            nextChar();
            while (true)
            {
                //end of file - check if this isn't a stray 0x0 char in file, if so pass it on as punctuation
                if (ch == '\0' && atEof)
                {
                    tok = new PPToken(PPTokenType.EOF, "<eof>");
                    break;
                }

                //end of line - does not include eolns in block comments or spliced lines
                if (ch == '\n')
                {
                    tok = new PPToken(PPTokenType.EOLN, "<eoln>");
                    break;
                }

                if (isSpace(ch))
                {
                    skipWhitespace();
                    tok = new PPToken(PPTokenType.SPACE, " ");
                    break;
                }

                //line comment
                if (ch == '/' && (getChar() == '/'))
                {
                    skipLineComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //block comment
                if (ch == '/' && (getChar() == '*'))
                {
                    skipBlockComment();
                    ch = ' ';                   //replace comment with single space
                    continue;
                }

                //L is a special case since it can start long char constants or long string constants, as well as identifiers
                if (ch == 'L')
                {
                    if (getChar() == '\'')
                    {
                        string chstr = scanCharLiteral(true);
                        tok = new PPToken(PPTokenType.CHAR, chstr);
                        break;
                    }
                    else if (getChar() == '"')
                    {
                        string sstr = scanString(true);
                        tok = new PPToken(PPTokenType.STRING, sstr);
                        break;
                    }
                }

                //if L doesn't start a string or char constant, it falls through to here
                //identifier
                if (isAlpha(ch))
                {
                    string idstr = scanIdentifier(ch);
                    tok = new PPToken(PPTokenType.WORD, idstr);
                    break;
                }

                //numeric constant
                //'.' can start a float const
                if (isDigit(ch) || (ch == '.' && isDigit(getChar())))
                {
                    bool isInt;
                    string numstr = scanNumber(ch, out isInt);
                    tok = new PPToken(isInt ? PPTokenType.INTEGER : PPTokenType.FLOAT, numstr);
                    break;
                }

                //char constant
                if (ch == '\'')
                {
                    string chstr = scanCharLiteral(false);
                    tok = new PPToken(PPTokenType.CHAR, chstr);
                    break;
                }

                //string constant
                if (ch == '"')
                {
                    string sstr = scanString(false);
                    tok = new PPToken(PPTokenType.STRING, sstr);
                    break;
                }

                //translate chars before handling punctuation
                ch = TranslateDiagraphs(ch);

                //anything else is punctuation
                tok = new PPToken(PPTokenType.PUNCT, "" + ch);
                break;
            }

            tok.pos = tokpos;
            tok.line = tokline;
            return tok;
        }
    }

    //- pptoken class ----------------------------------------------

    public enum PPTokenType
    {
        WORD,
        INTEGER,
        FLOAT,
        STRING,
        CHAR,
        PUNCT,
        SPACE,
        COMMENT,
        EOLN,
        EOF
    }

    public class PPToken
    {
        public PPTokenType type;
        public String str;
        public int pos;
        public int line;

        public PPToken(PPTokenType _type, String _str)
        {
            type = _type;
            str = _str;
            pos = 0;
            line = 0;
        }

        String[] typeNames = { "WORD", "INT", "REAL", "STRING", "CHAR", "PUNCT", "SPACE", "COMMENT", "EOLN", "EOF" };

        public override string ToString()
        {
            return typeNames[(int)type] + "(" + str + ")";
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");