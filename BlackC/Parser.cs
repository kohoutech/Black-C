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

// the grammar this parser is pased on:
//https://en.wikipedia.org/wiki/C99
//http://www.open-std.org/jtc1/sc22/WG14/www/docs/n1256.pdf

namespace BlackC
{
    class Parser
    {
        Preprocessor preprocessor;
        Scanner scanner;
        Arbor arbor;
        int declarCount;
        int funcCount;

        public Parser()
        {
            declarCount = 0;
            funcCount = 0;
        }

        public bool isIdentifier(Token token, out Node idNode)
        {
            idNode = null;
            bool result = (token is tIdentifier);
            if (result)
            {
                tIdentifier id = (tIdentifier)token;
                result = !arbor.isTypedef(id.ident);
            }
            return result;
        }

        //- expressions ------------------------------------------------------

        /*(6.5.1) 
         primary-expression:
            identifier
            constant
            string-literal
            ( expression )
         */
        public ExprNode parsePrimaryExpression()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            ExprNode node = arbor.makeIdentExprNode(token);
            bool result = (node != null);
            if (!result)
            {
                result = (token is tIntegerConstant);
                if (result)
                {
                    node = arbor.makeIntegerConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token is tFloatConstant);
                if (result)
                {
                    node = arbor.makeFloatConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token is tCharacterConstant);
                if (result)
                {
                    node = arbor.makeCharConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token is tStringConstant);
                if (result)
                {
                    node = arbor.makeStringConstantNode(token);
                }
            }
            if (!result)
            {
                node = arbor.makeEnumExprNode(token);
                result = (node != null);
            }
            if (!result)
            {
                scanner.rewind(cuepoint);                   //( expression )
                token = scanner.getToken();
                result = (token is tLParen);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            node = arbor.makeSubexpressionNode(expr);
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
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
            int cuepoint = scanner.record();
            ExprNode node = parsePrimaryExpression();         //primary-expression
            bool result = (node != null);
            if (!result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();           //( type-name ) { initializer-list }
                result = (token is tLParen);
                if (result)
                {
                    TypeNameNode name = parseTypeName();
                    result = (name != null);
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tLBrace);
                            if (result)
                            {
                                List<InitializerNode> initList = parseInitializerList();
                                result = (initList != null);
                                if (result)
                                {
                                    token = scanner.getToken();
                                    result = (token is tRBrace);
                                    if (!result)
                                    {
                                        result = (token is tComma);             //the comma is optional
                                        if (result)
                                        {
                                            token = scanner.getToken();
                                            result = (token is tRBrace);
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
                    scanner.rewind(cuepoint2);
                }
            }
            bool notEmpty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();           //postfix-expression [ expression ]
                Token token = scanner.getToken();
                result = (token is tLBracket);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    result = (expr != null);
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRBracket);
                        if (result)
                        {
                            node = arbor.makeIndexExprNode(node, expr);
                            result = (node != null);
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //postfix-expression ( argument-expression-list[opt] )
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        List<AssignExpressionNode> argList = parseArgExpressionList();
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            node = arbor.makeFuncCallExprNode(node, argList);
                            result = (node != null);
                        }

                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //postfix-expression . identifier
                    token = scanner.getToken();
                    result = (token is tPeriod);
                    if (result)
                    {
                        token = scanner.getToken();
                        IdentNode idNode = arbor.makeIdentifierNode(token);
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
                    scanner.rewind(cuepoint2);                  //postfix-expression -> identifier
                    token = scanner.getToken();
                    result = (token is tArrow);
                    if (result)
                    {
                        token = scanner.getToken();
                        IdentNode idNode = arbor.makeIdentifierNode(token);
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
                    scanner.rewind(cuepoint2);                  //postfix-expression ++
                    token = scanner.getToken();
                    result = (token is tPlusPlus);
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makePostPlusPlusExprNode(node);
                        result = (node != null);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //postfix-expression --
                    token = scanner.getToken();
                    result = (token is tMinusMinus);
                    result = (node != null);
                    if (result)
                    {
                        node = arbor.makePostMinusMinusExprNode(node);
                        result = (node != null);
                    }
                    if (!result)
                    {
                        scanner.rewind(cuepoint2);
                    }
                }
            }
            if (!notEmpty)
            {
                scanner.rewind(cuepoint);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
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
                    scanner.rewind(cuepoint2);
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
            int cuepoint = scanner.record();
            ExprNode node = parsePostfixExpression();         //postfix-expression
            bool result = (node != null);
            if (!result)
            {
                Token token = scanner.getToken();           //++ unary-expression
                result = (token is tPlusPlus);
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
                scanner.rewind(cuepoint);
                Token token = scanner.getToken();           //-- unary-expression
                result = (token is tMinusMinus);
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
                scanner.rewind(cuepoint);                   //unary-operator cast-expression
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
                scanner.rewind(cuepoint);
                Token token = scanner.getToken();           //sizeof unary-expression
                result = (token is tSizeof);
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
                scanner.rewind(cuepoint);
                Token token = scanner.getToken();           //sizeof ( type-name )
                result = (token is tSizeof);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        TypeNameNode name = parseTypeName();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRParen);
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
                scanner.rewind(cuepoint);
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
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tAmpersand) || (token is tAsterisk) || (token is tPlus) || (token is tMinus) ||
                (token is tTilde) || (token is tExclaim));
            if (result)
            {
                node = arbor.makeUnaryOperatorNode(token);
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLParen);
                if (result)
                {
                    TypeNameNode name = parseTypeName();
                    result = (name != null);
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tAsterisk);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tSlash);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tPercent);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tPlus);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tMinus);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLeftShift);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tRightShift);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLessThan);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tGtrThan);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tLessEqual);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tGtrEqual);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tEqualEqual);
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
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tNotEqual);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tAmpersand);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tCaret);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tBar);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tDoubleAmp);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tDoubleBar);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tQuestion);
                if (result2)
                {
                    ExpressionNode expr = parseExpression();
                    result = (expr != null);
                    if (result2)
                    {
                        token = scanner.getToken();
                        result2 = (token is tColon);
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
                    scanner.rewind(cuepoint2);
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
                int cuepoint = scanner.record();
                oper = parseAssignOperator();
                bool result2 = (oper != null);
                if (result2)
                {
                    rhs = parseAssignExpression();
                    result = (rhs != null);
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint);
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
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tEqual) || (token is tMultEqual) || (token is tSlashEqual) || (token is tPercentEqual) ||
                (token is tPlusEqual) || (token is tMinusEqual) || (token is tLShiftEqual) || (token is tRShiftEqual) ||
                (token is tAmpEqual) || (token is tCaretEqual) || (token is tBarEqual));
            if (result)
            {
                node = arbor.makeAssignOperatorNode(token);
            }
            else
            {
                scanner.rewind(cuepoint);
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
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
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
                    scanner.rewind(cuepoint2);
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

        //- declarations ------------------------------------------------------

        /* (6.7) 
         declaration:
            declaration-specifiers init-declarator-list[opt] ;
         */
        public DeclarationNode parseDeclaration()
        {
            DeclarationNode node = null;
            int cuepoint = scanner.record();
            List<DeclarSpecNode> declarspecs = parseDeclarationSpecs();
            bool result = (declarspecs != null);
            if (result)
            {
                List<InitDeclaratorNode> initdeclarlist = parseInitDeclaratorList();
                Token token = scanner.getToken();
                result = (token is tSemicolon);
                if (result)
                {
                    declarCount++;
                    Console.WriteLine("parsed declaration " + declarCount.ToString());
                    node = arbor.makeDeclaration(declarspecs, initdeclarlist);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /* (6.7) 
         declaration-specifiers:
            storage-class-specifier declaration-specifiers[opt]
            type-specifier declaration-specifiers[opt]
            type-qualifier declaration-specifiers[opt]
            function-specifier declaration-specifiers[opt]
         */
        public List<DeclarSpecNode> parseDeclarationSpecs()
        {
            List<DeclarSpecNode> specs = null;
            int cuepoint = scanner.record();
            DeclarSpecNode node = parseStorageClassSpec();
            if (node == null)
            {
                node = parseTypeSpec();
            }
            if (node == null)
            {
                node = parseTypeQual();
            }
            if (node == null)
            {
                node = parseFunctionSpec();
            }
            if (node != null)
            {
                specs = new List<DeclarSpecNode>();
                specs.Add(node);
                List<DeclarSpecNode> tail = parseDeclarationSpecs();
                if (tail != null)
                {
                    specs.AddRange(tail);
                }
            }
            else
            {
                scanner.rewind(cuepoint);
            }
            return specs;
        }

        /*(6.7) 
         init-declarator-list:
            init-declarator
            init-declarator-list , init-declarator
         */
        public List<InitDeclaratorNode> parseInitDeclaratorList()
        {
            List<InitDeclaratorNode> nodelist = null;
            InitDeclaratorNode node = parseInitDeclarator();
            bool result = (node != null);
            if (result)
            {
                nodelist = new List<InitDeclaratorNode>();
                nodelist.Add(node);
            }
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    node = parseInitDeclarator();
                    if (result)
                    {
                        nodelist.Add(node);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return nodelist;
        }

        /*(6.7) 
         init-declarator:
            declarator
            declarator = initializer
         */
        public InitDeclaratorNode parseInitDeclarator()
        {
            InitDeclaratorNode node = null;
            DeclaratorNode declarnode = parseDeclarator();
            bool result = (declarnode != null);
            if (result)
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tEqual);
                if (!result2)
                {
                    node = arbor.makeInitDeclaratorNode(declarnode, null);          //declarator
                }
                if (result2)
                {
                    InitializerNode initialnode = parseInitializer();
                    result2 = (initialnode != null);
                    if (!result2)
                    {
                        node = arbor.makeInitDeclaratorNode(declarnode, initialnode);       //declarator = initializer
                    }
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint);
                }
            }
            return node;
        }

        /*(6.7.1) 
         storage-class-specifier:
            typedef
            extern
            static
            auto
            register
         */
        public StorageClassNode parseStorageClassSpec()
        {
            StorageClassNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tTypedef) || (token is tExtern) || (token is tStatic) || (token is tAuto) || (token is tRegister));
            if (result)
            {
                node = arbor.makeStoreageClassNode(token);
            }
            else 
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.2) 
         type-specifier:
            void
            char
            short
            int
            long
            float
            double
            signed
            unsigned
            _Bool
            _Complex
            struct-or-union-specifier
            enum-specifier
            typedef-name
         */
        public TypeSpecNode parseTypeSpec()
        {
            TypeSpecNode typespec = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tVoid) || (token is tChar) || (token is tShort) || (token is tInt) || (token is tLong)
                || (token is tFloat) || (token is tDouble) || (token is tSigned) || (token is tUnsigned));
            if (result)
            {
                typespec = arbor.makeBaseTypeSpec(token);
            }
            if (typespec == null)
            {
                scanner.rewind(cuepoint);
                typespec = parseStructOrUnionSpec();
            }
            if (typespec == null)
            {
                typespec = parseEnumeratorSpec();
            }
            if (typespec == null)
            {
                typespec = parseTypedefName();
            }
            if (typespec == null)
            {
                scanner.rewind(cuepoint);
            }
            return typespec;
        }

        // stuctures/unions -----------------------------------------

        /*(6.7.2.1) 
         struct-or-union-specifier:
            struct-or-union identifier[opt] { struct-declaration-list }
            struct-or-union identifier
         */
        // struct w/o ident is for anonymous struct (possibly part of a typedef)
        // struct w/o {list} is for a already defined struct type
        public StructSpecNode parseStructOrUnionSpec()
        {
            StructSpecNode node = null;
            int cuepoint = scanner.record();
            StructUnionNode tag = parseStuctOrUnion();
            bool result = (tag != null);
            if (result)
            {
                Token token = scanner.getToken();
                IdentNode name = arbor.makeIdentifierNode(token);
                result = (name != null);
                if (result)
                {
                    int cuepoint2 = scanner.record();
                    token = scanner.getToken();
                    bool result2 = (token is tLBrace);
                    if (!result2)
                    {
                        node = arbor.makeStructSpec(tag, name, null);       //struct-or-union ident
                    }
                    if (result2)
                    {
                        List<StructDeclarationNode> declarList = parseStructDeclarationList();
                        result2 = (declarList != null);
                        if (result2)
                        {
                            token = scanner.getToken();
                            result2 = (token is tRBrace);
                            if (result2)
                            {
                                node = arbor.makeStructSpec(tag, name, declarList);         //struct-or-union ident struct-declar-list
                            }
                        }
                    }
                    if (!result2)
                    {
                        scanner.rewind(cuepoint2);
                    }
                }
                else
                {
                    result = (token is tLBrace);
                    if (result)
                    {
                        List<StructDeclarationNode> declarList = parseStructDeclarationList();
                        result = (declarList != null);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRBrace);
                            if (result)
                            {
                                node = arbor.makeStructSpec(tag, null, declarList);         //struct-or-union struct-declar-list
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.2.1) 
         struct-or-union:
            struct
            union
         */
        public StructUnionNode parseStuctOrUnion()
        {
            StructUnionNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tStruct) || (token is tUnion));
            if (result)
            {
                node = arbor.makeStructUnionNode(token);
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.2.1) 
         struct-declaration-list:
            struct-declaration
            struct-declaration-list struct-declaration
         */
        // the list of struct field defs
        public List<StructDeclarationNode> parseStructDeclarationList()
        {
            List<StructDeclarationNode> fieldlist = null;
            StructDeclarationNode fieldnode = parseStructDeclaration();         //the first field def
            if (fieldnode != null)
            {
                fieldlist = new List<StructDeclarationNode>();
                fieldlist.Add(fieldnode);
            }
            while (fieldnode != null)
            {
                fieldnode = parseStructDeclaration();          //the next field def
                if (fieldnode != null)
                {
                    fieldlist.Add(fieldnode);
                }
            }
            return fieldlist;
        }

        /*(6.7.2.1) 
         struct-declaration:
            specifier-qualifier-list struct-declarator-list ;
         */
        // a single struct field def (can have mult fields, ie int a, b;)
        public StructDeclarationNode parseStructDeclaration()
        {
            StructDeclarationNode node = null;
            int cuepoint = scanner.record();
            List<DeclarSpecNode> specqual = parseSpecQualList();          //field type
            bool result = (specqual != null);          
            if (result)
            {
                List<StructDeclaratorNode> fieldnames = parseStructDeclaratorList();           //list of field names 
                result = (fieldnames != null);           
                if (result)
                {
                    Token token = scanner.getToken();
                    result = (token is tSemicolon);
                    if (result)
                    {
                        node = arbor.makeStructDeclarationNode(specqual, fieldnames);
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.2.1) 
         specifier-qualifier-list:
            type-specifier specifier-qualifier-list[opt]
            type-qualifier specifier-qualifier-list[opt]
         */
        // struct field's type - same as declaration-specifiers, w/o the storage-class-specifier or function-specifier
        public List<DeclarSpecNode> parseSpecQualList()
        {
            List<DeclarSpecNode> speclist = null;
            DeclarSpecNode specnode = parseTypeSpec();
            if (specnode == null)
            {
                specnode = parseTypeQual();
            }
            if (specnode != null)
            {
                speclist = new List<DeclarSpecNode>();
                speclist.Add(specnode);
                List<DeclarSpecNode> taillist = parseSpecQualList();
                if (taillist != null)
                {
                    speclist.AddRange(taillist);
                }
            }
            return speclist;
        }

        /*(6.7.2.1) 
         struct-declarator-list:
            struct-declarator
            struct-declarator-list , struct-declarator
         */
        // the list of field names, fx the "a, b, c" in "int a, b, c;" that def's three fields of type int
        public List<StructDeclaratorNode> parseStructDeclaratorList()
        {
            List<StructDeclaratorNode> fieldlist = null;
            StructDeclaratorNode fieldnode = parseStructDeclarator();      //the first field name
            bool result = (fieldnode != null);
            if (result)
            {
                fieldlist = new List<StructDeclaratorNode>();
                fieldlist.Add(fieldnode);
            }
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    fieldnode = parseStructDeclarator();       //the next field name
                    result = (fieldnode != null);
                    if (result)
                    {
                        fieldlist.Add(fieldnode);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return fieldlist;
        }

        /*(6.7.2.1) 
         struct-declarator:
            declarator
            declarator[opt] : constant-expression
         */
        //a single field name, possibly followed by a field width (fx foo : 4;)
        public StructDeclaratorNode parseStructDeclarator()
        {
            StructDeclaratorNode node = null;
            int cuepoint = scanner.record();
            DeclaratorNode declarnode = parseDeclarator();
            bool result = (declarnode != null);
            if (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tColon);
                if (result2)
                {
                    ExprNode constexpr = parseConstantExpression();
                    result2 = (constexpr != null);
                    if (result2)
                    {
                        node = arbor.makeStructDeclaractorNode(declarnode, constexpr);      //declarator : constant-expression
                    }
                }
                if (!result2)
                {
                    node = arbor.makeStructDeclaractorNode(declarnode, null);       //declarator
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            if (!result)
            {
                Token token = scanner.getToken();
                result = (token is tColon);
                if (result)
                {
                    ExprNode constexpr = parseConstantExpression();
                    result = (constexpr != null);
                    if (result)
                    {
                        //Console.WriteLine("parsed const-exp struct-declar");
                        node = arbor.makeStructDeclaractorNode(null, constexpr);      // : constant-expression
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        // enumerations ---------------------------------------------

        /*(6.7.2.2) 
         enum-specifier:
            enum identifier[opt] { enumerator-list }
            enum identifier[opt] { enumerator-list , }
            enum identifier
         */
        public EnumSpecNode parseEnumeratorSpec()
        {
            EnumSpecNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tEnum);             
            if (result)
            {
                token = scanner.getToken();
                IdentNode idNode = arbor.makeIdentifierNode(token);
                result = (idNode != null);
                if (result)
                {
                    int cuepoint2 = scanner.record();
                    token = scanner.getToken();
                    bool result2 = (token is tLBrace);             //enum identifier { enumerator-list }
                    if (result2)
                    {
                        List<EnumeratorNode> enumList = parseEnumeratorList();
                        result2 = (enumList != null);
                        if (result2)
                        {
                            token = scanner.getToken();
                            result2 = (token is tRBrace);
                            if (!result2)
                            {
                                result2 = (token is tComma);            //enum identifier { enumerator-list , }
                                if (result2)
                                {
                                    token = scanner.getToken();
                                    result2 = (token is tRBrace);
                                }
                            }
                            if (result2)
                            {
                                node = arbor.makeEnumSpec(idNode, enumList);
                            }
                        }
                    }
                    if (!result2)
                    {
                        node = arbor.makeEnumSpec(idNode, null);        //enum identifier
                    }
                    if (!result2)
                    {
                        scanner.rewind(cuepoint2);
                    }
                }
                else
                {
                    token = scanner.getToken();
                    result = (token is tLBrace);             //enum { enumerator-list }
                    if (result)
                    {
                        List<EnumeratorNode> enumList = parseEnumeratorList();
                        result = (enumList != null);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRBrace);
                            if (!result)
                            {
                                result = (token is tComma);            //enum { enumerator-list , }
                                if (result)
                                {
                                    token = scanner.getToken();
                                    result = (token is tRBrace);
                                }
                            }
                            if (result)
                            {
                                node = arbor.makeEnumSpec(null, enumList);
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.2.2) 
         enumerator-list:
            enumerator
            enumerator-list , enumerator
         */
        public List<EnumeratorNode> parseEnumeratorList()
        {
            List<EnumeratorNode> enumlistnode = null;
            EnumeratorNode enumnode = parseEnumerator();
            bool result = (enumnode != null);
            if (result)
            {
                enumlistnode = new List<EnumeratorNode>();
                enumlistnode.Add(enumnode);
            }
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    enumnode = parseEnumerator();
                    result = (enumnode != null);
                    if (result)
                    {
                        enumlistnode.Add(enumnode);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return enumlistnode;
        }

        /*(6.7.2.2) 
         enumerator:
            enumeration-constant
            enumeration-constant = constant-expression
         */
        public EnumeratorNode parseEnumerator()
        {
            EnumeratorNode node = null;
            EnumConstantNode enumconst = parseEnumerationConstant();
            ExprNode constexpr = null;
            bool result = (enumconst != null);
            if (result)
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tEqual);
                if (result2)
                {
                    constexpr = parseConstantExpression();
                    result2 = (constexpr != null);
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint);
                }
                node = arbor.makeEnumeratorNode(enumconst, constexpr);
            }
            return node;
        }

        /*(6.4.4.3) 
         enumeration-constant:
            identifier
         */
        public EnumConstantNode parseEnumerationConstant()
        {
            EnumConstantNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            if (define)
            {
                node = arbor.makeEnumConstNode(token);
            }
            else
            {
                node = arbor.getEnumConstNode(token);
            }
            if (node == null)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        //- end of enumerations -------------------------------------

        /*(6.7.3) 
         type-qualifier:
            const
            restrict
            volatile
         */
        public TypeQualNode parseTypeQual()
        {
            TypeQualNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            if ((token is tConst) || (token is tRestrict) || (token is tVolatile))
            {
                node = arbor.makeTypeQualNode(token);
            }
            else
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.4) 
         function-specifier:
            inline
         */
        public FuncSpecNode parseFunctionSpec()
        {
            FuncSpecNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            if (token is tInline)
            {
                node = arbor.makeFuncSpecNode(token);
            }
            else
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        //- declarator ----------------------------------------------

        /*(6.7.5) 
         declarator:
            pointer[opt] direct-declarator
         */
        public DeclaratorNode parseDeclarator()
        {
            int cuepoint = scanner.record();
            bool result = parsePointer();
            if (result)
            {
                result = parseDirectDeclarator();
                if (result)
                {
                    //Console.WriteLine("parsed pointer direct-declar declar");
                }
            }
            else
            {
                result = parseDirectDeclarator();
                if (result)
                {
                    //Console.WriteLine("parsed direct-declar declar");
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.5) 
         direct-declarator:
            identifier
            ( declarator )
            direct-declarator [ type-qualifier-list[opt] assignment-expression[opt] ]
            direct-declarator [ static type-qualifier-list[opt] assignment-expression ]
            direct-declarator [ type-qualifier-list static assignment-expression ]
            direct-declarator [ type-qualifier-list[opt] * ]
            direct-declarator ( parameter-type-list )
            direct-declarator ( identifier-list[opt] )
         */
        public bool parseDirectDeclarator()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = isIdentifier(token);               //identifier
            if (result)
            {
                //Console.WriteLine("parsed ident direct-declar");
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
                token = scanner.getToken();
                result = (token is tLParen);                    //( declarator )
                if (result)
                {
                    result = parseDeclarator();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            //Console.WriteLine("parsed declar direct-declar");
                        }
                    }
                }
            }
            bool empty = !result;
            while (result)
            {
                int cuepoint2 = scanner.record();           //direct-declarator [ type-qualifier-list[opt] assignment-expression[opt] ]
                token = scanner.getToken();
                result = (token is tLBracket);
                if (result)
                {
                    parseTypeQualList();
                    parseAssignExpression();
                    token = scanner.getToken();
                    result = (token is tRBracket);
                    if (result)
                    {
                        //Console.WriteLine("parsed assign expr direct-declar");
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);              //direct-declarator [ static type-qualifier-list[opt] assignment-expression ]
                    token = scanner.getToken();
                    result = (token is tLBracket);
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tStatic);
                        if (result)
                        {
                            parseTypeQualList();
                            result = parseAssignExpression();
                            if (result)
                            {
                                token = scanner.getToken();
                                result = (token is tRBracket);
                                if (result)
                                {
                                    //Console.WriteLine("parsed static assign expr direct-declar");
                                }
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //direct-declarator [ type-qualifier-list static assignment-expression ]
                    token = scanner.getToken();
                    result = (token is tLBracket);
                    if (result)
                    {
                        result = parseTypeQualList();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tStatic);
                            if (result)
                            {
                                result = parseAssignExpression();
                                if (result)
                                {
                                    token = scanner.getToken();
                                    result = (token is tRBracket);
                                    if (result)
                                    {
                                        //Console.WriteLine("parsed type-qual-list direct-declar");
                                    }
                                }
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                      //direct-declarator [ type-qualifier-list[opt] * ]
                    token = scanner.getToken();
                    result = (token is tLBracket);
                    if (result)
                    {
                        parseTypeQualList();
                        token = scanner.getToken();
                        result = (token is tAsterisk);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRBrace);
                            if (result)
                            {
                                //Console.WriteLine("parsed pointer direct-declar");
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                      //direct-declarator ( parameter-type-list )
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        result = parseParameterTypeList();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRParen);
                            if (result)
                            {
                                //Console.WriteLine("parsed param-type-list direct-declar");
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //direct-declarator ( identifier-list[opt] )
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        parseIdentifierList();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRParen);
                            if (result)
                            {
                                //Console.WriteLine("parsed ident-list direct-declar");
                            }
                        }
                    }
                    if (!result)
                    {
                        scanner.rewind(cuepoint2);
                    }
                }
            }
            if (empty)
            {
                scanner.rewind(cuepoint);
            }
            return !empty;
        }

        /*(6.7.5) 
         pointer:
            * type-qualifier-list[opt]
            * type-qualifier-list[opt] pointer
         */
        public PointerNode parsePointer()
        {
            PointerNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tAsterisk);
            if (result)
            {
                List<TypeQualNode> qualList = parseTypeQualList();
                PointerNode ptr = parsePointer();
                result = (ptr != null);
                if (result)
                {
                    node = arbor.makePointerNode(qualList, ptr);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.5) 
         type-qualifier-list:
            type-qualifier
            type-qualifier-list type-qualifier
         */
        public List<TypeQualNode> parseTypeQualList()
        {
            List<TypeQualNode> list = null;
            TypeQualNode node = parseTypeQual();
            bool result = (node != null);
            if (result)
            {
                list = new List<TypeQualNode>();
                list.Add(node);
            }
            bool notEmpty = result;
            while (result)
            {
                node = parseTypeQual();
                result = (node != null);
                if (result)
                {
                    list.Add(node);
                }
            }
            return notEmpty;
        }

        /*(6.7.5) 
         parameter-type-list:
            parameter-list
            parameter-list , ...
         */
        public ParamTypeListNode parseParameterTypeList()
        {
            bool result = parseParameterList();
            if (result)
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tComma);
                if (result2)
                {
                    token = scanner.getToken();
                    result2 = (token is tEllipsis);
                    if (result2)
                    {
                        //Console.WriteLine("parsed param-list elipsis param-type-list");
                    }
                }
                if (!result2)
                {
                    //Console.WriteLine("parsed param-list param-type-list");
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint);
                }
            }
            return result;
        }

        /*(6.7.5) 
         parameter-list:
            parameter-declaration
            parameter-list , parameter-declaration
         */
        public bool parseParameterList()
        {
            bool result = parseParameterDeclar();
            if (result)
            {
                //Console.WriteLine("parsed param-declar param-list");
            }
            bool notEmpty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    result = parseParameterDeclar();
                    if (result)
                    {
                        //Console.WriteLine("parsed another param-declar param-list");
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return notEmpty;
        }

        /*(6.7.5) 
         parameter-declaration:
            declaration-specifiers declarator
            declaration-specifiers abstract-declarator[opt]
         */
        public bool parseParameterDeclar()
        {
            int cuepoint = scanner.record();
            bool result = parseDeclarationSpecs();
            if (result)
            {
                bool result2 = parseDeclarator();
                if (result)
                {
                    //Console.WriteLine("parsed declar-spec declar param-declar");
                }
                if (!result2)
                {
                    parseAbstractDeclarator();
                    if (result)
                    {
                        //Console.WriteLine("parsed declar-spec abs-declar param-declar");
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.5) 
         identifier-list:
            identifier
            identifier-list , identifier
         */
        public List<IdentNode> parseIdentifierList()
        {
            List<IdentNode> list = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            IdentNode id = arbor.makeIdentifierNode(token);
            bool result = (id != null);
            if (result)
            {
                list = new List<IdentNode>();
                list.Add(id);
            }
            bool empty = !result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    token = scanner.getToken();
                    id = arbor.makeIdentifierNode(token);
                    result = (id != null);
                    if (result)
                    {
                        list.Add(id);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            if (empty)
            {
                scanner.rewind(cuepoint);
            }
            return list;
        }

        /*(6.7.6) 
         type-name:
            specifier-qualifier-list abstract-declarator[opt]
         */
        public TypeNameNode parseTypeName()
        {
            TypeNameNode node = null;
            List<DeclarSpecNode> list = parseSpecQualList();
            bool result = (list != null);
            if (result)
            {
                AbstractDeclaratorNode declar = parseAbstractDeclarator();
                result = (declar != null);
                if (result)
                {
                    //Console.WriteLine("parsed spec-qual abstractor-declarator type-name");
                    node = arbor.makeTypeNameNode(list, declar);
                }
            }
            return node;
        }

        /*(6.7.6) 
         abstract-declarator:
            pointer
            pointer[opt] direct-abstract-declarator
         */
        public AbstractDeclaratorNode parseAbstractDeclarator()
        {
            AbstractDeclaratorNode node = null;
            PointerNode ptr = parsePointer();
            DirectDeclatorNode direct = parseDirectAbstractDeclarator();
            node = arbor.makeAbstractDeclaratorNode(ptr, direct);
            return node;
        }

        /*(6.7.6) 
         direct-abstract-declarator:
            ( abstract-declarator )
            direct-abstract-declarator[opt] [ type-qualifier-list[opt] assignment-expression[opt] ]
            direct-abstract-declarator[opt] [ static type-qualifier-list[opt] assignment-expression ]
            direct-abstract-declarator[opt] [ type-qualifier-list static assignment-expression ]
            direct-abstract-declarator[opt] [ * ]
            direct-abstract-declarator[opt] ( parameter-type-list[opt] )
         */
        public DirectDeclatorNode parseDirectAbstractDeclarator()
        {
            DirectDeclatorNode node = null;
            AbstractDeclaratorNode declar = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tLParen);
            if (result)
            {
                declar = parseAbstractDeclarator();         //( abstract-declarator )
                result = (declar != null);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tRParen);
                }
            }
            bool empty = !result;
            result = true;                  //the base is optional, so we parse for the clauses, even if we didn't match the base
            if (empty)                      //but if the base didn't match, need to roll that back
            {
                scanner.rewind(cuepoint);
            }
            while (result)
            {
                int cuepoint2 = scanner.record();     //direct-abstract-declarator[opt] [ type-qualifier-list[opt] assignment-expression[opt] ]
                token = scanner.getToken();
                result = (token is tLBracket);
                if (result)
                {
                    List<TypeQualNode> list = parseTypeQualList();
                    AssignExpressionNode assign = parseAssignExpression();
                    token = scanner.getToken();
                    result = (token is tRBracket);
                    if (result)
                    {
                        node = arbor.makeDirectAbstractIndexNode(declar, false, list, false, assign);
                    }
                }

                if (!result)
                {
                    scanner.rewind(cuepoint2);        //direct-abstract-declarator[opt] [ static type-qualifier-list[opt] assignment-expression ]
                    token = scanner.getToken();
                    result = (token is tLBracket);
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tStatic);
                        if (result)
                        {
                            List<TypeQualNode> list = parseTypeQualList();
                            AssignExpressionNode assign = parseAssignExpression();
                            result = (assign != null);
                            if (result)
                            {
                                token = scanner.getToken();
                                result = (token is tRBracket);
                                if (result)
                                {
                                    node = arbor.makeDirectAbstractIndexNode(declar, true, list, false, assign);
                                }
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);        //direct-abstract-declarator[opt] [ type-qualifier-list static assignment-expression ]
                    token = scanner.getToken();
                    result = (token is tLBracket);
                    if (result)
                    {
                        List<TypeQualNode> list = parseTypeQualList();
                        result = (list != null);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tStatic);
                            if (result)
                            {
                                AssignExpressionNode assign = parseAssignExpression();
                                result = (assign != null);
                                if (result)
                                {
                                    token = scanner.getToken();
                                    result = (token is tRBracket);
                                    if (result)
                                    {
                                        node = arbor.makeDirectAbstractIndexNode(declar, false, list, true, assign);
                                    }
                                }
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);        //direct-abstract-declarator[opt] [ * ]
                    token = scanner.getToken();
                    result = (token is tLBracket);
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tAsterisk);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRBracket);
                            if (result)
                            {
                                node = arbor.makeDirectAbstractIndexNode(declar, false, null, true, null);
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);               //direct-abstract-declarator[opt] ( parameter-type-list[opt] )
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        ParamTypeListNode list = parseParameterTypeList();
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            //Console.WriteLine("parsed param type list direct-abstractor-declarator");
                            node = arbor.makeDirectAbstractParamNode(declar, list);
                        }
                    }
                    if (!result)
                    {
                        scanner.rewind(cuepoint2);
                    }
                }
            }
            if (empty)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        //- end of declarators ------------------------------------

        /*(6.7.7) 
         typedef-name:
            identifier
         */
        public TypedefNode parseTypedefName()
        {
            TypedefNode node = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            node = arbor.getTypedefNode(token);
            if (node == null)
            {
                scanner.rewind(cuepoint);
            }
            return node;
        }

        //- declaration initializers ------------------------------------

        /*(6.7.8) 
         initializer:
            assignment-expression
            { initializer-list }
            { initializer-list , }
         */
        public InitializerNode parseInitializer()
        {
            bool result = parseAssignExpression();
            if (result)
            {
                //Console.WriteLine("parsed assign expr initializer");
            }
            else
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLBrace);
                if (result)
                {
                    result = parseInitializerList();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRBrace);
                        if (result)
                        {
                            //Console.WriteLine("parsed initializer-list initializer");
                        }
                        else
                        {
                            result = (token is tComma);
                            if (result)
                            {
                                token = scanner.getToken();
                                result = (token is tRBrace);
                                if (result)
                                {
                                    //Console.WriteLine("parsed initializer-list and comma initializer");
                                }
                            }
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint);
                }
            }
            return result;
        }

        /*(6.7.8) 
         initializer-list:
            designation[opt] initializer
            initializer-list , designation[opt] initializer
         */
        public List<InitializerNode> parseInitializerList()
        {
            parseDesignation();
            bool result = parseInitializer();
            bool notEmpty = result;
            if (result)
            {
                //Console.WriteLine("parsed (designation) initializer initializer-list");
            }
            while (result)
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    parseDesignation();
                    result = parseInitializer();
                    if (result)
                    {
                        //Console.WriteLine("parsed another (designation) initializer initializer-list");
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint);
                }
            }
            return notEmpty;
        }

        /*(6.7.8) 
         designation:
            designator-list =
         */
        public bool parseDesignation()
        {
            int cuepoint = scanner.record();
            bool result = parseDesignatorList();
            if (result)
            {
                Token token = scanner.getToken();
                result = (token is tEqual);
                if (result)
                {
                    //Console.WriteLine("parsed designation");
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.8) 
         designator-list:
            designator
            designator-list designator
         */
        public bool parseDesignatorList()
        {
            bool result = parseDesignator();
            if (result)
            {
                //Console.WriteLine("parsed designator designator-list");
            }
            bool notEmpty = result;
            while (result)
            {
                result = parseDesignator();
                if (result)
                {
                    //Console.WriteLine("parsed another designator designator-list");
                }
            }
            return notEmpty;
        }

        /*(6.7.8) 
         designator:
            [ constant-expression ]
            . identifier
         */
        public bool parseDesignator()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tLBracket);
            if (result)
            {
                result = parseConstantExpression();
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tRBracket);
                    if (result)
                    {
                        //Console.WriteLine("parsed constant expression designator");
                    }
                }
            }
            if (!result)
            {
                result = (token is tPeriod);
                if (result)
                {
                    token = scanner.getToken();
                    result = isIdentifier(token);
                    if (result)
                    {
                        //Console.WriteLine("parsed dot identifier designator");
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        //- statements --------------------------------------------------------

        /* (6.8)
         statement:
             labeled-statement
             compound-statement
             expression-statement
             selection-statement
             iteration-statement
             jump-statement
        */
        public bool parseStatement()
        {
            bool result = parseLabeledStatement();
            if (!result)
            {
                result = parseCompoundStatement();
            }
            if (!result)
            {
                result = parseExpressionStatement();
            }
            if (!result)
            {
                result = parseSelectionStatement();
            }
            if (!result)
            {
                result = parseIterationStatement();
            }
            if (!result)
            {
                result = parseJumpStatement();
            }
            return result;
        }

        /*(6.8.1) 
         labeled-statement:
            identifier : statement
            case constant-expression : statement
            default : statement
         */
        public bool parseLabeledStatement()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = isIdentifier(token);             //identifier : statement
            if (result)
            {
                token = scanner.getToken();
                result = (token is tColon);
                if (result)
                {
                    result = parseStatement();
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);        //case constant-expression : statement
                token = scanner.getToken();
                result = (token is tCase);
                if (result)
                {
                    result = parseConstantExpression();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tColon);
                        if (result)
                        {
                            result = parseStatement();

                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);        //default : statement
                token = scanner.getToken();
                result = (token is tDefault);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tColon);
                    if (result)
                    {
                        result = parseStatement();

                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.8.2) 
         compound-statement:
            { block-item-list[opt] }
         */
        public bool parseCompoundStatement()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tLBrace);
            if (result)
            {
                parseBlockItemList();
                token = scanner.getToken();
                result = (token is tRBrace);
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.8.2) 
         block-item-list:
            block-item
            block-item-list block-item
         */
        public bool parseBlockItemList()
        {
            bool result = parseBlockItem();
            bool notEmpty = result;
            while (result)
            {
                result = parseBlockItem();
            }
            return notEmpty;
        }

        /*(6.8.2) 
         block-item:
            declaration
            statement
         */
        public bool parseBlockItem()
        {
            bool result = parseDeclaration();
            if (!result)
            {
                result = parseStatement();
            }
            return result;
        }

        /*(6.8.3) 
         expression-statement:
            expression[opt] ;
         */
        public bool parseExpressionStatement()
        {
            int cuepoint = scanner.record();
            parseExpression();
            Token token = scanner.getToken();
            bool result = (token is tSemicolon);
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.8.4) 
         selection-statement:
            if ( expression ) statement
            if ( expression ) statement else statement
            switch ( expression ) statement
         */
        public bool parseSelectionStatement()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tIf);             //if ( expression ) statement
            if (result)
            {
                token = scanner.getToken();
                result = (token is tLParen);
                if (result)
                {
                    result = parseExpression();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            result = parseStatement();
                            if (result)
                            {
                                int cuepoint2 = scanner.record();     //if ( expression ) statement else statement
                                token = scanner.getToken();
                                bool result2 = (token is tElse);
                                if (result2)
                                {
                                    result2 = parseStatement();
                                }
                                if (!result2)
                                {
                                    scanner.rewind(cuepoint2);
                                }
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);            //switch ( expression ) statement
                token = scanner.getToken();
                result = (token is tSwitch);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tLParen);

                    if (result)
                    {
                        result = parseExpression();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRParen);
                            if (result)
                            {
                                result = parseStatement();
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.8.5) 
         iteration-statement:
            while ( expression ) statement
            do statement while ( expression ) ;
            for ( expression[opt] ; expression[opt] ; expression[opt] ) statement
            for ( declaration expression[opt] ; expression[opt] ) statement
         */
        public bool parseIterationStatement()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tWhile);             //while ( expression ) statement
            if (result)
            {
                token = scanner.getToken();
                result = (token is tLParen);
                if (result)
                {
                    result = parseExpression();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            result = parseStatement();
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);               //do statement while ( expression ) ;
                token = scanner.getToken();
                result = (token is tDo);
                if (result)
                {
                    result = parseStatement();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tWhile);
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tLParen);
                            if (result)
                            {
                                result = parseExpression();
                                if (result)
                                {
                                    token = scanner.getToken();
                                    result = (token is tRParen);
                                    if (result)
                                    {
                                        token = scanner.getToken();
                                        result = (token is tSemicolon);
                                    }
                                }

                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);           //for ( expression[opt] ; expression[opt] ; expression[opt] ) statement
                token = scanner.getToken();
                result = (token is tFor);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        parseExpression();
                        token = scanner.getToken();
                        result = (token is tSemicolon);
                        if (result)
                        {
                            parseExpression();
                            token = scanner.getToken();
                            result = (token is tSemicolon);
                            if (result)
                            {
                                parseExpression();
                                token = scanner.getToken();
                                result = (token is tRParen);
                                if (result)
                                {
                                    result = parseStatement();
                                }
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);           //for ( declaration expression[opt] ; expression[opt] ) statement
                token = scanner.getToken();
                result = (token is tFor);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        result = parseDeclaration();
                        if (result)
                        {
                            parseExpression();
                            token = scanner.getToken();
                            result = (token is tSemicolon);
                            if (result)
                            {
                                parseExpression();
                                token = scanner.getToken();
                                result = (token is tRParen);
                                if (result)
                                {
                                    result = parseStatement();
                                }
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.8.6) 
         jump-statement:
            goto identifier ;
            continue ;
            break ;
            return expression[opt] ;
         */
        public bool parseJumpStatement()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tGoto);             //goto identifier ;
            if (result)
            {
                token = scanner.getToken();
                result = isIdentifier(token);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tSemicolon);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);        //continue ;
                token = scanner.getToken();
                result = (token is tContinue);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tSemicolon);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);        //break ;
                token = scanner.getToken();
                result = (token is tBreak);
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tSemicolon);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);        //return expression[opt] ;
                token = scanner.getToken();
                result = (token is tReturn);
                if (result)
                {
                    parseExpression();
                    token = scanner.getToken();
                    result = (token is tSemicolon);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);                
            }
            return result;
        }

        //- external definitions ----------------------------------------------

        /*(6.9) 
         translation-unit:
            external-declaration
            translation-unit external-declaration 
        */
        public void parseTranslationUnit()
        {
            bool result = parseExternalDeclaration();
            while (result)
            {
                result = parseExternalDeclaration();
            }
        }

        /* (6.9) 
         external-declaration:
            function-definition
            declaration
        */
        public bool parseExternalDeclaration()
        {
            bool result = parseFunctionDef();
            if (!result)
            {
                result = parseDeclaration();
            }
            return result;
        }

        /* (6.9.1) 
         function-definition:
            declaration-specifiers declarator declaration-list[opt] compound-statement
         */
        public bool parseFunctionDef()
        {
            int cuepoint = scanner.record();
            bool result = parseDeclarationSpecs();
            if (result)
            {
                result = parseDeclarator();
                if (result)
                {
                    parseDeclarationList();
                    result = parseCompoundStatement();
                }
            }
            if (result)
            {
                //arbor.buildFunctionDef();
                funcCount++;
                Console.WriteLine("parsed function def " + funcCount.ToString());
            }
            else
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.9.1) 
         declaration-list:
            declaration
            declaration-list declaration
        */
        public List<DeclarationNode> parseDeclarationList()
        {
            List<DeclarationNode> declarList = null;
            DeclarationNode declar = parseDeclaration();
            if (declar != null)
            {
                declarList = new List<DeclarationNode>();
                declarList.Add(declar);
            }
            while (declar != null)
            {
                declar = parseDeclaration();
                declarList.Add(declar);
            }
            return declarList;
        }

        //-----------------------------------------------------------------------------

        public void parseFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);

            preprocessor = new Preprocessor(lines);
            preprocessor.process();

            scanner = new Scanner(lines);
            arbor = new Arbor();
            scanner.arbor = arbor;

            parseTranslationUnit();

            Console.WriteLine("done parsing");
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");