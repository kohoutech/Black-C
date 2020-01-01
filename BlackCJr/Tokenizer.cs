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
        Dictionary<String, TokenType> reservedWords;
        List<String> typedefs;

        public Tokenizer(string _sourceName)
        {
            sourceName = _sourceName;
            scanner = new Scanner(sourceName);

            //build reserved word list
            reservedWords = new Dictionary<string, TokenType>();
            reservedWords.Add("int", TokenType.INT);
            reservedWords.Add("return", TokenType.RETURN);
        }

        public Token getToken()
        {
            Token tok = null;
            String frag = scanner.getFrag();
            if (scanner.fragtype == FragType.SPACE)
            {
                frag = scanner.getFrag();
            }

            if (scanner.fragtype == FragType.WORD)
            {
                if (reservedWords.ContainsKey(frag))
                {
                    tok = new Token(reservedWords[frag]);
                }
                else
                {
                    tok = new Token(TokenType.IDENT);
                    tok.ident = frag;
                }
            }

            else if (scanner.fragtype == FragType.NUMBER)
            {
                tok = new Token(TokenType.INTCONST);
                tok.intval = Int32.Parse(frag);
            }
            else if (scanner.fragtype == FragType.CHAR)
            {
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
                    case ';':
                        tok = new Token(TokenType.SEMICOLON);
                        break;
                }
            }
            else if (scanner.fragtype == FragType.EOF)
            {
                tok = new Token(TokenType.EOF);
            }

            return tok;
        }
    }
}
