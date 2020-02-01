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

namespace BlackC.Lexer
{
    public class Token
    {
        public TokenType type;
        public String chars;
        public SourceLocation loc;
        public bool startsLine;             //if this is the first non-whitespace token on a line
        public bool leadingSpace;           //if this has whitespace before it

        public string strval;
        public int intval;
        public double floatval;
        public String stringval;

        //public Token(TokenType _type, String _chars, SourceLocation _loc)
        //{
        //    type = _type;
        //    chars = _chars;
        //    loc = _loc;
        //    startsLine = false;
        //    leadingSpace = false;
        //}

        public Token(TokenType _type)
        {
            type = _type;
            strval = "";
            intval = 0;
            floatval = 0.0;
            stringval = "";
        }

        //these must be int he same order as the TokenType enum
        String[] spelling = new String[] { "ident", "typename", "int const", "float const", "char const", "string const",
                    "auto", "break", "case", "char", "const", "continue", "default", "do", "double", "else", "enum", "extern",
                    "float", "for", "goto", "if", "inline", "int", "long", "register", "restrict", "return", "short",
                    "signed", "sizeof", "static", "struct", "switch", "typedef", "union", "unsigned", "void", "volatile", "while",
                    "(", ")", "{", "}", "[", "]", ".", ",", ":", ";", "*", "...", "->","&", "~", "!", "?", 
                    "+", "-","++", "--", "/", "%", "<<", ">>", "<", ">","<=", ">=", "==", "!=", 
                    "^", "|","&&", "||", "=", "*=", "/=", "%=", "+=", "-=", "<<=",">>=", "&=", "^=", "|=", 
                    "eof", "error"};

        public override string ToString()
        {
            String spell = spelling[(int)type];
            switch (type) {

                case TokenType.IDENT:
                case TokenType.TYPENAME:
                case TokenType.STRINGCONST:            
                spell = spell + " (" + strval + ")";
                    break;

                case TokenType.INTCONST:
                    spell = spell + " (" + intval + ")";
                    break;

                case TokenType.FLOATCONST:
                    spell = spell + " (" + floatval + ")";
                    break;

                case TokenType.CHARCONST:
                    spell = spell + " (" + (char)intval + ")";
                    break;

                default:
                    break;
            }
            return spell;
        }
    }

    //public class IntConstToken : Token
    //{
    //    bool unsigned;
    //    bool islong;
    //    ulong uval;
    //    long lval;

    //    public IntConstToken(TokenType _type, String _chars, SourceLocation _loc, String intstr, bool _unsigned, int _long) :
    //        base(_type, _chars, _loc)
    //    {
    //        unsigned = _unsigned;
    //        islong = (_long > 1);
    //        if (unsigned)
    //        {
    //            uval = Convert.ToUInt64(intstr);
    //        }
    //        else
    //        {
    //            lval = Convert.ToInt64(intstr);
    //        }
    //    }
    //}

    //public class FloatConstToken : Token
    //{
    //    float val;

    //    public FloatConstToken(TokenType _type, String _chars, SourceLocation _loc, float _val) :
    //        base(_type, _chars, _loc)
    //    {
    //        val = _val;
    //    }
    //}

    //-------------------------------------------------------------------------

    public enum TokenType
    {
        IDENT,
        TYPENAME,

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