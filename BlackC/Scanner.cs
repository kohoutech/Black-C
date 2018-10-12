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

        public Scanner(string[] _lines)
        {
            lines = _lines;
            linenum = 0;
            curline = lines[linenum];
            pos = 0;
            atEOF = false;
        }

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
                    token = new tAuto(); 
                    break;

                case "break": 
                    token = new tBreak(); 
                    break;

                case "case": 
                    token = new tCase(); 
                    break;

                case "char": 
                    token = new tChar(); 
                    break;

                case "const": 
                    token = new tConst(); 
                    break;

                case "continue": 
                    token = new tContinue(); 
                    break;

                case "default": 
                    token = new tDefault(); 
                    break;

                case "do": 
                    token = new tDo(); 
                    break;

                case "double":
                    token = new tDouble(); 
                    break;

                case "else": 
                    token = new tElse(); 
                    break;

                case "enum": 
                    token = new tEnum(); 
                    break;

                case "extern": 
                    token = new tExtern(); 
                    break;

                case "float": 
                    token = new tFloat(); 
                    break;

                case "for": 
                    token = new tFor(); 
                    break;

                case "goto": 
                    token = new tGoto(); 
                    break;

                case "if": 
                    token = new tIf(); 
                    break;

                case "inline": 
                    token = new tInline(); 
                    break;

                case "int": 
                    token = new tInt(); 
                    break;

                case "long": 
                    token = new tLong(); 
                    break;

                case "register": 
                    token = new tRegister(); 
                    break;

                case "restrict": 
                    token = new tRestrict(); 
                    break;

                case "return": 
                    token = new tReturn(); 
                    break;

                case "short": 
                    token = new tShort(); 
                    break;

                case "signed": 
                    token = new tSigned(); 
                    break;

                case "sizeof": 
                    token = new tSizeof(); 
                    break;

                case "static": 
                    token = new tStatic(); 
                    break;

                case "struct": 
                    token = new tStruct(); 
                    break;

                case "switch": 
                    token = new tSwitch(); 
                    break;

                case "typedef": 
                    token = new tTypedef(); 
                    break;

                case "union": 
                    token = new tUnion(); 
                    break;

                case "unsigned": 
                    token = new tUnsigned(); 
                    break;

                case "void": 
                    token = new tVoid(); 
                    break;

                case "volatile": 
                    token = new tVolatile(); 
                    break;

                case "while": 
                    token = new tWhile(); 
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
                token = new tIdentifier(id);
            }
            return token;

        }

        public Token scanNumber(char c)
        {
            String num = "" + c;
            int bass = 10;
            if (c == '0') 
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
            int value = Convert.ToInt32(num, bass);
            Token intconst = new tIntegerConstant(value);
            return intconst;
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
                    str = str + c;
                    atend = !(pos < curline.Length);
                }
                else
                {
                    pos--;
                    atend = true;
                }
            }
            Token strconst = new tStringConstant(str);
            return strconst;            
        }

        public Token getToken()
        {
            Token token = null;

            //goto start of next token in source file
            skipWhitespace();

            //scan chars until we get a token or reach end of file
            if (!atEOF)
            {
                char c = curline[pos++];
                switch (c)
                {
                    case ' ':
                    case '\t':
                        
                        break;

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
                        token = scanNumber(c);
                        break;

                    case '\'':
                    case '"':
                        token = scanString(c);
                        break;

//punctuation
                    case '[':
                        token = new tLBracket();
                        break;

                    case ']':
                        token = new tRBracket();
                        break;

                    case '(':
                        token = new tLParen();
                        break;

                    case ')':
                        token = new tRParen();
                        break;

                    case '{':
                        token = new tLBrace();
                        break;

                    case '}':
                        token = new tRBrace();
                        break;

                    case '.':
                        if ((pos < curline.Length - 1) && (curline[pos] == '.') && (curline[pos + 1] == '.'))
                        {
                            token = new tEllipsis();
                            pos += 2;
                        }
                        else
                        {
                            token = new tPeriod();
                        }
                        break;

                    case '+':
                        if ((pos < curline.Length) && (curline[pos] == '+'))
                        {
                            token = new tPlusPlus();
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tPlusEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tPlus();
                        }
                        break;

                    case '-':
                        if ((pos < curline.Length) && (curline[pos] == '-'))
                        {
                            token = new tMinusMinus();
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tMinusEqual();
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '>'))
                        {
                            token = new tArrow();
                            pos++;
                        }
                        else
                        {
                            token = new tMinus();
                        }
                        break;

                    case '&':
                        if ((pos < curline.Length) && (curline[pos] == '&'))
                        {
                            token = new tDoubleAmp();
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tAmpEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tAmpersand();                            
                        }
                        break;

                    case '*':
                        if ((pos < curline.Length) && (curline[pos] == '&'))
                        {
                            token = new tDoubleAmp();
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tAmpEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tAsterisk();
                        }
                        break;

                    case '~':
                        token = new tTilde();
                        break;

                    case '!':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tNotEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tExclaim();
                        }
                        break;

                    case '/':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tSlashEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tSlash();
                        }
                        break;

                    case '%':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tPercentEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tPercent();
                        }
                        break;

                    case '<':
                        if ((pos < curline.Length) && (curline[pos] == '<'))
                        {
                            if ((pos < curline.Length - 1) && (curline[pos + 1] == '='))
                            {
                                token = new tLShiftEqual();
                                pos += 2;
                            }
                            else
                            {
                                token = new tLeftShift();
                                pos++;
                            }
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tLessEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tLessThan();
                        }
                        break;

                    case '>':
                        if ((pos < curline.Length) && (curline[pos] == '>'))
                        {
                            if ((pos < curline.Length - 1) && (curline[pos + 1] == '='))
                            {
                                token = new tRShiftEqual();
                                pos += 2;
                            }
                            else
                            {
                                token = new tRightShift();
                                pos++;
                            }
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tGtrEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tGtrThan();
                        }
                        break;

                    case '=':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tEqualEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tEqual();
                        }
                        break;

                    case '^':
                        if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tCaretEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tCaret();
                        }
                        break;

                    case '|':
                        if ((pos < curline.Length) && (curline[pos] == '|'))
                        {
                            token = new tDoubleBar();
                            pos++;
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tBarEqual();
                            pos++;
                        }
                        else
                        {
                            token = new tBar();
                        }
                        break;

                    case '?':
                        token = new tQuestion();
                        break;

                    case ':':
                        token = new tColon();
                        break;

                    case ';':
                        token = new tSemicolon();
                        break;

                    case ',':
                        token = new tComma();
                        break;

                    default:
                        break;
                }
            }

            if (atEOF)
            {
                token = new tEOF();
            }

            return token;
        }
    }
}
