/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2020  George E Greaney

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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kohoutech.OIL;

namespace BlackC.Parse
{
    public enum SYMTYPE { DECLAR, FIELD, STRUCT, ENUM, LABEL, TYPEDEF, UNSET };

    public class SymbolTable
    {
        public List<SymTable> symbolStack;
        public SymTable global;

        public SymbolTable()
        {
            symbolStack = new List<SymTable>();
            global = new SymTable();
            symbolStack.Add(global);
        }

        public void enterScope()
        {
            SymTable symtbl = new SymTable();
            symbolStack.Add(symtbl);
        }

        public void exitscope()
        {
            if (symbolStack.Count > 1)          //don't remove the global symbol tbl
            {
                symbolStack.RemoveAt(symbolStack.Count - 1);
            }
        }

        public OILNode findSymbol(String ident)
        {
            OILNode node = null;
            int i = symbolStack.Count - 1;
            bool found = false;
            while (!found && (i >= 0))
            {
                SymTable symtbl = symbolStack[i--];
                node = symtbl.findSym(ident);
                found = (node != null);
            }
            return node;
        }

        public void addSymbol(String ident, OILNode def)
        {
            symbolStack[symbolStack.Count - 1].addSym(ident, def);
        }

        //inner class, representing a single symbol table
        public class SymTable
        {
            Dictionary<String, OILNode> symtbl;

            public SymTable()
            {
                symtbl = new Dictionary<string, OILNode>();
            }

            public OILNode findSym(String id)
            {
                OILNode def = null;
                if (symtbl.ContainsKey(id))
                {
                    def = symtbl[id];
                }
                return def;
            }

            public void addSym(String id, OILNode def)
            {
                symtbl[id] = def;
            }
        }
    }
}
