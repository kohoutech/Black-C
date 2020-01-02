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
    class Token
    {
        public TokenType type;
        public string ident;
        public int intval;
        public double floatval;
        public String stringval;

        public Token(TokenType _type)
        {
            type = _type;
            ident = "";
            intval = 0;
            floatval = 0.0;
            stringval = "";
        }

        //keep these in sync with the TokenType enum
        String[] spellings = { "identifier", "int const", "float const", "char const", "string const",
                               "break", "case", "char", "const", "continue", "default", "do", "double", "else", "enum",
                               "extern", "float", "for", "goto", "if", "int", "long", "return", "short", "signed",
                               "static", "struct", "switch", "typedef", "union", "unsigned", "void", "while",
                               "[", "]", "(", ")", "{", "}", ".", "->", "++", "--", "&", "*", "+", "-", "~", "!", "/", "%",
                               "<<", ">>", "<", ">", "<=", ">=", "==", "!=", "^", "|", "&&", "||", "?", ":", ";",
                               "...", "=", "*=", "/=", "%=", "+=", "-=", "<<=", ">>=", "&=", "^=", "|=", ",",
                               "eof", "error"
                             };

        public override string ToString()
        {
            String result = spellings[(int)type];
            if (type == TokenType.IDENT)
            {
                result = result + "(" + ident + ")";
            }
            if (type == TokenType.INTCONST)
            {
                result = result + "(" + intval + ")";
            }
            if (type == TokenType.FLOATCONST)
            {
                result = result + "(" + floatval + ")";
            }
            if (type == TokenType.CHARCONST)
            {
                result = result + "(" + intval.ToString("X2") + ")";
            }
            if (type == TokenType.STRINGCONST)
            {
                result = result + "(" + stringval + ")";
            }
            return result;
        }
    }

    enum TokenType
    {
        IDENT,
        INTCONST,
        FLOATCONST,
        CHARCONST,
        STRINGCONST,

        //keywords
        BREAK,
        CASE,
        CHAR,
        CONST,
        CONTINUE,
        DEFAULT,
        DO,
        DOUBLE,
        ELSE,
        ENUM,
        EXTERN,
        FLOAT,
        FOR,
        GOTO,
        IF,
        INT,
        LONG,
        RETURN,
        SHORT,
        SIGNED,
        STATIC,
        STRUCT,
        SWITCH,
        TYPEDEF,
        UNION,
        UNSIGNED,
        VOID,
        WHILE,

        //punctuation
        LBRACKET,
        RBRACKET,
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        PERIOD,
        ARROW,
        PLUSPLUS,
        MINUSMINUS,
        AMPERSAND,
        STAR,
        PLUS,
        MINUS,
        TILDE,
        EXCLAIM,
        SLASH,
        PERCENT,
        LESSLESS,
        GTRGTR,
        LESSTHAN,
        GTRTHAN,
        LESSEQUAL,
        GTREQUAL,
        EQUALEQUAL,
        NOTEQUAL,
        CARET,
        BAR,
        AMPAMP,
        BARBAR,
        QUESTION,
        COLON,
        SEMICOLON,
        ELIPSIS,
        EQUAL,
        STAREQUAL,
        SLASHEQUAL,
        PERCENTEQUAL,
        PLUSEQUAL,
        MINUSEQUAL,
        LESSLESSEQUAL,
        GTRGTREQUAL,
        AMPEQUAL,
        CARETEQUAL,
        BAREQUAL,
        COMMA,

        //end of file
        EOF,

        //anything else
        ERROR
    }

}
