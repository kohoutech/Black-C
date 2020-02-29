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

using BlackC.Lexer;
using Origami.OIL;

//arbor - a place where trees are grown

namespace BlackC
{
    public class Arbor
    {
        public Parser parser;

        public SymbolTable symbolTable;

        public Arbor(Parser _parser)
        {
            parser = _parser;
            symbolTable = new SymbolTable();
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

        //not handling oldparamlist now -- if ever?
        public void completeFuncDef(FuncDeclNode funcdef, List<Declaration> oldparamlist, CompoundStatementNode block)
        {
            funcdef.body = new List<StatementNode>();
            foreach (StatementNode stmt in block.stmts)
            {
                funcdef.body.Add(stmt);
            }
        }

        public void addFuncDefToModule(Module module, FuncDeclNode funcdef)
        {
            module.funcs.Add(funcdef);
        }

        public void addDeclToModule(Module module, Declaration decl)
        {

        }

        //- declarations --------------------------------------------------------

        public TypeDeclNode makeTypeDeclNode(DeclSpecNode declarspecs)
        {
            return new TypeDeclNode("foo");
        }

        public FuncDeclNode makeFuncDeclNode(DeclSpecNode declarspecs, DeclaratorNode declarator)
        {
            FuncDeclNode func = new FuncDeclNode();
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
            return func;            
        }

        public Declaration makeVarDeclNode(Declaration decl, DeclSpecNode declarspecs, DeclaratorNode declarator, InitializerNode initializer)
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

            if (decl == null)
            {
                decl = new Declaration();
            }
            decl.decls.Add(vardecl);
            return decl;
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
            TypeDeclNode typdef = (TypeDeclNode)symbolTable.findSymbol(typename);
            return typdef;
        }

        //- struct/unions -----------------------------------------------------

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

        public StructDeclaratorNode makeStructDeclaractorNode(DeclaratorNode declarnode, ConstExpressionNode constexpr)
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

        public EnumeratorNode makeEnumeratorNode(EnumConstantNode enumconst, ConstExpressionNode constexpr)
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

        public ParamListNode makeParamList(List<ParamDeclNode> paramList)
        {
            bool hasElipsis = false;
            List<ParamDeclNode> paramList2 = new List<ParamDeclNode>();
            foreach (ParamDeclNode p in paramList)
            {
                if (p.name.Equals("..."))
                {
                    hasElipsis = true;
                }
                else
                {
                    paramList2.Add(p);
                }
            }
            return new ParamListNode(paramList2, hasElipsis);
        }

        public ParamDeclNode makeParamDeclarNode(DeclSpecNode declarspecs, DeclaratorNode declar)
        {
            String pname = "";
            TypeDeclNode ptype = declarspecs.baseType;
            DeclaratorNode dnode = declar;
            while (dnode != null)
            {
                if (dnode is IdentDeclaratorNode)
                {
                    pname = ((IdentDeclaratorNode)dnode).ident;
                }
                dnode = dnode.next;
            }
            ParamDeclNode p = new ParamDeclNode(pname, ptype);
            return p;
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

        public DesignationNode makeDesignationNode(DesignationNode node, ConstExpressionNode expr)
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

        public CompoundStatementNode makeCompoundStatementNode(CompoundStatementNode comp, Declaration decl)
        {
            comp.decls.Add(decl);
            return comp;
        }

        public CompoundStatementNode makeCompoundStatementNode(CompoundStatementNode comp, StatementNode stmt)
        {
            comp.stmts.Add(stmt);
            return comp;
        }

        public ExpressionStatementNode makeExpressionStatementNode(ExprNode expr)
        {
            return null;
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

        public ForStatementNode makeForStatementNode(ExprNode expr1, ExprNode expr2, ExprNode expr3, StatementNode stmt)
        {
            return null;
        }

        public ForStatementNode makeForStatementNode(Declaration decl, ExprNode expr2, ExprNode expr3, StatementNode stmt)
        {
            return null;
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

        public ExprNode getExprIdentNode(Token token)
        {
            return null;
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

        public PostPlusPlusExprNode makePostPlusPlusExprNode(ExprNode node)
        {
            return null;
        }

        public PostMinusMinusExprNode makePostMinusMinusExprNode(ExprNode node)
        {
            return null;
        }

        public ArgumentExprNode makeArgumentExprList(ExprNode list, ExprNode expr)
        {
            return null;
        }

        public PlusPlusExprNode makePlusPlusExprNode(ExprNode node)
        {
            return null;
        }

        public MinusMinusExprNode makeMinusMinusExprNode(ExprNode node)
        {
            return null;
        }

        public UnaryOperatorNode makeUnaryOperatorNode(ExprNode expr, UnaryOperatorNode.OPERATOR op)
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

        public MultiplyExprNode makeMultiplyExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public DivideExprNode makeDivideExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public ModuloExprNode makeModuloExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public AddExprNode makeAddExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public SubtractExprNode makeSubtractExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public ShiftLeftExprNode makeShiftLeftExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public ShiftRightExprNode makeShiftRightExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LessThanExprNode makeLessThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public GreaterThanExprNode makeGreaterThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LessEqualExprNode makeLessEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public GreaterEqualExprNode makeGreaterEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public EqualsExprNode makeEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public NotEqualsExprNode makeNotEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public ANDExprNode makeANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public XORExprNode makeXORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public ORExprNode makeORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LogicalANDExprNode makeLogicalANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public LogicalORExprNode makeLogicalORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return null;
        }

        public ConditionalExprNode makeConditionalExprNode(ExprNode lhs, ExprNode trueexpr, ExprNode falseexpr)
        {
            return null;
        }

        public AssignExpressionNode makeAssignExpressionNode(ExprNode lhs, ASSIGNOP oper, ExprNode rhs)
        {
            return null;
        }

        public ExpressionNode makeExpressionNode(ExprNode node, ExprNode expr)
        {
            return null;
        }

        public ConstExpressionNode makeConstantExprNode(ExprNode condit)
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
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");