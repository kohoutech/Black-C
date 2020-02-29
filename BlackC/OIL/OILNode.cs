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

    public class TypeDeclNode : OILNode
    {
        public string name;

        public TypeDeclNode(string _name)
        {
            type = OILType.TypeDecl;
            name = _name;
        }
    }

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

    public class FuncDeclNode : OILNode
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

    //base statement class
    public class StatementNode : OILNode
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

    public class DeclarationStatementNode : StatementNode
    {
        public OILNode decl;

        public DeclarationStatementNode(OILNode _decl)
        {
            type = OILType.DeclarationStmt;
            decl = _decl;
        }
    }

    public class ExpressionStatementNode : StatementNode
    {
        public ExprNode expr;

        public ExpressionStatementNode(ExprNode _val)
        {
            type = OILType.ExpressionStmt;
            expr = _val;
        }
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
        public List<OILNode> decl1;
        public ExprNode expr1;
        public ExprNode expr2;
        public ExprNode expr3;
        public StatementNode body;

        public ForStatementNode(List<OILNode> _decl1, ExprNode _expr1, ExprNode _expr2, ExprNode _expr3, StatementNode _body)
        {
            type = OILType.ForStmt;
            decl1 = _decl1;
            expr1 = _expr1;
            expr2 = _expr2;
            expr3 = _expr3;
            body = _body;
        }
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

    public class IdentExprNode : ExprNode
    {
        public OILNode idsym;

        public IdentExprNode(OILNode _idsym)
        {
            idsym = _idsym;
        }
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

    public class UnaryOperatorNode : ExprNode
    {
        public enum OPERATOR { AMPERSAND, STAR, TILDE, EXCLAIM };
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

    public class ArithmeticExprNode : ExprNode
    {
        public enum OPERATOR
        {
            PLUS, MINUS, INC, DEC, ADD, SUB, MULT, DIV, MOD
        }

        OPERATOR op;
        ExprNode lhs, rhs;

        public ArithmeticExprNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.ArithmeticExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ComparisonExprNode : ExprNode
    {
        public enum OPERATOR
        {
            EQUAL, NOTEQUAL, LESSTHAN, GTRTHAN, LESSEQUAL, GTREQUAL
        }

        OPERATOR op;
        ExprNode lhs, rhs;

        public ComparisonExprNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.CompareExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
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
        public enum OPERATOR
        {
            EQUAL, MULTEQUAL, DIVEQUAL, MODEQUAL, ADDEQUAL, SUBEQUAL,
            LSHIFTEQUAL, RSHIFTEQUAL, ANDEQUAL, XOREQUAL, OREQUAL
        }

        OPERATOR op;
        ExprNode lhs, rhs;

        public AssignExpressionNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.AssignExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
    }


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

        ExpressionStmt,
        ForStmt,
        ReturnStmt,

        IntConst,
        FloatConst,
        ArithmeticExpr,
        CompareExpr,
        AssignExpr,
        DeclarationStmt,
    }
}
