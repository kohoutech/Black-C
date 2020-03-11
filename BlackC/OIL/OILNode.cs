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

//still a lot to sort out & clean up

namespace Origami.OIL
{
    //base OIL class
    public class OILNode
    {
        public OILType type;
        public String OILname;
        public CGNode cgnode;
    }

    //code generator base class
    public class CGNode
    {
    }

    //- external defs ---------------------------------------------------------

    public class Module
    {
        public String name;
        public List<TypeDeclNode> typedefs;
        public List<VarDeclNode> globals;
        public List<FuncDefNode> funcs;

        public Module(String _name)
        {
            name = _name;
            typedefs = new List<TypeDeclNode>();
            globals = new List<VarDeclNode>();
            funcs = new List<FuncDefNode>();
        }
    }

    //- declarations ----------------------------------------------------------

    //base declaration class
    public class Declaration : OILNode
    {
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

    public class VarDeclNode : Declaration
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

    public class ParamDeclNode : Declaration
    {
        public string name;
        public TypeDeclNode pType;

        public ParamDeclNode()
        {
            type = OILType.ParamDecl;
            name = "";
            pType = null;
        }
    }

    public class FuncDefNode : Declaration
    {
        public String name;
        public TypeDeclNode returnType;
        public List<ParamDeclNode> paramList;           //'params' is a reserved word in C#
        public bool isVaradic;
        public List<VarDeclNode> locals;
        public List<StatementNode> body;
        public bool isInline;

        public FuncDefNode()
        {
            type = OILType.FuncDecl;
            name = "";
            returnType = null;
            paramList = new List<ParamDeclNode>();
            isVaradic = false;
            locals = new List<VarDeclNode>();
            body = null;
            isInline = false;
        }
    }

    //- struct/unions/enums -----------------------------------------------

    //public class StructDeclNode : TypeDeclNode
    //{
        //     StructUnionNode tag;
        //     IdentNode name; 
        //     List<StructDeclarationNode> declarList;

        //     public StructSpecNode(StructUnionNode _tag, IdentNode _name, List<StructDeclarationNode> _declarList)
        //     {
        //         tag = _tag;
        //         name = _name;
        //         declarList = _declarList;
        //     }        
    //}

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

        public ParamListNode()
            : base()
        {
            paramList = new List<ParamDeclNode>();
            hasElipsis = false;
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

    public class DeclarInitStatementNode : StatementNode
    {
        public VarDeclNode decl;
        public ExprNode initexpr;

        public DeclarInitStatementNode(VarDeclNode _decl, ExprNode _initexpr)
        {
            type = OILType.DeclarationStmt;
            decl = _decl;
            initexpr = _initexpr;
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
        public List<StatementNode> decl1;
        public ExprNode expr1;
        public ExprNode expr2;
        public ExprNode expr3;
        public List<StatementNode> body;

        public ForStatementNode(List<StatementNode> _decl1, ExprNode _expr1, ExprNode _expr2, ExprNode _expr3, List<StatementNode> _body)
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

    //base expression class
    public class ExprNode : OILNode
    {        
    }

    public class IdentExprNode : ExprNode
    {
        public OILNode idsym;

        public IdentExprNode(OILNode _idsym)
        {
            type = OILType.IdentExpr;
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

    public class UnaryOpExprNode : ExprNode
    {
        public enum OPERATOR { AMPERSAND, STAR };
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

        public static string[] opname = { "plus", "minus", "inc", "dec", "add", "sub", "mult", "div", "mod" };
        
        public OPERATOR op;
        public ExprNode lhs, rhs;

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

        public static string[] opname = {"equal", "notequal", "lessthan", "gtrthan", "lessequal", "gtrequal"};        

        public OPERATOR op;
        public ExprNode lhs, rhs;

        public ComparisonExprNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.CompareExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class BitwiseExprNode : ExprNode
    {
        public enum OPERATOR
        {
            AND, OR, XOR, NOT, LSHIFT, RSHIFT
        }

        OPERATOR op;
        ExprNode lhs, rhs;

        public BitwiseExprNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.BitwiseExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class LogicalExprNode : ExprNode
    {
        public enum OPERATOR
        {
            AND, OR, NOT
        }

        OPERATOR op;
        ExprNode lhs, rhs;

        public LogicalExprNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.LogicalExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ConditionalExprNode : ExprNode
    {
        ExprNode testexpr;
        ExprNode trueexpr;
        ExprNode falseexpr;

        public ConditionalExprNode(ExprNode _testexpr, ExpressionNode _trueexpr, ExprNode _falseexpr)
        {
            type = OILType.ConditionalExpr;
            testexpr = _testexpr;
            trueexpr = _trueexpr;
            falseexpr = _falseexpr;
        }
    }

    public class AssignExprNode : ExprNode
    {
        public enum OPERATOR
        {
            EQUAL, MULTEQUAL, DIVEQUAL, MODEQUAL, ADDEQUAL, SUBEQUAL, LSHIFTEQUAL, RSHIFTEQUAL, ANDEQUAL, XOREQUAL, OREQUAL
        }

        public static string[] opname = { "equal", "multequal", "divequal", "modequal", "addequal", "subequal",
                                          "lshiftequal", "rshiftequal", "andequal", "xorequal", "orequal" };        

        public OPERATOR op;
        public ExprNode lhs, rhs;

        public AssignExprNode(OPERATOR _op, ExprNode _lhs, ExprNode _rhs)
        {
            type = OILType.AssignExpr;
            op = _op;
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ConstExprNode : ExprNode
    {
    }

    public class ExpressionNode : ExprNode
    {
    }

    //-------------------------------------------------------------------------

    public enum OILType
    {
        TypeDecl,
        VarDecl,
        FuncDecl,

        DeclarationStmt,
        ExpressionStmt,
        ForStmt,
        ReturnStmt,

        IdentExpr,
        IntConst,
        FloatConst,
        ArithmeticExpr,
        CompareExpr,
        BitwiseExpr,
        LogicalExpr,
        ConditionalExpr,
        AssignExpr,
        ParamDecl,
    }
}
