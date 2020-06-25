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
using System.IO;

using BlackC.Scan;
using Kohoutech.OIL;

//arbor - a place where trees are grown

namespace BlackC.Parse
{
    public class Arbor
    {
        public Parser parser;

        public SymbolTable symbolTable;

        public Module curModule;
        public FuncDefNode curFunc;
        public List<Block> blockStack;
        public Block curBlock;
        public ParamListNode curParamList;

        public Arbor(Parser _parser)
        {
            parser = _parser;

            symbolTable = new SymbolTable();
            curModule = null;
            curFunc = null;
            blockStack = new List<Block>();
            curBlock = null;
            curParamList = null;

            defineBaseTypes();
        }

        public void defineBaseTypes()
        {
            symbolTable.addSymbol("void", new TypeDeclNode("void"));
            symbolTable.addSymbol("char", new TypeDeclNode("char"));
            symbolTable.addSymbol("signed-char", new TypeDeclNode("signed-char"));
            symbolTable.addSymbol("unsigned-char", new TypeDeclNode("unsigned-char"));
            symbolTable.addSymbol("short-int", new TypeDeclNode("short-int"));
            symbolTable.addSymbol("unsigned-short-int", new TypeDeclNode("unsigned-short-int"));
            symbolTable.addSymbol("int", new TypeDeclNode("int"));
            symbolTable.addSymbol("unsigned-int", new TypeDeclNode("unsigned-int"));
            symbolTable.addSymbol("long-int", new TypeDeclNode("long-int"));
            symbolTable.addSymbol("unsigned-long-int", new TypeDeclNode("unsigned-long-int"));
            symbolTable.addSymbol("long-long-int", new TypeDeclNode("long-long-int"));
            symbolTable.addSymbol("unsigned-long-long-int", new TypeDeclNode("unsigned-long-long-int"));
            symbolTable.addSymbol("float", new TypeDeclNode("float"));
            symbolTable.addSymbol("double", new TypeDeclNode("double"));
            symbolTable.addSymbol("long-double", new TypeDeclNode("long-double"));
        }

        //- external definitions ----------------------------------------------

        public void startModule(string filename)
        {
            curModule = new Module(filename);            
        }

        public Module finishModule()
        {
            return curModule;
        }

        public bool hasFuncDef()
        {
            return (curFunc != null);
        }

        public void startFuncDef(DeclSpecNode declarspecs, DeclaratorNode declarator)
        {
            FuncDefNode func = new FuncDefNode();
            func.returnType = declarspecs.baseType;
            DeclaratorNode dnode = declarator;
            while (dnode != null)
            {
                if (dnode is IdentDeclaratorNode)
                {
                    func.name = ((IdentDeclaratorNode)dnode).ident;
                }
                if (dnode is ParamListNode)
                {
                    func.paramList = ((ParamListNode)dnode).paramList;
                }
                dnode = dnode.next;
            }
            symbolTable.addSymbol(func.name, func);     //add func def to global symbol tbl
            curModule.funcs.Add(func);                  //add func to module's func list
            curFunc = func;
            enterBlock();                               //enter function "super" block
        }

        public void startoldparamlist()
        {            
        }

        public void finisholdparamlist()
        {         
        }

        public void addFuncParamsToBlock()
        {
            foreach (ParamDeclNode p in curFunc.paramList)
            {
                symbolTable.addSymbol(p.name, p);
            }
        }

        public void finishFuncDef(Block block)
        {
            exitBlock();                    //exit function "super" block

            curFunc.body = new List<StatementNode>();
            foreach (StatementNode stmt in block.stmts)
            {
                curFunc.body.Add(stmt);
            }
        }

        //- declarations --------------------------------------------------------

