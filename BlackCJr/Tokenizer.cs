/* ----------------------------------------------------------------------------
Black C Jr - a frontend C parser
Copyright (C) 2019  George E Greaney

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

namespace BlackCJr
{
    class Tokenizer
    {
        string sourceName;
        Scanner scanner;
        Dictionary<String, TokenType> keywords;
        List<String> typedefs;

        public Tokenizer(string _sourceName)
        {
            sourceName = _sourceName;
            scanner = new Scanner(sourceName);

            //build keyword list
            keywords = new Dictionary<string, TokenType>();
            keywords.Add("break", TokenType.BREAK);
            keywords.Add("case", TokenType.CASE);
            keywords.Add("char", TokenType.CHAR);
            keywords.Add("const", TokenType.CONST);
            keywords.Add("continue", TokenType.CONTINUE);
            keywords.Add("default", TokenType.DEFAULT);
            keywords.Add("do", TokenType.DO);
            keywords.Add("double", TokenType.DOUBLE);
            keywords.Add("else", TokenType.ELSE);
            keywords.Add("enum", TokenType.ENUM);
            keywords.Add("extern", TokenType.EXTERN);
            keywords.Add("float", TokenType.FLOAT);
            keywords.Add("for", TokenType.FOR);
            keywords.Add("goto", TokenType.GOTO);
            keywords.Add("if", TokenType.IF);
            keywords.Add("int", TokenType.INT);
            keywords.Add("long", TokenType.LONG);
            keywords.Add("return", TokenType.RETURN);
            keywords.Add("short", TokenType.SHORT);
            keywords.Add("signed", TokenType.SIGNED);
            keywords.Add("static", TokenType.STATIC);
            keywords.Add("struct", TokenType.STRUCT);
            keywords.Add("switch", TokenType.SWITCH);
            keywords.Add("typedef", TokenType.TYPEDEF);
            keywords.Add("union", TokenType.UNION);
            keywords.Add("unsigned", TokenType.UNSIGNED);
            keywords.Add("void", TokenType.VOID);
            keywords.Add("while", TokenType.WHILE);
        }

        public void setTypedef(String s) 
        {
            typedefs.Add(s);
        }

        public Token getToken()
        {
            Token tok = null;
            String frag = scanner.getFrag();

            //skip spaces
            while (scanner.fragtype == FragType.SPACE)
            {
                frag = scanner.getFrag();
            }

            switch (scanner.fragtype)
            {
                case FragType.WORD:
                    if (keywords.ContainsKey(frag))
                    {
                        tok = new Token(keywords[frag]);
                    }
                    else if (typedefs.Contains(frag))
                    {
                        tok = new Token(TokenType.TYPEDEF);
                    }
                    else
                    {
                        tok = new Token(TokenType.IDENT);
                        tok.ident = frag;
                    }
                    break;

                case FragType.NUMBER:
                    tok = new Token(TokenType.INTCONST);
                    tok.intval = Int32.Parse(frag);
                    break;

                case FragType.CHAR:
                    switch (frag[0])
                    {

                        case '[':
                            tok = new Token(TokenType.LBRACKET);
                            break;
                        case ']':
                            tok = new Token(TokenType.RBRACKET);
                            break;
                        case '(':
                            tok = new Token(TokenType.LPAREN);
                            break;
                        case ')':
                            tok = new Token(TokenType.RPAREN);
                            break;
                        case '{':
                            tok = new Token(TokenType.LBRACE);
                            break;
                        case '}':
                            tok = new Token(TokenType.RBRACE);
                            break;

                        case '+':
                            if (scanner.isNextChar('+'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.PLUSPLUS);
                            }
                            else if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.PLUSEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.PLUS);
                            }
                            break;
                        case '-':
                            if (scanner.isNextChar('-'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.MINUSMINUS);
                            }
                            else if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.MINUSEQUAL);
                            }
                            else if (scanner.isNextChar('>'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.ARROW);
                            }
                            else
                            {
                                tok = new Token(TokenType.MINUS);
                            }
                            break;
                        case '*':
                            if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.STAREQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.STAR);
                            }
                            break;
                        case '/':
                            if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.SLASHEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.SLASH);
                            }
                            break;
                        case '%':
                            if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.PERCENTEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.PERCENT);
                            }
                            break;
                        case '&':
                            if (scanner.isNextChar('&'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.AMPAMP);
                            }
                            else if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.AMPEQUAL);
                            }
                            else
                                tok = new Token(TokenType.AMPERSAND);
                            break;
                        case '|':
                            if (scanner.isNextChar('|'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.BARBAR);
                            }
                            else if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.BAREQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.BAR);
                            }
                            break;
                        case '~':
                            tok = new Token(TokenType.TILDE);
                            break;
                        case '^':
                            if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.CARETEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.CARET);
                            }
                            break;


                        case '=':
                            if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.EQUALEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.EQUAL);
                            }
                            break;
                        case '!':
                            if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.NOTEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.EXCLAIM);
                            }
                            break;
                        case '<':
                            if (scanner.isNextChar('<'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.LESSLESS);
                            }
                            else if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.LESSEQUAL);
                            }
                            else if (scanner.areNextTwoChars("<="))
                            {
                                scanner.skipNextTwoChars();
                                tok = new Token(TokenType.LESSLESSEQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.LESSTHAN);
                            }
                            break;
                        case '>':
                            if (scanner.isNextChar('>'))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.GTRGTR);
                            }
                            else if (scanner.isNextChar('='))
                            {
                                scanner.skipNextChar();
                                tok = new Token(TokenType.GTREQUAL);
                            }
                            else if (scanner.areNextTwoChars(">="))
                            {
                                scanner.skipNextTwoChars();
                                tok = new Token(TokenType.GTRGTREQUAL);
                            }
                            else
                            {
                                tok = new Token(TokenType.GTRTHAN);
                            }
                            break;

                        case ',':
                            tok = new Token(TokenType.COMMA);
                            break;
                        case '.':
                            if (scanner.areNextTwoChars(".."))
                            {
                                scanner.skipNextTwoChars();
                                tok = new Token(TokenType.ELIPSIS);
                            }
                            else
                            {
                                tok = new Token(TokenType.PERIOD);
                            }
                            break;
                        case '?':
                            tok = new Token(TokenType.QUESTION);
                            break;
                        case ':':
                            tok = new Token(TokenType.COLON);
                            break;
                        case ';':
                            tok = new Token(TokenType.SEMICOLON);
                            break;                        

                        default:
                            tok = new Token(TokenType.ERROR);
                            break;

                    }
                    break;

                case FragType.EOF:
                    tok = new Token(TokenType.EOF);
                    break;
            }

            return tok;
        }
    }
}
