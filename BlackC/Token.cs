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
            return "IDENTIFIER";
        }
    }

    class tIntegerConstant : Token
    {
        int val;

        public tIntegerConstant(int _val)
            : base()
        {
            val = _val;
        }

        public override string ToString()
        {
            return "INTEGER CONSTANT";
        }
    }

    class tFloatConstant : Token
    {
        double val;

        public tFloatConstant(double _val)
            : base()
        {
            val = _val;
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
            return "STRING CONSTANT";
        }
    }

    //- punctuation -----------------------------------------------------------

    class tLBracket : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tRBracket : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tLParen : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tRParen : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tLBrace : Token
    {
    }

    class tRBrace : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tPeriod : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tArrow : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tPlusPlus : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tMinusMinus : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tAmpersand : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tAsterisk : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tPlus : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tMinus : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tTilde : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tExclaim : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tSlash : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tPercent : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tLeftShift : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tRightShift : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tLessThan : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tGtrThan : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tLessEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tGtrEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tEqualEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tNotEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tCaret : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tBar : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tDoubleAmp : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tDoubleBar : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tQuestion : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tColon : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tSemicolon : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tEllipsis : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tMultEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tSlashEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tPercentEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tPlusEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tMinusEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tLShiftEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tRShiftEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tAmpEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tCaretEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
        }
    }

    class tBarEqual : Token
    {
        public override string ToString()
        {
            return "COMMA";
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
