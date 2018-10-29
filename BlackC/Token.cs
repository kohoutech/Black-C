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
    public class Token
    {
    }

    public class tEOF : Token
    {
        public override string ToString()
        {
            return "END OF FILE";
        }
    }

    //- keywords -----------------------------------------------------------

    public class tAuto : Token 
    {
        public override string ToString()
        {
            return "AUTO";
        }
    }

    public class tBreak : Token
    {
        public override string ToString()
        {
            return "BREAK";
        }
    }

    public class tCase : Token
    {
        public override string ToString()
        {
            return "CASE";
        }
    }

    public class tChar : Token
    {
        public override string ToString()
        {
            return "CHAR";
        }
    }

    public class tConst : Token
    {
        public override string ToString()
        {
            return "CONST";
        }
    }

    public class tContinue : Token
    {
        public override string ToString()
        {
            return "CONTINUE";
        }
    }

    public class tDefault : Token
    {
        public override string ToString()
        {
            return "DEFAULT";
        }
    }

    public class tDo : Token
    {
        public override string ToString()
        {
            return "DO";
        }
    }

    public class tDouble : Token
    {
        public override string ToString()
        {
            return "DOUBLE";
        }
    }

    public class tElse : Token
    {
        public override string ToString()
        {
            return "ELSE";
        }
    }

    public class tEnum : Token
    {
        public override string ToString()
        {
            return "ENUM";
        }
    }

    public class tExtern : Token
    {
        public override string ToString()
        {
            return "EXTERN";
        }
    }

    public class tFloat : Token
    {
        public override string ToString()
        {
            return "FLOAT";
        }
    }

    public class tFor : Token
    {
        public override string ToString()
        {
            return "FOR";
        }
    }

    public class tGoto : Token
    {
        public override string ToString()
        {
            return "GOTO";
        }
    }

    public class tIf : Token
    {
        public override string ToString()
        {
            return "IF";
        }
    }

    public class tInline : Token
    {
        public override string ToString()
        {
            return "INLINE";
        }
    }

    public class tInt : Token
    {
        public override string ToString()
        {
            return "INT";
        }
    }

    public class tLong : Token
    {
        public override string ToString()
        {
            return "LONG";
        }
    }

    public class tRegister : Token
    {
        public override string ToString()
        {
            return "REGISTER";
        }
    }

    public class tRestrict : Token
    {
        public override string ToString()
        {
            return "RESTRICT";
        }
    }

    public class tReturn : Token
    {
        public override string ToString()
        {
            return "RETURN";
        }
    }

    public class tShort : Token
    {
        public override string ToString()
        {
            return "SHORT";
        }
    }

    public class tSigned : Token
    {
        public override string ToString()
        {
            return "SIGNED";
        }
    }

    public class tSizeof : Token
    {
        public override string ToString()
        {
            return "SIZEOF";
        }
    }

    public class tStatic : Token
    {
        public override string ToString()
        {
            return "STATIC";
        }
    }

    public class tStruct : Token
    {
        public override string ToString()
        {
            return "STRUCT";
        }
    }

    public class tSwitch : Token
    {
        public override string ToString()
        {
            return "SWITCH";
        }
    }

    public class tTypedef : Token
    {
        public override string ToString()
        {
            return "TYPEDEF";
        }
    }

    public class tUnion : Token
    {
        public override string ToString()
        {
            return "UNION";
        }
    }

    public class tUnsigned : Token
    {
        public override string ToString()
        {
            return "UNSIGNED";
        }
    }

    public class tVoid : Token
    {
        public override string ToString()
        {
            return "VOID";
        }
    }

    public class tVolatile : Token
    {
        public override string ToString()
        {
            return "VOLATILE";
        }
    }

    public class tWhile : Token
    {
        public override string ToString()
        {
            return "WHILE";
        }
    }

    //- identifier -----------------------------------------------------------

    public class tIdentifier : Token
    {
        public String ident;

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

    public class tIntegerConstant : Token
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

    public class tFloatConstant : Token
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

    public class tCharacterConstant : Token
    {
        Char val;

        public tCharacterConstant(Char _val)
            : base()
        {
            val = _val;
        }

        public override string ToString()
        {
            return "CHAR CONSTANT (" + val + ")";
        }
    }

    public class tStringConstant : Token
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

    public class tLParen : Token                   //(
    {
        public override string ToString()
        {
            return "LEFT PAREN";
        }
    }

    public class tRParen : Token                   //)
    {
        public override string ToString()
        {
            return "RIGHT PAREN";
        }
    }

    public class tLBrace : Token                   //{
    {
        public override string ToString()
        {
            return "LEFT BRACE";
        }
    }

    public class tRBrace : Token                   //}
    {
        public override string ToString()
        {
            return "RIGHT BRACE";
        }
    }

    public class tLBracket : Token                 //[
    {
        public override string ToString()
        {
            return "LEFT BRACKET";
        }
    }

    public class tRBracket : Token                 //]
    {
        public override string ToString()
        {
            return "RIGHT BRACKET";
        }
    }

    public class tPeriod : Token
    {
        public override string ToString()
        {
            return "PERIOD";
        }
    }

    public class tComma : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    public class tColon : Token
    {
        public override string ToString()
        {
            return "COLON";
        }
    }

    public class tSemicolon : Token
    {
        public override string ToString()
        {
            return "SEMICOLON";
        }
    }

    public class tAsterisk : Token
    {
        public override string ToString()
        {
            return "ASTERISK";
        }
    }

    public class tEllipsis : Token                     //...
    {
        public override string ToString()
        {
            return "ELLIPSIS";
        }
    }

    public class tArrow : Token                //->
    {
        public override string ToString()
        {
            return "ARROW";
        }
    }

    public class tAmpersand : Token
    {
        public override string ToString()
        {
            return "AMPERSAND";
        }
    }

    public class tTilde : Token
    {
        public override string ToString()
        {
            return "TILDE";
        }
    }

    public class tExclaim : Token
    {
        public override string ToString()
        {
            return "EXCLAMATION POINT";
        }
    }

    public class tQuestion : Token
    {
        public override string ToString()
        {
            return "QUESTION";
        }
    }

    public class tPlus : Token
    {
        public override string ToString()
        {
            return "PLUS";
        }
    }

    public class tMinus : Token
    {
        public override string ToString()
        {
            return "MINUS";
        }
    }

    public class tPlusPlus : Token
    {
        public override string ToString()
        {
            return "PLUS PLUS";
        }
    }

    public class tMinusMinus : Token
    {
        public override string ToString()
        {
            return "MINUS MINUS";
        }
    }

    public class tSlash : Token
    {
        public override string ToString()
        {
            return "SLASH";
        }
    }

    public class tPercent : Token
    {
        public override string ToString()
        {
            return "PERCENT";
        }
    }

    public class tLeftShift : Token                //<<
    {
        public override string ToString()
        {
            return "LEFT SHIFT";
        }
    }

    public class tRightShift : Token               //>>
    {
        public override string ToString()
        {
            return "RIGHT SHIFT";
        }
    }

    public class tLessThan : Token
    {
        public override string ToString()
        {
            return "LESS THAN";
        }
    }

    public class tGtrThan : Token
    {
        public override string ToString()
        {
            return "GREATER THAN";
        }
    }

    public class tLessEqual : Token
    {
        public override string ToString()
        {
            return "LESS EQUAL";
        }
    }

    public class tGtrEqual : Token
    {
        public override string ToString()
        {
            return "GREATER EQUAL";
        }
    }

    public class tEqualEqual : Token
    {
        public override string ToString()
        {
            return "EQUAL EQUAL";
        }
    }

    public class tNotEqual : Token
    {
        public override string ToString()
        {
            return "NOT EQUAL";
        }
    }

    public class tCaret : Token
    {
        public override string ToString()
        {
            return "CARET";
        }
    }

    public class tBar : Token
    {
        public override string ToString()
        {
            return "BAR";
        }
    }

    public class tDoubleAmp : Token                    //&&
    {
        public override string ToString()
        {
            return "AMP AMP";
        }
    }

    public class tDoubleBar : Token                    //||
    {
        public override string ToString()
        {
            return "BAR BAR";
        }
    }

    public class tEqual : Token
    {
        public override string ToString()
        {
            return "EQUAL";
        }
    }

    public class tMultEqual : Token
    {
        public override string ToString()
        {
            return "MULT EQUAL";
        }
    }

    public class tSlashEqual : Token
    {
        public override string ToString()
        {
            return "SLASH EQUAL";
        }
    }

    public class tPercentEqual : Token
    {
        public override string ToString()
        {
            return "PERCENT EQUAL";
        }
    }

    public class tPlusEqual : Token
    {
        public override string ToString()
        {
            return "PLUS EQUAL";
        }
    }

    public class tMinusEqual : Token
    {
        public override string ToString()
        {
            return "MINUS EQUAL";
        }
    }

    public class tLShiftEqual : Token
    {
        public override string ToString()
        {
            return "LEFT SHIFT EQUAL";
        }
    }

    public class tRShiftEqual : Token
    {
        public override string ToString()
        {
            return "RIGHT SHIFT EQUAL";
        }
    }

    public class tAmpEqual : Token
    {
        public override string ToString()
        {
            return "AMP EQUAL";
        }
    }

    public class tCaretEqual : Token
    {
        public override string ToString()
        {
            return "CARET EQUAL";
        }
    }

    public class tBarEqual : Token
    {
        public override string ToString()
        {
            return "BAR EQUAL";
        }
    }
}
