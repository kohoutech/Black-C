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
    public enum TokenType
    {
        tEOF,
        tIDENTIFIER,

        //keywords
        tAUTO,
        tBREAK,
        tCASE,
        tCHAR,
        TCONST,
        tCONTINUE,
        tDEFAULT,
        tDO,
        tDOUBLE,
        tELSE,
        tENUM,
        tEXTERN,
        tFLOAT,
        tFOR,
        tGOTO,
        tIF,
        tINLINE,
        tINT,
        tLONG,
        tREGISTER,
        tRESTRICT,
        tRETURN,
        tSHORT,
        tSIGNED,
        tSIZEOF,
        tSTATIC,
        tSTRUCT,
        tSWITCH,
        tTYPEDEF,
        tUNION,
        tUNSIGNED,
        tVOID,
        tVOLATILE,
        tWHILE,

        //constants
        tINTCONST,
        tFLOATCONST,
        tCHARCONST,
        tSTRINGCONST,

        //punctuation
        tLPAREN,
        tRPAREN,
        tLBRACE,
        tRBRACE,
        tLBRACKET,
        tRBRACKET,
        tPERIOD,
        tCOMMA,
        tCOLON,
        tSEMICOLON,
        tASTERISK,
        tELLIPSIS,
        tARROW,
        tAMPERSAND,
        tTILDE,
        tEXCLAIM,
        tQUESTION,
        tPLUS,
        tMINUS,
        tPLUSPLUS,
        tMINUSMINUS,
        tSLASH,
        tPERCENT,
        tLEFTSHIFT,
        tRIGHTSHIFT,
        tLESSTHAN,
        tGTRTHAN,
        tLESSEQUAL,
        tGTREQUAL,
        tEQUALEQUAL,
        tNOTEQUAL,
        tCARET,
        tBAR,
        tDOUBLEAMP,
        tDOUBLEBAR,
        tEQUAL,
        tMULTEQUAL,
        tSLASHEQUAL,
        tPERCENTEQUAL,
        tPLUSEQUAL,
        tMINNUSEQUAL,
        tLSHIFTEQUAL,
        tRSHIFTEQUAL,
        tAMPEQUAL,
        tCARETEQUAL,
        tBAREQUAL
    }

    public class Token
    {
        public TokenType type;
        public String ident;

        public Token(TokenType _type)
        {
            type = _type;
        }
    }
}