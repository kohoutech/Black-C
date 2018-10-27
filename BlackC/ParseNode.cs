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

using Origami.AST;

namespace BlackC
{
    public class ParseNode : Node
    {
    }

    public class IdentNode : Node
    {
    }

    //- expressions -----------------------------------------------------------

    public class ExprNode : ParseNode
    {
    }

    public class IdentExprNode : ExprNode
    {
    }

    public class IntegerExprNode : ExprNode
    {
    }

    public class FloatExprNode : ExprNode
    {
    }

    public class CharExprNode : ExprNode
    {
    }

    public class StringExprNode : ExprNode
    {
    }

    public class EnumExprNode : ExprNode
    {
    }

    public class SubExpressionNode : ExprNode
    {
    }

    public class TypeInitExprNode : ExprNode
    {
    }

    public class IndexExprNode : ExprNode
    {
    }

    public class FuncCallExprNode : ExprNode
    {
    }

    public class FieldExprNode : ExprNode
    {
    }

    public class RefFieldExprNode : ExprNode
    {
    }

    public class PostPlusPlusExprNode : ExprNode
    {
    }

    public class PostMinusMinusExprNode : ExprNode
    {
    }

    public class PlusPlusExprNode : ExprNode
    {
    }
    
    public class MinusMinusExprNode : ExprNode
    {
    }
    
    public class UnaryCastExprNode : ExprNode
    {
    }

    public class SizeofUnaryExprNode : ExprNode
    {
    }

    public class SizeofTypeExprNode : ExprNode
    {
    }

    public class UnaryOperatorNode : ParseNode
    {
    }

    public class CastExprNode : ExprNode
    {
    }

    public class MultiplyExprNode : ExprNode
    {
    }

    public class DivideExprNode : ExprNode
    {
    }

    public class ModuloExprNode : ExprNode
    {
    }

    public class AddExprNode : ExprNode
    {        
    }

    public class SubtractExprNode : ExprNode
    {
    }

    public class ShiftLeftExprNode : ExprNode
    {     
    }

    public class ShiftRightExprNode : ExprNode
    {
    }

    public class LessThanExprNode : ExprNode
    {     
    }

    public class GreaterThanExprNode : ExprNode
    {
    }

    public class LessEqualExprNode : ExprNode
    {
    }

    public class GreaterEqualExprNode : ExprNode
    {
    }

    public class EqualsExprNode : ExprNode
    {     
    }

    public class NotEqualsExprNode : ExprNode
    {
    }

    public class ANDExprNode : ExprNode
    {     
    }

    public class XORExprNode : ExprNode
    {     
    }

    public class ORExprNode : ExprNode
    {        
    }

    public class LogicalANDExprNode : ExprNode
    {     
    }

    public class LogicalORExprNode : ExprNode
    {     
    }

    public class ConditionalExprNode : ExprNode
    {     
    }

    public class AssignExpressionNode : ExprNode
    {     
    }

    public class AssignOperatorNode : ParseNode
    {     
    }

    public class ExpressionNode : ParseNode
    {
    }

    public class ConstExpressionNode : ParseNode
    {
    }

    //- declarations ----------------------------------------------------------

    public class DeclarationNode : BlockItemNode
    {
    }

    public class InitDeclaratorNode : ParseNode
    {
    }

    public class DeclarSpecNode : ParseNode
    {
    }

    public class StorageClassNode : DeclarSpecNode
    {
    }

    public class TypeSpecNode : DeclarSpecNode
    {
    }

    public class BaseTypeSpecNode : TypeSpecNode
    {
    }

    public class StructSpecNode : TypeSpecNode
    {
    }

    public class StructUnionNode : ParseNode
    {
    }

    public class StructDeclarationNode : ParseNode
    {
    }

    public class StructDeclaratorNode : ParseNode
    {
    }

    public class EnumSpecNode : TypeSpecNode
    {
    }

    public class EnumeratorNode : ParseNode
    {
    }

    public class EnumConstantNode : ParseNode
    {
    }

    public class TypeQualNode : DeclarSpecNode
    {
    }

    public class FuncSpecNode : DeclarSpecNode
    {
    }

    public class DeclaratorNode : ParseNode
    {
    }

    public class DirectDeclaratorNode : ParseNode
    {
    }

    public class PointerNode : ParseNode
    {
    }

    public class ParamTypeListNode : ParseNode
    {
    }

    public class ParamDeclarNode : ParseNode
    {
    }

    public class TypeNameNode : ParseNode
    {
    }

    public class AbstractDeclaratorNode : ParseNode
    {
    }

    public class DirectAbstractNode : ParseNode
    {
    }

    public class TypedefNode : TypeSpecNode
    {
    }

    public class InitializerNode : ParseNode
    {
    }

    //- statements ----------------------------------------------------------

    public class StatementNode : BlockItemNode
    {
    }

    public class LabelStatementNode : StatementNode
    {
    }

    public class CaseStatementNode : StatementNode
    {
    }

    public class DefaultStatementNode : StatementNode
    {
    }

    public class CompoundStatementNode : StatementNode
    {
    }

    public class ExpressionStatementNode : StatementNode
    {
    }

    public class EmptyStatementNode : StatementNode
    {
    }

    public class IfStatementNode : StatementNode
    {
    }
    
    public class SwitchStatementNode : StatementNode
    {
    }
    
    public class WhileStatementNode : StatementNode
    {
    }

    public class DoStatementNode : StatementNode
    {
    }

    public class ForStatementNode : StatementNode
    {
    }

    public class GotoStatementNode : StatementNode
    {
    }

    public class ContinueStatementNode : StatementNode
    {
    }

    public class BreakStatementNode : StatementNode
    {
    }

    public class ReturnStatementNode : StatementNode
    {
    }

    public class BlockItemNode : ParseNode
    {
    }

}
