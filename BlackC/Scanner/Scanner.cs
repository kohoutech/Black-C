/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2019  George E Greaney

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

namespace BlackC
{
    public class Scanner
    {
        SourceBuffer buffer;
        string[] lines;
        int linenum;
        int pos;
        SourceLocation tokenLoc;

        string curline;
        bool atEOF;
        bool atBOL;                 //token was at beginning of line
        bool sawWS;                 //token was preceeded by whitespace (including comments)

        public Scanner()
        {
            tokenLoc = null;
        }

        //- source file mgmt --------------------------------------------------

        public void saveSource()
        {
            //buffer.curline = curline;
            //buffer.linenum = linenum;
            //buffer.linepos = pos;
            //buffer.atBOL = atBOL;
            //buffer.eolnCount = eolnCount;
        }

        public void setSource(SourceBuffer srcbuf)
        {
            buffer = srcbuf;
            //lines = srcbuf.lines;
            //linenum = srcbuf.linenum;
            //pos = srcbuf.linepos;
            //atBOL = srcbuf.atBOL;            

            //getCurrentLine();
            //atEOF = false;
        }

        //- skipping whitespace & comments  -----------------------------------

        public void getCurrentLine()
        {
            String newline = lines[linenum].Trim();

            //concatenate continued lines
            while ((linenum < lines.Length - 1) && (newline.EndsWith("\\")))
            {
                newline = newline.Remove(newline.Length - 1, 1);
                newline += lines[++linenum].Trim();
            }

            //translate any trigraphs
            newline = newline.Replace("??=", "#");
            newline = newline.Replace("??(", "[");
            newline = newline.Replace("??/", "\\");
            newline = newline.Replace("??)", "]");
            newline = newline.Replace("??'", "^");
            newline = newline.Replace("??<", "{");
            newline = newline.Replace("??!", "|");
            newline = newline.Replace("??>", "}");
            newline = newline.Replace("??-", "~");

            curline = newline;
        }

        //public void gotoNextLine()
        //{
        //    do
        //    {
        //        linenum++;
        //        eolnCount++;
        //        atEOF = (linenum == lines.Length);
        //        if (!atEOF)
        //        {
        //            getCurrentLine();
        //        }
        //    }
        //    while ((!atEOF) && (curline.Length == 0));      //skip empty lines

        //    pos = 0;                    //at start of line
        //    atBOL = true;
        //    sawWS = false;              //any whitespace has been removed from line start
        //}

        //skip remainder of current line & eoln chars
        public void skipLineComment()
        {
            while ((buffer.ch == '\n') ||
                    (buffer.ch == '\r') && (buffer.peekNextChar() == '\n'))
            {
                buffer.gotoNextChar();
            }
            if (buffer.ch == '\r')
                buffer.gotoNextChar();
            buffer.onNewLine();
            buffer.gotoNextChar();
        }

        //skip source characters until reach next '*/'
        public void skipBlockComment()
        {
            while ((buffer.ch == '*') && (buffer.peekNextChar() == '/'))
            {
                if ((buffer.ch == '\n') ||
                    (buffer.ch == '\r') && (buffer.peekNextChar() == '\n'))
                {
                    if (buffer.ch == '\r')
                        buffer.gotoNextChar();
                    buffer.onNewLine();
                }

                buffer.gotoNextChar();
            }
        }

        public Token skipWhitespace()
        {
            sawWS = false;
            bool done = true;
            do
            {
                done = true;

                //first skip any whitespace
                if ((buffer.ch == ' ') || (buffer.ch == '\t') || (buffer.ch == '\f') || (buffer.ch == '\v'))
                {
                    buffer.gotoNextChar();
                    sawWS = true;
                    done = false;
                }

                //next skip any eolns (/n or /r/n)
                if ((buffer.ch == '\n') ||
                    (buffer.ch == '\r') && (buffer.peekNextChar() == '\n'))
                {
                    if (buffer.ch == '\r')
                        buffer.gotoNextChar();
                    buffer.onNewLine();
                    buffer.gotoNextChar();
                    sawWS = true;
                    done = false;
                }

                //then skip any following comments, if we found a comment, then we're not done yet
                if ((buffer.ch == '/') && (buffer.peekNextChar() == '/'))
                {
                    skipLineComment();
                    done = false;
                }

                if ((buffer.ch == '/') && (buffer.peekNextChar() == '*'))
                {
                    skipBlockComment();
                    done = false;
                }
            } while (!done);

            return null;
        }

