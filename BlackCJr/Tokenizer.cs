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

                        case '{':
                            tok = new Token(TokenType.LBRACE);
                            break;
                        case '}':
                            tok = new Token(TokenType.RBRACE);
                            break;
                        case '(':
                            tok = new Token(TokenType.LPAREN);
                            break;
                        case ')':
                            tok = new Token(TokenType.RPAREN);
                            break;

                            
        //ARROW,
        //PLUSPLUS,
        //MINUSMINUS,
                        case '&':
                            tok = new Token(TokenType.AMPERSAND);
                            break;

                        case '*':
                            tok = new Token(TokenType.STAR);
                            break;
                        case '+':
                            tok = new Token(TokenType.PLUS);
                            break;
                                case '-':
                            tok = new Token(TokenType.MINUS);
                            break;

        //TILDE,
        //EXCLAIM,
        //SLASH,
        //PERCENT,
        //LESSLESS,
        //GTRGTR,
        //LESSTHAN,
        //GTRTHAN,
        //LESSEQUAL,
        //GTREQUAL,
        //EQUALEQUAL,
        //NOTEQUAL,
        //CARET,
        //BAR,
        //AMPAMP,
        //BARBAR,
        //QUESTION,
        //COLON,

                        case ';':
                            tok = new Token(TokenType.SEMICOLON);
                            break;

        //                            ELIPSIS,
        //EQUAL,
        //STAREQUAL,
        //SLASHEQUAL,
        //PERCENTEQUAL,
        //PLUSEQUAL,
        //MINUSEQUAL,
        //LESSLESSEQUAL,
        //GTRGTREQUAL,
        //AMPEQUAL,
        //CARETEQUAL,
        //BAREQUAL,
        //COMMA,

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
