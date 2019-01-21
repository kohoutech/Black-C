/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2019  George E Greaney

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

namespace BlackC
{
    public class ParseExpr
    {
        public Preprocessor prep;
        public Arbor arbor;
        public ParseDeclar pdeclar;

        public ParseExpr(Preprocessor _prep, Arbor _arbor)
        {
            prep = _prep;
            arbor = _arbor;
        }

        /*(6.5.1) 
         primary-expression:
            identifier
            constant
            string-literal
            ( expression )
         */
        public ExprNode parsePrimaryExpression()
        {
            int cuepoint = prep.record();
            Token token = prep.getToken();
            ExprNode node = arbor.getExprIdentNode(token);
            bool result = (node != null);
            if (!result)
            {
                result = (token.type == TokenType.tINTCONST);
                if (result)
                {
                    node = arbor.makeIntegerConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token.type == TokenType.tFLOATCONST);
                if (result)
                {
                    node = arbor.makeFloatConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token.type == TokenType.tCHARCONST);
                if (result)
                {
                    node = arbor.makeCharConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token.type == TokenType.tSTRINGCONST);
                if (result)
                {
                    node = arbor.makeStringConstantNode(token);
                }
            }
            if (!result)
            {
                node = arbor.getExprEnumNode(token);
                result = (node != null);
            }
            if (!result)
            {
                prep.rewind(cuepoint);                   //( expression )
                token = prep.getToken();
                result = (token.type == TokenType.tLPAREN);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRPAREN);
                        if (result)
                        {
                            node = arbor.makeSubexpressionNode(expr);
                        }
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.5.2) 
         postfix-expression:
            primary-expression
            ( type-name ) { initializer-list }
            ( type-name ) { initializer-list , }
            postfix-expression [ expression ]
            postfix-expression ( argument-expression-list[opt] )
            postfix-expression . identifier
            postfix-expression -> identifier
            postfix-expression ++
            postfix-expression --
         */
        public ExprNode parsePostfixExpression()
        {
            int cuepoint = prep.record();
            ExprNode node = parsePrimaryExpression();         //primary-expression
            bool result = (node != null);
            if (!result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();           //( type-name ) { initializer-list }
                result = (token.type == TokenType.tLPAREN);
                if (result)
                {
                    TypeNameNode name = pdeclar.parseTypeName();
                    result = (name != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRPAREN);
                        if (result)
                        {
                            token = prep.getToken();
                            result = (token.type == TokenType.tLBRACE);
                            if (result)
                            {
                                List<InitializerNode> initList = pdeclar.parseInitializerList();
                                result = (initList != null);
                                if (result)
                                {
                                    token = prep.getToken();
                                    result = (token.type == TokenType.tRBRACE);
                                    if (!result)
                                    {
                                        result = (token.type == TokenType.tCOMMA);             //the comma is optional
                                        if (result)
                                        {
                                            token = prep.getToken();
                                            result = (token.type == TokenType.tRBRACE);
                                            if (result)
                                            {
                                                node = arbor.makeTypeInitExprNode(node);
                                                result = (node != null);
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            bool notEmpty = result;
            while (result)
            {
                int cuepoint2 = prep.record();           //postfix-expression [ expression ]
                Token token = prep.getToken();
                result = (token.type == TokenType.tLBRACKET);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    result = (expr != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRBRACKET);
                        if (result)
                        {
                            node = arbor.makeIndexExprNode(node, expr);
                            result = (node != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);                  //postfix-expression ( argument-expression-list[opt] )
                    token = prep.getToken();
                    result = (token.type == TokenType.tLPAREN);
                    if (result)
                    {
                        List<AssignExpressionNode> argList = parseArgExpressionList();
                        token = prep.getToken();
                        result = (token.type == TokenType.tRPAREN);
                        if (result)
                        {
                            node = arbor.makeFuncCallExprNode(node, argList);
                            result = (node != null);
                        }

                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);                  //postfix-expression . identifier
                    token = prep.getToken();
                    result = (token.type == TokenType.tPERIOD);
                    if (result)
                    {
                        token = prep.getToken();
                        IdentNode idNode = arbor.getFieldIdentNode(token);
                        result = (idNode != null);
                        if (result)
                        {
                            node = arbor.makeFieldExprNode(node, idNode);
                            result = (node != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);                  //postfix-expression -> identifier
                    token = prep.getToken();
                    result = (token.type == TokenType.tARROW);
                    if (result)
                    {
                        token = prep.getToken();
                        IdentNode idNode = arbor.getFieldIdentNode(token);
                        result = (idNode != null);
                        if (result)
                        {
                            node = arbor.makeRefFieldExprNode(node, idNode);
                            result = (node != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);                  //postfix-expression ++
                    token = prep.getToken();
                    result = (token.type == TokenType.tPLUSPLUS);
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makePostPlusPlusExprNode(node);
                        result = (node != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);                  //postfix-expression --
                    token = prep.getToken();
                    result = (token.type == TokenType.tMINUSMINUS);
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makePostMinusMinusExprNode(node);
                        result = (node != null);
                    }
                    if (!result)
                    {
                        prep.rewind(cuepoint2);
                    }
                }
            }
            if (!notEmpty)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.5.2) 
         argument-expression-list:
            assignment-expression
            argument-expression-list , assignment-expression
         */
        public List<AssignExpressionNode> parseArgExpressionList()
        {
            List<AssignExpressionNode> list = null;
            AssignExpressionNode node = parseAssignExpression();
            bool result = (node != null);
            if (result)
            {
                list = new List<AssignExpressionNode>();
                list.Add(node);
            }
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
                if (result)
                {
                    node = parseAssignExpression();
                    result = (node != null);
                    if (result)
                    {
                        list.Add(node);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return list;
        }

        /*(6.5.3) 
         unary-expression:
            postfix-expression
            ++ unary-expression
            -- unary-expression
            unary-operator cast-expression
            sizeof unary-expression
            sizeof ( type-name )
         */
        public ExprNode parseUnaryExpression()
        {
            int cuepoint = prep.record();
            ExprNode node = parsePostfixExpression();         //postfix-expression
            bool result = (node != null);
            if (!result)
            {
                Token token = prep.getToken();           //++ unary-expression
                result = (token.type == TokenType.tPLUSPLUS);
                if (result)
                {
                    node = parseUnaryExpression();
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makePlusPlusExprNode(node);
                        result = (node != null);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
                Token token = prep.getToken();           //-- unary-expression
                result = (token.type == TokenType.tMINUSMINUS);
                if (result)
                {
                    node = parseUnaryExpression();
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makeMinusMinusExprNode(node);
                        result = (node != null);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);                   //unary-operator cast-expression
                UnaryOperatorNode uniOp = parseUnaryOperator();
                result = (uniOp != null);
                if (result)
                {
                    ExprNode castExpr = parseCastExpression();
                    result = (castExpr != null);
                    if (result)
                    {
                        node = arbor.makeUnaryCastExprNode(uniOp, castExpr);
                        result = (node != null);
                    }

                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
                Token token = prep.getToken();           //sizeof unary-expression
                result = (token.type == TokenType.tSIZEOF);
                if (result)
                {
                    node = parseUnaryExpression();
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makeSizeofUnaryExprNode(node);
                        result = (node != null);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
                Token token = prep.getToken();           //sizeof ( type-name )
                result = (token.type == TokenType.tSIZEOF);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tLPAREN);
                    if (result)
                    {
                        TypeNameNode name = pdeclar.parseTypeName();
                        if (result)
                        {
                            token = prep.getToken();
                            result = (token.type == TokenType.tRPAREN);
                            result = (node != null);
                            if (result)
                            {
                                node = arbor.makeSizeofTypeExprNode(name);
                                result = (node != null);
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.5.3) 
         unary-operator: one of
            & * + - ~ !
         */
        public UnaryOperatorNode parseUnaryOperator()
        {
            UnaryOperatorNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            switch (token.ToString())
            {
                case "AMPERSAND":
                    node = new UnaryOperatorNode(UnaryOperatorNode.OPERATOR.AMPERSAND);
                    break;

                case "ASTERISK":
                    node = new UnaryOperatorNode(UnaryOperatorNode.OPERATOR.ASTERISK);
                    break;

                case "PLUS":
                    node = new UnaryOperatorNode(UnaryOperatorNode.OPERATOR.PLUS);
                    break;

                case "MINUS":
                    node = new UnaryOperatorNode(UnaryOperatorNode.OPERATOR.MINUS);
                    break;

                case "TILDE":
                    node = new UnaryOperatorNode(UnaryOperatorNode.OPERATOR.TILDE);
                    break;

                case "EXCLAIM":
                    node = new UnaryOperatorNode(UnaryOperatorNode.OPERATOR.EXCLAIM);
                    break;
            }
            if (node == null)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.5.4) 
         cast-expression:
            unary-expression
            ( type-name ) cast-expression
         */
        public ExprNode parseCastExpression()
        {
            ExprNode node = parseUnaryExpression();
            bool result = (node != null);
            if (!result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tLPAREN);
                if (result)
                {
                    TypeNameNode name = pdeclar.parseTypeName();
                    result = (name != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRPAREN);
                        if (result)
                        {
                            ExprNode rhs = parseCastExpression();
                            result = (rhs != null);
                            if (result)
                            {
                                node = arbor.makeCastExprNode(name, rhs);
                                result = (node != null);
                            }
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return node;
        }

        /*(6.5.5) 
         multiplicative-expression:
            cast-expression
            multiplicative-expression * cast-expression
            multiplicative-expression / cast-expression
            multiplicative-expression % cast-expression
         */
        public ExprNode parseMultExpression()
        {
            ExprNode lhs = parseCastExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tSTAR);
                if (result)
                {
                    ExprNode rhs = parseCastExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeMultiplyExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tSLASH);
                    if (result)
                    {
                        ExprNode rhs = parseCastExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeDivideExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tPERCENT);
                    if (result)
                    {
                        ExprNode rhs = parseCastExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeModuloExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.6) 
         additive-expression:
            multiplicative-expression
            additive-expression + multiplicative-expression
            additive-expression - multiplicative-expression
         */
        public ExprNode parseAddExpression()
        {
            ExprNode lhs = parseMultExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tPLUS);
                if (result)
                {
                    ExprNode rhs = parseMultExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeAddExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tMINUS);
                    if (result)
                    {
                        ExprNode rhs = parseMultExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeSubtractExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.7) 
         shift-expression:
            additive-expression
            shift-expression << additive-expression
            shift-expression >> additive-expression
         */
        public ExprNode parseShiftExpression()
        {
            ExprNode lhs = parseAddExpression();
            bool result = (lhs != null);
            bool empty = result;
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tLEFTSHIFT);
                if (result)
                {
                    ExprNode rhs = parseAddExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeShiftLeftExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tRIGHTSHIFT);
                    if (result)
                    {
                        ExprNode rhs = parseAddExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeShiftRightExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.8) 
         relational-expression:
            shift-expression
            relational-expression < shift-expression
            relational-expression > shift-expression
            relational-expression <= shift-expression
            relational-expression >= shift-expression
         */
        public ExprNode parseRelationalExpression()
        {
            ExprNode lhs = parseShiftExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tLESSTHAN);
                if (result)
                {
                    ExprNode rhs = parseShiftExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeLessThanExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tGTRTHAN);
                    if (result)
                    {
                        ExprNode rhs = parseShiftExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeGreaterThanExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tLESSEQUAL);
                    if (result)
                    {
                        ExprNode rhs = parseShiftExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeLessEqualExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tGTREQUAL);
                    if (result)
                    {
                        ExprNode rhs = parseShiftExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeGreaterEqualExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.9) 
         equality-expression:
            relational-expression
            equality-expression == relational-expression
            equality-expression != relational-expression
         */
        public ExprNode parseEqualityExpression()
        {
            ExprNode lhs = parseRelationalExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tEQUALEQUAL);
                if (result)
                {
                    ExprNode rhs = parseRelationalExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeEqualsExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                    token = prep.getToken();
                    result = (token.type == TokenType.tNOTEQUAL);
                    if (result)
                    {
                        ExprNode rhs = parseRelationalExpression();
                        result = (rhs != null);
                        if (result)
                        {
                            lhs = arbor.makeNotEqualsExprNode(lhs, rhs);
                            result = (lhs != null);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.10) 
         AND-expression:
            equality-expression
            AND-expression & equality-expression
         */
        public ExprNode parseANDExpression()
        {
            ExprNode lhs = parseEqualityExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tAMPERSAND);
                if (result)
                {
                    ExprNode rhs = parseEqualityExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeANDExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.11) 
         exclusive-OR-expression:
            AND-expression
            exclusive-OR-expression ^ AND-expression
         */
        public ExprNode parseXORExpression()
        {
            ExprNode lhs = parseANDExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCARET);
                if (result)
                {
                    ExprNode rhs = parseANDExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeXORExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.12) 
         inclusive-OR-expression:
            exclusive-OR-expression
            inclusive-OR-expression | exclusive-OR-expression
         */
        public ExprNode parseORExpression()
        {
            ExprNode lhs = parseXORExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tBAR);
                if (result)
                {
                    ExprNode rhs = parseXORExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeORExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.13) 
         logical-AND-expression:
            inclusive-OR-expression
            logical-AND-expression && inclusive-OR-expression
         */
        public ExprNode parseLogicalANDExpression()
        {
            ExprNode lhs = parseORExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tDOUBLEAMP);
                if (result)
                {
                    ExprNode rhs = parseORExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeLogicalANDExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.14) 
         logical-OR-expression:
            logical-AND-expression
            logical-OR-expression || logical-AND-expression
         */
        public ExprNode parseLogicalORExpression()
        {
            ExprNode lhs = parseLogicalANDExpression();
            bool result = (lhs != null);
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tDOUBLEBAR);
                if (result)
                {
                    ExprNode rhs = parseLogicalANDExpression();
                    result = (rhs != null);
                    if (result)
                    {
                        lhs = arbor.makeLogicalORExprNode(lhs, rhs);
                        result = (lhs != null);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        /*(6.5.15) 
         conditional-expression:
            logical-OR-expression
            logical-OR-expression ? expression : conditional-expression
         */
        public ExprNode parseConditionalExpression()
        {
            ExprNode lhs = parseLogicalORExpression();
            bool result = (lhs != null);
            if (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                bool result2 = (token.type == TokenType.tQUESTION);
                if (result2)
                {
                    ExpressionNode expr = parseExpression();
                    result = (expr != null);
                    if (result2)
                    {
                        token = prep.getToken();
                        result2 = (token.type == TokenType.tCOLON);
                        if (result2)
                        {
                            ExprNode condit = parseConditionalExpression();
                            result = (condit != null);
                            if (result)
                            {
                                lhs = arbor.makeConditionalExprNode(lhs, expr, condit);
                                result = (lhs != null);
                            }
                        }
                    }
                }
                if (!result2)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return lhs;
        }

        //the last three productions in this section are referenced by productions in other sections
        //so they return specific node types instead of <ExprNode> which in internal to Expressions

        /*(6.5.16) 
         assignment-expression:
            conditional-expression
            unary-expression assignment-operator assignment-expression
         */
        //for parsing purposes, we change the second rule to:
        //conditional-expression assignment-operator assignment-expression
        public AssignExpressionNode parseAssignExpression()
        {
            ExprNode lhs = parseConditionalExpression();
            AssignOperatorNode oper = null;
            ExprNode rhs = null;
            bool result = (lhs != null);
            if (result)
            {
                int cuepoint = prep.record();
                oper = parseAssignOperator();
                bool result2 = (oper != null);
                if (result2)
                {
                    rhs = parseAssignExpression();
                    result = (rhs != null);
                }
                if (!result2)
                {
                    prep.rewind(cuepoint);
                }
            }
            return arbor.makeAssignExpressionNode(lhs, oper, rhs);
        }

        /* (6.5.16) 
         assignment-operator: one of
            = *= /= %= += -= <<= >>= &= ^= |=
         */
        public AssignOperatorNode parseAssignOperator()
        {
            AssignOperatorNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            switch (token.ToString())
            {
                case "EQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.EQUAL);
                    break;

                case "MULTEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.MULTEQUAL);
                    break;

                case "SLASHEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.SLASHEQUAL);
                    break;

                case "PERCENTEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.PERCENTEQUAL);
                    break;

                case "PLUSEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.PLUSEQUAL);
                    break;

                case "MINUSEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.MINUSEQUAL);
                    break;

                case "LSHIFTEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.LSHIFTEQUAL);
                    break;

                case "RSHIFTEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.RSHIFTEQUAL);
                    break;

                case "AMPEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.AMPEQUAL);
                    break;

                case "CARETEQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.CARETEQUAL);
                    break;

                case "BAREQUAL":
                    node = new AssignOperatorNode(AssignOperatorNode.OPERATOR.BAREQUAL);
                    break;
            }
            if (node == null)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.5.17) 
         expression:
            assignment-expression
            expression , assignment-expression
         */
        public ExpressionNode parseExpression()
        {
            ExpressionNode node = null;
            AssignExpressionNode expr = parseAssignExpression();
            bool result = (expr != null);
            if (result)
            {
                node = arbor.makeExpressionNode(null, expr);
            }
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
                if (result)
                {
                    expr = parseAssignExpression();
                    if (result)
                    {
                        node = arbor.makeExpressionNode(node, expr);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return node;
        }

        /*(6.6) 
         constant-expression:
            conditional-expression
         */
        public ConstExpressionNode parseConstantExpression()
        {
            ExprNode condit = parseConditionalExpression();
            return arbor.makeConstantExprNode(condit);
        }
    }
}
