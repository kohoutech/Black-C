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

        internal OILNode buildTranslationUnit(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
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

        internal OILNode buildFunctionDefinition(OILNode node1, OILNode node2, List<OILNode> nodelist, OILNode node3)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildStorageClassSpecifier(Token tok)
        {
            throw new NotImplementedException();
        }

        public DeclSpecNode makeDeclSpecs(List<Token> storageClassSpecs, List<TypeDeclNode> typeDefs, List<Token> typeModifers,
            List<Token> typeQuals, List<Token> functionSpecs)
        {
            DeclSpecNode declspec = null;

            //first, check the type modifiers
            bool isSigned = false;
            bool isUnsigned = false;
            bool isShort = false;
            bool isLong = false;
            bool isLongLong = false;
            foreach (Token tok in typeModifers)
            {
                isSigned = (tok.type == TokenType.SIGNED);
                isUnsigned = (tok.type == TokenType.UNSIGNED);
                isShort = (tok.type == TokenType.SHORT);
                if (isLong)
                {
                    isLongLong = (tok.type == TokenType.LONG);      //if we've already seen long
                }
                else
                {
                    isLong = (tok.type == TokenType.LONG);
                }
            }

            //then get the base type
            TypeDeclNode typdef = null;
            if (typeDefs.Count > 0)
            {
                typdef = typeDefs[0];
            }
            else if (isShort | isLong | isLongLong | isSigned | isUnsigned)
            {
                typdef = GetTypeDef("int");
            }
            
            //and modifiy base type & create declspec
            if (typdef != null)
            {
                if (typdef.name.Equals("int") && (isShort | isLong | isLongLong | isUnsigned)) 
                {
                    string typname = ((isUnsigned) ? "unsigned-" : "") + ((isShort) ? "short-" : "") + 
                        ((isLong) ? "long-" : "") + ((isLongLong) ? "long-long-" : "") + "int";
                    typdef = GetTypeDef(typname);
                }
                if (typdef.name.Equals("char") && (isSigned | isUnsigned))
                {
                    string typname = ((isUnsigned) ? "unsigned-" : "") + ((isSigned) ? "signed-" : "") + "char";                        
                    typdef = GetTypeDef(typname);
                }
                if (typdef.name.Equals("double") && isLong)
                {                    
                    typdef = GetTypeDef("long-double");
                }

                declspec = new DeclSpecNode(typdef);            

                //set storage class
                foreach (Token tok in storageClassSpecs)
                {
                    declspec.isTypedef = (tok.type == TokenType.TYPEDEF);
                    declspec.isExtern = (tok.type == TokenType.EXTERN);
                    declspec.isStatic = (tok.type == TokenType.STATIC);
                    declspec.isAuto = (tok.type == TokenType.AUTO);
                    declspec.isRegister = (tok.type == TokenType.REGISTER);
                }

                //type qualfiers
                foreach (Token tok in typeQuals)
                {
                    declspec.isConst = (tok.type == TokenType.CONST);
                    declspec.isRestrict = (tok.type == TokenType.RESTRICT);
                    declspec.isVolatile = (tok.type == TokenType.VOLATILE);
                }

                //and function specs
                foreach (Token tok in functionSpecs)
                {
                    declspec.isInline = (tok.type == TokenType.INLINE);
                }
            }

            return declspec;
        }

        internal OILNode buildInitDeclaratorList(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildDeclarationSpecifiers(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildTypeSpecifier(Token tok)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildDeclaration(OILNode node1, OILNode node2)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildTypeQualifier(Token tok)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildTypeSpecifier(OILNode node)
        {
            throw new NotImplementedException();
        }

        internal Token isTypedefName(Token tok)
        {
            throw new NotImplementedException();
        }

        public TypeDeclNode GetTypeDef(Token token)
        {
            TypeDeclNode typdef = null;
            switch (token.type)
            {
                case TokenType.VOID:
                    typdef = (TypeDeclNode)symbolTable.findSymbol("void");
                    break;

                case TokenType.CHAR:
                    typdef = (TypeDeclNode)symbolTable.findSymbol("char");
                    break;

                case TokenType.INT:
                    typdef = (TypeDeclNode)symbolTable.findSymbol("int");
                    break;

                case TokenType.FLOAT:
                    typdef = (TypeDeclNode)symbolTable.findSymbol("float");
                    break;

                case TokenType.DOUBLE:
                    typdef = (TypeDeclNode)symbolTable.findSymbol("double");
                    break;

                default:
                    break;
            }
            return typdef;
        }

        public TypeDeclNode GetTypeDef(string typename)
        {
            OILNode obj = symbolTable.findSymbol(typename);
            if (obj is TypeDeclNode)
            {
                return (TypeDeclNode)obj;
            }
            return null;
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

        internal OILNode buildStatement(OILNode node)
        {
            throw new NotImplementedException();
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

        internal OILNode buildLabeledStatement(Token tok, OILNode node1)
        {
            throw new NotImplementedException();
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

        internal OILNode buildCaseStatement(OILNode node1, OILNode node2)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildDefaultStatement(Token tok, OILNode node1)
        {
            throw new NotImplementedException();
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

        internal OILNode buildGotoStatement(Token tok1)
        {
            throw new NotImplementedException();
        }

        //- identifiers -------------------------------------------------------------

        //vars
        public IdentDeclaratorNode makeIdentDeclaratorNode(string ident)
        {
            IdentDeclaratorNode node = new IdentDeclaratorNode(ident);
            return node;
        }

        internal OILNode buildContinueStatement()
        {
            throw new NotImplementedException();
        }

        internal OILNode buildBreakStatement()
        {
            throw new NotImplementedException();
        }

        internal OILNode buildReturnStatement(OILNode node1)
        {
            throw new NotImplementedException();
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

        internal OILNode buildeArgumentExpressionList(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
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

        internal OILNode buildExpressionStatement(OILNode node)
        {
            throw new NotImplementedException();
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

        internal OILNode buildMultiplicativeExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            throw new NotImplementedException();
        }

        public IntConstant makeIntegerConstantNode(int value)
        {
            IntConstant node = new IntConstant(value);
            return node;            
        }

        internal OILNode buildAdditiveExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildSizeOfExpression(OILNode node1)
        {
            throw new NotImplementedException();
        }

        public FloatConstant makeFloatConstantNode(double value)
        {
            FloatConstant node = new FloatConstant(value);
            return node;
        }

        internal OILNode buildShiftExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            throw new NotImplementedException();
        }

        public ExprNode makeCharConstantNode(Token token)
        {
            return null;
        }

        internal OILNode buildIncDecExpression(Token tok1, OILNode node1)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildCastExpression(List<OILNode> nodeList, OILNode node2)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildAndExpression(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
        }

        public ExprNode makeStringConstantNode(Token token)
        {
            return null;
        }

        internal OILNode buildUnaryExpression(Token tok1, OILNode node1)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildEqualityExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            throw new NotImplementedException();
        }

        public SubExpressionNode makeSubexpressionNode(ExprNode expr)
        {
            return null;
        }

        internal OILNode buildExclusiveOrExpression(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
        }

        public TypeInitExprNode makeTypeInitExprNode(ExprNode node)
        {
            return null;
        }

        public IndexExprNode makeIndexExprNode(ExprNode node, ExprNode expr)
        {
            return null;
        }

        internal OILNode buildInclusiveOrExpression(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildRelationalExpression(List<OILNode> nodeList, List<Token> tokenList)
        {
            throw new NotImplementedException();
        }

        public FuncCallExprNode makeFuncCallExprNode(ExprNode node, ExprNode argList)
        {
            return null;
        }

        internal OILNode buildLogicalAndExpression(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
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

        internal OILNode buildeLogicalOrExpression(List<OILNode> nodeList)
        {
            throw new NotImplementedException();
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

        internal OILNode buildExpression()
        {
            throw new NotImplementedException();
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

        internal OILNode buildConstantExpression()
        {
            throw new NotImplementedException();
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

        internal OILNode buildConditionalExpression(OILNode node, OILNode node1, OILNode node2)
        {
            throw new NotImplementedException();
        }

        internal OILNode buildAssignmentExpression(List<OILNode> nodelist, List<Token> tokenList, OILNode node2)
        {
            throw new NotImplementedException();
        }
    }

    //-------------------------------------------------------------------------

    public class DeclSpecNode
    {
        public TypeDeclNode baseType;

        //storage class
        public bool isTypedef;
        public bool isExtern;
        public bool isStatic;
        public bool isAuto;
        public bool isRegister;

        //type qualifiers
        public bool isConst;
        public bool isRestrict;
        public bool isVolatile;

        //function specs
        public bool isInline;

        public DeclSpecNode(TypeDeclNode _baseType)
        {
            baseType = _baseType;

            isTypedef = false;
            isExtern = false;
            isStatic = false;
            isAuto = false;
            isRegister = false;

            isConst = false;
            isRestrict = false;
            isVolatile = false;
            isInline = false;
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