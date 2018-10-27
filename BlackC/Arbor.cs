/* ----------------------------------------------------------------------------
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
    class Arbor
    {
        Dictionary<string, int> typepdefids;

        public Arbor()
        {
            typepdefids = new Dictionary<string, int>();
        }

        //temproary kludge to get around ambiguity in C99's grammar between typedef and identifier 
        //see https://en.wikipedia.org/wiki/The_lexer_hack
        //this will be removed once the rest of the semantic analysis is up & running and this is not needed anymore
        //crazy eh?

        public bool isTypedef(String id)
        {
            bool result = false;
            if (typepdefids.ContainsKey(id))
            {
                result = true;
            }
            return result;
        }

        internal void buildFunctionDef()
        {
            throw new NotImplementedException();
        }

        internal void setTypeDef(string typeid)
        {
            typepdefids[typeid] = 0;
        }

        internal void unsetTypeDef(string typeid)
        {
            typepdefids.Remove(typeid);
        }

        internal IdentNode makeIdentifierNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal TypeQualNode makeTypeQualNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal FuncSpecNode makeFuncSpecNode(Token token)
        {
            throw new NotImplementedException();
        }

        //- struct/unions -----------------------------------------------------

        public StructSpecNode makeStructSpec(StructUnionNode tag, IdentNode name, List<StructDeclarationNode> declarList)
        {
            throw new NotImplementedException();
        }

        public StructUnionNode makeStructUnionNode(Token token)
        {
            throw new NotImplementedException();
        }

        public StructDeclarationNode makeStructDeclarationNode(List<DeclarSpecNode> specqual, List<StructDeclaratorNode> fieldnames)
        {
            throw new NotImplementedException();
        }

        public StructDeclaratorNode makeStructDeclaractorNode(DeclaratorNode declarnode, ExprNode constexpr)
        {
            throw new NotImplementedException();
        }

        //- enums -------------------------------------------------------------

        public EnumSpecNode makeEnumSpec(IdentNode idNode, List<EnumeratorNode> enumList)
        {
            throw new NotImplementedException();
        }

        public EnumeratorNode makeEnumeratorNode(EnumConstantNode enumconst, ExprNode constexpr)
        {
            throw new NotImplementedException();
        }

        public EnumConstantNode makeEnumConstNode(Token token)
        {
            throw new NotImplementedException();
        }

        public EnumConstantNode getEnumConstNode(Token token)
        {
            throw new NotImplementedException();
        }

        public BaseTypeSpecNode makeBaseTypeSpec(Token token)
        {
            throw new NotImplementedException();
        }

        public TypedefNode getTypedefNode(Token token)
        {
            throw new NotImplementedException();
        }

        public StorageClassNode makeStoreageClassNode(Token token)
        {
            throw new NotImplementedException();
        }

        public DeclarationNode makeDeclaration(List<DeclarSpecNode> declarspecs, List<InitDeclaratorNode> initdeclarlist)
        {
            throw new NotImplementedException();
        }

        public InitDeclaratorNode makeInitDeclaratorNode(DeclaratorNode declarnode, InitializerNode initialnode)
        {
            throw new NotImplementedException();
        }

        //- expressions -------------------------------------------------------------

        public ExprNode makeIdentExprNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal IntegerExprNode makeIntegerConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal FloatExprNode makeFloatConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal CharExprNode makeCharConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        internal StringExprNode makeStringConstantNode(Token token)
        {
            throw new NotImplementedException();
        }

        public EnumExprNode makeEnumExprNode(Token token)
        {
            throw new NotImplementedException();
        }

        public SubExpressionNode makeSubexpressionNode(ExpressionNode expr)
        {
            throw new NotImplementedException();
        }
        
        public TypeInitExprNode makeTypeInitExprNode(ExprNode node)
        {
            throw new NotImplementedException();
        }

        public IndexExprNode makeIndexExprNode(ExprNode node, ExpressionNode expr)
        {
            throw new NotImplementedException();
        }

        public FuncCallExprNode makeFuncCallExprNode(ExprNode node, List<AssignExpressionNode> argList)
        {
            throw new NotImplementedException();
        }

        public FieldExprNode makeFieldExprNode(ExprNode node, IdentNode idNode)
        {
            throw new NotImplementedException();
        }

        public RefFieldExprNode makeRefFieldExprNode(ExprNode node, IdentNode idNode)
        {
            throw new NotImplementedException();
        }

        public PostPlusPlusExprNode makePostPlusPlusExprNode(ExprNode node)
        {
            throw new NotImplementedException();
        }

        public PostMinusMinusExprNode makePostMinusMinusExprNode(ExprNode node)
        {
            throw new NotImplementedException();
        }
        
        public CastExprNode makeCastExprNode(TypeNameNode name, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public UnaryOperatorNode makeUnaryOperatorNode(Token token)
        {
            throw new NotImplementedException();
        }

        public PlusPlusExprNode makePlusPlusExprNode(ExprNode node)
        {
            throw new NotImplementedException();
        }

        public MinusMinusExprNode makeMinusMinusExprNode(ExprNode node)
        {
            throw new NotImplementedException();
        }

        public UnaryCastExprNode makeUnaryCastExprNode(UnaryOperatorNode uniOp, ExprNode castExpr)
        {
            throw new NotImplementedException();
        }

        public SizeofUnaryExprNode makeSizeofUnaryExprNode(ExprNode node)
        {
            throw new NotImplementedException();
        }

        public SizeofTypeExprNode makeSizeofTypeExprNode(TypeNameNode name)
        {
            throw new NotImplementedException();
        }
        
        public AddExprNode makeAddExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public SubtractExprNode makeSubtractExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public MultiplyExprNode makeMultiplyExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public DivideExprNode makeDivideExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public ModuloExprNode makeModuloExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public ShiftLeftExprNode makeShiftLeftExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public ShiftRightExprNode makeShiftRightExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public LessThanExprNode makeLessThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public GreaterThanExprNode makeGreaterThanExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public LessEqualExprNode makeLessEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public GreaterEqualExprNode makeGreaterEqualExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public EqualsExprNode makeEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public NotEqualsExprNode makeNotEqualsExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public ANDExprNode makeANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public XORExprNode makeXORExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public ORExprNode makeORExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public LogicalANDExprNode makeLogicalANDExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public LogicalORExprNode makeLogicalORExprNode(ExprNode lhs, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public ConditionalExprNode makeConditionalExprNode(ExprNode lhs, ExpressionNode expr, ExprNode condit)
        {
            throw new NotImplementedException();
        }

        public AssignExpressionNode makeAssignExpressionNode(ExprNode lhs, AssignOperatorNode oper, ExprNode rhs)
        {
            throw new NotImplementedException();
        }

        public AssignOperatorNode makeAssignOperatorNode(Token token)
        {
            throw new NotImplementedException();
        }

        public ExpressionNode makeExpressionNode(ExpressionNode node, AssignExpressionNode expr)
        {
            throw new NotImplementedException();
        }

        public ConstExpressionNode makeConstantExprNode(ExprNode condit)
        {
            throw new NotImplementedException();
        }

        internal PointerNode makePointerNode(List<TypeQualNode> qualList, PointerNode ptr)
        {
            throw new NotImplementedException();
        }

        internal TypeNameNode makeTypeNameNode(List<DeclarSpecNode> list, AbstractDeclaratorNode declar)
        {
            throw new NotImplementedException();
        }

        internal AbstractDeclaratorNode makeAbstractDeclaratorNode(PointerNode ptr, DirectDeclatorNode direct)
        {
            throw new NotImplementedException();
        }


        internal DirectDeclatorNode makeDirectAbstractIndexNode(AbstractDeclaratorNode declar, bool p, List<TypeQualNode> list, bool p_2, AssignExpressionNode assign)
        {
            throw new NotImplementedException();
        }

        internal DirectDeclatorNode makeDirectAbstractParamNode(AbstractDeclaratorNode declar, ParamTypeListNode list)
        {
            throw new NotImplementedException();
        }
    }
}
