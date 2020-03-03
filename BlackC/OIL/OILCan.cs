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

using Origami.ENAML;

namespace Origami.OIL
{
    class OILCan
    {
        public string filename;
        EnamlData oilcan;
        public int nodenum;

        public OILCan(String _filename)
        {
            filename = _filename;
            oilcan = null;
            nodenum = 0;
        }

        //- reading in --------------------------------------------------------

        public Module load(String filename, String modname)
        {
            Module module = new Module(modname);
            oilcan = EnamlData.loadFromFile(filename);

            //root.decls.Add(new VarDeclar("i", "int"));
            //root.decls.Add(new VarDeclar("j", "int"));
            //root.decls.Add(new VarDeclar("k", "int"));

            //root.statements.Add(new AssignStmt(new Identifier("i"), new PrimaryIntConst(2)));
            //root.statements.Add(new AssignStmt(new Identifier("j"), new PrimaryIntConst(3)));
            //root.statements.Add(new AssignStmt(new Identifier("k"), new AddOpNode(new PrimaryId("i"), new PrimaryId("j"))));
            //root.statements.Add(new PrintVarStmt(new Identifier("k")));

            return module;
        }

        //- writing out -------------------------------------------------------

        public void save(Module module)
        {
            nodenum = 0;
            generateOILNames(module);

            oilcan = new EnamlData();
            oilcan.setStringValue("OILCan.version", "0.20");
            oilcan.setStringValue("module.name", module.name);

            int fnum = 0;
            foreach (FuncDefNode func in module.funcs)
            {
                saveFuncDef(fnum++, func);
            }

            oilcan.saveToFile(filename);
        }

        //name what needs to be named
        public void generateOILNames(Module module)
        {
            for (int tnum = 0; tnum < module.typedefs.Count; tnum++)
            {
                module.typedefs[tnum].OILname = "typedef" + tnum.ToString();
            }

            for (int gnum = 0; gnum < module.globals.Count; gnum++)
            {
                module.globals[gnum].OILname = "global" + gnum.ToString();
            }

            for (int fnum = 0; fnum < module.funcs.Count; fnum++)
            {
                module.funcs[fnum].OILname = "func" + fnum.ToString();
                generateFunctionNames(module.funcs[fnum]);
            }
        }

        public void generateFunctionNames(FuncDefNode func)
        {
            for (int pnum = 0; pnum < func.paramList.Count; pnum++)
            {
                func.paramList[pnum].OILname = "param" + pnum.ToString();
            }

            for (int lnum = 0; lnum < func.locals.Count; lnum++)
            {
                func.locals[lnum].OILname = "local" + lnum.ToString();
            }
        }

        //- declarations --------------------------------------------

        public void saveFuncDef(int fnum, FuncDefNode func)
        {
            string fname = "module." + func.OILname;

            oilcan.setStringValue(fname + ".name", func.name);
            saveTypeDecl(fname + ".return-type", func.returnType);

            string pname = fname + ".param";
            int pnum = 0;
            foreach (ParamDeclNode param in func.paramList)
            {
                saveParamDecl(pname + pnum.ToString(), param);
                pnum++;
            }

            string lname = fname + ".local";
            int lnum = 0;
            foreach (VarDeclNode local in func.locals)
            {
                saveVarDecl(lname + lnum.ToString(), local);
                lnum++;
            }

            string bname = fname + ".body";
            saveStatementList(bname, func.body);
        }

        public void saveTypeDecl(string path, TypeDeclNode typdef)
        {
            oilcan.setStringValue(path, typdef.name);
        }

        public void saveParamDecl(string path, ParamDeclNode param)
        {
            oilcan.setStringValue(path + ".name", param.name);
            saveTypeDecl(path + ".type", param.pType);
        }

        public void saveVarDecl(string path, VarDeclNode v)
        {
            oilcan.setStringValue(path + ".name", v.name);
            saveTypeDecl(path + ".type", v.varType);            
        }

        //- statements ------------------------------------

