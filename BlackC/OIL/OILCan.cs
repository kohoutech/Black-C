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
        const string OILCANVERSION = "0.2.0";

        public string filename;
        EnamlData oilcan;
        public int nodenum;

        public OILCan(String _filename)
        {
            filename = _filename;
            oilcan = null;
            nodenum = 0;
        }

        //---------------------------------------------------------------------
        // READING IN
        //---------------------------------------------------------------------

        public Dictionary<string, Declaration> globalSymbols;
        public Dictionary<string, Declaration> localSymbols;

        public Module load()
        {
            Module module = null;
            try
            {
                globalSymbols = new Dictionary<string, Declaration>();
                localSymbols = new Dictionary<string, Declaration>();

                oilcan = EnamlData.loadFromFile(filename);

                string oilVersion = oilcan.getStringValue("OILCan.version", "");
                string modname = oilcan.getStringValue("module.name", "");

                module = new Module(modname);

                List<String> funcs = oilcan.getPathKeys("module.funcs");
                foreach (String funcname in funcs)
                {
                    FuncDefNode func = loadFuncDef("module.funcs." + funcname);
                    module.funcs.Add(func);
                    globalSymbols[funcname] = func;
                }
            }
            catch (ENAMLException e)
            {
                return null;
            }

            return module;
        }

        public Declaration loadSymbolRef(string path)
        {
            Declaration node = null;
            string name = oilcan.getStringValue(path, "");
            if (name[0] == 'g' || name[0] == 'f')
            {
                node = globalSymbols[name];
            }
            else if (name[0] == 'p' || name[0] == 'l')
            {
                node = localSymbols[name];
            }
            return node;
        }

        //- reading declarations ------------------------------------

        public FuncDefNode loadFuncDef(String path)
        {
            localSymbols.Clear();
            FuncDefNode func = new FuncDefNode();
            func.name = oilcan.getStringValue(path + ".name", "");
            func.returnType = loadTypeDecl(path + ".return-type");

            List<String> paramnames = oilcan.getPathKeys(path + ".params");
            foreach (String pname in paramnames)
            {
                ParamDeclNode p = loadParamDecl(path + ".params." + pname);
                func.paramList.Add(p);
                localSymbols[pname] = p;
            }

            List<String> localnames = oilcan.getPathKeys(path + ".locals");
            foreach (String lname in localnames)
            {
                VarDeclNode l = loadVarDecl(path + ".locals." + lname);
                func.locals.Add(l);
                localSymbols[lname] = l;
            }

            func.body = loadStatementList(path + ".body");

            return func;
        }

        public TypeDeclNode loadTypeDecl(string path)
        {
            string tname = oilcan.getStringValue(path, "");
            TypeDeclNode tnode = new TypeDeclNode(tname);
            return tnode;
        }

        public ParamDeclNode loadParamDecl(string path)
        {
            ParamDeclNode pnode = new ParamDeclNode();
            pnode.name = oilcan.getStringValue(path + ".name", "");
            pnode.pType = loadTypeDecl(path + ".type");
            return pnode;
        }

        public VarDeclNode loadVarDecl(string path)
        {
            VarDeclNode vnode = new VarDeclNode();
            vnode.name = oilcan.getStringValue(path + ".name", "");
            vnode.varType = loadTypeDecl(path + ".type");
            return vnode;
        }

        //- reading statements --------------------------------------

        public List<StatementNode> loadStatementList(string path)
        {
            List<String> stmtnames = oilcan.getPathKeys(path);
            List<StatementNode> stmts = new List<StatementNode>();
            foreach (String sname in stmtnames)
            {
                StatementNode v = loadStatement(path + "." + sname);
                stmts.Add(v);
            }
            return stmts;
        }

        public StatementNode loadStatement(string path)
        {
            StatementNode snode = null;
            string stmttype = oilcan.getStringValue(path + ".type", "");
            switch (stmttype)
            {
                case "decl-init":
                    snode = loadDeclarationStmt(path);
                    break;

                case "expr-stmt":
                    snode = loadExpressionStmt(path);
                    break;

                case "for-stmt":
                    snode = loadForStmt(path);
                    break;

                case "ret-stmt":
                    snode = loadReturnStmt(path);
                    break;

                default:
                    break;
            }
            return snode;
        }

        public DeclarInitStatementNode loadDeclarationStmt(string path)
        {
            VarDeclNode decl = (VarDeclNode)loadSymbolRef(path + ".var");
            ExprNode expr = loadExpression(path + ".expr");
            DeclarInitStatementNode snode = new DeclarInitStatementNode(decl, expr);
            return snode;
        }

        public ExpressionStatementNode loadExpressionStmt(string path)
        {
            ExprNode expr = loadExpression(path + ".expr");
            ExpressionStatementNode snode = new ExpressionStatementNode(expr);
            return snode;
        }

        public ForStatementNode loadForStmt(string path)
        {
            List<StatementNode> decl1 = loadStatementList(path + ".decl1");
            ExprNode expr1 = loadExpression(path + ".expr1");
            ExprNode expr2 = loadExpression(path + ".expr2");
            ExprNode expr3 = loadExpression(path + ".expr3");
            List<StatementNode> body = loadStatementList(path + ".body");
            ForStatementNode snode = new ForStatementNode(decl1, expr1, expr2, expr3, body);
            return snode;            
        }

        public ReturnStatementNode loadReturnStmt(string path)
        {
            ExprNode expr = loadExpression(path + ".expr");
            ReturnStatementNode snode = new ReturnStatementNode(expr);
            return snode;
        }

        //- reading expressions -------------------------------------

        public ExprNode loadExpression(string path)
        {
            ExprNode enode = null;
            string exprtype = oilcan.getStringValue(path + ".type", "");
            switch (exprtype)
            {
                case "ident-expr":
                    OILNode idsym = loadSymbolRef(path + ".ref");
                    enode = new IdentExprNode(idsym);
                    break;

                case "int-const":
                    int intval = oilcan.getIntValue(path + ".val", 0);
                    enode = new IntConstant(intval);
                    break;

                case "float-const":
                    double dblval = oilcan.getFloatValue(path + ".val", 0.0);
                    enode = new FloatConstant(dblval);
                    break;

                case "arith-expr":
                    enode = loadArithmeticExpression(path);
                    break;

                case "comp-expr":
                    enode = loadComparisionExpression(path);
                    break;

                case "assign-expr":
                    enode = loadAssignmentExpression(path);
                    break;

                default:
                    break;
            }
            return enode;    
        }

        public ArithmeticExprNode loadArithmeticExpression(string path)
        {
            string opstr = oilcan.getStringValue(path + ".op", "");
            ArithmeticExprNode.OPERATOR op = ArithmeticExprNode.OPERATOR.PLUS;
            switch (opstr)
            {
                case "plus":
                    op = ArithmeticExprNode.OPERATOR.PLUS;
                    break;

                case "minus": 
                    op = ArithmeticExprNode.OPERATOR.MINUS;
                    break;

                case "inc": 
                    op = ArithmeticExprNode.OPERATOR.INC;
                    break;

                case "dec":                     
                    op = ArithmeticExprNode.OPERATOR.DEC;
                    break;

                case "add": 
                    op = ArithmeticExprNode.OPERATOR.ADD;
                    break;

                case "sub": 
                    op = ArithmeticExprNode.OPERATOR.SUB;
                    break;

                case "mult":
                    op = ArithmeticExprNode.OPERATOR.MULT;
                    break;

                case "div":
                    op = ArithmeticExprNode.OPERATOR.DIV;
                    break;

                case "mod":
                    op = ArithmeticExprNode.OPERATOR.MOD;
                    break;

                default:
                        break;
            }
            ExprNode lhs = loadExpression(path + ".lhs");
            ExprNode rhs = loadExpression(path + ".rhs");
            ArithmeticExprNode enode = new ArithmeticExprNode(op, lhs, rhs);
            return enode;
        }

        public ComparisonExprNode loadComparisionExpression(string path)
        {
            string opstr = oilcan.getStringValue(path + ".op", "");
            ComparisonExprNode.OPERATOR op = ComparisonExprNode.OPERATOR.EQUAL;
            switch (opstr)
            {
                case "equal":
                    op = ComparisonExprNode.OPERATOR.EQUAL;
                    break;

                case "notequal":
                    op = ComparisonExprNode.OPERATOR.NOTEQUAL;
                    break;

                case "lessthan":
                    op = ComparisonExprNode.OPERATOR.LESSTHAN;
                    break;

                case "gtrthan":
                    op = ComparisonExprNode.OPERATOR.GTRTHAN;
                    break;

                case "lessequal":
                    op = ComparisonExprNode.OPERATOR.LESSEQUAL;
                    break;

                case "gtrequal":
                    op = ComparisonExprNode.OPERATOR.GTREQUAL;
                    break;

                default:
                    break;
            }
            ExprNode lhs = loadExpression(path + ".lhs");
            ExprNode rhs = loadExpression(path + ".rhs");
            ComparisonExprNode enode = new ComparisonExprNode(op, lhs, rhs);
            return enode;
        }

        public AssignExprNode loadAssignmentExpression(string path)
        {
            string opstr = oilcan.getStringValue(path + ".op", "");
            AssignExprNode.OPERATOR op = AssignExprNode.OPERATOR.EQUAL;
            switch (opstr)
            {
                case "equal":
                    op = AssignExprNode.OPERATOR.EQUAL;
                    break;

                case "multequal":
                    op = AssignExprNode.OPERATOR.MULTEQUAL;
                    break;

                case "divequal":
                    op = AssignExprNode.OPERATOR.DIVEQUAL;
                    break;

                case "modequal":
                    op = AssignExprNode.OPERATOR.MODEQUAL;
                    break;

                case "addequal":
                    op = AssignExprNode.OPERATOR.ADDEQUAL;
                    break;

                case "subequal":
                    op = AssignExprNode.OPERATOR.SUBEQUAL;
                    break;

                case "lshiftequal":
                    op = AssignExprNode.OPERATOR.LSHIFTEQUAL;
                    break;

                case "rshiftequal":
                    op = AssignExprNode.OPERATOR.RSHIFTEQUAL;
                    break;

                case "andequal":
                    op = AssignExprNode.OPERATOR.ANDEQUAL;
                    break;

                case "xorequal":
                    op = AssignExprNode.OPERATOR.XOREQUAL;
                    break;

                case "orequal":
                    op = AssignExprNode.OPERATOR.OREQUAL;
                    break;

                default:
                    break;
            }
            ExprNode lhs = loadExpression(path + ".lhs");
            ExprNode rhs = loadExpression(path + ".rhs");
            AssignExprNode enode = new AssignExprNode(op, lhs, rhs);
            return enode;
        }

        //---------------------------------------------------------------------
        // WRITING OUT
        //---------------------------------------------------------------------

        //these should mirror the loading methods above

        public void save(Module module)
        {
            nodenum = 0;
            generateOILNames(module);

            oilcan = new EnamlData();
            oilcan.setStringValue("OILCan.version", OILCANVERSION);
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
                module.typedefs[tnum].OILname = "t" + tnum.ToString();
            }

            for (int gnum = 0; gnum < module.globals.Count; gnum++)
            {
                module.globals[gnum].OILname = "g" + gnum.ToString();
            }

            for (int fnum = 0; fnum < module.funcs.Count; fnum++)
            {
                module.funcs[fnum].OILname = "f" + fnum.ToString();
                generateFunctionNames(module.funcs[fnum]);
            }
        }

        public void generateFunctionNames(FuncDefNode func)
        {
            for (int pnum = 0; pnum < func.paramList.Count; pnum++)
            {
                func.paramList[pnum].OILname = "p" + pnum.ToString();
            }

            for (int lnum = 0; lnum < func.locals.Count; lnum++)
            {
                func.locals[lnum].OILname = "l" + lnum.ToString();
            }
        }

        //- writing declarations ------------------------------------

        public void saveFuncDef(int fnum, FuncDefNode func)
        {
            string fname = "module.funcs." + func.OILname;

            oilcan.setStringValue(fname + ".name", func.name);
            saveTypeDecl(fname + ".return-type", func.returnType);

            string pname = fname + ".params.";
            int pnum = 0;
            foreach (ParamDeclNode param in func.paramList)
            {
                saveParamDecl(pname + param.OILname, param);
                pnum++;
            }

            string lname = fname + ".locals.";
            int lnum = 0;
            foreach (VarDeclNode local in func.locals)
            {
                saveVarDecl(lname + local.OILname, local);
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

        //- writing statements --------------------------------------

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
                string spath = path + ".s" + stmtnum.ToString();
                switch (stmt.type)
                {
                    case OILType.DeclarationStmt:
                        saveDeclarationStmt(spath, (DeclarInitStatementNode)stmt);
                        break;

                    case OILType.ExpressionStmt:
                        saveExpressionStmt(spath, (ExpressionStatementNode)stmt);
                        break;

                    case OILType.ForStmt:
                        saveForStmt(spath, (ForStatementNode)stmt);
                        break;

                    case OILType.ReturnStmt:
                        saveReturnStmt(spath, (ReturnStatementNode)stmt);
                        break;

                    default:
                        break;
                }
                stmtnum++;
            }
        }

        public void saveDeclarationStmt(string path, DeclarInitStatementNode stmt)
        {
            oilcan.setStringValue(path + ".type", "decl-init");
            oilcan.setStringValue(path + ".var", stmt.decl.OILname);
            saveExpression(path + ".expr", stmt.initexpr);
        }

        public void saveExpressionStmt(string path, ExpressionStatementNode stmt)
        {
            oilcan.setStringValue(path + ".type", "expr-stmt");
            saveExpression(path + ".expr", stmt.expr);
        }

        public void saveForStmt(string path, ForStatementNode stmt)
        {
            oilcan.setStringValue(path + ".type", "for-stmt");
            saveStatementList(path + ".decl1", stmt.decl1);
            saveExpression(path + ".expr1", stmt.expr1);
            saveExpression(path + ".expr2", stmt.expr2);
            saveExpression(path + ".expr3", stmt.expr3);
            saveStatementList(path + ".body", stmt.body);
        }

        public void saveReturnStmt(string path, ReturnStatementNode stmt)
        {
            oilcan.setStringValue(path + ".type", "ret-stmt");
            if (stmt.retval != null)
            {
                saveExpression(path + ".expr", stmt.retval);
            }
        }

        //- writing expressions -------------------------------------

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
                    oilcan.setStringValue(path + ".type", "ident-expr");
                    oilcan.setStringValue(path + ".ref", idexpr.idsym.OILname);
                    break;

                case OILType.IntConst:
                    IntConstant iconst = (IntConstant)expr;
                    oilcan.setStringValue(path + ".type", "int-const");
                    oilcan.setIntValue(path + ".val", iconst.value);
                    break;

                case OILType.FloatConst:
                    FloatConstant fconst = (FloatConstant)expr;
                    oilcan.setStringValue(path + ".type", "float-const");
                    oilcan.setFloatValue(path + ".val", fconst.value);
                    break;

                case OILType.ArithmeticExpr:
                    ArithmeticExprNode arithexpr = (ArithmeticExprNode)expr;
                    oilcan.setStringValue(path + ".type", "arith-expr");
                    saveExpression(path + ".lhs", arithexpr.lhs);
                    oilcan.setStringValue(path + ".op", ArithmeticExprNode.opname[(int)arithexpr.op]);
                    saveExpression(path + ".rhs", arithexpr.rhs);
                    break;

                case OILType.CompareExpr:
                    ComparisonExprNode compexpr = (ComparisonExprNode)expr;
                    oilcan.setStringValue(path + ".type", "comp-expr");
                    saveExpression(path + ".lhs", compexpr.lhs);
                    oilcan.setStringValue(path + ".op", ComparisonExprNode.opname[(int)compexpr.op]);
                    saveExpression(path + ".rhs", compexpr.rhs);
                    break;

                case OILType.AssignExpr:
                    AssignExprNode assignexpr = (AssignExprNode)expr;
                    oilcan.setStringValue(path + ".type", "assign-expr");
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
