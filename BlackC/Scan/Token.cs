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

namespace BlackC.Scan
{
    public class Token
    {
        public TokenType type;
        public int pos;
        public int line;

        public Token(TokenType _type)
        {
            type = _type;            
            pos = 0;
            line = 0;
        }

        //these must be int he same order as the TokenType enum
        String[] spelling = new String[] { "ident", "int const", "float const", "char const", "string const",
                    "auto", "break", "case", "char", "const", "continue", "default", "do", "double", "else", "enum", "extern",
                    "float", "for", "goto", "if", "inline", "int", "long", "register", "restrict", "return", "short",
                    "signed", "sizeof", "static", "struct", "switch", "typedef", "union", "unsigned", "void", "volatile", "while",
                    "(", ")", "{", "}", "[", "]", ".", ",", ":", ";", "*", "...", "->","&", "~", "!", "?", 
                    "+", "-","++", "--", "/", "%", "<<", ">>", "<", ">","<=", ">=", "==", "!=", 
                    "^", "|","&&", "||", "=", "*=", "/=", "%=", "+=", "-=", "<<=",">>=", "&=", "^=", "|=", 
                    "eof", "error"};

        public override string ToString()
        {
            return spelling[(int)type];
        }
    }

    public class IdentToken : Token
    {
        public String idstr;

        public IdentToken(String _idstr) :
            base(TokenType.IDENT)
        {
            idstr = _idstr;
        }

        public override string ToString()
        {
            return "ident (" + idstr + ")";
        }
    }

    public class IntConstToken : Token
    {
        public ulong val;
        public bool unsigned;
        public int width;         //0 = not long, 1 = long, 2 = long long

        public IntConstToken(ulong _val, bool _unsigned, int _width) :
            base(TokenType.INTCONST)
        {
            val = _val;
            unsigned = _unsigned;
            width = _width;
        }

        public override string ToString()
        {
            return "int const (" + val.ToString() + ")";
        }
    }

    public class FloatConstToken : Token
    {
        public double val;
        public int width;         //0 = float, 1 = double, 2 = long double

        public FloatConstToken(double _val, int _width) :
            base(TokenType.FLOATCONST)
        {
            val = _val;
            width = _width;
        }

        public override string ToString()
        {
            return "float const (" + val.ToString() + ")";
        }
    }


    public class StringConstToken : Token
    {
        public String val;
        public bool isWide;

        public StringConstToken(String _val, bool _isWide) :
            base(TokenType.STRINGCONST)
        {
            val = _val;
            isWide = _isWide;
        }

        public override string ToString()
        {
            return "string const (" + val + ")";
        }
    }

    public class CharConstToken : Token
    {
        public String val;
        public bool isWide;

        public CharConstToken(String _val, bool _isWide) :
            base(TokenType.CHARCONST)
        {
            val = _val;
            isWide = _isWide;
        }

        public override string ToString()
        {
            return "char const (" + val + ")";
        }
    }

    //-------------------------------------------------------------------------

    public enum TokenType
    {
        IDENT,

        //constants
        INTCONST,
        FLOATCONST,
        CHARCONST,
        STRINGCONST,

        //keywords
        AUTO,
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
        INLINE,
        INT,
        LONG,
        REGISTER,
        RESTRICT,
        RETURN,
        SHORT,
        SIGNED,
        SIZEOF,
        STATIC,
        STRUCT,
        SWITCH,
        TYPEDEF,
        UNION,
        UNSIGNED,
        VOID,
        VOLATILE,
        WHILE,

        //punctuation
        LPAREN,
        RPAREN,
        LBRACE,            //{
        RBRACE,            //}
        LBRACKET,          //[
        RBRACKET,          //]

        PERIOD,
        COMMA,
        COLON,
        SEMICOLON,
        STAR,              //*
        ELLIPSIS,          //...
        ARROW,             //->
        AMPERSAND,
        TILDE,
        EXCLAIM,           //!
        QUESTION,

        PLUS,
        MINUS,
        PLUSPLUS,
        MINUSMINUS,
        SLASH,
        PERCENT,
        LESSLESS,         //<<
        GTRGTR,           //>>

        LESSTHAN,
        GTRTHAN,
        LESSEQUAL,
        GTREQUAL,
        EQUALEQUAL,
        NOTEQUAL,

        CARET,
        BAR,
        AMPAMP,             //&&
        BARBAR,             //|| (not the elephant)

        EQUAL,
        MULTEQUAL,
        SLASHEQUAL,
        PERCENTEQUAL,
        PLUSEQUAL,
        MINUSEQUAL,
        LESSLESSEQUAL,
        GTRGTREQUAL,
        AMPEQUAL,
        CARETEQUAL,
        BAREQUAL,

        EOF,

        ERROR              //any char we don't recognize
    }
}