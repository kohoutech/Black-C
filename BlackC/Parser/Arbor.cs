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
            symbolTable.addSymbol("int", new TypeDeclNode("int"));
            symbolTable.addSymbol("float", new TypeDeclNode("float"));
            symbolTable.addSymbol("double", new TypeDeclNode("double"));
        }

        //- external definitions ----------------------------------------------

        public FuncDefinition completeFuncDef(DeclSpecNode declspecs, DeclaratorNode declar, List<Declaration> oldparamlist, StatementNode block)
        {
            return null;
        }

        public void addFuncDefToModule(Module module, FuncDefinition funcdef)
        {
            
        }

        public void addDeclToModule(Module module, Declaration decl)
        {
            
        }

        //- declarations --------------------------------------------------------

        public TypeDeclNode makeTypeDeclNode(DeclSpecNode declarspecs)
        {
            return new TypeDeclNode("foo");
        }

        public Declaration makeFuncDeclNode(DeclSpecNode declarspecs, DeclaratorNode declarator)
        {
            //FuncDeclNode func = new FuncDeclNode();
            //func.name = declarator.ident;
            //func.returnType = declarspecs;
            //func.paramList = declarator.paramList;
            //return func;
            return null;
        }

        public Declaration makeVarDeclNode(Declaration decl, DeclSpecNode declarspecs, DeclaratorNode declarnode, InitializerNode initialnode)
        {
            return null;
        }

        public DeclSpecNode makeDeclSpecs(List<Token> storageClassSpecs, List<TypeDeclNode> typeDefs, 
            List<Token> typeQuals, List<Token> functionSpecs)
        {
            DeclSpecNode declspec = null;
            //if (typeDefs.Count > 0)
            //{
            //    declspec = new DeclSpecNode();
            //    declspec.baseType = typeDefs[0];
            //}
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

        public DeclaratorNode makePointerNode(DeclSpecNode qualList, DeclaratorNode declar)
        {        
            return null;
        }

        public DeclaratorNode makeFuncDeclarNode(ParamTypeListNode paramList)
        {
            return null;
        }

        public DeclaratorNode makeDirectIndexNode(DeclaratorNode head, int mode, DeclSpecNode qualList, AssignExpressionNode assign)
        {
            return null;
        }

        public void addParameterList(DeclaratorNode head, ParamTypeListNode paramlist)
        {
            
        }

        public ParamTypeListNode makeParamList(ParamTypeListNode paramList, ParamDeclNode param)
        {
            return null;
        }

        public ParamDeclNode makeParamDeclarNode(DeclSpecNode declarspecs, DeclaratorNode declar)
        {
            return null;
        }

        public TypeNameNode makeTypeNameNode(DeclSpecNode list, DeclaratorNode declar)
        {
            return null;
        }

        //- initializers ------------------------------------------------------

        public InitializerNode makeInitializerNode(AssignExpressionNode expr)
        {
            return null;
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
        public DeclaratorNode makeIdentDeclaratorNode(string ident)
        {
            DeclaratorNode node = new DeclaratorNode();
            node.ident = ident;
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
            return null;
        }

        public CompoundStatementNode makeCompoundStatementNode(CompoundStatementNode comp, StatementNode stmt)
        {
            return null;
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
            return null;
        }

        //- expressions -------------------------------------------------------------

        public ExprNode getExprIdentNode(Token token)
        {
            return null;
        }

        public IntConstant makeIntegerConstantNode(Token token)
        {
            return null;
        }

        public FloatConstant makeFloatConstantNode(Token token)
        {
            return null;
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

        public AssignExpressionNode makeAssignExpressionNode(AssignExpressionNode lhs, ASSIGNOP oper, ExprNode rhs)
        {
            return null;
        }

        public ExpressionNode makeExpressionNode(ExpressionNode node, ExprNode expr)
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
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");