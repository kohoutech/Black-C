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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BlackC.Lexer;
using Origami.OIL;

namespace BlackC
{
    public class ParseNode : OILNode
    {
    }

   //- external defs ---------------------------------------------------------

   // public class TranslationUnit : ParseNode
   // {
   //     Arbor arbor;
   //     List<ParseNode> defs;

   //     public TranslationUnit(Arbor _arbor)
   //     {
   //         arbor = _arbor;
   //         defs = new List<ParseNode>();
   //         arbor.pushSymbolTable();                //global symbol tbl
   //     }

   //     public void addFunctionDef(FunctionDefNode func)
   //     {
   //         defs.Add(func);
   //         Console.WriteLine("parsed function " + defs.Count);
   //     }

   //     public void addDeclaration(DeclarationNode declar)
   //     {
   //         defs.Add(declar);
   //         Console.WriteLine("parsed declaration " + defs.Count);
   //     }

   //     public void write()
   //     {
   //         Console.WriteLine("done parsing");
   //     }
   // }

   // class FunctionDeclarNode : ParseNode
   // {
   // }

   // public class FunctionDefNode : ParseNode
   // {
   //     public DeclarSpecNode specs;
   //     public DeclaratorNode signature;
   //     public List<DeclarationNode> oldparamlist;
   //     public StatementNode block;
   //     private DeclarationNode declars;

   //     public FunctionDefNode(DeclarSpecNode _specs, DeclaratorNode _sig, List<DeclarationNode> _oldparams, StatementNode _block)
   //     {
   //         specs = _specs;
   //         signature = _sig;
   //         oldparamlist = _oldparams;
   //         block = _block;
   //     }

   //     public FunctionDefNode(DeclarationNode declars)
   //     {
   //         // TODO: Complete member initialization
   //         this.declars = declars;
   //     }

   //     internal void setOldParams(List<DeclarationNode> oldparamlist)
   //     {
   //         throw new NotImplementedException();
   //     }

   //     internal void setFuncBody(StatementNode block)
   //     {
   //         throw new NotImplementedException();
   //     }
    //}
}

//Console.WriteLine("There's no sun in the shadow of the wizard");