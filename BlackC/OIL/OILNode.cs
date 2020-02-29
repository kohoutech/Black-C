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

namespace Origami.OIL
{
    //base OIL class
    public class OILNode
    {
        public OILType type;
    }

    //- external defs ---------------------------------------------------------

    public class Module
    {
        public String name;
        public List<TypeDeclNode> typedefs;
        public List<VarDeclNode> globals;
        public List<FuncDeclNode> funcs;

        public Module(String _name)
        {
            name = _name;
            typedefs = new List<TypeDeclNode>();
            globals = new List<VarDeclNode>();
            funcs = new List<FuncDeclNode>();
        }
    }

    //- declarations ----------------------------------------------------------

    public class Declaration : OILNode
    {
        //public DeclSpecNode declspecs;
        //public List<DeclaratorNode> declarList;
        public List<OILNode> decls;

        public Declaration()
        {
            //    declspecs = specs;
            //    declarList = list;
            decls = new List<OILNode>();
        }
    }

    public class TypeDeclNode : Declaration
    {
        public string name;

        public TypeDeclNode(string _name)
        {
            type = OILType.TypeDecl;
            name = _name;
        }
    }

    // public class TypeSpecNode : DeclarSpecNode
    // {
    // }

    public class VarDeclNode : OILNode
    {
        public string name;
        public TypeDeclNode varType;
        public InitializerNode initializer;
        
        public VarDeclNode()
        {
            type = OILType.VarDecl;
            name = "";
            varType = null;
            initializer = null;
        }
    }

    public class FuncDeclNode : Declaration
    {
        public String name;
        public TypeDeclNode returnType;
        public List<ParamDeclNode> paramList;
        public List<VarDeclNode> locals;
        public List<StatementNode> body;
        public bool isFuncDef;

        public FuncDeclNode()
        {
            type = OILType.FuncDecl;
            name = "";
            returnType = null;
            paramList = null;
            body = null;
            isFuncDef = false;
        }
    }

    public class DeclSpecNode : OILNode
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

    //- struct/unions/enums -----------------------------------------------

    //public class StructSpecNode : TypeDeclNode
    // {
    //     StructUnionNode tag;
    //     IdentNode name; 
    //     List<StructDeclarationNode> declarList;

    //     public StructSpecNode(StructUnionNode _tag, IdentNode _name, List<StructDeclarationNode> _declarList)
    //     {
    //         tag = _tag;
    //         name = _name;
    //         declarList = _declarList;
    //     }        
    // }

    // public class StructUnionNode : ParseNode
    // {
    //     public enum LAYOUT { STRUCT, UNION }
    //     LAYOUT layout;

    //     public StructUnionNode(LAYOUT _layout) 
    //     {
    //         layout = _layout;
    //     }
    // }

    public class StructDeclNode : TypeDeclNode
    {
        public StructDeclNode()
            : base("struct")
        {
        }
    }

    public class StructDeclarationNode : OILNode
    {
    }


    public class StructDeclaratorNode : OILNode
    {
    }

    public class EnumDeclNode : TypeDeclNode
    {
        private IdentNode idNode;

        //     public String id;
        //     public List<EnumeratorNode> enumList;

        public EnumDeclNode()
            : base("enum")
        {
        }

        public EnumDeclNode(IdentNode idNode)
            : base("enum")
        {
            // TODO: Complete member initialization
            this.idNode = idNode;
        }

        //     public EnumSpecNode(string _id, List<EnumeratorNode> _list)
        //     {
        //         id = id;
        //         enumList = _list;
        //     }
    }

    public class EnumeratorNode : OILNode
    {
        //     public EnumConstantNode name;
        //     public ConstExpressionNode expr;

        //     public EnumeratorNode(EnumConstantNode _name, ConstExpressionNode _expr)
        //     {
        //         name = _name;
        //         expr = _expr;
        //     }
    }

    public class EnumConstantNode : OILNode
    {
        //     String id;

        //     public EnumConstantNode(String _id)
        //     {
        //         id = _id;
        //     }
    }