        private Token scanEndOfFile()
        {
            return  new Token(TokenType.tEOF, "<eof>", tokenLoc);            
        }

        //- token scanning ------------------------------------------------

        //(6.4.1) is identifier a keyword?
        public TokenType findKeyword(String id)
        {
            switch (id)
            {
                case "auto":
                    return TokenType.tAUTO;

                case "break":
                    return TokenType.tBREAK;

                case "case":
                    return TokenType.tCASE;

                case "char":
                    return TokenType.tCHAR;

                case "const":
                    return TokenType.tCONST;

                case "continue":
                    return TokenType.tCONTINUE;

                case "default":
                    return TokenType.tDEFAULT;

                case "do":
                    return TokenType.tDO;

                case "double":
                    return TokenType.tDOUBLE;

                case "else":
                    return TokenType.tELSE;

                case "enum":
                    return TokenType.tENUM;

                case "extern":
                    return TokenType.tEXTERN;

                case "float":
                    return TokenType.tFLOAT;

                case "for":
                    return TokenType.tFOR;

                case "goto":
                    return TokenType.tGOTO;

                case "if":
                    return TokenType.tIF;

                case "inline":
                    return TokenType.tINLINE;

                case "int":
                    return TokenType.tINT;

                case "long":
                    return TokenType.tLONG;

                case "register":
                    return TokenType.tREGISTER;

                case "restrict":
                    return TokenType.tRESTRICT;

                case "return":
                    return TokenType.tRETURN;

                case "short":
                    return TokenType.tSHORT;

                case "signed":
                    return TokenType.tSIGNED;

                case "sizeof":
                    return TokenType.tSIZEOF;

                case "static":
                    return TokenType.tSTATIC;

                case "struct":
                    return TokenType.tSTRUCT;

                case "switch":
                    return TokenType.tSWITCH;

                case "typedef":
                    return TokenType.tTYPEDEF;

                case "union":
                    return TokenType.tUNION;

                case "unsigned":
                    return TokenType.tUNSIGNED;

                case "void":
                    return TokenType.tVOID;

                case "volatile":
                    return TokenType.tVOLATILE;

                case "while":
                    return TokenType.tWHILE;
            }
            return TokenType.tIDENTIFIER;
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
        public Token scanIdentifier(char c)
        {
            String id = "" + c;

            //get ident chars
            char c1 = buffer.ch;
            while ((c1 >= 'A' && c1 <= 'Z') || (c1 >= 'a' && c1 <= 'z') || (c1 >= '0' && c1 <= '9') || (c1 == '_') || (c1 == '$'))
            {
                id = id + c1;
                buffer.gotoNextChar();
                c1 = buffer.ch;
            }

            TokenType ttype = findKeyword(id);                      //check if ident is a keyword
            return new Token(ttype, id, tokenLoc);
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
        public Token scanFloatConst(String num)
        {
            Token token;
            bool atend;
            if (num.EndsWith("."))      //get optional decimal part
            {
                atend = !(pos < curline.Length);
                while (!atend)
                {
                    char c1 = curline[pos++];
                    if (c1 >= '0' && c1 <= '9')
                    {
                        num = num + c1;
                        atend = !(pos < curline.Length);
                    }
                    else
                    {
                        pos--;
                        atend = true;
                    }
                }
                if (num.EndsWith("."))      //if we didn't have a decimal part above, we add one anyway
                {
                    num = num + '0';
                }
                if ((pos < curline.Length) && ((curline[pos] == 'e') || (curline[pos] == 'E')))     //then check for exponent part
                {
                    num = num + 'E';
                    pos++;
                }
            }
            if (num.EndsWith("E"))      //get optional decimal part
            {
                if ((pos < curline.Length) && ((curline[pos] == '+') || (curline[pos] == '-')))
                {
                    char s1 = curline[pos++];
                    num = num + s1;
                }
                atend = !(pos < curline.Length);
                while (!atend)
                {
                    char c1 = curline[pos++];
                    if (c1 >= '0' && c1 <= '9')
                    {
                        num = num + c1;
                        atend = !(pos < curline.Length);
                    }
                    else
                    {
                        pos--;
                        atend = true;
                    }
                }
            }
            bool fsuffix = false;
            if ((pos < curline.Length) && ((curline[pos] == 'f') || (curline[pos] == 'F')))
            {
                fsuffix = true;
                num = num + "F";
                pos++;
            }
            if ((!fsuffix) && (pos < curline.Length) && ((curline[pos] == 'l') || (curline[pos] == 'L')))
            {
                num = num + "L";
                pos++;
            }

            return new Token(TokenType.tFLOATCONST, num, tokenLoc);
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
        public Token scanNumber(char c)
        {
            String num = "";
            int bass = 10;                  //number base
            bool floatpt = false;           //haven't seen '.' yet


            if (!floatpt)       //get mantissa
            {
                num += c;
                if (c == '0')             //set base
                {
                    if ((buffer.ch == 'X' || buffer.ch == 'x'))
                    {
                        bass = 16;
                        buffer.gotoNextChar();
                    }
                    else
                    {
                        bass = 8;
                    }
                }
                char c1 = buffer.ch;
                while (((bass == 10) && (c1 >= '0' && c1 <= '9')) ||
                        ((bass == 8) && (c1 >= '0' && c1 <= '7')) ||
                        ((bass == 16) && ((c1 >= '0' && c1 <= '9') || (c1 >= 'A' && c1 <= 'F') || (c1 >= 'a' && c1 <= 'f'))))
                {
                    num = num + c1;
                    buffer.gotoNextChar();
                    c1 = buffer.ch;
                }

            }

            //got the mantissa, if the next char is decimal point or exponent, then it's a float const
            if ((buffer.ch == '.') || (buffer.ch == 'E') || (buffer.ch == 'e'))
            {
                char c2 = buffer.gotoNextChar();           //get '.' or 'E' or 'e'
                num = num + Char.ToUpper(c2);
                return scanFloatConst(num);                 //get floating point constant token
            }
            else
            {
                bool usuffix = false;
                bool lsuffix = false;
                for (int i = 0; i < 2; i++)     //check for int const suffixes
                {
                    if ((!usuffix) && ((buffer.ch == 'u') || (buffer.ch == 'U')))
                    {
                        usuffix = true;
                        num = num + "U";
                        buffer.gotoNextChar();
                    }

                    if ((!lsuffix) && ((buffer.ch == 'l') || (buffer.ch == 'L')))
                    {
                        lsuffix = true;
                        num = num + "L";
                        buffer.gotoNextChar();
                        if ((buffer.ch == 'l') || (buffer.ch == 'L'))
                        {
                            num = num + "L";
                            buffer.gotoNextChar();
                        }
                    }
                }
                
            }
            return new Token(TokenType.tINTCONST, num, tokenLoc);
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
        private Token scanCharLiteral(char c)
        {
            Token token;
            String cstr = (c == 'L') ? "L\'" : "";

            while ((pos < curline.Length) && (curline[pos] != '\''))
            {
                if ((curline[pos] == '\\') && (pos < curline.Length - 1) && (curline[pos + 1] == '\''))
                {
                    cstr = cstr + "\\\'";
                    pos += 2;                   //skip over escaped single quotes
                }
                else
                {
                    cstr = cstr + curline[pos];
                    pos++;
                }
            }
            if ((pos < curline.Length))         //skip the closing quote if not at eoln
            {
                pos++;
            }
            return new Token(TokenType.tCHARCONST, cstr, tokenLoc);
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
        public Token scanString(char c)
        {
            Token token;
            String str = (c == 'L') ? "L\"" : "";

            while ((pos < curline.Length) && (curline[pos] != '\"'))
            {
                if ((curline[pos] == '\\') && (pos < curline.Length - 1) && (curline[pos + 1] == '\"'))
                {
                    str = str + "\\\"";
                    pos += 2;                   //skip over escaped single quotes
                }
                else
                {
                    str = str + curline[pos];
                    pos++;
                }
            }
            if ((pos < curline.Length))         //skip the closing quote if not at eoln
            {
                pos++;
            }
            return new Token(TokenType.tSTRINGCONST, str, tokenLoc);
        }

        //- main scanning method ----------------------------------------------

        //translation phase 3 : scan source line into preprocessing tokens
        public Token scanToken()
        {
            TokenType ttype = TokenType.tUNKNOWN;
            String tokenStr = "";

            //goto start of next token in source file
            Token wstoken = skipWhitespace();
            if (wstoken != null) 
                return wstoken;

            //scan next token
            tokenLoc = buffer.getCurPos();
            char c = buffer.gotoNextChar();
            switch (c)
            {
                //eof
                case '\0':
                    if (buffer.atEnd())
                        return scanEndOfFile();
                    break;

                //identifier
                case '_':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    return scanIdentifier(c);

                //L is a special case since it can start long char constants or long string constants, as well as identifiers
                case 'L':
                    if ((pos < curline.Length) && (curline[pos] == '\''))
                    {
                        return scanCharLiteral(c);
                    }
                    else if ((pos < curline.Length) && (curline[pos] == '\"'))
                    {
                        return scanString(c);
                    }
                    else
                    {
                        return scanIdentifier(c);
                    }

                //numeric constant
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return scanNumber(c);

                //char constant
                case '\'':
                    return scanCharLiteral(c);

                //string constant
                case '"':
                    return scanString(c);

                //punctuation
                case '[':
                    ttype = TokenType.tLBRACKET;
                    tokenStr = "[";
                    break;

                case ']':
                    ttype = TokenType.tRBRACKET;
                    tokenStr = "]";
                    break;

                case '(':
                    ttype = TokenType.tLPAREN;
                    tokenStr = "(";
                    break;

                case ')':
                    ttype = TokenType.tRPAREN;
                    tokenStr = ")";
                    break;

                case '{':
                    ttype = TokenType.tLBRACE;
                    tokenStr = "{";
                    break;

                case '}':
                    ttype = TokenType.tRBRACE;
                    tokenStr = "}";
                    break;

                //'.' can start float const or '...' and '.' tokens
                case '.':
                    if (buffer.ch >= '0' && buffer.ch < '9')
                    {
                        return scanNumber(c);
                    }
                    else if (buffer.ch == '.')
                    {
                        if (buffer.peekNextChar() == '.')
                        {
                            buffer.gotoNextChar();
                            buffer.gotoNextChar();                  //consume next TWO chars
                            ttype = TokenType.tELLIPSIS;       
                            tokenStr = "...";
                        }
                    }
                    else
                    {
                        ttype = TokenType.tPERIOD;
                        tokenStr = ".";                    
                    }
                    break;

                case '+':
                    if (buffer.ch == '+')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tPLUSPLUS;       
                        tokenStr = "++";
                    }
                    else if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tPLUSEQUAL;      
                        tokenStr = "+=";
                    }
                    else
                    {
                        ttype = TokenType.tPLUS;
                        tokenStr = "+";
                    }
                    break;

                case '-':
                    if (buffer.ch == '-')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tMINUSMINUS;     
                        tokenStr = "--";
                    }
                    else if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tMINNUSEQUAL;    
                        tokenStr = "-=";
                    }
                    else if (buffer.ch == '>')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tARROW;         
                        tokenStr = "->";
                    }
                    else
                    {
                        ttype = TokenType.tMINUS;
                        tokenStr = "-";
                    }
                    break;

                case '&':
                    if (buffer.ch == '&')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tDOUBLEAMP;      
                        tokenStr = "&&";
                    }
                    else if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tAMPEQUAL;       
                        tokenStr = "&=";
                    }
                    else
                    {
                        ttype = TokenType.tAMPERSAND;
                        tokenStr = "&";
                    }
                    break;

