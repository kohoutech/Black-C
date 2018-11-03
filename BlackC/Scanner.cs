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
    class Scanner
    {
        string[] lines;
        string curline;
        int linenum;
        int pos;
        bool atEOF;

        List<Token> replay;
        int recpos;

        public Scanner(string[] _lines)
        {
            lines = _lines;
            linenum = 0;
            curline = lines[linenum];
            pos = 0;
            atEOF = false;
            replay = new List<Token>();
            recpos = 0;
        }

        //- token lookahead ---------------------------------------------------

        public int record()
        {
            return recpos;
        }

        public void rewind(int cuepoint)
        {
            recpos = cuepoint;
        }

        public void reset()
        {
            recpos = 0;
        }

        //---------------------------------------------------------------------

        public void gotoNextLine()
        {
            linenum++;
            pos = 0;

            atEOF = (linenum == lines.Length);
            if (!atEOF)
            {
                curline = lines[linenum];
            }
        }

        public void skipWhitespace()
        {
            while (!atEOF && ((pos >= curline.Length) || curline[pos] == ' ' || curline[pos] == '\t'))
            {
                if (pos >= curline.Length)      //if at eoln
                {
                    gotoNextLine();
                }
                else
                {
                    pos++;
                }
            }
        }

        public Token findKeyword(String id)
        {
            Token token = null;
            switch (id)
            {
                case "auto":
                    token = new Token(TokenType.tAUTO);
                    break;

                case "break":
                    token = new Token(TokenType.tBREAK);
                    break;

                case "case":
                    token = new Token(TokenType.tCASE);
                    break;

                case "char":
                    token = new Token(TokenType.tCHAR);
                    break;

                case "const":
                    token = new Token(TokenType.tCONST);
                    break;

                case "continue":
                    token = new Token(TokenType.tCONTINUE);
                    break;

                case "default":
                    token = new Token(TokenType.tDEFAULT);
                    break;

                case "do":
                    token = new Token(TokenType.tDO);
                    break;

                case "double":
                    token = new Token(TokenType.tDOUBLE);
                    break;

                case "else":
                    token = new Token(TokenType.tELSE);
                    break;

                case "enum":
                    token = new Token(TokenType.tENUM);
                    break;

                case "extern":
                    token = new Token(TokenType.tEXTERN);
                    break;

                case "float":
                    token = new Token(TokenType.tFLOAT);
                    break;

                case "for": 
                    token = new Token(TokenType.tFOR);
                    break;

                case "goto":
                    token = new Token(TokenType.tGOTO);
                    break;

                case "if":
                    token = new Token(TokenType.tIF);
                    break;

                case "inline":
                    token = new Token(TokenType.tINLINE);
                    break;

                case "int":
                    token = new Token(TokenType.tINT);
                    break;

                case "long":
                    token = new Token(TokenType.tLONG);
                    break;

                case "register":
                    token = new Token(TokenType.tREGISTER);
                    break;

                case "restrict":
                    token = new Token(TokenType.tRESTRICT);
                    break;

                case "return":
                    token = new Token(TokenType.tRETURN);
                    break;

                case "short":
                    token = new Token(TokenType.tSHORT);
                    break;

                case "signed":
                    token = new Token(TokenType.tSIGNED);
                    break;

                case "sizeof":
                    token = new Token(TokenType.tSIZEOF);
                    break;

                case "static":
                    token = new Token(TokenType.tSTATIC);
                    break;

                case "struct":
                    token = new Token(TokenType.tSTRUCT);
                    break;

                case "switch":
                    token = new Token(TokenType.tSWITCH);
                    break;

                case "typedef":
                    token = new Token(TokenType.tTYPEDEF);
                    break;

                case "union":
                    token = new Token(TokenType.tUNION);
                    break;

                case "unsigned":
                    token = new Token(TokenType.tUNSIGNED);
                    break;

                case "void":
                    token = new Token(TokenType.tVOID);
                    break;

                case "volatile":
                    token = new Token(TokenType.tVOLATILE);
                    break;

                case "while":
                    token = new Token(TokenType.tWHILE);
                    break;
            }
            return token;
        }

        public Token scanIdentifier(char c)
        {
            String id = "" + c;
            bool atend = !(pos < curline.Length);
            while (!atend)
            {
                char c1 = curline[pos++];
                if ((c1 >= 'A' && c1 <= 'Z') || (c1 >= 'a' && c1 <= 'z') || (c1 >= '0' && c1 <= '9') || (c1 == '_'))
                {
                    id = id + c1;
                    atend = !(pos < curline.Length);
                }
                else
                {
                    pos--;
                    atend = true;
                }
            }
            Token token = findKeyword(id);
            if (token == null) 
            {
                token = new Token(TokenType.tIDENTIFIER, id);
            }
            return token;

        }

        public Token scanFloatConst(String num)
        {
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
                if (num.EndsWith("."))      //if no decimal part, we add one anywat
                {
                    num = num + '0';
                }
                if ((pos < curline.Length) && ((curline[pos] == 'e') || (curline[pos] == 'E')))
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
            bool lsuffix = false;
            if ((pos < curline.Length) && ((curline[pos] == 'f') || (curline[pos] == 'F')))
            {
                fsuffix = true;
                pos++;
            }
            if ((pos < curline.Length) && ((curline[pos] == 'l') || (curline[pos] == 'L')))
            {
                lsuffix = true;
                pos++;
            }

            //double val = Convert.ToDouble(num);
            return new Token(TokenType.tFLOATCONST, num);
        }

        public Token scanNumber(char c)
        {
            String num = "0";
            int bass = 10;
            bool floatpt = false;

            //float const can start with '.' followed by digits, check this first
            //handle '...' and '.' tokens here
            if (c == '.')
            {
                if ((pos < curline.Length - 1) && (curline[pos] == '.') && (curline[pos + 1] == '.'))
                {
                    pos += 2;
                    return new Token(TokenType.tELLIPSIS);
                }
                else if ((pos < curline.Length) && !((curline[pos] >= '0') && (curline[pos] <= '9')))
                {
                    return new Token(TokenType.tPERIOD);
                }
                else floatpt = true;
            }

            if (!floatpt)       //get mantissa
            {
                num = "" + c;
                if (c == '0')             //set base
                {
                    if ((pos < curline.Length) && (curline[pos] == 'X' || curline[pos] == 'x'))
                    {
                        bass = 16;
                        pos++;
                    }
                    else
                    {
                        bass = 8;
                    }
                }
                bool atend = !(pos < curline.Length);
                while (!atend)
                {
                    char c1 = curline[pos++];
                    if (((bass == 10) && (c1 >= '0' && c1 <= '9')) ||
                        ((bass == 8) && (c1 >= '0' && c1 <= '7')) ||
                        ((bass == 16) && ((c1 >= '0' && c1 <= '9') || (c1 >= 'A' && c1 <= 'F') || (c1 >= 'a' && c1 <= 'f'))))
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

            //got the mantissa, if the next char is decimal point or exponent, then it's a float const
            if ((pos < curline.Length) && ((curline[pos] == '.') || (curline[pos] == 'E') || (curline[pos] == 'e')))
            {
                floatpt = true;
                char c2 = curline[pos++];
                num = num + Char.ToUpper(c2);
                return scanFloatConst(num);
            }
            else
            {
                bool usuffix = false;
                bool lsuffix = false;
                bool llsuffix = false;
                for (int i = 0; i < 2; i++)     //check for int const suffixes
                {
                    if ((pos < curline.Length) && (!usuffix) && ((curline[pos] == 'u') || (curline[pos] == 'U')))
                    {
                        usuffix = true;
                        pos++;
                    }

                    if ((pos < curline.Length) && (!lsuffix) && ((curline[pos] == 'l') || (curline[pos] == 'L')))
                    {
                        lsuffix = true;
                        pos++;
                        if ((pos < curline.Length) && ((curline[pos] == 'l') || (curline[pos] == 'L')))
                        {
                            llsuffix = true;
                            pos++;
                        }
                    }
                }
                //int value = Convert.ToInt32(num, bass);
                return new Token(TokenType.tINTCONST, num);
            }
        }

        //works for now, needs improvement
        private Token scanCharLiteral(char c)
        {
            char ch = (char)0;
            if ((pos < curline.Length - 1) && (curline[pos] != '\\'))
            {
                ch = curline[pos];
                pos += 2;
            }
            else
            {
                ch = curline[pos+1];
                ch = (char)((int)ch - (int)'0');
                pos += 3;
            }
            return new Token(TokenType.tCHARCONST, ("" + ch));
        }

        public Token scanString(char c)
        {
            String str = "";
            char endchar = c;
            bool atend = !(pos < curline.Length);
            while (!atend)
            {
                char c1 = curline[pos++];
                if (c1 != endchar)
                {
                    str = str + c1;
                    atend = !(pos < curline.Length);
                }
                else
                {
                    //pos--;
                    atend = true;
                }
            }
            Token strconst = new Token(TokenType.tSTRINGCONST, str);
            return strconst;            
        }

        public Token getToken()
        {
            Token token = null;

            if (recpos < replay.Count)
            {
                token = replay[recpos++];
                return token;
            }

            //goto start of next token in source file
            skipWhitespace();

            //scan chars until we get a token or reach end of file
            if (!atEOF)
            {
                char c = curline[pos++];
                switch (c)
                {

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
                    case 'L':
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
                        token = scanIdentifier(c);
                        break;

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
                    case '.':
                        token = scanNumber(c);
                        break;

                    case '\'':
                        token = scanCharLiteral(c);
                        break;

                    case '"':
                        token = scanString(c);
                        break;

//punctuation
                    case '[':
                        token = new Token(TokenType.tLBRACKET);
                        break;

                    case ']':
                        token = new Token(TokenType.tRBRACKET);
                        break;

                    case '(':
                        token = new Token(TokenType.tLPAREN);
                        break;

                    case ')':
                        token = new Token(TokenType.tRPAREN);
                        break;

                    case '{':
                        token = new Token(TokenType.tLBRACE);
                        break;

                    case '}':
                        token = new Token(TokenType.tRBRACE);
                        break;

                    case '+':
                        if ((pos < curline.Length) && (curline[pos] == '+'))
                        {
                            token = new Token(TokenType.tPLUSPLUS);
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tPLUSEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tPLUS);
                        }
                        break;

                    case '-':
                        if ((pos < curline.Length) && (curline[pos] == '-'))
                        {
                            token = new Token(TokenType.tMINUSMINUS);
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tMINNUSEQUAL);
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '>'))
                        {
                            token = new Token(TokenType.tARROW);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tMINUS);
                        }
                        break;

                    case '&':
                        if ((pos < curline.Length) && (curline[pos] == '&'))
                        {
                            token = new Token(TokenType.tDOUBLEAMP);
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tAMPEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tAMPERSAND);
                        }
                        break;

                    case '*':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tMULTEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tASTERISK);
                        }
                        break;

                    case '~':
                        token = new Token(TokenType.tTILDE);
                        break;

                    case '!':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tNOTEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tEXCLAIM);
                        }
                        break;

                    case '/':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tSLASHEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tSLASH);
                        }
                        break;

                    case '%':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tPERCENTEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tPERCENT);
                        }
                        break;

                    case '<':
                        if ((pos < curline.Length) && (curline[pos] == '<'))
                        {
                            if ((pos < curline.Length - 1) && (curline[pos + 1] == '='))
                            {
                                token = new Token(TokenType.tLSHIFTEQUAL);
                                pos += 2;
                            }
                            else
                            {
                                token = new Token(TokenType.tLEFTSHIFT);
                                pos++;
                            }
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tLESSEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tLESSTHAN);
                        }
                        break;

                    case '>':
                        if ((pos < curline.Length) && (curline[pos] == '>'))
                        {
                            if ((pos < curline.Length - 1) && (curline[pos + 1] == '='))
                            {
                                token = new Token(TokenType.tRSHIFTEQUAL);
                                pos += 2;
                            }
                            else
                            {
                                token = new Token(TokenType.tRIGHTSHIFT);
                                pos++;
                            }
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tGTREQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tGTRTHAN);
                        }
                        break;

                    case '=':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tEQUALEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tEQUAL);
                        }
                        break;

                    case '^':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tCARETEQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tCARET);
                        }
                        break;

                    case '|':
                        if ((pos < curline.Length) && (curline[pos] == '|'))
                        {
                            token = new Token(TokenType.tDOUBLEBAR);
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new Token(TokenType.tBAREQUAL);
                            pos++;
                        }
                        else
                        {
                            token = new Token(TokenType.tBAR);
                        }
                        break;

                    case '?':
                        token = new Token(TokenType.tQUESTION);
                        break;

                    case ':':
                        token = new Token(TokenType.tCOLON);
                        break;

                    case ';':
                        token = new Token(TokenType.tSEMICOLON);
                        break;

                    case ',':
                        token = new Token(TokenType.tCOMMA);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                token = new Token(TokenType.tEOF);
            }

            if (recpos == replay.Count)
            {
                replay.Add(token);
                recpos++;
            }

            return token;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");