        public void makeDeclarationNode(DeclSpecNode declarspecs, DeclaratorNode declarator, InitializerNode initializer)
        {
            VarDeclNode vardecl = new VarDeclNode();
            vardecl.varType = declarspecs.baseType;
            DeclaratorNode dnode = declarator;
            while (dnode != null)
            {
                if (dnode is IdentDeclaratorNode)
                {
                    vardecl.name = ((IdentDeclaratorNode)dnode).ident;
                }
                dnode = dnode.next;
            }
            vardecl.initializer = initializer;
            symbolTable.addSymbol(vardecl.name, vardecl);

            //add decl to either global or local decl list
            if (curFunc != null)
            {
                curFunc.locals.Add(vardecl);
            }
            else
            {
                curModule.globals.Add(vardecl);
            }

            if (initializer != null && curBlock != null)
            {
                DeclarInitStatementNode dstmt = new DeclarInitStatementNode(vardecl, initializer.initExpr);
                addStmtToBlock(dstmt);
                vardecl.initializer = null;
            }
        }

        //- struct/unions -----------------------------------------------------

        //public Declaration makeTypeDeclNode(DeclSpecNode declspecs)
        //{
        //    TypeDeclNode typdef = declspecs.baseType;

        //    Declaration decl = new Declaration();
        //    decl.decls.Add(typdef);
        //    return decl;
        //}

        public StructDeclNode starttructSpec(StructDeclNode node, IdentNode idNode, StructDeclarationNode field, bool isUnion)
        {
            return null;
        }

        public StructDeclNode makeStructSpec(StructDeclNode node, StructDeclarationNode field)
        {
            return null;
        }

        public StructDeclNode getStructDecl(IdentNode idNode, bool isUnion)
        {
            return null;
        }

        public StructDeclarationNode makeStructDeclarationNode(DeclSpecNode specqual, List<StructDeclaratorNode> fieldnames)
        {
            return null;
        }

        public StructDeclaratorNode makeStructDeclaractorNode(DeclaratorNode declarnode, ConstExprNode constexpr)
        {
            return null;
        }

        //- enums -------------------------------------------------------------

        public EnumDeclNode startEnumSpec(IdentNode idNode, EnumeratorNode enumer)
        {
            return null;
        }

        public EnumDeclNode makeEnumSpec(EnumDeclNode node, EnumeratorNode enumer)
        {
            return null;
        }

        public EnumDeclNode getEnumDecl(IdentNode idNode)
        {
            return null;
        }

        public EnumeratorNode makeEnumeratorNode(EnumConstantNode enumconst, ConstExprNode constexpr)
        {
            return null;
        }

        //- declarators -------------------------------------------------------

        public DeclaratorNode makePointerNode(TypeQualNode qualList, DeclaratorNode declar)
        {
            return null;
        }

        public DeclaratorNode makeFuncDeclarNode(ParamListNode paramList)
        {
            return null;
        }

        public DeclaratorNode makeDirectIndexNode(int mode, TypeQualNode qualList, ExprNode assign)
        {
            return null;
        }
                
        public void startParamList()
        {
            curParamList = new ParamListNode();
            symbolTable.enterScope();
        }

        public void makeParamDeclarNode(DeclSpecNode declarspecs, DeclaratorNode declar)
        {
            ParamDeclNode pdecl = new ParamDeclNode();
            pdecl.pType = declarspecs.baseType;
            DeclaratorNode dnode = declar;
            while (dnode != null)
            {
                if (dnode is IdentDeclaratorNode)
                {
                    pdecl.name = ((IdentDeclaratorNode)dnode).ident;
                }
                dnode = dnode.next;
            }
            symbolTable.addSymbol(pdecl.name, pdecl);
            curParamList.paramList.Add(pdecl);
        }

        public void addElipsisParam()
        {
            curParamList.hasElipsis = true;
        }

        public ParamListNode finishParamList()
        {
            ParamListNode paramList = curParamList;
            curParamList = null;
            symbolTable.exitscope();
            return paramList;
        }

        public TypeNameNode makeTypeNameNode(DeclSpecNode list, DeclaratorNode declar)
        {
            return null;
        }

        //- initializers ------------------------------------------------------

        public InitializerNode makeInitializerNode(ExprNode expr)
        {
            InitializerNode node = new InitializerNode(expr);
            return node;
        }

        public InitializerNode makeInitializerNode(List<InitializerNode> list)
        {
            return null;
        }

        public List<InitializerNode> makeInitializerList(List<InitializerNode> list, DesignationNode desinode, InitializerNode initnode)
        {
            return null;
        }

        public DesignationNode makeDesignationNode(DesignationNode node, ConstExprNode expr)
        {
            return null;
        }

