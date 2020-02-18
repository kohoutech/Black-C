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

namespace BlackC
{
    public enum SYMTYPE { DECLAR, FIELD, STRUCT, ENUM, LABEL, TYPEDEF, UNSET };

    public class SymbolTable
    {

        //public Dictionary<String, List<IdentNode>> symbols;
        //public SymbolTable parent;

        //public SymbolTable(SymbolTable _parent)
        //{
        //    symbols = new Dictionary<String, List<IdentNode>>();
        //    parent = _parent;
        //}

        //public static IdentNode findSymbol(SymbolTable symtbl, String ident, SYMTYPE symtype) 
        //{
        //    IdentNode node = null;
        //    bool found = false;
        //    SymbolTable curTable = symtbl;
        //    while ((curTable != null) && (!found))
        //    {
        //        if (curTable.symbols.ContainsKey(ident))
        //        {
        //            List<IdentNode> defs = curTable.symbols[ident];
        //            foreach (IdentNode identnode in defs)
        //            {
        //                if (identnode.symtype == symtype)
        //                {
        //                    node = identnode;
        //                    found = true;
        //                }
        //            }
        //        }
        //        if (!found)
        //        {
        //            curTable = curTable.parent;
        //        }
        //    }
        //    return node;
        //}

        //public static IdentNode addSymbol(SymbolTable symtbl, String ident)
        //{
        //    IdentNode node = new IdentNode(ident);
        //    List<IdentNode> defs;
        //    if (symtbl.symbols.ContainsKey(ident))
        //    {
        //        defs = symtbl.symbols[ident];                
        //    }
        //    else
        //    {
        //        defs = new List<IdentNode>();
        //        symtbl.symbols.Add(ident, defs);
        //    }
        //    defs.Add(node);
        //    return node;
        //}

        //public static void deleteSymbol(SymbolTable symtbl, IdentNode node)
        //{
        //    if (symtbl.symbols.ContainsKey(node.ident))
        //    {
        //        List<IdentNode> defs = symtbl.symbols[node.ident];
        //        defs.Remove(node);
        //    }
        //}
    }
}