                case '*':
                    if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tMULTEQUAL;      
                        tokenStr = "*=";
                    }
                    else
                    {
                        ttype = TokenType.tSTAR;
                        tokenStr = "*";
                    }
                    break;

                case '~':
                    ttype = TokenType.tTILDE;
                    tokenStr = "~";
                    break;

                case '!':
                    if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tNOTEQUAL;       
                        tokenStr = "!=";
                    }
                    else
                    {
                        ttype = TokenType.tEXCLAIM;
                        tokenStr = "!";
                    }
                    break;

                case '/':
                    if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tSLASHEQUAL;    
                        tokenStr = "/=";
                    }
                    else
                    {
                        ttype = TokenType.tSLASH;
                        tokenStr = "/";
                    }
                    break;

                case '%':
                    if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tPERCENTEQUAL;       
                        tokenStr = "%=";
                    }
                    else if (buffer.ch == '>')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tRBRACE;             //diagraph %> == }
                        tokenStr = "}";
                    }
                    else if (buffer.ch == ':')
                    {
                        buffer.gotoNextChar();
                        if ((buffer.ch == '%') && (buffer.peekNextChar() == ':'))
                        {
                            buffer.gotoNextChar();
                            buffer.gotoNextChar();                  //consume next two chars
                            ttype = TokenType.tDOUBLEHASH;     //diagraph %:%: == ##
                            tokenStr = "##";
                        }
                        else
                        {
                            ttype = TokenType.tHASH;         //diagraph %: == #
                            tokenStr = "#";
                        }
                    }
                    else
                    {
                        ttype = TokenType.tPERCENT;
                        tokenStr = "%";
                    }
                    break;

                case '<':
                    if (buffer.ch == '<')
                    {
                        buffer.gotoNextChar();
                        if (buffer.ch == '=')
                        {
                            buffer.gotoNextChar();
                            ttype = TokenType.tLSHIFTEQUAL;
                            tokenStr = "<<=";
                        }
                        else
                        {
                            ttype = TokenType.tLEFTSHIFT;      
                            tokenStr = "<<";
                        }
                    }
                    else if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tLESSEQUAL;      
                        tokenStr = "<=";
                    }
                    else if (buffer.ch == ':')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tLBRACKET;      //diagraph <: == [ 
                        tokenStr = "[";
                    }
                    else if (buffer.ch == '%')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tLBRACE;         //diagraph <& == {
                        tokenStr = "{";
                    }
                    else
                    {
                        ttype = TokenType.tLESSTHAN;
                        tokenStr = "<";
                    }
                    break;

                case '>':
                    if (buffer.ch == '>')
                    {
                        buffer.gotoNextChar();
                        if (buffer.ch == '=')
                        {
                            buffer.gotoNextChar();
                            ttype = TokenType.tRSHIFTEQUAL;       
                            tokenStr = ">>=";
                        }
                        else
                        {
                            ttype = TokenType.tRIGHTSHIFT;     
                            tokenStr = ">>";
                        }
                    }
                    else if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tGTREQUAL;       
                        tokenStr = ">=";
                    }
                    else
                    {
                        ttype = TokenType.tGTRTHAN;
                        tokenStr = ">";
                    }
                    break;

                case '=':
                    if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tEQUALEQUAL;     
                        tokenStr = "==";
                    }
                    else
                    {
                        ttype = TokenType.tEQUAL;
                        tokenStr = "=";
                    }
                    break;

                case '^':
                    if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tCARETEQUAL;     
                        tokenStr = "^=";
                    }
                    else
                    {
                        ttype = TokenType.tCARET;
                        tokenStr = "^";
                    }
                    break;

                case '|':
                    if (buffer.ch == '|')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tDOUBLEBAR;      
                        tokenStr = "||";
                    }
                    else if (buffer.ch == '=')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tBAREQUAL;       
                        tokenStr = "|=";
                    }
                    else
                    {
                        ttype = TokenType.tBAR;
                        tokenStr = "|";
                    }
                    break;

                case '?':
                    ttype = TokenType.tQUESTION;
                    tokenStr = "?";
                    break;

                case ':':
                    if (buffer.ch == '>')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tRBRACKET;      //diagraph :> == ]
                        tokenStr = "]";
                    }
                    else
                    {
                        ttype = TokenType.tCOLON;
                        tokenStr = ":";
                    }
                    break;

                case ';':
                    ttype = TokenType.tSEMICOLON;
                    tokenStr = ";";
                    break;

                case ',':
                    ttype = TokenType.tCOMMA;
                    tokenStr = ",";
                    break;

                //proprocessing
                case '#':
                    if (buffer.ch == '#')
                    {
                        buffer.gotoNextChar();
                        ttype = TokenType.tDOUBLEHASH;     
                        tokenStr = "##";
                    }
                    else
                    {
                        ttype = TokenType.tHASH;
                        tokenStr = "#";
                    }
                    break;

                //any other char we don't recognize
                default:
                    ttype = TokenType.tOTHER;
                    tokenStr = "" + c;
                    break;

            }

            Token token = new Token(ttype, tokenStr, tokenLoc);

            token.LeadingSpace = sawWS;
            token.atBOL = atBOL;
            atBOL = false;          //if we've read a token, then we're not at the beginning of the line anymore

            return token;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");