        public DesignationNode makeDesignationNode(DesignationNode node, IdentNode ident)
        {
            return null;
        }

        //- identifiers -------------------------------------------------------------

        //vars
        public IdentDeclaratorNode makeIdentDeclaratorNode(string ident)
        {
            IdentDeclaratorNode node = new IdentDeclaratorNode(ident);
            return node;
        }

        public IdentNode getArgIdentNode(Token token)
        {
            return null;
        }

        ////these only return new ident nodes
        //public IdentNode makeDeclarIdentNode(Token token)
        //{
        //    IdentNode node = null;
        //    if (token.type == TokenType.IDENT)
        //    {
        //        String id = token.chars;
        //        node = SymbolTable.addSymbol(curSymbolTable, id);
        //        node.symtype = SYMTYPE.DECLAR;
        //    }
        //    return node;
        //}

        ////these return declared ident nodes 
        //public IdentNode getDeclarIdentNode(String id)
        //{
        //    IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.DECLAR);
        //    return node;
        //}

        //structs
        public IdentNode getStructIdentNode(Token token)
        {
            return null;
        }

        ////these return either declared or new ident nodes 
        //public IdentNode getStructIdentNode(Token token)
        //{
        //    String id = token.chars;
        //    IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.STRUCT);
        //    if (node == null)
        //    {
        //        node = SymbolTable.addSymbol(curSymbolTable, id);
        //        node.symtype = SYMTYPE.STRUCT;
        //    }            
        //    return node;
        //}

        //fields
        public IdentNode getFieldIdentNode(Token token)
        {
            return null;
        }

        ////called before <makeFieldExprNode> and <makeRefFieldExprNode>
        //public IdentNode getFieldIdentNode(Token token)
        //{
        //    return null;
        //}

        //public IdentNode getFieldInitializerNode(Token token)
        //{
        //    return null;
        //}

        //enums
        public IdentNode getEnumIdentNode(Token token)
        {
            //    String id = token.chars;
            //    IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.ENUM);
            //    if (node == null)
            //    {
            //        node = SymbolTable.addSymbol(curSymbolTable, id);
            //        node.symtype = SYMTYPE.ENUM;
            //    }
            //    return node;
            return null;
        }

        public EnumConstantNode getEnumerationConstant(Token token)
        {
            return null;
        }

        //public EnumConstantNode makeEnumConstNode(Token token)
        //{
        //    String id = token.chars;
        //    return new EnumConstantNode(id);
        //}        

        //typedefs
        //public TypedefNode getTypedefNode(Token token)
        //{
        //    TypedefNode node = null;
        //    if (token.type == TokenType.IDENT)
        //    {
        //        String id = token.chars;
        //        IdentNode idnode = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.TYPEDEF);
        //        if (idnode != null)
        //        {
        //            node = (TypedefNode)idnode.def;
        //        }
        //    }
        //    return node;
        //}

        //labels
        //public IdentNode makeLabelIdentNode(Token token)
        //{
        //    String id = token.chars;
        //    IdentNode node = SymbolTable.addSymbol(curSymbolTable, id);
        //    node.symtype = SYMTYPE.LABEL;
        //    return node;
        //}

        //public IdentNode getLabelIdentNode(Token token)
        //{
        //    return null;
        //}

        //- statements --------------------------------------------------------

        public void enterBlock()
        {
            curBlock = new Block();
            blockStack.Add(curBlock);
            symbolTable.enterScope();
        }

        public void addStmtToBlock(StatementNode stmt)
        {
            curBlock.stmts.Add(stmt);
        }

        public Block exitBlock()
        {
            Block thisBlock = blockStack[blockStack.Count - 1];
            blockStack.RemoveAt(blockStack.Count - 1);
            curBlock = (blockStack.Count > 0) ? blockStack[blockStack.Count - 1] : null;
            symbolTable.exitscope();
            return thisBlock;
        }

        public LabelStatementNode makeLabelStatementNode(Token labelId, StatementNode stmt)
        {
            return null;
        }

        public CaseStatementNode makeCaseStatementNode(ExprNode expr, StatementNode stmt)
        {
            return null;
        }

