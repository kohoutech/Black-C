/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2018  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your [opt]ion) any later version.

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
using System.IO;

using Origami.AST;

//arbor - a place where trees are grown

namespace BlackC
{
    class Arbor
    {
        Dictionary<string, int> typepdefids;

        public Arbor()
        {
            typepdefids = new Dictionary<string, int>();
        }

        //temproary kludge to get around ambiguity in C99's grammar between typedef and identifier 
        //see https://en.wikipedia.org/wiki/The_lexer_hack
        //this will be removed once the rest of the semantic analysis is up & running and this is not needed anymore
        //crazy eh?

        public bool isTypedef(String id)
        {
            bool result = false;
            if (typepdefids.ContainsKey(id))
            {
                result = true;
            }
            return result;
        }

        internal void buildFunctionDef()
        {
            throw new NotImplementedException();
        }

        internal void setTypeDef(string typeid)
        {
            typepdefids[typeid] = 0;
        }

        internal void unsetTypeDef(string typeid)
        {
            typepdefids.Remove(typeid);
        }

        internal ExprNode makeIntegerConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal ExprNode makeFloatConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal ExprNode makeCharConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal ExprNode makeStringConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal EnumeratorNode makeEnumeratorNode(EnumConstantNode enumconst, ConstExprNode constexpr)
        {
            throw new NotImplementedException();
        }
    }
}
