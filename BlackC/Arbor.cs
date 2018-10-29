﻿/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2018  George E Greaney

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

using Origami.AST;

//arbor - a place where trees are grown

namespace BlackC
{
    public class Arbor
    {
        Dictionary<string, int> typepdefids;
        public SymbolTable curSymbolTable;

        public Arbor()
        {
            typepdefids = new Dictionary<string, int>();
            curSymbolTable = null;
        }

        //temproary kludge to get around ambiguity in C99's grammar between typedef and identifier 
        //see https://en.wikipedia.org/wiki/The_lexer_hack
        //this will be removed once the rest of the semantic analysis is up & running and this is not needed anymore
        //crazy eh?

        //public bool isTypedef(String id)
        //{
        //    bool result = false;
        //    if (typepdefids.ContainsKey(id))
        //    {
        //        result = true;
        //    }
        //    return result;
        //}

        //cruft
        public void setTypeDef(string typeid)
        {
            typepdefids[typeid] = 0;
        }

        public void unsetTypeDef(string typeid)
        {
            typepdefids.Remove(typeid);
        }

        //- symbol table ------------------------------------------------------------

        public SymbolTable pushSymbolTable()
        {
            SymbolTable newtbl = new SymbolTable(curSymbolTable);
            curSymbolTable = newtbl;
            return newtbl;
        }

        public SymbolTable popSymbolTable()
        {
            SymbolTable oldtbl = curSymbolTable;
            curSymbolTable = curSymbolTable.parent;
            return oldtbl;
        }

        //- identifiers -------------------------------------------------------------

        //these only return new ident nodes
        public IdentNode makeDeclarIdentNode(Token token)
        {
            String id = ((tIdentifier)token).ident;
            IdentNode node = SymbolTable.addSymbol(curSymbolTable, id);
            node.symtype = SYMTYPE.DECLAR;
            return node;
        }

        public IdentNode makeLabelIdentNode(Token token)
        {
            String id = ((tIdentifier)token).ident;
            IdentNode node = SymbolTable.addSymbol(curSymbolTable, id);
            node.symtype = SYMTYPE.LABEL;
            return node;
        }

        //these return either declared or new ident nodes 
        public IdentNode getStructIdentNode(Token token)
        {
            String id = ((tIdentifier)token).ident;
            IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.STRUCT);
            if (node == null)
            {
                node = SymbolTable.addSymbol(curSymbolTable, id);
                node.symtype = SYMTYPE.STRUCT;
            }            
            return node;
        }