    public class TypeQualNode : OILNode
    {
        //     public bool isConst;
        //     public bool isRestrict;
        //     public bool isVolatile;
        public bool isEmpty;

        //     public TypeQualNode()
        //     {
        //         isConst = false;
        //         isRestrict = false;
        //         isVolatile = false;
        //         isEmpty = true;
        //     }

        //     public void setQualifer(Token token)
        //     {
        //         switch (token.type)
        //         {
        //             case TokenType.tCONST:
        //                 isConst = true;
        //                 break;

        //             case TokenType.tRESTRICT:
        //                 isRestrict = true;
        //                 break;

        //             case TokenType.tVOLATILE:
        //                 isVolatile = true;
        //                 break;
        //         }
        //         isEmpty = false;
        //     }
    }

    //- declarators -------------------------------------------------------

    public class DeclaratorNode : OILNode
    {
        public DeclaratorNode next;

        public DeclaratorNode()
        {
            next = null;
        }
    }

    public class PointerDeclNode : DeclaratorNode
    {
        public bool isConst;
        public bool isRestrict;
        public bool isVolatile;

        public PointerDeclNode()
            : base()
        {
            isConst = false;
            isRestrict = false;
            isVolatile = false;
        }
    }

    public class IdentDeclaratorNode : DeclaratorNode
    {
        public String ident;

        public IdentDeclaratorNode(string id)
            : base()
        {
            ident = id;
        }
    }

    public class ArrayDeclaratorNode : DeclaratorNode
    {
    }

    public class ParamListNode : DeclaratorNode
    {
        public List<ParamDeclNode> paramList;
        public bool hasElipsis;

        public ParamListNode(List<ParamDeclNode> _list, bool _hasElipsis)
            : base()
        {
            paramList = _list;
            hasElipsis = _hasElipsis;
        }
    }

    public class ParamDeclNode : OILNode
    {
        public string name;
        public TypeDeclNode type;

        public ParamDeclNode(string _name, TypeDeclNode _type)
        {
            name = _name;
            type = _type;
        }
    }

    public class TypeNameNode : OILNode
    {
    }

    // public class TypedefNode : ParseNode
    // {
    //     public TypeSpecNode typedef;

    //     public TypedefNode(TypeSpecNode def)
    //     {
    //         typedef = def;
    //     }
    // }

    //- initializers ------------------------------------------------------

    public class InitializerNode : OILNode
    {
        public ExprNode initExpr;

        public InitializerNode(ExprNode _initExpr)
        {
            initExpr = _initExpr;
        }
    }

    // public class InitializerNode : ParseNode
    // {
    //     public void addDesignation(DesignationNode desinode)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }    

    //public class IdentDeclaratorNode : OILNode
    //{
    //    private string p;

    //    public IdentDeclaratorNode(string p)
    //    {
    //        // TODO: Complete member initialization
    //        this.p = p;
    //    }
    //}

    // public class InitDeclaratorNode : ParseNode
    // {
    //     public DeclaratorNode declarnode;
    //     public InitializerNode initialnode;

    //     public InitDeclaratorNode(DeclaratorNode declar, InitializerNode initial)
    //     {
    //         declarnode = declar;
    //         initialnode = initial;
    //     }
    // }

    public class DesignationNode : OILNode
    {
    }

    public class IdentNode : OILNode
    {
        //     public String ident;
        //     public ParseNode def;
        //     public SYMTYPE symtype;

        //     public IdentNode(String id)
        //     {
        //         ident = id;
        //         def = null;
        //         symtype = SYMTYPE.UNSET;
        //     }
    }

    //- statements ------------------------------------------------------------

    public class StatementNode : OILNode
    {
        public ExprNode expr;
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
        public List<Declaration> decls;
        public List<StatementNode> stmts;

        public CompoundStatementNode()
        {
            decls = new List<Declaration>();
            stmts = new List<StatementNode>();
        }
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
        public ExprNode retval;

        public ReturnStatementNode(ExprNode _val)
        {
            type = OILType.ReturnStmt;
            retval = _val;
        }
    }

    //- expressions -----------------------------------------------------------

    public class ExprNode : OILNode
    {
        
    }

