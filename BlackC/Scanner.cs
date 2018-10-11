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
            Token ident = new tIdentifier(id);
            return ident;

        }

        public Token scanNumber(char c)
        {
            String num = "" + c;
            bool atend = !(pos < curline.Length);
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
            int value = Int32.Parse(num);
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
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tPlusEqual();
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
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tMinusEqual();
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '>'))
                        {
                            token = new tArrow();
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
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tAmpEqual();
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
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tAmpEqual();
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
                            }
                            else
                            {
                                token = new tLeftShift();
                            }
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tLessEqual();
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
                            }
                            else
                            {
                                token = new tRightShift();
                            }
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tGtrEqual();
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
                        }
                        else if ((pos < curline.Length) && (curline[pos] == '='))
                        {
                            token = new tBarEqual();
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
