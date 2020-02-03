/* ----------------------------------------------------------------------------
LibOriOIL - a library for working with Origami Internal Language
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

namespace Origami.OIL
{
    //base class
    public class OILNode
    {
        public OILNodeType type;

        public virtual void printNode(int indent)
        {
        }

        public void printIndention(int indent)
        {
            for (int i = 0; i < indent; i++)
                Console.Out.Write(' ');
        }
    }

    public class VarDecl : OILNode
    {
        public VarDecl()
        {
            type = OILNodeType.VarDecl; 
        }
    }

    public class FunctionDecl : OILNode
    {
        public String name;
        public ReturnStmt stmt;

        public FunctionDecl()
        {
            type = OILNodeType.FuncDecl;
        }

        public override void printNode(int indent)
        {
            printIndention(indent);
            Console.Out.WriteLine("<function {0}>", name);
            printIndention(indent);
            Console.Out.WriteLine("  params: ()");
            printIndention(indent);
            Console.Out.WriteLine("  body:");
            stmt.printNode(indent + 2);
        }
    }

    public class ReturnStmt : OILNode
    {
        public Expression expr;

        public override void printNode(int indent)
        {
            printIndention(indent);
            Console.Out.WriteLine("<return statement>");
            expr.printNode(indent + 2);
        }
    }

    public class Expression : OILNode
    {
        public IntConstant retval;

        public override void printNode(int indent)
        {
            printIndention(indent);
            Console.Out.WriteLine("<expression>");
            retval.printNode(indent + 2);
        }
    }

    public class IntConstant : OILNode
    {
        public int value;

        public override void printNode(int indent)
        {
            printIndention(indent);
            Console.Out.WriteLine("<int constant {0}>", value);
        }
    }

    //-------------------------------------------------------------------------

    public class Module
    {
        public List<VarDecl> globals;
        public List<FunctionDecl> funcs;

        public Module()
        {
            globals = new List<VarDecl>();
            funcs = new List<FunctionDecl>();
        }

        public void write()
        {
            //throw new NotImplementedException();
        }
    }

    //-------------------------------------------------------------------------

    public enum OILNodeType
    {
        VarDecl,
        FuncDecl
    }
}
