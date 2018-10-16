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
    class Token
    {
    }

    class tEOF : Token
    {
        public override string ToString()
        {
            return "END OF FILE";
        }
    }

    //- keywords -----------------------------------------------------------

    class tAuto : Token 
    {
        public override string ToString()
        {
            return "AUTO";
        }
    }

    class tBreak : Token
    {
        public override string ToString()
        {
            return "BREAK";
        }
    }

    class tCase : Token
    {
        public override string ToString()
        {
            return "CASE";
        }
    }

    class tChar : Token
    {
        public override string ToString()
        {
            return "CHAR";
        }
    }

    class tConst : Token
    {
        public override string ToString()
        {
            return "CONST";
        }
    }

    class tContinue : Token
    {
        public override string ToString()
        {
            return "CONTINUE";
        }
    }

    class tDefault : Token
    {
        public override string ToString()
        {
            return "DEFAULT";
        }
    }

    class tDo : Token
    {
        public override string ToString()
        {
            return "DO";
        }
    }

    class tDouble : Token
    {
        public override string ToString()
        {
            return "DOUBLE";
        }
    }

    class tElse : Token
    {
        public override string ToString()
        {
            return "ELSE";
        }
    }

    class tEnum : Token
    {
        public override string ToString()
        {
            return "ENUM";
        }
    }

    class tExtern : Token
    {
        public override string ToString()
        {
            return "EXTERN";
        }
    }

    class tFloat : Token
    {
        public override string ToString()
        {
            return "FLOAT";
        }
    }

    class tFor : Token
    {
        public override string ToString()
        {
            return "FOR";
        }
    }

    class tGoto : Token
    {
        public override string ToString()
        {
            return "GOTO";
        }
    }

    class tIf : Token
    {
        public override string ToString()
        {
            return "IF";
        }
    }

    class tInline : Token
    {
        public override string ToString()
        {
            return "INLINE";
        }
    }
    
    class tInt : Token
    {
        public override string ToString()
        {
            return "INT";
        }
    }

    class tLong : Token
    {
        public override string ToString()
        {
            return "LONG";
        }
    }

    class tRegister : Token
    {
        public override string ToString()
        {
            return "REGISTER";
        }
    }

    class tRestrict : Token
    {
        public override string ToString()
        {
            return "RESTRICT";
        }
    }

    class tReturn : Token
    {
        public override string ToString()
        {
            return "RETURN";
        }
    }

    class tShort : Token
    {
        public override string ToString()
        {
            return "SHORT";
        }
    }

    class tSigned : Token
    {
        public override string ToString()
        {
            return "SIGNED";
        }
    }

    class tSizeof : Token
    {
        public override string ToString()
        {
            return "SIZEOF";
        }
    }

    class tStatic : Token
    {
        public override string ToString()
        {
            return "STATIC";
        }
    }

    class tStruct : Token
    {
        public override string ToString()
        {
            return "STRUCT";
        }
    }

    class tSwitch : Token
    {
        public override string ToString()
        {
            return "SWITCH";
        }
    }

    class tTypedef : Token
    {
        public override string ToString()
        {
            return "TYPEDEF";
        }
    }

    class tUnion : Token
    {
        public override string ToString()
        {
            return "UNION";
        }
    }

    class tUnsigned : Token
    {
        public override string ToString()
        {
            return "UNSIGNED";
        }
    }

    class tVoid : Token
    {
        public override string ToString()
        {
            return "VOID";
        }
    }

    class tVolatile : Token
    {
        public override string ToString()
        {
            return "VOLATILE";
        }
    }

    class tWhile : Token
    {
        public override string ToString()
        {
            return "WHILE";
        }
    }

    //- identifier -----------------------------------------------------------

    class tIdentifier : Token
    {
        String ident;

        public tIdentifier(String _ident)
            : base()
        {
            ident = _ident;
        }

        public override string ToString()
        {
            return "IDENTIFIER (" + ident + ")";
        }
    }

    //- constants -----------------------------------------------------------

    class tIntegerConstant : Token
    {
        enum WIDTH { NONE, LONG, LONGLONG };

        int val;
        bool  isSigned;
        WIDTH width;        

        public tIntegerConstant(int _val, bool _signed, bool _long, bool _longlong)
            : base()
        {
            val = _val;
            isSigned = _signed;
            width = (_longlong) ? WIDTH.LONGLONG : (_long) ? WIDTH.LONG : WIDTH.NONE;
        }

        public override string ToString()
        {
            return "INTEGER CONSTANT (" + val.ToString() + ")";
        }
    }

    class tFloatConstant : Token
    {
        enum WIDTH { NONE, FLOAT, LONG };

        double val;
        WIDTH width;

        public tFloatConstant(double _val, bool _float, bool _long)
            : base()
        {
            val = _val;
            width = (_float) ? WIDTH.FLOAT : (_long) ? WIDTH.LONG : WIDTH.NONE;
        }

        public override string ToString()
        {
            return "FLOAT CONSTANT";
        }
    }

    class tStringConstant : Token
    {
        String val;

        public tStringConstant(String _val)
            : base()
        {
            val = _val;
        }

        public override string ToString()
        {
            return "STRING CONSTANT (" + val + ")";
        }
    }

    //- punctuation -----------------------------------------------------------

    class tLBracket : Token
    {
        public override string ToString()
        {
            return "LEFT BRACKET";
        }
    }

    class tRBracket : Token
    {
        public override string ToString()
        {
            return "RIGHT BRACKET";
        }
    }

    class tLParen : Token
    {
        public override string ToString()
        {
            return "LEFT PAREN";
        }
    }

    class tRParen : Token
    {
        public override string ToString()
        {
            return "RIGHT PAREN";
        }
    }

    class tLBrace : Token
    {
        public override string ToString()
        {
            return "LEFT BRACE";
        }
    }

    class tRBrace : Token
    {
        public override string ToString()
        {
            return "RIGHT BRACE";
        }
    }

    class tPeriod : Token
    {
        public override string ToString()
        {
            return "PERIOD";
        }
    }

    class tArrow : Token
    {
        public override string ToString()
        {
            return "ARROW";
        }
    }

    class tPlusPlus : Token
    {
        public override string ToString()
        {
            return "PLUS PLUS";
        }
    }

    class tMinusMinus : Token
    {
        public override string ToString()
        {
            return "MINUS MINUS";
        }
    }

    class tAmpersand : Token
    {
        public override string ToString()
        {
            return "AMPERSAND";
        }
    }

    class tAsterisk : Token
    {
        public override string ToString()
        {
            return "ASTERISK";
        }
    }

    class tPlus : Token
    {
        public override string ToString()
        {
            return "PLUS";
        }
    }

    class tMinus : Token
    {
        public override string ToString()
        {
            return "MINUS";
        }
    }

    class tTilde : Token
    {
        public override string ToString()
        {
            return "TILDE";
        }
    }

    class tExclaim : Token
    {
        public override string ToString()
        {
            return "EXCLAMATION POINT";
        }
    }

    class tSlash : Token
    {
        public override string ToString()
        {
            return "SLASH";
        }
    }

    class tPercent : Token
    {
        public override string ToString()
        {
            return "PERCENT";
        }
    }

    class tLeftShift : Token
    {
        public override string ToString()
        {
            return "LEFT SHIFT";
        }
    }

    class tRightShift : Token
    {
        public override string ToString()
        {
            return "RIGHT SHIFT";
        }
    }

    class tLessThan : Token
    {
        public override string ToString()
        {
            return "LESS THAN";
        }
    }

    class tGtrThan : Token
    {
        public override string ToString()
        {
            return "GREATER THAN";
        }
    }

    class tLessEqual : Token
    {
        public override string ToString()
        {
            return "LESS EQUAL";
        }
    }

    class tGtrEqual : Token
    {
        public override string ToString()
        {
            return "GREATER EQUAL";
        }
    }

    class tEqualEqual : Token
    {
        public override string ToString()
        {
            return "EQUAL EQUAL";
        }
    }

    class tNotEqual : Token
    {
        public override string ToString()
        {
            return "NOT EQUAL";
        }
    }

    class tCaret : Token
    {
        public override string ToString()
        {
            return "CARET";
        }
    }

    class tBar : Token
    {
        public override string ToString()
        {
            return "BAR";
        }
    }

    class tDoubleAmp : Token
    {
        public override string ToString()
        {
            return "AMP AMP";
        }
    }

    class tDoubleBar : Token
    {
        public override string ToString()
        {
            return "BAR BAR";
        }
    }

    class tQuestion : Token
    {
        public override string ToString()
        {
            return "QUESTION";
        }
    }

    class tColon : Token
    {
        public override string ToString()
        {
            return "COLON";
        }
    }

    class tSemicolon : Token
    {
        public override string ToString()
        {
            return "SEMICOLON";
        }
    }

    class tEllipsis : Token
    {
        public override string ToString()
        {
            return "ELLIPSIS";
        }
    }

    class tEqual : Token
    {
        public override string ToString()
        {
            return "EQUAL";
        }
    }

    class tMultEqual : Token
    {
        public override string ToString()
        {
            return "MULT EQUAL";
        }
    }

    class tSlashEqual : Token
    {
        public override string ToString()
        {
            return "SLASH EQUAL";
        }
    }

    class tPercentEqual : Token
    {
        public override string ToString()
        {
            return "PERCENT EQUAL";
        }
    }

    class tPlusEqual : Token
    {
        public override string ToString()
        {
            return "PLUS EQUAL";
        }
    }

    class tMinusEqual : Token
    {
        public override string ToString()
        {
            return "MINUS EQUAL";
        }
    }

    class tLShiftEqual : Token
    {
        public override string ToString()
        {
            return "LEFT SHIFT EQUAL";
        }
    }

    class tRShiftEqual : Token
    {
        public override string ToString()
        {
            return "RIGHT SHIFT EQUAL";
        }
    }

    class tAmpEqual : Token
    {
        public override string ToString()
        {
            return "AMP EQUAL";
        }
    }

    class tCaretEqual : Token
    {
        public override string ToString()
        {
            return "CARET EQUAL";
        }
    }

    class tBarEqual : Token
    {
        public override string ToString()
        {
            return "BAR EQUAL";
        }
    }

    class tComma : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }
}
