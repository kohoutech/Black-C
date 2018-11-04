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
        public String ident;
        public ParseNode def;
        public SYMTYPE symtype;

        public IdentNode(String id)
        {
            ident = id;
            def = null;
            symtype = SYMTYPE.UNSET;
        }
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
        public enum OPERATOR { AMPERSAND, ASTERISK, PLUS, MINUS, TILDE, EXCLAIM };
        OPERATOR op;

        public UnaryOperatorNode(OPERATOR _op)
        {
            op = _op;
        }
    }

    public class CastExprNode : ExprNode
    {
    }

    public class MultiplyExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public MultiplyExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class DivideExprNode : ExprNode
    {
         ExprNode lhs, rhs;

        public DivideExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
   }

    public class ModuloExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public ModuloExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class AddExprNode : ExprNode
    {   
        ExprNode lhs, rhs;

        public AddExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class SubtractExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public SubtractExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ShiftLeftExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public ShiftLeftExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ShiftRightExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public ShiftRightExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class LessThanExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public LessThanExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class GreaterThanExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public GreaterThanExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class LessEqualExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public LessEqualExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class GreaterEqualExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public GreaterEqualExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class EqualsExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public EqualsExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class NotEqualsExprNode : ExprNode
    {
        ExprNode lhs, rhs;

        public NotEqualsExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ANDExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public ANDExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class XORExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public XORExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ORExprNode : ExprNode
    {        
        ExprNode lhs, rhs;

        public ORExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class LogicalANDExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public LogicalANDExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class LogicalORExprNode : ExprNode
    {     
        ExprNode lhs, rhs;

        public LogicalORExprNode(ExprNode _lhs, ExprNode _rhs)
        {
            lhs = _lhs;
            rhs = _rhs;
        }
    }

    public class ConditionalExprNode : ExprNode
    {     
        ExprNode lhs;
        ExpressionNode trueexpr;
        ExprNode falseexpr;

        public ConditionalExprNode(ExprNode _lhs, ExpressionNode _trueexpr, ExprNode _falseexpr)
        {
            lhs = _lhs;
            trueexpr = _trueexpr;
            falseexpr = _falseexpr;
        }
    }

    public class AssignExpressionNode : ExprNode
    {     
    }

    public class AssignOperatorNode : ParseNode
    {     
        public enum OPERATOR {EQUAL, MULTEQUAL, SLASHEQUAL, PERCENTEQUAL, PLUSEQUAL, MINUSEQUAL, 
            LSHIFTEQUAL, RSHIFTEQUAL, AMPEQUAL, CARETEQUAL, BAREQUAL}
        OPERATOR op;

        public AssignOperatorNode(OPERATOR _op)
        {
            op = _op;
        }
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
        public DeclarSpecNode declarspecs;
        public List<InitDeclaratorNode> declarlist;
        public bool isFuncDef;

        public DeclarationNode(DeclarSpecNode specs, List<InitDeclaratorNode> list)
        {
            declarspecs = specs;
            declarlist = list;
            isFuncDef = false;
        }
    }

    public class InitDeclaratorNode : ParseNode
    {
        public DeclaratorNode declarnode;
        public InitializerNode initialnode;

        public InitDeclaratorNode(DeclaratorNode declar, InitializerNode initial)
        {
            declarnode = declar;
            initialnode = initial;
        }
    }

    public class DeclarSpecNode : ParseNode
    {
        public enum STORAGE { TYPEDEF, EXTERN, STATIC, AUTO, REGISTER, NONE };
        public STORAGE storage;

        //type modifiers
        public bool isShort;
        public bool isLong;
        public bool isLongLong;
        public bool isSigned;
        public bool isUnsigned;

        //type qualifiers
        public bool isConst;
        public bool isRestrict;
        public bool isVolatile;

        //function specifier
        public bool isInline;

        //type specifier
        public TypeSpecNode typeSpec;

        public DeclarSpecNode()
        {
            storage = STORAGE.NONE;

            isShort = false;
            isLong = false;
            isLongLong = false;
            isSigned = false;
            isUnsigned = false;

            isConst = false;
            isRestrict = false;
            isVolatile = false;

            isInline = false;

            typeSpec = null;
        }

        public void setStorageClassSpec(Token token)
        {
            switch (token.type)
            {
                case TokenType.tTYPEDEF:
                    storage = DeclarSpecNode.STORAGE.TYPEDEF;
                    break;

                case TokenType.tEXTERN:
                    storage = DeclarSpecNode.STORAGE.EXTERN;
                    break;

                case TokenType.tSTATIC:
                    storage = DeclarSpecNode.STORAGE.STATIC;
                    break;

                case TokenType.tAUTO:
                    storage = DeclarSpecNode.STORAGE.AUTO;
                    break;

                case TokenType.tREGISTER:
                    storage = DeclarSpecNode.STORAGE.REGISTER;
                    break;
            }        
        }

        public void setBaseClassSpec(Token token)
        {
            BaseTypeSpecNode basespec = new BaseTypeSpecNode();
            switch (token.type)
            {
                case TokenType.tVOID:
                    basespec.baseclass = BaseTypeSpecNode.BASE.VOID;
                    break;

                case TokenType.tCHAR:
                    basespec.baseclass = BaseTypeSpecNode.BASE.CHAR;
                    break;

                case TokenType.tINT:
                    basespec.baseclass = BaseTypeSpecNode.BASE.INT;
                    break;

                case TokenType.tFLOAT:
                    basespec.baseclass = BaseTypeSpecNode.BASE.FLOAT;
                    break;

                case TokenType.tDOUBLE:
                    basespec.baseclass = BaseTypeSpecNode.BASE.DOUBLE;
                    break;                    
            }
            typeSpec = basespec;
        }

        public void setBaseClassModifier(Token token)
        {
            switch (token.type)
            {
                case TokenType.tSHORT:
                    isShort = true;
                    break;

                case TokenType.tLONG:
                    if (isLong)
                    {
                        isLongLong = true;
                    }
                    else
                    {
                        isLong = true;
                    }
                    break;

                case TokenType.tSIGNED:
                    isSigned = true;
                    break;

                case TokenType.tUNSIGNED:
                    isUnsigned = true;
                    break;
            }
        }

        public void setTypeQual(Token token)
        {
            switch (token.type)
            {
                case TokenType.tCONST:
                    isConst = true;
                    break;

                case TokenType.tRESTRICT:
                    isRestrict = true;
                    break;

                case TokenType.tVOLATILE:
                    isVolatile = true;
                    break;
            }
        }

        public void setFunctionSpec(Token token)
        {
            isInline = true;
        }

        public void complete()
        {
            if ((typeSpec != null) && (typeSpec is BaseTypeSpecNode))
            {
                BaseTypeSpecNode spec = (BaseTypeSpecNode)typeSpec;
                spec.isShort = isShort;
                spec.isLong = isLong;
                spec.isLongLong = isLongLong;
                spec.isSigned = isSigned;
                spec.isUnsigned = isUnsigned;
            }
        }
    }


    public class TypeSpecNode : DeclarSpecNode
    {
    }

    public class BaseTypeSpecNode : TypeSpecNode
    {
        public enum BASE { VOID, CHAR, INT, FLOAT, DOUBLE, NONE }
        public BASE baseclass;

        public bool isShort;
        public bool isLong;
        public bool isLongLong;
        public bool isSigned;
        public bool isUnsigned;

        public BaseTypeSpecNode()
        {
            isShort = false;
            isLong = false;
            isLongLong = false;
            isSigned = false;
            isUnsigned = false;
            baseclass = BASE.NONE;
        }
    }

    public class StructSpecNode : TypeSpecNode
    {
        StructUnionNode tag;
        IdentNode name; 
        List<StructDeclarationNode> declarList;

        public StructSpecNode(StructUnionNode _tag, IdentNode _name, List<StructDeclarationNode> _declarList)
        {
            tag = _tag;
            name = _name;
            declarList = _declarList;
        }        
    }

    public class StructUnionNode : ParseNode
    {
        public enum LAYOUT { STRUCT, UNION }
        LAYOUT layout;

        public StructUnionNode(LAYOUT _layout) 
        {
            layout = _layout;
        }
    }

    public class StructDeclarationNode : ParseNode
    {
    }

    public class StructDeclaratorNode : ParseNode
    {
    }

    public class EnumSpecNode : TypeSpecNode
    {
        public String id;
        public List<EnumeratorNode> enumList;

        public EnumSpecNode(string _id, List<EnumeratorNode> _list)
        {
            id = id;
            enumList = _list;
        }
    }

    public class EnumeratorNode : ParseNode
    {
        public EnumConstantNode name;
        public ConstExpressionNode expr;

        public EnumeratorNode(EnumConstantNode _name, ConstExpressionNode _expr)
        {
            name = _name;
            expr = _expr;
        }
    }

    public class EnumConstantNode : ParseNode
    {
        String id;

        public EnumConstantNode(String _id)
        {
            id = _id;
        }
    }

    public class TypeQualNode : DeclarSpecNode
    {
        public enum QUAL { CONST, RESTRICT, VOLATILE };
        QUAL qual;

        public TypeQualNode(QUAL _qual)
        {
            qual = _qual;
        }
    }

    public class DeclaratorNode : ParseNode
    {
        public PointerNode ptr;
        public DirectDeclaratorNode declar;

        public DeclaratorNode(PointerNode _ptr, DirectDeclaratorNode _declar)
        {
            ptr = _ptr;
            declar = _declar;
        }

        public DeclaratorNode()
        {
            // TODO: Complete member initialization
        }
    }

    public class DirectDeclaratorNode : ParseNode
    {
        public IdentNode ident;
        public DeclaratorNode declar;
        public bool staticQualList;
        public List<TypeQualNode> indexQualList;
        public bool staticAssign;
        public AssignExpressionNode indexAssign;
        public ParamTypeListNode paramList;
        public List<IdentNode> identList;
        public DirectDeclaratorNode chain;

        public DirectDeclaratorNode()
        {
            ident = null;
            declar = null;
            staticQualList = false;
            indexQualList = null;
            staticAssign = false;
            indexAssign = null;
            paramList = null;
            identList = null;
            chain = null;
        }

    }

    public class PointerNode : ParseNode
    {
        public DeclarSpecNode qualList;
        public DeclaratorNode chain;

        public PointerNode(DeclarSpecNode list, DeclaratorNode declar)
        {
            qualList = list;
            chain = declar;
        }
    }

    public class ParamTypeListNode : ParseNode
    {
        List<ParamDeclarNode> list;
        bool hasElipsis;

        public ParamTypeListNode(List<ParamDeclarNode> _list, bool _hasElipsis)
        {
            list = _list;
            hasElipsis = _hasElipsis;
        }
    }

    public class ParamDeclarNode : ParseNode
    {
        DeclarSpecNode specs;
        DeclaratorNode declar;
        AbstractDeclaratorNode absdeclar;

        public ParamDeclarNode(DeclarSpecNode _specs, DeclaratorNode _declar, AbstractDeclaratorNode _absdeclar)
        {
            specs = _specs;
            declar = _declar;
            absdeclar = _absdeclar;
        }
    }

    public class TypeNameNode : ParseNode
    {
    }

    public class AbstractDeclaratorNode : ParseNode
    {
        public PointerNode ptr;
        public DirectAbstractNode direct;

        public AbstractDeclaratorNode(PointerNode _ptr, DirectAbstractNode _direct)
        {
            ptr = _ptr;
            direct = _direct;
        }
    }

    public class DirectAbstractNode : ParseNode
    {
    }

    public class TypedefNode : ParseNode
    {
        public TypeSpecNode typedef;

        public TypedefNode(TypeSpecNode def)
        {
            typedef = def;
        }
    }

    public class InitializerNode : ParseNode
    {
        public void addDesignation(DesignationNode desinode)
        {
            throw new NotImplementedException();
        }
    }    

    public class DesignationNode : ParseNode
    {
    }

    public class DesignatorNode : ParseNode
    {
    }

    //- statements ----------------------------------------------------------

    public class BlockItemNode : ParseNode
    {
    }

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

    //- external defs ---------------------------------------------------------

    public class TranslationUnit : ParseNode
    {
        Arbor arbor;
        List<ParseNode> defs;

        public TranslationUnit(Arbor _arbor)
        {
            arbor = _arbor;
            defs = new List<ParseNode>();
            arbor.pushSymbolTable();                //global symbol tbl
        }

        public void addFunctionDef(FunctionDefNode func)
        {
            defs.Add(func);
            Console.WriteLine("parsed function " + defs.Count);
        }

        public void addDeclaration(DeclarationNode declar)
        {
            defs.Add(declar);
            Console.WriteLine("parsed declaration " + defs.Count);
        }

        public void write()
        {
            Console.WriteLine("done parsing");
        }
    }

    public class FunctionDefNode : ParseNode
    {
        public DeclarSpecNode specs;
        public DeclaratorNode signature;
        public List<DeclarationNode> oldparamlist;
        public StatementNode block;
        private DeclarationNode declars;

        public FunctionDefNode(DeclarSpecNode _specs, DeclaratorNode _sig, List<DeclarationNode> _oldparams, StatementNode _block)
        {
            specs = _specs;
            signature = _sig;
            oldparamlist = _oldparams;
            block = _block;
        }

        public FunctionDefNode(DeclarationNode declars)
        {
            // TODO: Complete member initialization
            this.declars = declars;
        }

        internal void setOldParams(List<DeclarationNode> oldparamlist)
        {
            throw new NotImplementedException();
        }

        internal void setFuncBody(StatementNode block)
        {
            throw new NotImplementedException();
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");