        public IdentNode getEnumIdentNode(Token token)
        {
            String id = ((tIdentifier)token).ident;
            IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.ENUM);
            if (node == null)
            {
                node = SymbolTable.addSymbol(curSymbolTable, id);
                node.symtype = SYMTYPE.ENUM;
            }
            return node;
        }

        //these return declared ident nodes 
        public IdentNode getDeclarIdentNode(String id)
        {
            IdentNode node = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.DECLAR);
            return node;
        }

        //called before <makeFieldExprNode> and <makeRefFieldExprNode>
        public IdentNode getFieldIdentNode(Token token)
        {
            return null;
        }

        public IdentNode getArgIdentNode(Token token)
        {
            return null;
        }

        public IdentNode getFieldInitializerNode(Token token)
        {
            return null;
        }

        public IdentNode getLabelIdentNode(Token token)
        {
            return null;
        }

        public TypedefNode getTypedefNode(Token token)
        {
            TypedefNode node = null;
            if (token is tIdentifier)
            {
                String id = ((tIdentifier)token).ident;
                IdentNode idnode = SymbolTable.findSymbol(curSymbolTable, id, SYMTYPE.TYPEDEF);
                if (idnode != null)
                {
                    node = (TypedefNode)idnode.def;
                }
            }
            return node;
        }

        //- expressions -------------------------------------------------------------

        public ExprNode getExprIdentNode(Token token)
        {
            return null;
        }

        public IntegerExprNode makeIntegerConstantNode(Token token)
        {
            return null;
        }

        public FloatExprNode makeFloatConstantNode(Token token)
        {
            return null;
        }

        public CharExprNode makeCharConstantNode(Token token)
        {
            return null;
        }

        public StringExprNode makeStringConstantNode(Token token)
        {
            return null;
        }

        public EnumExprNode getExprEnumNode(Token token)
        {
            return null;
        }

        public SubExpressionNode makeSubexpressionNode(ExpressionNode expr)
        {
            return null;
        }

        public TypeInitExprNode makeTypeInitExprNode(ExprNode node)
        {
            return null;
        }

        public IndexExprNode makeIndexExprNode(ExprNode node, ExpressionNode expr)
        {
            return null;
        }

        public FuncCallExprNode makeFuncCallExprNode(ExprNode node, List<AssignExpressionNode> argList)
        {
            return null;
        }

        public FieldExprNode makeFieldExprNode(ExprNode node, IdentNode idNode)
        {
            return null;
        }

        public RefFieldExprNode makeRefFieldExprNode(ExprNode node, IdentNode idNode)
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

        public CastExprNode makeCastExprNode(TypeNameNode name, ExprNode rhs)
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

        public UnaryCastExprNode makeUnaryCastExprNode(UnaryOperatorNode uniOp, ExprNode castExpr)
        {
            return null;
        }

        public SizeofUnaryExprNode makeSizeofUnaryExprNode(ExprNode node)
        {
            return null;
        }

        public SizeofTypeExprNode makeSizeofTypeExprNode(TypeNameNode name)
        {
            return null;            
        }

        public MultiplyExprNode makeMultiplyExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new MultiplyExprNode(lhs, rhs);
        }

        public DivideExprNode makeDivideExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new DivideExprNode(lhs, rhs);
        }

        public ModuloExprNode makeModuloExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new ModuloExprNode(lhs, rhs);
        }

        public AddExprNode makeAddExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new AddExprNode(lhs, rhs);
        }

        public SubtractExprNode makeSubtractExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new SubtractExprNode(lhs, rhs);
        }

        public ShiftLeftExprNode makeShiftLeftExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new ShiftLeftExprNode(lhs, rhs);
        }

        public ShiftRightExprNode makeShiftRightExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new ShiftRightExprNode(lhs, rhs);
        }

        public LessThanExprNode makeLessThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new LessThanExprNode(lhs, rhs);
        }

        public GreaterThanExprNode makeGreaterThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new GreaterThanExprNode(lhs, rhs);
        }

        public LessEqualExprNode makeLessEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new LessEqualExprNode(lhs, rhs);
        }

        public GreaterEqualExprNode makeGreaterEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new GreaterEqualExprNode(lhs, rhs);
        }

        public EqualsExprNode makeEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new EqualsExprNode(lhs, rhs);
        }

        public NotEqualsExprNode makeNotEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new NotEqualsExprNode(lhs, rhs);
        }

        public ANDExprNode makeANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new ANDExprNode(lhs, rhs);
        }

        public XORExprNode makeXORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new XORExprNode(lhs, rhs);
        }

        public ORExprNode makeORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new ORExprNode(lhs, rhs);
        }

        public LogicalANDExprNode makeLogicalANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new LogicalANDExprNode(lhs, rhs);
        }

        public LogicalORExprNode makeLogicalORExprNode(ExprNode lhs, ExprNode rhs)
        {
            return new LogicalORExprNode(lhs, rhs);
        }

        public ConditionalExprNode makeConditionalExprNode(ExprNode lhs, ExpressionNode trueexpr, ExprNode falseexpr)
        {
            return new ConditionalExprNode(lhs, trueexpr, falseexpr);
        }

        public AssignExpressionNode makeAssignExpressionNode(ExprNode lhs, AssignOperatorNode oper, ExprNode rhs)
        {
            return null;
        }

        public ExpressionNode makeExpressionNode(ExpressionNode node, AssignExpressionNode expr)
        {
            return null;
        }

        public ConstExpressionNode makeConstantExprNode(ExprNode condit)
        {
            return null;
        }

        //- declarations --------------------------------------------------------

        public DeclarationNode makeDeclaration(List<DeclarSpecNode> specs, List<InitDeclaratorNode> list)
        {
            return new DeclarationNode(specs, list);
        }

        public InitDeclaratorNode makeInitDeclaratorNode(DeclaratorNode declar, InitializerNode initial)
        {
            return new InitDeclaratorNode(declar, initial);
        }

        //- struct/unions -----------------------------------------------------

        public StructSpecNode makeStructSpec(StructUnionNode tag, IdentNode name, List<StructDeclarationNode> declarList)
        {
            return null;
        }

        public StructDeclarationNode makeStructDeclarationNode(List<DeclarSpecNode> specqual, List<StructDeclaratorNode> fieldnames)
        {
            return null;
        }

        public StructDeclaratorNode makeStructDeclaractorNode(DeclaratorNode declarnode, ConstExpressionNode constexpr)
        {
            return null;
        }

        //- enums -------------------------------------------------------------

        public EnumSpecNode makeEnumSpec(IdentNode idNode, List<EnumeratorNode> enumList)
        {
            String id = idNode.ident;
            return new EnumSpecNode(id, enumList);
        }

        public EnumeratorNode makeEnumeratorNode(EnumConstantNode enumconst, ConstExpressionNode constexpr)
        {
            return new EnumeratorNode(enumconst, constexpr);
        }

        public EnumConstantNode makeEnumConstNode(Token token)
        {
            String id = ((tIdentifier)token).ident;
            return new EnumConstantNode(id);
        }

        //- declarators -------------------------------------------------------

        public DeclaratorNode makeDeclaratorNode(PointerNode ptr, DirectDeclaratorNode declar)
        {
            return new DeclaratorNode(ptr, declar);
        }

        public DirectDeclaratorNode makeDirectIdentNode(IdentNode ident)
        {
            DirectDeclaratorNode node = new DirectDeclaratorNode();
            node.ident = ident;
            return node;
        }

        public DirectDeclaratorNode makeDirectDeclarNode(DeclaratorNode declar)
        {
            return null;
        }

        public DirectDeclaratorNode makeDirectIndexNode(DirectDeclaratorNode node, bool p, List<TypeQualNode> list, bool p_2, AssignExpressionNode assign)
        {
            return null;
        }

        public DirectDeclaratorNode makeDirectParamNode(DirectDeclaratorNode node, global::BlackC.ParamTypeListNode list)
        {
            return null;
        }

        public DirectDeclaratorNode makeDirectArgumentNode(DirectDeclaratorNode node, List<IdentNode> list)
        {
            return null;
        }

        public PointerNode makePointerNode(List<TypeQualNode> qualList, PointerNode ptr)
        {
            return null;
        }

        public ParamTypeListNode ParamTypeListNode(List<ParamDeclarNode> list, bool p)
        {
            return null;
        }

        public ParamDeclarNode makeParamDeclarNode(List<DeclarSpecNode> declarspecs, DeclaratorNode declar, AbstractDeclaratorNode absdeclar)
        {
            return null;
        }

        public TypeNameNode makeTypeNameNode(List<DeclarSpecNode> list, AbstractDeclaratorNode declar)
        {
            return null;
        }

        public AbstractDeclaratorNode makeAbstractDeclaratorNode(PointerNode ptr, DirectAbstractNode direct)
        {
            return null;
        }

        public DirectAbstractNode makeDirectAbstractDeclarNode(AbstractDeclaratorNode declar)
        {
            return null; 
        }

        public DirectAbstractNode makeDirectAbstractParamNode(DirectAbstractNode node, global::BlackC.ParamTypeListNode list)
        {
            return null;
        }

        public DirectAbstractNode makeDirectAbstractIndexNode(DirectAbstractNode node, bool p, List<TypeQualNode> list, bool p_2, AssignExpressionNode assign)
        {
            return null;
        }

        //- declaration initializers ------------------------------------

        public InitializerNode makeInitializerNode(AssignExpressionNode expr)
        {
            return null;
        }

        public InitializerNode makeInitializerNode(List<InitializerNode> list)
        {
            return null;
        }

        public DesignationNode makeDesignationNode(List<DesignatorNode> list)
        {
            return null; 
        }

        public DesignatorNode makeDesignatorNode(ConstExpressionNode expr)
        {
            return null;
        }

        public DesignatorNode makeDesignatorNode(IdentNode ident)
        {
            return null;
        }

        //- statements --------------------------------------------------------

        public LabelStatementNode makeLabelStatement(IdentNode labelId)
        {
            return null;
        }

        public CaseStatementNode makeCaseStatementNode(ConstExpressionNode expr, StatementNode stmt)
        {
            return null;
        }

        public DefaultStatementNode makeDefaultStatementNode(StatementNode stmt)
        {
            return null;
        }

        public CompoundStatementNode makeCompoundStatementNode(List<BlockItemNode> list)
        {
            return null;
        }

        public ExpressionStatementNode makeExpressionStatement(ExpressionNode expr)
        {
            return null;
        }

        public EmptyStatementNode makeEmptyStatement(ExpressionNode expr)
        {
            return null;
        }

        public IfStatementNode makeIfStatementNode(ExpressionNode expr, StatementNode thenstmt, StatementNode elsestmt)
        {
            return null;
        }

        public SwitchStatementNode makeSwitchStatement(ExpressionNode expr, StatementNode stmt)
        {
            return null;
        }

        public WhileStatementNode makeWhileStatementNode(ExpressionNode expr, StatementNode stmt)
        {
            return null;
        }

        public DoStatementNode makeDoStatementNode(StatementNode stmt, ExpressionNode expr)
        {
            return null;
        }

        public ForStatementNode makeForStatementNode(DeclarationNode declar, ExpressionNode expr2, ExpressionNode expr3, StatementNode stmt)
        {
            return null;
        }

        public ForStatementNode makeForStatementNode(ExpressionNode expr1, ExpressionNode expr2, ExpressionNode expr3, StatementNode stmt)
        {
            return null;
        }

        public GotoStatementNode makeGotoStatementNode(IdentNode idNode)
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

        public ReturnStatementNode makeReturnStatementNode(ExpressionNode expr)
        {
            return null;
        }

        public FunctionDefNode makeFunctionDefNode(List<DeclarSpecNode> specs, DeclaratorNode signature, 
            List<DeclarationNode> oldparamlist, StatementNode block)
        {
            return new FunctionDefNode(specs, signature, oldparamlist, block);
        }

        public bool handleTypeDef(DeclarationNode declar)
        {
            List<DeclarSpecNode> specs = declar.declarspecs;
            bool isTypedef = false;
            foreach (DeclarSpecNode spec in specs)
            {
                if (spec is StorageClassNode)
                {
                    StorageClassNode storspec = (StorageClassNode)spec;
                    if (storspec.storage == StorageClassNode.STORAGE.TYPEDEF)
                    {
                        isTypedef = true;
                        break;
                    }
                }
            }
            if (isTypedef)
            {
                TypeSpecNode def = getTypeSpec(declar.declarspecs);
                TypedefNode tdnode = new TypedefNode(def);
                IdentNode idnode = declar.declarlist[0].declarnode.declar.ident;
                idnode.symtype = SYMTYPE.TYPEDEF;
                idnode.def = tdnode;                
            }
            return isTypedef;
        }

        private TypeSpecNode getTypeSpec(List<DeclarSpecNode> list)
        {
            List<TypeSpecNode> typespecs = new List<TypeSpecNode>();
            foreach (DeclarSpecNode spec in list)
            {
                if (spec is TypeSpecNode)
                {
                    typespecs.Add((TypeSpecNode)spec);
                }
            }
            if (typespecs.Count == 1)
            {
                return typespecs[0];
            }
            else
            {
                BaseTypeSpecNode basespec = (BaseTypeSpecNode)typespecs[0];
                for (int i = 1; i < typespecs.Count; i++)
                {
                    basespec.setModifer(((BaseTypeSpecNode)typespecs[1]).baseclass);
                }
                return basespec;
            }
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");