        public void saveStatementList(string path, List<StatementNode> stmts)
        {
            if (stmts.Count == 0)
            {
                oilcan.setStringValue(path, "empty");
                return;
            }

            int stmtnum = 0;
            foreach (StatementNode stmt in stmts)
            {
                switch (stmt.type)
                {
                    case OILType.DeclarationStmt:
                        saveDeclarationStmt(path, (DeclarInitStatementNode)stmt, stmtnum);
                        break;

                    case OILType.ExpressionStmt:
                        saveExpressionStmt(path, (ExpressionStatementNode)stmt, stmtnum);
                        break;

                    case OILType.ForStmt:
                        saveForStmt(path, (ForStatementNode)stmt, stmtnum);
                        break;

                    case OILType.ReturnStmt:
                        saveReturnStmt(path, (ReturnStatementNode)stmt, stmtnum);
                        break;

                    default:
                        break;
                }
                stmtnum++;
            }
        }

        public void saveDeclarationStmt(string path, DeclarInitStatementNode stmt, int stmtnum)
        {
            string spath = path + ".decl-init-stmt" + stmtnum.ToString();
            oilcan.setStringValue(spath + ".var", stmt.decl.OILname);
            saveExpression(spath + ".expr", stmt.initexpr);
        }

        public void saveExpressionStmt(string path, ExpressionStatementNode stmt, int stmtnum)
        {
            string spath = path + ".expr-stmt" + stmtnum.ToString();
            saveExpression(spath + ".expr", stmt.expr);
        }

        public void saveForStmt(string path, ForStatementNode stmt, int stmtnum)
        {
            string spath = path + ".for-stmt" + stmtnum.ToString();
            saveStatementList(spath + ".decl1", stmt.decl1);
            saveExpression(spath + ".expr1", stmt.expr1);
            saveExpression(spath + ".expr2", stmt.expr2);
            saveExpression(spath + ".expr3", stmt.expr3);
            saveStatementList(spath + ".body", stmt.body);
        }

        public void saveReturnStmt(string path, ReturnStatementNode stmt, int stmtnum)
        {
            string spath = path + ".return-stmt" + stmtnum.ToString();
            if (stmt.retval != null)
            {
                saveExpression(spath + ".expr", stmt.retval);
            }
            else
            {
                oilcan.setStringValue(spath + ".expr", "none");
            }
        }

        //- expressions -----------------------------------

        public void saveExpression(string path, ExprNode expr)
        {
            if (expr == null)
            {
                return;
            }

            switch (expr.type)
            {
                case OILType.IdentExpr:
                    IdentExprNode idexpr = (IdentExprNode)expr;
                    oilcan.setStringValue(path + ".ident-expr", idexpr.idsym.OILname);
                    break;

                case OILType.IntConst:
                    IntConstant iconst = (IntConstant)expr;
                    oilcan.setIntValue(path + ".int-const", iconst.value);
                    break;

                case OILType.FloatConst:
                    FloatConstant fconst = (FloatConstant)expr;
                    oilcan.setFloatValue(path + ".float-const", fconst.value);
                    break;

                case OILType.ArithmeticExpr:
                    ArithmeticExprNode arithexpr = (ArithmeticExprNode)expr;
                    saveExpression(path + ".lhs", arithexpr.lhs);
                    oilcan.setStringValue(path + ".op", ArithmeticExprNode.opname[(int)arithexpr.op]);
                    saveExpression(path + ".rhs", arithexpr.rhs);
                    break;

                case OILType.CompareExpr:
                    ComparisonExprNode compexpr = (ComparisonExprNode)expr;
                    saveExpression(path + ".lhs", compexpr.lhs);
                    oilcan.setStringValue(path + ".op", ComparisonExprNode.opname[(int)compexpr.op]);
                    saveExpression(path + ".rhs", compexpr.rhs);
                    break;

                case OILType.AssignExpr:
                    AssignExprNode assignexpr = (AssignExprNode)expr;
                    saveExpression(path + ".lhs", assignexpr.lhs);
                    oilcan.setStringValue(path + ".op", AssignExprNode.opname[(int)assignexpr.op]);
                    saveExpression(path + ".rhs", assignexpr.rhs);
                    break;

                default:
                    break;
            }
        }
    }
}
