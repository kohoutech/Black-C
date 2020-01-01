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
    class Parser
    {
        string sourceName;
        Tokenizer tokenizer;

        public Parser(string _sourceName)
        {
            sourceName = _sourceName;
            tokenizer = new Tokenizer(sourceName);
        }

        public Program parseProgram()
        {
            Program prog = new Program();
            prog.func = parseFunctionDecl();
            return prog;
        }

        public FunctionDecl parseFunctionDecl()
        {
            FunctionDecl func = new FunctionDecl();
            Token tok = tokenizer.getToken();         //int
            tok = tokenizer.getToken();               //main
            func.name = tok.ident;
            tok = tokenizer.getToken();               //(
            tok = tokenizer.getToken();               //)
            tok = tokenizer.getToken();               //{
            func.stmt = parseReturnStatement();     //return 2;
            tok = tokenizer.getToken();               //}
            return func;
        }

        public ReturnStmt parseReturnStatement()
        {
            ReturnStmt stmt = new ReturnStmt();
            Token tok = tokenizer.getToken();         //return
            stmt.expr = parseExpression();          //2
            tok = tokenizer.getToken();               //;
            return stmt;
        }

        public Expression parseExpression()
        {
            Expression expr = new Expression();
            expr.retval = parseIntConstant();       //2
            return expr;
        }

        public IntConstant parseIntConstant()
        {
            IntConstant intconst = new IntConstant();
            Token tok = tokenizer.getToken();
            intconst.value = tok.intval;
            return intconst;
        }

        public void printAST(Program root)
        {
            root.printNode(0);
        }
    }
}