        public DefaultStatementNode makeDefaultStatementNode(StatementNode stmt)
        {
            return null;
        }

        public ExpressionStatementNode makeExpressionStatementNode(ExprNode expr)
        {
            ExpressionStatementNode node = new ExpressionStatementNode(expr);
            return node;
        }

        public EmptyStatementNode makeEmptyStatementNode()
        {
            return null;
        }

        public IfStatementNode makeIfStatementNode(ExprNode expr, StatementNode thenstmt, StatementNode elsestmt)
        {
            return null;
        }

        public SwitchStatementNode makeSwitchStatementNode(ExprNode expr, StatementNode stmt)
        {
            return null;
        }

        public WhileStatementNode makeWhileStatementNode(ExprNode expr, StatementNode stmt)
        {
            return null;
        }

        public DoStatementNode makeDoStatementNode(StatementNode stmt, ExprNode expr)
        {
            return null;
        }

        public ForStatementNode makeForStatementNode(Block forBlock, ExprNode expr1, ExprNode expr2, ExprNode expr3, StatementNode body)
        {
            List<StatementNode> bodyList = null;
            if (body is Block)
            {
                bodyList = ((Block)body).stmts;
            }
            else
            {
                bodyList = new List<StatementNode>();
                bodyList.Add(body);
            }
            ForStatementNode node = new ForStatementNode(forBlock.stmts, expr1, expr2, expr3, bodyList);
            return node;            
        }

        public GotoStatementNode makeGotoStatementNode(Token ident)
        {
            return null;
        }

        public ContinueStatementNode makeContinueStatementNode()
        {
            return null;
        }

        public BreakStatementNode makeBreakStatementNode()
        {
            return null;
        }

        public ReturnStatementNode makeReturnStatementNode(ExprNode expr)
        {
            ReturnStatementNode node = new ReturnStatementNode(expr);
            return node;
        }

        //- expressions -------------------------------------------------------------

        public IdentExprNode getExprIdentNode(String id)
        {
            OILNode idsym = symbolTable.findSymbol(id);
            IdentExprNode node = new IdentExprNode(idsym);
            return node;
        }

        public IntConstant makeIntegerConstantNode(int value)
        {
            IntConstant node = new IntConstant(value);
            return node;            
        }

        public FloatConstant makeFloatConstantNode(double value)
        {
            FloatConstant node = new FloatConstant(value);
            return node;
        }

        public ExprNode makeCharConstantNode(Token token)
        {
            return null;
        }

        public ExprNode makeStringConstantNode(Token token)
        {
            return null;
        }

        public SubExpressionNode makeSubexpressionNode(ExprNode expr)
        {
            return null;
        }

        public TypeInitExprNode makeTypeInitExprNode(ExprNode node)
        {
            return null;
        }

        public IndexExprNode makeIndexExprNode(ExprNode node, ExprNode expr)
        {
            return null;
        }

        public FuncCallExprNode makeFuncCallExprNode(ExprNode node, ExprNode argList)
        {
            return null;
        }

        public FieldExprNode makeFieldExprNode(ExprNode node, OILNode idNode)
        {
            return null;
        }

        public RefFieldExprNode makeRefFieldExprNode(ExprNode node, OILNode idNode)
        {
            return null;
        }

        public ArgumentExprNode makeArgumentExprList(ExprNode list, ExprNode expr)
        {
            return null;
        }

        public UnaryOpExprNode makeUnaryOperatorNode(ExprNode expr, UnaryOpExprNode.OPERATOR op)
        {
            return null;
        }

        public SizeofTypeExprNode makeSizeofTypeExprNode(TypeNameNode name)
        {
            return null;
        }

        public SizeofUnaryExprNode makeSizeofUnaryExprNode(ExprNode node)
        {
            return null;
        }

        public UnaryCastExprNode makeCastExprNode(ExprNode namelist, TypeNameNode name)
        {
            return null;
        }

        public UnaryCastExprNode makeCastExprNode(ExprNode uexpr, ExprNode namelist)
        {
            return null;
        }
       
        //- arithmetic expressions ------------------------

