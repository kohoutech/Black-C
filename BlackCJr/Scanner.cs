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
    class Scanner
    {
        string sourceName;
        Preprocessor pp;

        public Scanner(string _sourceName)
        {
            sourceName = _sourceName;
            pp = new Preprocessor(sourceName);
        }

        public Token getToken()
        {
            while (pp.fragtype != FragType.EOF)
            {
                string frag = pp.getFrag();
                switch (pp.fragtype)
                {
                    case FragType.WORD:
                        Console.Out.WriteLine("word - {0}", frag);
                        break;
                    case FragType.NUMBER:
                        Console.Out.WriteLine("number - {0}", frag);
                        break;
                    case FragType.CHAR:
                        Console.Out.WriteLine("char - {0}", frag);
                        break;
                    case FragType.SPACE:
                        Console.Out.WriteLine("space - {0}", frag);
                        break;
                }

            }
            return new Token();
        }
    }
}