    public class IntConstant : ExprNode
    {
        public int value;

        public IntConstant(int _value)
        {
            type = OILType.IntConst;
            value = _value;
        }
    }

    public class FloatConstant : ExprNode
    {
        public double value;

        public FloatConstant(double _value)
        {
            type = OILType.FloatConst;
            value = _value;
        }
    }

    public class CharConstant : ExprNode
    {
        public int value;
    }

    public class StringConstant : ExprNode
    {
        public int value;
    }

    //public class IdentExprNode : ExprNode
    //{
    //}

    //public class IntegerExprNode : ExprNode
    //{
    //}

    //public class FloatExprNode : ExprNode
    //{
    //}

    //public class CharExprNode : ExprNode
    //{
    //}

    //public class StringExprNode : ExprNode
    //{
    //}

    //public class EnumExprNode : ExprNode
    //{
    //}

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

    public class ArgumentExprNode : ExprNode
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

    public class UnaryOperatorNode : ExprNode
    {
        public enum OPERATOR { AMPERSAND, STAR, PLUS, MINUS, TILDE, EXCLAIM };
        //     OPERATOR op;

        //     public UnaryOperatorNode(OPERATOR _op)
        //     {
        //         op = _op;
        //     }
    }

    public class SizeofTypeExprNode : ExprNode
    {
    }

    public class SizeofUnaryExprNode : ExprNode
    {
    }

    public class UnaryCastExprNode : ExprNode
    {
    }

    //public class CastExprNode : ExprNode
    //{
    //}

    public class MultiplyExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public MultiplyExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class DivideExprNode : ExprNode
    {
        //      ExprNode lhs, rhs;

        //     public DivideExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class ModuloExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public ModuloExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class AddExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public AddExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class SubtractExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public SubtractExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class ShiftLeftExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public ShiftLeftExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class ShiftRightExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public ShiftRightExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class LessThanExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public LessThanExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class GreaterThanExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public GreaterThanExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class LessEqualExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public LessEqualExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class GreaterEqualExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public GreaterEqualExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class EqualsExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public EqualsExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class NotEqualsExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public NotEqualsExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class ANDExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public ANDExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class XORExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public XORExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class ORExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public ORExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class LogicalANDExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public LogicalANDExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class LogicalORExprNode : ExprNode
    {
        //     ExprNode lhs, rhs;

        //     public LogicalORExprNode(ExprNode _lhs, ExprNode _rhs)
        //     {
        //         lhs = _lhs;
        //         rhs = _rhs;
        //     }
    }

    public class ConditionalExprNode : ExprNode
    {
        //     ExprNode lhs;
        //     ExpressionNode trueexpr;
        //     ExprNode falseexpr;

        //     public ConditionalExprNode(ExprNode _lhs, ExpressionNode _trueexpr, ExprNode _falseexpr)
        //     {
        //         lhs = _lhs;
        //         trueexpr = _trueexpr;
        //         falseexpr = _falseexpr;
        //     }
    }

    public class AssignExpressionNode : ExprNode
    {
        public ExprNode lhs;

        public AssignExpressionNode(ExprNode _lhs)
        {
            lhs = _lhs;
        }
    }

    public enum ASSIGNOP
    {
        EQUAL, MULTEQUAL, SLASHEQUAL, PERCENTEQUAL, PLUSEQUAL, MINUSEQUAL,
        LESSLESSEQUAL, GTRGTREQUAL, AMPEQUAL, CARETEQUAL, BAREQUAL
    }

    // public class AssignOperatorNode : ParseNode
    // {     
    //     OPERATOR op;

    //     public AssignOperatorNode(OPERATOR _op)
    //     {
    //         op = _op;
    //     }
    // }

    public class ExpressionNode : ExprNode
    {
    }

    public class ConstExpressionNode : ExprNode
    {
    }

    //-------------------------------------------------------------------------

    public enum OILType
    {
        TypeDecl,
        VarDecl,
        FuncDecl,

        ReturnStmt,

        IntConst,
        FloatConst
    }
}
