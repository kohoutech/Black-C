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

        //- declarations --------------------------------------------------------

        public TypeDeclNode makeTypeDeclNode(DeclSpecNode declarspecs)
        {
            return new TypeDeclNode("foo");
        }

        public Declaration makeVarOrFuncNode(DeclSpecNode declarspecs, DeclaratorNode declarnode)
        {
            return new Declaration();
        }

        public VarDeclNode makeVarDeclNode(DeclSpecNode declarspecs, DeclaratorNode declarnode, InitializerNode initialnode)
        {
            return new VarDeclNode();
        }

        public FuncDeclNode makeFuncDeclNode(DeclSpecNode declarspecs, DeclaratorNode declarator)
        {
            FuncDeclNode func = new FuncDeclNode();
            func.name = declarator.ident;
            func.returnType = declarspecs;
            func.paramList = declarator.paramList;
            return func;
        }

        public FuncDeclNode completeFuncDef(FuncDeclNode declar, List<Declaration> oldparamlist, StatementNode block)
        {
            return declar;
        }

        public DeclSpecNode makeDeclSpecs(List<Token> storageClassSpecs, List<Token> baseTypeModifiers, List<TypeDeclNode> typeDefs, 
            List<Token> typeQuals, List<Token> functionSpecs)
        {
            DeclSpecNode declspec = null;
            if (typeDefs.Count > 0)
            {
                declspec = new DeclSpecNode();
                declspec.baseType = typeDefs[0];
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

        //public InitDeclaratorNode makeInitDeclaratorNode(DeclaratorNode declar, InitializerNode initial)
        //{
        //    return new InitDeclaratorNode(declar, initial);
        //}

        public DeclaratorNode makeDirectIndexNode(DeclaratorNode head, int mode, Declaration qualList, Declaration assign)
        {
            throw new NotImplementedException();
        }

        public void addParameterList(DeclaratorNode head, List<ParamDeclNode> paramlist)
        {
            head.paramList = paramlist;
        }

        //public bool handleTypeDef(DeclarationNode declar)
        //{
        //    DeclarSpecNode specs = declar.declarspecs;
        //    bool isTypedef = false;
        //    //foreach (DeclarSpecNode spec in specs)
        //    //{
        //    //    if (spec is StorageClassNode)
        //    //    {
        //    //        StorageClassNode storspec = (StorageClassNode)spec;
        //    //        if (storspec.storage == StorageClassNode.STORAGE.TYPEDEF)
        //    //        {
        //    //            isTypedef = true;
        //    //            break;
        //    //        }
        //    //    }
        //    //}
        //    //if (isTypedef)
        //    //{
        //    //    TypeSpecNode def = getTypeSpec(declar.declarspecs);
        //    //    TypedefNode tdnode = new TypedefNode(def);
        //    //    IdentNode idnode = declar.declarlist[0].declarnode.declar.ident;
        //    //    if (idnode == null)
        //    //    {
        //    //        idnode = declar.declarlist[0].declarnode.declar.chain.declar.declar.ident;
        //    //    }
        //    //    idnode.symtype = SYMTYPE.TYPEDEF;
        //    //    idnode.def = tdnode;                
        //    //}
        //    return isTypedef;
        //}

        //private TypeSpecNode getTypeSpec(List<DeclarSpecNode> list)
        //{
        //    List<TypeSpecNode> typespecs = new List<TypeSpecNode>();
        //    foreach (DeclarSpecNode spec in list)
        //    {
        //        if (spec is TypeSpecNode)
        //        {
        //            typespecs.Add((TypeSpecNode)spec);
        //        }
        //    }
        //    if (typespecs.Count == 1)
        //    {
        //        return typespecs[0];
        //    }
        //    else
        //    {
        //        BaseTypeSpecNode basespec = (BaseTypeSpecNode)typespecs[0];
        //        for (int i = 1; i < typespecs.Count; i++)
        //        {
        //            //basespec.setModifer(((BaseTypeSpecNode)typespecs[1]).baseclass);
        //        }
        //        return basespec;
        //    }
        //}

        //- struct/unions -----------------------------------------------------

        //public StructSpecNode makeStructSpec(StructUnionNode tag, IdentNode name, List<StructDeclarationNode> declarList)
        //{
        //    StructSpecNode node = new StructSpecNode(tag, name, declarList);
        //    name.def = node;
        //    return node;
        //}

        //public StructDeclarationNode makeStructDeclarationNode(List<DeclarSpecNode> specqual, List<StructDeclaratorNode> fieldnames)
        //{
        //    return null;
        //}

        //public StructDeclaratorNode makeStructDeclaractorNode(DeclaratorNode declarnode, ConstExpressionNode constexpr)
        //{
        //    return null;
        //}

        //- enums -------------------------------------------------------------

        //public EnumSpecNode makeEnumSpec(IdentNode idNode, List<EnumeratorNode> enumList)
        //{
        //    String id = idNode.ident;
        //    return new EnumSpecNode(id, enumList);
        //}

        //public EnumeratorNode makeEnumeratorNode(EnumConstantNode enumconst, ConstExpressionNode constexpr)
        //{
        //    return new EnumeratorNode(enumconst, constexpr);
        //}

        //public EnumConstantNode makeEnumConstNode(Token token)
        //{
        //    String id = token.chars;
        //    return new EnumConstantNode(id);
        //}
        
        //- declarators -------------------------------------------------------

        //public DeclaratorNode makePointerNode(TypeQualNode qualList, DeclaratorNode declar)
        //{
        //    //return new DeclaratorNode(qualList, declar);
        //    return null;
        //}

        //public ParamTypeListNode ParamTypeListNode(List<ParamDeclarNode> list, bool hasElipsis)
        //{
        //    return new ParamTypeListNode(list, hasElipsis);
        //}

        //public ParamDeclarNode makeParamDeclarNode(DeclarSpecNode declarspecs, DeclaratorNode declar, AbstractDeclaratorNode absdeclar)
        //{
        //    return new ParamDeclarNode(declarspecs, declar, absdeclar);
        //}

        //public TypeNameNode makeTypeNameNode(List<DeclarSpecNode> list, AbstractDeclaratorNode declar)
        //{
        //    return null;
        //}

        //- initializers ------------------------------------------------------

        //public InitializerNode makeInitializerNode(AssignExpressionNode expr)
        //{
        //    return null;
        //}

        //public InitializerNode makeInitializerNode(List<InitializerNode> list)
        //{
        //    return null;
        //}

        //public DesignationNode makeDesignationNode(List<DesignatorNode> list)
        //{
        //    return null; 
        //}

        //public DesignatorNode makeDesignatorNode(ConstExpressionNode expr)
        //{
        //    return null;
        //}

        //public DesignatorNode makeDesignatorNode(IdentNode ident)
        //{
        //    return null;
        //}

        //- identifiers -------------------------------------------------------------

        public DeclaratorNode makeIdentDeclaratorNode(string ident)
        {
            DeclaratorNode node = new DeclaratorNode();
            node.ident = ident;
            return node;
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

        //public IdentNode makeLabelIdentNode(Token token)
        //{
        //    String id = token.chars;
        //    IdentNode node = SymbolTable.addSymbol(curSymbolTable, id);
        //    node.symtype = SYMTYPE.LABEL;
        //    return node;
        //}

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

        //public IdentNode getEnumIdentNode(Token token)
        //{
        //    String id = token.chars;
        //    IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.ENUM);
        //    if (node == null)
        //    {
        //        node = SymbolTable.addSymbol(curSymbolTable, id);
        //        node.symtype = SYMTYPE.ENUM;
        //    }
        //    return node;
        //}

        ////these return declared ident nodes 
        //public IdentNode getDeclarIdentNode(String id)
        //{
        //    IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.DECLAR);
        //    return node;
        //}

        ////called before <makeFieldExprNode> and <makeRefFieldExprNode>
        //public IdentNode getFieldIdentNode(Token token)
        //{
        //    return null;
        //}

        //public IdentNode getArgIdentNode(Token token)
        //{
        //    return null;
        //}

        //public IdentNode getFieldInitializerNode(Token token)
        //{
        //    return null;
        //}

        //public IdentNode getLabelIdentNode(Token token)
        //{
        //    return null;
        //}

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

        //- statements --------------------------------------------------------

        public StatementNode makeBlockItemListNode(StatementNode list, OILNode item)
        {
            return null;
        }

        public CompoundStatementNode makeCompoundStatementNode(List<OILNode> list)
        {
            return null;
        }

        public StatementNode makeLabelStatementNode(Token labelId, StatementNode stmt)
        {
            return null;
        }

        public StatementNode makeExpressionStatementNode(ExprNode expr)
        {
            return null;
        }

        public StatementNode makeIfStatementNode(ExprNode expr, StatementNode thenstmt, StatementNode elsestmt)
        {
            return null;
        }

        public StatementNode makeSwitchStatementNode(ExprNode expr, StatementNode stmt)
        {
            return null;
        }

        public StatementNode makeCaseStatementNode(ExprNode expr, StatementNode stmt)
        {
            return null;
        }

        public StatementNode makeDefaultStatementNode(StatementNode stmt)
        {
            return null;
        }

        public StatementNode makeWhileStatementNode(ExprNode expr, StatementNode stmt)
        {
            return null;
        }

        public StatementNode makeDoStatementNode(StatementNode stmt, ExprNode expr)
        {
            return null;
        }

        public StatementNode makeForStatementNode(OILNode expr1, ExprNode expr2, ExprNode expr3, StatementNode stmt)
        {
            return null;
        }

        public StatementNode makeGotoStatementNode(Token ident)
        {
            return null;
        }

        public StatementNode makeContinueStatementNode()
        {
            return null;
        }

        public StatementNode makeBreakStatementNode()
        {
            return null;
        }

        public StatementNode makeReturnStatementNode(ExprNode expr)
        {
            return null;
        }

        //- expressions -------------------------------------------------------------

        public ExprNode getExprIdentNode(Token token)
        {
            return null;
        }

        public ExprNode makeIntegerConstantNode(Token token)
        {
            return null;
        }

        public ExprNode makeFloatConstantNode(Token token)
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

        public ExprNode getExprEnumNode(Token token)
        {
            return null;
        }

        public ExprNode makeSubexpressionNode(ExprNode expr)
        {
            return null;
        }

        public ExprNode makeTypeInitExprNode(ExprNode node)
        {
            return null;
        }

        public ExprNode makeIndexExprNode(ExprNode node, ExprNode expr)
        {
            return null;
        }

        public ExprNode makeFuncCallExprNode(ExprNode node, List<ExprNode> argList)
        {
            return null;
        }

        public ExprNode makeFieldExprNode(ExprNode node, OILNode idNode)
        {
            return null;
        }

        public ExprNode makeRefFieldExprNode(ExprNode node, OILNode idNode)
        {
            return null;
        }

        public ExprNode makePostPlusPlusExprNode(ExprNode node)
        {
            return null;
        }

        public ExprNode makePostMinusMinusExprNode(ExprNode node)
        {
            return null;
        }

        public ExprNode makeCastExprNode(String name, ExprNode rhs)
        {
            return null;
        }

        public ExprNode makePlusPlusExprNode(ExprNode node)
        {
            return null;
        }

        public ExprNode makeMinusMinusExprNode(ExprNode node)
        {
            return null;
        }

        public ExprNode makeUnaryCastExprNode(String uniOp, ExprNode castExpr)
        {
            return null;
        }

        public ExprNode makeSizeofUnaryExprNode(ExprNode node)
        {
            return null;
        }

        public ExprNode makeSizeofTypeExprNode(String name)
        {
            return null;
        }

        public ExprNode makeMultiplyExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new MultiplyExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeDivideExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new DivideExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeModuloExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new ModuloExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeAddExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new AddExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeSubtractExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new SubtractExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeShiftLeftExprNode(ExprNode lhs, ExprNode rhs)
        {
            //    return new ShiftLeftExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeShiftRightExprNode(ExprNode lhs, ExprNode rhs)
        {
            //    return new ShiftRightExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeLessThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new LessThanExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeGreaterThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new GreaterThanExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeLessEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new LessEqualExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeGreaterEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new GreaterEqualExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            //    return new EqualsExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeNotEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            //    return new NotEqualsExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new ANDExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeXORExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new XORExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeORExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new ORExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeLogicalANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new LogicalANDExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeLogicalORExprNode(ExprNode lhs, ExprNode rhs)
        {
            //return new LogicalORExprNode(lhs, rhs);
            return null;
        }

        public ExprNode makeConditionalExprNode(ExprNode lhs, ExprNode trueexpr, ExprNode falseexpr)
        {
            //return new ConditionalExprNode(lhs, trueexpr, falseexpr);
            return null;
        }

        public ExprNode makeAssignExpressionNode(ExprNode lhs, String oper, ExprNode rhs)
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

        internal StatementNode makeEmptyStatementNode()
        {
            throw new NotImplementedException();
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");