        public ArithmeticExprNode makeUnaryPlusExprNode(ExprNode term)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.PLUS, term, null);
            return node;
        }

        public ArithmeticExprNode makeUnaryMinusExprNode(ExprNode term)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.MINUS, term, null);
            return node;
        }

        public ArithmeticExprNode makePrePlusPlusExprNode(ExprNode term)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.INC, null, term);
            return node;
        }

        public ArithmeticExprNode makePreMinusMinusExprNode(ExprNode term)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.DEC, null, term);
            return node;
        }

        public ArithmeticExprNode makePostPlusPlusExprNode(ExprNode term)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.INC, term, null);
            return node;
        }

        public ArithmeticExprNode makePostMinusMinusExprNode(ExprNode term)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.DEC, term, null);
            return node;
        }

        public ArithmeticExprNode makeAdditionExprNode(ExprNode lhs, ExprNode rhs)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.ADD, lhs, rhs);
            return node;
        }

        public ArithmeticExprNode makeSubtractExprNode(ExprNode lhs, ExprNode rhs)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.SUB, lhs, rhs);
            return node;
        }

        public ArithmeticExprNode makeMultiplyExprNode(ExprNode lhs, ExprNode rhs)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.MULT, lhs, rhs);
            return node;
        }

        public ArithmeticExprNode makeDivideExprNode(ExprNode lhs, ExprNode rhs)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.DIV, lhs, rhs);
            return node;
        }

        public ArithmeticExprNode makeModuloExprNode(ExprNode lhs, ExprNode rhs)
        {
            ArithmeticExprNode node = new ArithmeticExprNode(ArithmeticExprNode.OPERATOR.MOD, lhs, rhs);
            return node;
        }

        //- comparision expressions -----------------------

        public ComparisonExprNode makeEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            ComparisonExprNode node = new ComparisonExprNode(ComparisonExprNode.OPERATOR.EQUAL, lhs, rhs);
            return node;
        }

        public ComparisonExprNode makeNotEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            ComparisonExprNode node = new ComparisonExprNode(ComparisonExprNode.OPERATOR.NOTEQUAL, lhs, rhs);
            return node;
        }

        public ComparisonExprNode makeLessThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            ComparisonExprNode node = new ComparisonExprNode(ComparisonExprNode.OPERATOR.LESSTHAN, lhs, rhs);
            return node;
        }

        public ComparisonExprNode makeGreaterThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            ComparisonExprNode node = new ComparisonExprNode(ComparisonExprNode.OPERATOR.GTRTHAN, lhs, rhs);
            return node;
        }

        public ComparisonExprNode makeLessEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            ComparisonExprNode node = new ComparisonExprNode(ComparisonExprNode.OPERATOR.LESSEQUAL, lhs, rhs);
            return node;
        }

        public ComparisonExprNode makeGreaterEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            ComparisonExprNode node = new ComparisonExprNode(ComparisonExprNode.OPERATOR.GTREQUAL, lhs, rhs);
            return node;
        }

        //- bitwise/logical expressions -------------------

        public BitwiseExprNode makeANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public BitwiseExprNode makeXORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public BitwiseExprNode makeORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public BitwiseExprNode makeNOTExprNode(ExprNode term)
        {
            return null;
        }

        public BitwiseExprNode makeShiftLeftExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public BitwiseExprNode makeShiftRightExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LogicalExprNode makeLogicalANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LogicalExprNode makeLogicalORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LogicalExprNode makeLogicalNOTExprNode(ExprNode term)
        {
            return null;
        }

        public ConditionalExprNode makeConditionalExprNode(ExprNode lhs, ExprNode trueexpr, ExprNode falseexpr)
        {
            return null;
        }

        //-------------------------------------------------

        public AssignExprNode makeAssignExpressionNode(AssignExprNode.OPERATOR oper, ExprNode lhs, ExprNode rhs)
        {
            AssignExprNode node = new AssignExprNode(oper, lhs, rhs);
            return node;
        }

        public ExpressionNode makeExpressionNode(ExprNode node, ExprNode expr)
        {
            return null;
        }

        public ConstExprNode makeConstantExprNode(ExprNode condit)
        {
            return null;
        }

        public bool isLVar(ExprNode lhs)
        {
            return false;
        }

        internal TypeQualNode makeTypeQualNode(List<Token> typeQuals)
        {
            throw new NotImplementedException();
        }

        //- build funcs -------------------------------------------------------

        //- external definitions ----------------------------------------------

        public OILNode buildTranslationUnit(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildFunctionDefinition(OILNode node1, OILNode node2, List<OILNode> nodelist, OILNode node3)
        {
            return new OILNode();
        }

        //- declarations --------------------------------------------------------

        public OILNode buildDeclaration(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public DeclSpecNode buildDeclarationSpecifiers(List<OILNode> nodeList)
        {
            DeclSpecNode dnode = new DeclSpecNode();
            foreach(OILNode node in nodeList)
            {
                DeclSpecNode node1 = (DeclSpecNode)node;
                dnode.merge(node1);
            }

            //adjust base type if necessary
            if (dnode.baseType == null)
            {
                dnode.baseType = (TypeDeclNode)symbolTable.findSymbol("int");       //default base type
            }
            if (dnode.baseType.name.Equals("char"))
            {
                if (dnode.isSigned)
                {
                    dnode.baseType = (TypeDeclNode)symbolTable.findSymbol("signed-char");
                }
                else if (dnode.isUnsigned)
                {
                    dnode.baseType = (TypeDeclNode)symbolTable.findSymbol("unsigned-char");
                }
            }
            else if (dnode.baseType.name.Equals("int"))
            {
                String tname = "int";
                if (dnode.isShort)
                {
                    tname = "short-" + tname;
                }
                else if (dnode.longCount == 1)
                {
                    tname = "long-" + tname;
                }
                else if (dnode.longCount == 2)
                {
                    tname = "long-long-" + tname;
                }
                if (dnode.isUnsigned)
                {
                    tname = "unsigned-" + tname;
                }
                dnode.baseType = (TypeDeclNode)symbolTable.findSymbol(tname);
            }
            else if (dnode.baseType.name.Equals("double"))
            {
                if (dnode.longCount == 1)
                {
                    dnode.baseType = (TypeDeclNode)symbolTable.findSymbol("long-double");
                }
            }

            return dnode;
        }

        public OILNode buildInitDeclaratorList(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildInitDeclarator(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }
        
        public DeclSpecNode buildStorageClassSpecifier(Token tok)
        {
            DeclSpecNode node = new DeclSpecNode();
            node.isTypedef = (tok.type == TokenType.TYPEDEF);
            node.isExtern = (tok.type == TokenType.EXTERN);
            node.isStatic = (tok.type == TokenType.STATIC);
            node.isAuto = (tok.type == TokenType.AUTO);
            node.isRegister = (tok.type == TokenType.REGISTER);
            return node;
        }

        public DeclSpecNode buildTypeSpecifier(Token tok)
        {
            DeclSpecNode node = new DeclSpecNode();
            switch(tok.type)
            {
                case TokenType.VOID:
                    node.baseType = (TypeDeclNode)symbolTable.findSymbol("void");
                    break;

                case TokenType.CHAR:
                    node.baseType = (TypeDeclNode)symbolTable.findSymbol("char");
                    break;

                case TokenType.INT:
                    node.baseType = (TypeDeclNode)symbolTable.findSymbol("int");
                    break;

                case TokenType.FLOAT:
                    node.baseType = (TypeDeclNode)symbolTable.findSymbol("float");
                    break;

                case TokenType.DOUBLE:
                    node.baseType = (TypeDeclNode)symbolTable.findSymbol("double");
                    break;

                case TokenType.SHORT:
                    node.isLong = true;
                    break;

                case TokenType.LONG:
                    node.isLong = true;
                    break;

                case TokenType.SIGNED:
                    node.isSigned = true;
                    break;

                case TokenType.UNSIGNED:
                    node.isUnsigned = true;
                    break;

                default:
                    break;
            }            
            return node;
        }

        public TypeDeclNode isTypedefName(Token tok)
        {
            String id = "typedef-" + tok.strval;
            TypeDeclNode node = (TypeDeclNode)symbolTable.findSymbol(id);
            return node;
        }

        public DeclSpecNode buildTypeSpecifier(OILNode typeNode)
        {
            DeclSpecNode node = new DeclSpecNode();
            node.baseType = (TypeDeclNode)typeNode;
            return node;
        }

        //- struct/unions -----------------------------------------------------

        public OILNode buildStructOrUnionSpecifier(bool isStruct, Token id, List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildStructDeclaration(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildSpecifierQualifierList(List<OILNode> nodelist)
        {
            return new OILNode();
        }

        public OILNode buildStructDeclaratorList()
        {
            return new OILNode();
        }

        public OILNode buildStructDeclarator()
        {
            return new OILNode();
        }

        //- enums -------------------------------------------------------------

        public Token isEnumerationConstant(Token tok)
        {
            return tok;
        }

        public OILNode buildEnumSpecifier(Token id, object nodelist)
        {
            return new OILNode();
        }

        public OILNode buildEnumerator(Token enumc, OILNode node1)
        {
            return new OILNode();
        }

        public DeclSpecNode buildTypeQualifier(Token tok)
        {
            DeclSpecNode node = new DeclSpecNode();
            node.isConst = (tok.type == TokenType.CONST);
            node.isRestrict = (tok.type == TokenType.RESTRICT);
            node.isVolatile = (tok.type == TokenType.VOLATILE);
            return node;
        }

        public DeclSpecNode buildFunctionSpecifier(Token tok)
        {
            DeclSpecNode node = new DeclSpecNode();
            node.isInline = (tok.type == TokenType.INLINE);
            return node;
        }

        //- declarators -------------------------------------------------------

        public OILNode buildDeclarator(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildBaseDirectDeclarator(Token id, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildArrayDirectDeclarator(OILNode node, bool isStatic1, OILNode node2, bool isStatic2, OILNode node3, bool isPointer)
        {
            return new OILNode();
        }

        public OILNode buildParamDirectDeclarator(OILNode node, OILNode node4)
        {
            return new OILNode();
        }

        public OILNode buildPointer(object p, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildTypeQualifierList(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildParameterTypeList(OILNode node1, List<OILNode> nodeList, bool hasElipsis)
        {
            return new OILNode();
        }

        public OILNode buildParameterDeclaration()
        {
            return new OILNode();
        }

        public OILNode buildIdentifierList(List<Token> tokenList)
        {
            return new OILNode();
        }

        public OILNode buildTypeName(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildAbstractDeclarator(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildBaseAbstractDeclarator(OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildArrayAbstractDeclarator(OILNode node, bool isStatic1, OILNode node2, bool isStatic2, OILNode node3, bool isPointer)
        {
            return new OILNode();
        }

        public OILNode buildParamAbstractDeclarator(OILNode node, OILNode node4)
        {
            return new OILNode();
        }

        //- initializers ------------------------------------------------------

        public OILNode buildInitializer(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildInitializerList(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildArrayDesignation(OILNode node)
        {
            return new OILNode();
        }

        public OILNode buildArrayDesignation(OILNode node, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildFieldDesignation(OILNode node, Token tok1)
        {
            return new OILNode();
        }

        //- statements --------------------------------------------------------

        public OILNode buildStatement(OILNode node)
        {
            return new OILNode();
        }

        public OILNode buildLabeledStatement(Token tok, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildCaseStatement(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildDefaultStatement(Token tok, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildCompoundStatement(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildExpressionStatement(OILNode node)
        {
            return new OILNode();
        }

        public OILNode buildIfStatement(OILNode node1, OILNode node2, OILNode node3)
        {
            return new OILNode();
        }

        public OILNode buildSwitchStatement(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildWhileStatement(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildDoWhileStatement(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildForStatement(OILNode node1, OILNode node2, OILNode node3, OILNode node4)
        {
            return new OILNode();
        }

        public OILNode buildGotoStatement(Token tok1)
        {
            return new OILNode();
        }

        public OILNode buildContinueStatement()
        {
            return new OILNode();
        }

        public OILNode buildBreakStatement()
        {
            return new OILNode();
        }

        public OILNode buildReturnStatement(OILNode node1)
        {
            return new OILNode();
        }

        //- expressions -------------------------------------------------------------

        public OILNode buildIdentExpression(Token tok)
        {
            return new OILNode();
        }

        public OILNode buildIntConstExpression(Token tok)
        {
            return new OILNode();
        }

        public OILNode buildFloatConstExpression(Token tok)
        {
            return new OILNode();
        }

        public OILNode buildCharConstExpression(Token tok)
        {
            return new OILNode();
        }

        public OILNode buildStringConstExpression(Token tok)
        {
            return new OILNode();
        }

        public OILNode buildSubExpression(OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildTypeNameInitializerList(OILNode node2, OILNode node3)
        {
            return new OILNode();
        }

        public OILNode buildArrayIndexExpression(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildArgumentListExpression(OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildFieldReference(OILNode node1, Token tok3)
        {
            return new OILNode();
        }

        public OILNode buildIndirectFieldReference(OILNode node1, Token tok3)
        {
            return new OILNode();
        }

        public OILNode buildPostIncDecExpression(Token tok1)
        {
            return new OILNode();
        }

        public OILNode buildeArgumentExpressionList(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildSizeOfExpression(OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildIncDecExpression(Token tok1, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildUnaryExpression(Token tok1, OILNode node1)
        {
            return new OILNode();
        }

        public OILNode buildCastExpression(List<OILNode> nodeList, OILNode node2)
        {
            return new OILNode();
        }

        //- arithmetic expressions ------------------------

        public OILNode buildMultiplicativeExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            return new OILNode();
        }

        public OILNode buildAdditiveExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            return new OILNode();
        }

        public OILNode buildShiftExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            return new OILNode();
        }

        //- comparision expressions -----------------------

        public OILNode buildRelationalExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            return new OILNode();
        }

        public OILNode buildEqualityExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            return new OILNode();
        }

        //- bitwise/logical expressions -------------------

        public OILNode buildAndExpression(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildExclusiveOrExpression(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildInclusiveOrExpression(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildLogicalAndExpression(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildeLogicalOrExpression(List<OILNode> nodeList)
        {
            return new OILNode();
        }

        public OILNode buildConditionalExpression(OILNode node, OILNode node1, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildAssignmentExpression(List<OILNode> nodelist, List<Token> tokenList, OILNode node2)
        {
            return new OILNode();
        }

        public OILNode buildExpression()
        {
            return new OILNode();
        }

        public OILNode buildConstantExpression()
        {
            return new OILNode();
        }
    }

    //-------------------------------------------------------------------------
    //   INTERNAL OIL NODES
    //-------------------------------------------------------------------------

    public class DeclSpecNode : OILNode
    {
        public TypeDeclNode baseType;

        //storage class
        public bool isTypedef;
        public bool isExtern;
        public bool isStatic;
        public bool isAuto;
        public bool isRegister;

        //type modifers
        public bool isShort;
        public bool isLong;
        public bool isSigned;
        public bool isUnsigned;
        public int longCount;

        //type qualifiers
        public bool isConst;
        public bool isRestrict;
        public bool isVolatile;

        //function specs
        public bool isInline;

        public DeclSpecNode()
        {
            baseType = null;

            isTypedef = false;
            isExtern = false;
            isStatic = false;
            isAuto = false;
            isRegister = false;

            isShort = false;
            isLong = false;
            isSigned = false;
            isUnsigned = false;
            longCount = 0;

            isConst = false;
            isRestrict = false;
            isVolatile = false;

            isInline = false;
        }

        public void merge(DeclSpecNode that)
        {
            if (baseType == null)
            {
                baseType = that.baseType;
            }

            isTypedef |= that.isTypedef;
            isExtern |= that.isExtern;
            isStatic |= that.isStatic;
            isAuto |= that.isAuto;
            isRegister |= that.isRegister;

            isShort |= that.isShort;
            isSigned |= that.isSigned;
            isUnsigned |= that.isUnsigned;
            if (that.isLong)
            {
                longCount++;
            }

            isConst |= that.isConst;
            isRestrict |= that.isRestrict;
            isVolatile |= that.isVolatile;

            isInline |= that.isInline;
        }
    }

    public class Block : StatementNode
    {
        public List<StatementNode> stmts;

        public Block()
        {
            stmts = new List<StatementNode>();
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");