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

// the grammar this parser is pased on:
//https://en.wikipedia.org/wiki/C99
//http://www.open-std.org/jtc1/sc22/WG14/www/docs/n1256.pdf

namespace BlackC
{
    public class Parser
    {
        public Options options;
        public Preprocessor prep;
        public Arbor arbor;

        public List<String> includePaths;

        public Parser(Options _options)
        {
            options = _options;
            prep = new Preprocessor(this);
            arbor = new Arbor(this);

            includePaths = options.includePaths;
        }

        //---------------------------------------------------------------------

        public void parseFile(String filename)
        {
            prep.setMainSourceFile(filename);

            Token token = null;
            do
            {
                //dump token stream from input file for testing purposes
                //to be removed
                token = prep.getToken();
                prep.next();
                if (token.type == TokenType.tEOLN)
                {
                    Console.WriteLine();
                }
                else
                {
                    if (token.sawWS) Console.Write(" ");
                    Console.Write(token.chars);
                }
            }
            while (token.type != TokenType.tEOF);

            TranslationUnit unit = parseTranslationUnit();
            unit.write();
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
                    TypeNameNode name = parseTypeName();
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
                                List<InitializerNode> initList = parseInitializerList();
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
                        TypeNameNode name = parseTypeName();
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
                    TypeNameNode name = parseTypeName();
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

        //- declarations ------------------------------------------------------

        /* 
         (6.7) 
         declaration:
            declaration-specifiers init-declarator-list[opt] ;
         
         (6.7) 
         init-declarator-list:
            init-declarator
            init-declarator-list , init-declarator

         (6.7) 
         init-declarator:
            declarator
            declarator = initializer
         */
        public DeclarationNode parseDeclaration()
        {
            DeclarationNode node = null;
            DeclarSpecNode declarspecs = parseDeclarationSpecs();

            //type definition, like struct foo {...}; or enum bar {...};
            if (prep.getToken().type == TokenType.tSEMICOLON)
            {
                prep.next();
                node = arbor.makeTypeDefNode(declarspecs);
                node.isFuncDef = false;
                return node;
            }

            bool done = false;
            bool isFuncDef = true;
            while (!done)
            {
                DeclaratorNode declarnode = parseDeclarator(false);
                if (prep.getToken().type == TokenType.tEQUAL)
                {
                    prep.next();
                    isFuncDef = false;
                    InitializerNode initialnode = parseInitializer();
                    node = arbor.makeDeclaration(declarspecs, declarnode, initialnode, node);       //declarator = initializer
                }
                else
                {
                    node = arbor.makeDeclaration(declarspecs, declarnode, null, node);      //declarator
                }

                if (prep.getToken().type == TokenType.tCOMMA)
                {
                    isFuncDef = false;
                    prep.next();
                }
                else if (prep.getToken().type == TokenType.tSEMICOLON)
                {
                    isFuncDef = false;
                    prep.next();
                    done = true;
                }
            }
            node.isFuncDef = isFuncDef;
            return node;
        }

        /* (6.7) 
         declaration-specifiers:
            storage-class-specifier declaration-specifiers[opt]
            type-specifier declaration-specifiers[opt]
            type-qualifier declaration-specifiers[opt]
            function-specifier declaration-specifiers[opt]      

         (6.7.1) 
         storage-class-specifier:
            typedef
            extern
            static
            auto
            register
         
         (6.7.2) 
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

         (6.7.3) 
         type-qualifier:
            const
            restrict
            volatile
         
         (6.7.4) 
         function-specifier:
            inline
        */
        public DeclarSpecNode parseDeclarationSpecs()
        {
            DeclarSpecNode specs = new DeclarSpecNode();
            bool done = false;
            while (!done)
            {
                Token token = prep.getToken();
                switch (token.type)
                {
                    case TokenType.tTYPEDEF:
                    case TokenType.tEXTERN:
                    case TokenType.tSTATIC:
                    case TokenType.tAUTO:
                    case TokenType.tREGISTER:
                        specs.setStorageClassSpec(token);
                        prep.next();
                        break;

                    case TokenType.tVOID:
                    case TokenType.tCHAR:
                    case TokenType.tINT:
                    case TokenType.tFLOAT:
                    case TokenType.tDOUBLE:
                        specs.setBaseClassSpec(token);
                        prep.next();
                        break;

                    case TokenType.tSHORT:
                    case TokenType.tLONG:
                    case TokenType.tSIGNED:
                    case TokenType.tUNSIGNED:
                        specs.setBaseClassModifier(token);
                        prep.next();
                        break;

                    case TokenType.tSTRUCT:
                    case TokenType.tUNION:
                        specs.typeSpec = parseStructOrUnionSpec();
                        break;

                    case TokenType.tENUM:
                        specs.typeSpec = parseEnumeratorSpec();
                        prep.next();
                        break;

                    case TokenType.tIDENTIFIER:
                        TypeSpecNode ts = parseTypedefName();
                        if (ts != null)
                        {
                            specs.typeSpec = ts;
                        }
                        else
                        {
                            done = true;
                        }
                        break;

                    case TokenType.tCONST:
                    case TokenType.tRESTRICT:
                    case TokenType.tVOLATILE:
                        specs.setTypeQual(token);
                        prep.next();
                        break;

                    case TokenType.tINLINE:
                        specs.setFunctionSpec(token);
                        prep.next();
                        break;

                    default:
                        done = true;
                        break;
                }
            }
            specs.complete();
            return specs;
        }

        /*(6.7.7) 
         typedef-name:
            identifier
        */
        public TypeSpecNode parseTypedefName()
        {
            Token token = prep.getToken();
            IdentNode tdnode = arbor.findIdent(token);
            if ((tdnode != null) && (tdnode.def != null) && (tdnode.def is TypeSpecNode))
            {
                prep.next();
                return (TypeSpecNode)tdnode.def;
            }
            return null;
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
            Token token = prep.getToken();
            StructSpecNode node = null;
            int cuepoint = prep.record();
            StructUnionNode tag = parseStuctOrUnion();
            bool result = (tag != null);
            if (result)
            {
                //Token token = prep.getToken();
                IdentNode name = arbor.getStructIdentNode(token);
                result = (name != null);
                if (result)
                {
                    int cuepoint2 = prep.record();
                    token = prep.getToken();
                    bool result2 = (token.type == TokenType.tLBRACE);
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
                            token = prep.getToken();
                            result2 = (token.type == TokenType.tRBRACE);
                            if (result2)
                            {
                                node = arbor.makeStructSpec(tag, name, declarList);         //struct-or-union ident struct-declar-list
                            }
                        }
                    }
                    if (!result2)
                    {
                        prep.rewind(cuepoint2);
                    }
                }
                else
                {
                    result = (token.type == TokenType.tLBRACE);
                    if (result)
                    {
                        List<StructDeclarationNode> declarList = parseStructDeclarationList();
                        result = (declarList != null);
                        if (result)
                        {
                            token = prep.getToken();
                            result = (token.type == TokenType.tRBRACE);
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
                prep.rewind(cuepoint);
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
            int cuepoint = prep.record();
            Token token = prep.getToken();
            switch (token.ToString())
            {
                case "STRUCT":
                    node = new StructUnionNode(StructUnionNode.LAYOUT.STRUCT);
                    break;

                case "UNION":
                    node = new StructUnionNode(StructUnionNode.LAYOUT.UNION);
                    break;
            }
            if (node == null)
            {
                prep.rewind(cuepoint);
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
            int cuepoint = prep.record();
            List<DeclarSpecNode> specqual = parseSpecQualList();          //field type
            bool result = (specqual != null);
            if (result)
            {
                List<StructDeclaratorNode> fieldnames = parseStructDeclaratorList();           //list of field names 
                result = (fieldnames != null);
                if (result)
                {
                    Token token = prep.getToken();
                    result = (token.type == TokenType.tSEMICOLON);
                    if (result)
                    {
                        node = arbor.makeStructDeclarationNode(specqual, fieldnames);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
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
            //DeclarSpecNode specnode = parseTypeSpec();
            //if (specnode == null)
            //{
            //    specnode = parseTypeQual();
            //}
            //if (specnode != null)
            //{
            //    speclist = new List<DeclarSpecNode>();
            //    speclist.Add(specnode);
            //    List<DeclarSpecNode> taillist = parseSpecQualList();
            //    if (taillist != null)
            //    {
            //        speclist.AddRange(taillist);
            //    }
            //}
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
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
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
                    prep.rewind(cuepoint2);
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
            int cuepoint = prep.record();
            DeclaratorNode declarnode = parseDeclarator(false);
            bool result = (declarnode != null);
            if (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                bool result2 = (token.type == TokenType.tCOLON);
                if (result2)
                {
                    ConstExpressionNode constexpr = parseConstantExpression();
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
                    prep.rewind(cuepoint2);
                }
            }
            if (!result)
            {
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOLON);
                if (result)
                {
                    ConstExpressionNode constexpr = parseConstantExpression();
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
                prep.rewind(cuepoint);
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
            int cuepoint = prep.record();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tENUM);
            if (result)
            {
                token = prep.getToken();
                IdentNode idNode = arbor.getEnumIdentNode(token);
                result = (idNode != null);
                if (result)
                {
                    int cuepoint2 = prep.record();
                    token = prep.getToken();
                    bool result2 = (token.type == TokenType.tLBRACE);             //enum identifier { enumerator-list }
                    if (result2)
                    {
                        List<EnumeratorNode> enumList = parseEnumeratorList();
                        result2 = (enumList != null);
                        if (result2)
                        {
                            token = prep.getToken();
                            result2 = (token.type == TokenType.tRBRACE);
                            if (!result2)
                            {
                                result2 = (token.type == TokenType.tCOMMA);            //enum identifier { enumerator-list , }
                                if (result2)
                                {
                                    token = prep.getToken();
                                    result2 = (token.type == TokenType.tRBRACE);
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
                        prep.rewind(cuepoint2);
                    }
                }
                else
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tLBRACE);             //enum { enumerator-list }
                    if (result)
                    {
                        List<EnumeratorNode> enumList = parseEnumeratorList();
                        result = (enumList != null);
                        if (result)
                        {
                            token = prep.getToken();
                            result = (token.type == TokenType.tRBRACE);
                            if (!result)
                            {
                                result = (token.type == TokenType.tCOMMA);            //enum { enumerator-list , }
                                if (result)
                                {
                                    token = prep.getToken();
                                    result = (token.type == TokenType.tRBRACE);
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
                prep.rewind(cuepoint);
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
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
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
                    prep.rewind(cuepoint2);
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
            ConstExpressionNode constexpr = null;
            bool result = (enumconst != null);
            if (result)
            {
                int cuepoint = prep.record();
                Token token = prep.getToken();
                bool result2 = (token.type == TokenType.tEQUAL);
                if (result2)
                {
                    constexpr = parseConstantExpression();
                    result2 = (constexpr != null);
                }
                if (!result2)
                {
                    prep.rewind(cuepoint);
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
            int cuepoint = prep.record();
            Token token = prep.getToken();
            if (token != null)
            {
                node = arbor.makeEnumConstNode(token);
            }
            if (node == null)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        //- declarators -------------------------------------------------------

        /*
         (6.7.5) 
         declarator:
            pointer[opt] direct-declarator
         
         (6.7.5) 
         pointer:
            * type-qualifier-list[opt]
            * type-qualifier-list[opt] pointer         

         (6.7.6) 
         abstract-declarator:
            pointer
            pointer[opt] direct-abstract-declarator
        */
        public DeclaratorNode parseDeclarator(bool isAbstract)
        {
            if (prep.getToken().type == TokenType.tSTAR)
            {
                prep.next();
                TypeQualNode qualList = parseTypeQualList();
                DeclaratorNode declar = parseDeclarator(isAbstract);
                DeclaratorNode node = arbor.makePointerNode(qualList, declar);
                return node;
            }
            return parseDirectDeclarator(isAbstract);
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
          
         (6.7.6) 
         direct-abstract-declarator:
            ( abstract-declarator )
            direct-abstract-declarator[opt] [ type-qualifier-list[opt] assignment-expression[opt] ]
            direct-abstract-declarator[opt] [ static type-qualifier-list[opt] assignment-expression ]
            direct-abstract-declarator[opt] [ type-qualifier-list static assignment-expression ]
            direct-abstract-declarator[opt] [ * ]
            direct-abstract-declarator[opt] ( parameter-type-list[opt] )
         */
        //this handles the base cases of both direct-declarator and direct-abstract-declarator
        //the trailing clauses are handled in <parseDirectDeclaratorTail>
        public DeclaratorNode parseDirectDeclarator(bool isAbstract)
        {
            DeclaratorNode node = null;
            Token token = prep.getToken();

            //identifier
            if (!isAbstract && (token.type == TokenType.tIDENTIFIER))
            {
                prep.next();
                IdentDeclaratorNode idnode = new IdentDeclaratorNode(token);
                node = parseDirectDeclaratorTail(idnode, isAbstract);
                return node;
            }

            //in direct-abstract-declarator[opt] [...] if the direct-abstract-declarator is omitted, the first token
            //we see is the '[' of the declarator tail, so call parseDirectDeclaratorTail() with no base declarator
            if (isAbstract && (token.type == TokenType.tLBRACKET))
            {
                node = parseDirectDeclaratorTail(null, isAbstract);
                return node;
            }

            //similarly, in direct-abstract-declarator[opt] ( parameter-type-list[opt] ), we see the '(' if the 
            //direct-abstract-declarator is omitted, BUT this also may be ( declarator ) or ( abstract-declarator )
            //so test for param list or '()' first and if not, then its a parenthesized declarator
            if (token.type == TokenType.tLPAREN)
            {
                prep.next();
                if (isAbstract)
                {
                    ParamTypeListNode paramlist = parseParameterTypeList();
                    if ((paramlist != null) || (prep.getToken().type == TokenType.tRPAREN))
                    {
                        DeclaratorNode funcDeclar = arbor.makeFuncDeclarNode(null, paramlist);
                        node = parseDirectDeclaratorTail(funcDeclar, isAbstract);
                        return node;
                    }
                }

                //( declarator ) or ( abstract-declarator )
                DeclaratorNode declar = parseDeclarator(isAbstract);
                if (prep.getToken().type == TokenType.tRPAREN)
                {
                    prep.next();
                    node = parseDirectDeclaratorTail(declar, isAbstract);
                    return node;
                }
            }

            return node;
        }

        //parse one or more declarator clauses recursively
        public DeclaratorNode parseDirectDeclaratorTail(DeclaratorNode head, bool isAbstract)
        {
            DeclaratorNode node = null;
            Token token = prep.getToken();

            //array index declarator clause
            //mode 1: [ type-qualifier-list[opt] assignment-expression[opt] ]
            //mode 2: [ static type-qualifier-list[opt] assignment-expression ]
            //mode 3: [ type-qualifier-list static assignment-expression ]
            //mode 4: [ * ]
            if (prep.getToken().type == TokenType.tLBRACKET)
            {
                int mode = 1;
                TypeQualNode qualList = parseTypeQualList();
                AssignExpressionNode assign = null;
                bool isStatic = (prep.getToken().type == TokenType.tSTATIC);
                if (isStatic)
                {
                    prep.next();
                    mode = 3;
                    if (qualList.isEmpty)
                    {
                        qualList = parseTypeQualList();
                        mode = 2;
                    }
                }
                if ((mode == 1) && (prep.getToken().type == TokenType.tSTAR))
                {
                    prep.next();
                    mode = 4;
                }
                else
                {
                    assign = parseAssignExpression();
                }
                if (prep.getToken().type == TokenType.tRBRACKET)
                {
                    prep.next();
                }
                DeclaratorNode index = arbor.makeDirectIndexNode(head, mode, qualList, assign);
                node = parseDirectDeclaratorTail(index, isAbstract);
                return node;
            }

            //parameter list declarator clause
            else if (prep.getToken().type == TokenType.tLPAREN)
            {
                ParamTypeListNode paramlist = parseParameterTypeList();
                DeclaratorNode funcDeclar = arbor.makeFuncDeclarNode(head, paramlist);
                node = parseDirectDeclaratorTail(funcDeclar, isAbstract);
                return node;
            }
            return head;
        }

        /*(6.7.5) 
         type-qualifier-list:
            type-qualifier
            type-qualifier-list type-qualifier
         */
        public TypeQualNode parseTypeQualList()
        {
            TypeQualNode specs = new TypeQualNode();
            bool done = false;
            while (!done)
            {
                Token token = prep.getToken();
                switch (token.type)
                {
                    case TokenType.tCONST:
                    case TokenType.tRESTRICT:
                    case TokenType.tVOLATILE:
                        specs.setQualifer(token);
                        prep.next();
                        break;

                    default:
                        done = true;
                        break;
                }
            }
            return specs;
        }

        /*(6.7.5) 
         parameter-type-list:
            parameter-list
            parameter-list , ...
         */
        public ParamTypeListNode parseParameterTypeList()
        {
            ParamTypeListNode node = null;
            List<ParamDeclarNode> list = parseParameterList();
            bool result = (list != null);
            if (result)
            {
                int cuepoint = prep.record();
                Token token = prep.getToken();
                bool result2 = (token.type == TokenType.tCOMMA);
                if (result2)
                {
                    token = prep.getToken();
                    result2 = (token.type == TokenType.tELLIPSIS);
                    if (result2)
                    {
                        node = arbor.ParamTypeListNode(list, true);
                    }
                }
                else
                {
                    node = arbor.ParamTypeListNode(list, false);
                }
                if (!result2)
                {
                    prep.rewind(cuepoint);
                }
            }
            return node;
        }

        /*(6.7.5) 
         parameter-list:
            parameter-declaration
            parameter-list , parameter-declaration
         */
        public List<ParamDeclarNode> parseParameterList()
        {
            List<ParamDeclarNode> list = null;
            ParamDeclarNode param = parseParameterDeclar();
            bool result = (param != null);
            if (result)
            {
                list = new List<ParamDeclarNode>();
                list.Add(param);
            }
            bool notEmpty = result;
            while (result)
            {
                int cuepoint2 = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
                if (result)
                {
                    param = parseParameterDeclar();
                    result = (param != null);
                    if (result)
                    {
                        list.Add(param);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            return list;
        }

        /*(6.7.5) 
         parameter-declaration:
            declaration-specifiers declarator
            declaration-specifiers abstract-declarator[opt]
         */
        public ParamDeclarNode parseParameterDeclar()
        {
            ParamDeclarNode node = null;
            DeclaratorNode absdeclar = null;
            int cuepoint = prep.record();
            DeclarSpecNode declarspecs = parseDeclarationSpecs();
            bool result = (declarspecs != null);
            if (result)
            {
                DeclaratorNode declar = parseDeclarator(false);
                bool result2 = (declar != null);
                if (result2)
                {
                    //node = arbor.makeParamDeclarNode(declarspecs, declar, absdeclar);
                }
                else
                {
                    absdeclar = parseDeclarator(true);
                    //node = arbor.makeParamDeclarNode(declarspecs, null, absdeclar);
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.5) 
         identifier-list:
            identifier
            identifier-list , identifier
         */
        public List<IdentNode> parseIdentifierList()
        {
            List<IdentNode> list = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            IdentNode id = arbor.getArgIdentNode(token);
            bool result = (id != null);
            if (result)
            {
                list = new List<IdentNode>();
                list.Add(id);
            }
            bool empty = !result;
            while (result)
            {
                int cuepoint2 = prep.record();
                token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
                if (!result)
                {
                    token = prep.getToken();
                    id = arbor.getArgIdentNode(token);
                    result = (id != null);
                    if (result)
                    {
                        list.Add(id);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint2);
                }
            }
            if (empty)
            {
                prep.rewind(cuepoint);
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
                DeclaratorNode declar = parseDeclarator(true);
                result = (declar != null);
                if (result)
                {
                    //Console.WriteLine("parsed spec-qual abstractor-declarator type-name");
                    //node = arbor.makeTypeNameNode(list, declar);
                }
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
            InitializerNode node = null;
            AssignExpressionNode expr = parseAssignExpression();
            bool result = (expr != null);
            if (result)
            {
                node = arbor.makeInitializerNode(expr);
            }
            else
            {
                int cuepoint = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tLBRACE);
                if (result)
                {
                    List<InitializerNode> list = parseInitializerList();
                    result = (list != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRBRACE);
                        if (!result)
                        {
                            result = (token.type == TokenType.tCOMMA);
                            if (result)
                            {
                                token = prep.getToken();
                                result = (token.type == TokenType.tRBRACE);
                            }
                        }
                        if (result)
                        {
                            node = arbor.makeInitializerNode(list);
                        }
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint);
                }
            }
            return node;
        }

        /*(6.7.8) 
         initializer-list:
            designation[opt] initializer
            initializer-list , designation[opt] initializer
         */
        public List<InitializerNode> parseInitializerList()
        {
            List<InitializerNode> list = null;
            DesignationNode desinode = parseDesignation();
            InitializerNode initnode = parseInitializer();
            bool result = (initnode != null);
            if (result)
            {
                list = new List<InitializerNode>();
                initnode.addDesignation(desinode);
                list.Add(initnode);
            }
            while (result)
            {
                int cuepoint = prep.record();
                Token token = prep.getToken();
                result = (token.type == TokenType.tCOMMA);
                if (result)
                {
                    desinode = parseDesignation();
                    initnode = parseInitializer();
                    result = (initnode != null);
                    if (result)
                    {
                        initnode.addDesignation(desinode);
                        list.Add(initnode);
                    }
                }
                if (!result)
                {
                    prep.rewind(cuepoint);
                }
            }
            return list;
        }

        /*(6.7.8) 
         designation:
            designator-list =
         */
        public DesignationNode parseDesignation()
        {
            DesignationNode node = null;
            int cuepoint = prep.record();
            List<DesignatorNode> list = parseDesignatorList();
            bool result = (list != null);
            if (result)
            {
                Token token = prep.getToken();
                result = (token.type == TokenType.tEQUAL);
                if (result)
                {
                    node = arbor.makeDesignationNode(list);
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.7.8) 
         designator-list:
            designator
            designator-list designator
         */
        public List<DesignatorNode> parseDesignatorList()
        {
            List<DesignatorNode> list = null;
            DesignatorNode node = parseDesignator();
            if (node != null)
            {
                list = new List<DesignatorNode>();
                list.Add(node);
            }
            while (node != null)
            {
                node = parseDesignator();
                if (node != null)
                {
                    list.Add(node);
                }
            }
            return list;
        }

        /*(6.7.8) 
         designator:
            [ constant-expression ]
            . identifier
         */
        public DesignatorNode parseDesignator()
        {
            DesignatorNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tLBRACKET);
            if (result)
            {
                ConstExpressionNode expr = parseConstantExpression();
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tRBRACKET);
                    if (result)
                    {
                        node = arbor.makeDesignatorNode(expr);              //[ constant-expression ]
                    }
                }
            }
            if (!result)
            {
                result = (token.type == TokenType.tPERIOD);
                if (result)
                {
                    token = prep.getToken();
                    IdentNode ident = arbor.getFieldInitializerNode(token);
                    result = (ident != null);
                    if (result)
                    {
                        node = arbor.makeDesignatorNode(ident);             //. identifier
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
            }
            return node;
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
        public StatementNode parseStatement()
        {
            StatementNode node = parseLabeledStatement();
            if (node == null)
            {
                node = parseCompoundStatement();
            }
            if (node == null)
            {
                node = parseExpressionStatement();
            }
            if (node == null)
            {
                node = parseSelectionStatement();
            }
            if (node == null)
            {
                node = parseIterationStatement();
            }
            if (node == null)
            {
                node = parseJumpStatement();
            }
            return node;
        }

        /*(6.8.1) 
         labeled-statement:
            identifier : statement
            case constant-expression : statement
            default : statement
         */
        public StatementNode parseLabeledStatement()
        {
            StatementNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            IdentNode labelId = arbor.makeLabelIdentNode(token);         //identifier : statement
            bool result = (labelId != null);
            if (result)
            {
                token = prep.getToken();
                result = (token.type == TokenType.tCOLON);
                if (result)
                {
                    StatementNode stmt = parseStatement();
                    result = (stmt != null);
                    if (result)
                    {
                        node = arbor.makeLabelStatement(labelId);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);        //case constant-expression : statement
                token = prep.getToken();
                result = (token.type == TokenType.tCASE);
                if (result)
                {
                    ConstExpressionNode expr = parseConstantExpression();
                    result = (expr != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tCOLON);
                        if (result)
                        {
                            StatementNode stmt = parseStatement();
                            result = (stmt != null);
                            if (result)
                            {
                                node = arbor.makeCaseStatementNode(expr, stmt);
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);        //default : statement
                token = prep.getToken();
                result = (token.type == TokenType.tDEFAULT);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tCOLON);
                    if (result)
                    {
                        StatementNode stmt = parseStatement();
                        result = (stmt != null);
                        if (result)
                        {
                            node = arbor.makeDefaultStatementNode(stmt);
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

        /*(6.8.2) 
         compound-statement:
            { block-item-list[opt] }
         */
        public StatementNode parseCompoundStatement()
        {
            StatementNode node = null;
            List<BlockItemNode> list = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tLBRACE);
            if (result)
            {
                list = parseBlockItemList();
                token = prep.getToken();
                result = (token.type == TokenType.tRBRACE);
            }
            if (result)
            {
                node = arbor.makeCompoundStatementNode(list);
            }
            else
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.8.2) 
         block-item-list:
            block-item
            block-item-list block-item
         */
        public List<BlockItemNode> parseBlockItemList()
        {
            List<BlockItemNode> list = null;
            BlockItemNode item = parseBlockItem();
            bool result = (item != null);
            if (result)
            {
                list = new List<BlockItemNode>();
                list.Add(item);
            }
            while (result)
            {
                item = parseBlockItem();
                result = (item != null);
                if (result)
                {
                    list.Add(item);
                }
            }
            return list;
        }

        /*(6.8.2) 
         block-item:
            declaration
            statement
         */
        public BlockItemNode parseBlockItem()
        {
            BlockItemNode item = parseDeclaration();
            if (item == null)
            {
                item = parseStatement();
            }
            return item;
        }

        /*(6.8.3) 
         expression-statement:
            expression[opt] ;
         */
        public StatementNode parseExpressionStatement()
        {
            StatementNode node = null;
            int cuepoint = prep.record();
            ExpressionNode expr = parseExpression();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tSEMICOLON);
            if (result)
            {
                if (expr != null)
                {
                    node = arbor.makeExpressionStatement(expr);
                }
                else
                {
                    node = arbor.makeEmptyStatement(expr);
                }
            }
            else
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        /*(6.8.4) 
         selection-statement:
            if ( expression ) statement
            if ( expression ) statement else statement
            switch ( expression ) statement
         */
        public StatementNode parseSelectionStatement()
        {
            StatementNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tIF);             //if ( expression ) statement
            if (result)
            {
                token = prep.getToken();
                result = (token.type == TokenType.tLPAREN);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    result = (expr != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRPAREN);
                        if (result)
                        {
                            StatementNode thenstmt = parseStatement();
                            result = (thenstmt != null);
                            if (result)
                            {
                                int cuepoint2 = prep.record();     //if ( expression ) statement else statement
                                StatementNode elsestmt = null;
                                token = prep.getToken();
                                bool result2 = (token.type == TokenType.tELSE);
                                if (result2)
                                {
                                    elsestmt = parseStatement();
                                    result2 = (elsestmt != null);
                                }
                                if (!result2)
                                {
                                    prep.rewind(cuepoint2);
                                }
                                node = arbor.makeIfStatementNode(expr, thenstmt, elsestmt);
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);            //switch ( expression ) statement
                token = prep.getToken();
                result = (token.type == TokenType.tSWITCH);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tLPAREN);

                    if (result)
                    {
                        ExpressionNode expr = parseExpression();
                        result = (expr != null);
                        if (result)
                        {
                            token = prep.getToken();
                            result = (token.type == TokenType.tRPAREN);
                            if (result)
                            {
                                StatementNode stmt = parseStatement();
                                result = (stmt != null);
                                if (result)
                                {
                                    node = arbor.makeSwitchStatement(expr, stmt);
                                }
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

        /*(6.8.5) 
         iteration-statement:
            while ( expression ) statement
            do statement while ( expression ) ;
            for ( expression[opt] ; expression[opt] ; expression[opt] ) statement
            for ( declaration expression[opt] ; expression[opt] ) statement
         */
        public StatementNode parseIterationStatement()
        {
            StatementNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tWHILE);             //while ( expression ) statement
            if (result)
            {
                token = prep.getToken();
                result = (token.type == TokenType.tLPAREN);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    result = (expr != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tRPAREN);
                        if (result)
                        {
                            StatementNode stmt = parseStatement();
                            result = (stmt != null);
                            if (result)
                            {
                                node = arbor.makeWhileStatementNode(expr, stmt);
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);               //do statement while ( expression ) ;
                token = prep.getToken();
                result = (token.type == TokenType.tDO);
                if (result)
                {
                    StatementNode stmt = parseStatement();
                    result = (stmt != null);
                    if (result)
                    {
                        token = prep.getToken();
                        result = (token.type == TokenType.tWHILE);
                        if (result)
                        {
                            token = prep.getToken();
                            result = (token.type == TokenType.tLPAREN);
                            if (result)
                            {
                                ExpressionNode expr = parseExpression();
                                result = (expr != null);
                                if (result)
                                {
                                    token = prep.getToken();
                                    result = (token.type == TokenType.tRPAREN);
                                    if (result)
                                    {
                                        token = prep.getToken();
                                        result = (token.type == TokenType.tSEMICOLON);
                                        if (result)
                                        {
                                            node = arbor.makeDoStatementNode(stmt, expr);
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
                prep.rewind(cuepoint);           //for ( expression[opt] ; expression[opt] ; expression[opt] ) statement
                token = prep.getToken();
                result = (token.type == TokenType.tFOR);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tLPAREN);
                    if (result)
                    {
                        ExpressionNode expr1 = parseExpression();
                        token = prep.getToken();
                        result = (token.type == TokenType.tSEMICOLON);
                        if (result)
                        {
                            ExpressionNode expr2 = parseExpression();
                            token = prep.getToken();
                            result = (token.type == TokenType.tSEMICOLON);
                            if (result)
                            {
                                ExpressionNode expr3 = parseExpression();
                                token = prep.getToken();
                                result = (token.type == TokenType.tRPAREN);
                                if (result)
                                {
                                    StatementNode stmt = parseStatement();
                                    result = (stmt != null);
                                    if (result)
                                    {
                                        node = arbor.makeForStatementNode(expr1, expr2, expr3, stmt);
                                    }

                                }
                            }
                        }
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);           //for ( declaration expression[opt] ; expression[opt] ) statement
                token = prep.getToken();
                result = (token.type == TokenType.tFOR);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tLPAREN);
                    if (result)
                    {
                        DeclarationNode declar = parseDeclaration();
                        result = (declar != null);
                        if (result)
                        {
                            ExpressionNode expr2 = parseExpression();
                            token = prep.getToken();
                            result = (token.type == TokenType.tSEMICOLON);
                            if (result)
                            {
                                ExpressionNode expr3 = parseExpression();
                                token = prep.getToken();
                                result = (token.type == TokenType.tRPAREN);
                                if (result)
                                {
                                    StatementNode stmt = parseStatement();
                                    result = (stmt != null);
                                    if (result)
                                    {
                                        node = arbor.makeForStatementNode(declar, expr2, expr3, stmt);
                                    }
                                }
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

        /*(6.8.6) 
         jump-statement:
            goto identifier ;
            continue ;
            break ;
            return expression[opt] ;
         */
        public StatementNode parseJumpStatement()
        {
            StatementNode node = null;
            int cuepoint = prep.record();
            Token token = prep.getToken();
            bool result = (token.type == TokenType.tGOTO);             //goto identifier ;
            if (result)
            {
                token = prep.getToken();
                IdentNode idNode = arbor.getLabelIdentNode(token);
                result = (idNode != null);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tSEMICOLON);
                    if (result)
                    {
                        node = arbor.makeGotoStatementNode(idNode);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);        //continue ;
                token = prep.getToken();
                result = (token.type == TokenType.tCONTINUE);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tSEMICOLON);
                    if (result)
                    {
                        node = arbor.makeContinueStatementNode();
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);        //break ;
                token = prep.getToken();
                result = (token.type == TokenType.tBREAK);
                if (result)
                {
                    token = prep.getToken();
                    result = (token.type == TokenType.tSEMICOLON);
                    if (result)
                    {
                        node = arbor.makeBreakStatementNode();
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);        //return expression[opt] ;
                token = prep.getToken();
                result = (token.type == TokenType.tRETURN);
                if (result)
                {
                    ExpressionNode expr = parseExpression();
                    token = prep.getToken();
                    result = (token.type == TokenType.tSEMICOLON);
                    if (result)
                    {
                        node = arbor.makeReturnStatementNode(expr);
                    }
                }
            }
            if (!result)
            {
                prep.rewind(cuepoint);
            }
            return node;
        }

        //- external definitions ----------------------------------------------

        /*(6.9) 
         translation-unit:
            external-declaration
            translation-unit external-declaration 
        */

        public TranslationUnit parseTranslationUnit()
        {
            TranslationUnit unit = new TranslationUnit(arbor);
            while (prep.getToken().type != TokenType.tEOF)
            {
                parseExternalDef();
            }
            return unit;
        }

        /* (6.9)     
         external-declaration:
            declaration
            function-definition

         (6.7) 
         declaration:
            declaration-specifiers init-declarator-list[opt] ;

         (6.9.1)
         function-definition:
            declaration-specifiers declarator declaration-list[opt] compound-statement          
        */
        public void parseExternalDef()
        {
            FunctionDefNode funcDef = null;
            int cuepoint = prep.record();
            DeclarationNode declars = parseDeclaration();
            if (declars.isFuncDef)
            {
                funcDef = new FunctionDefNode(declars);
                List<DeclarationNode> oldparamlist = parseDeclarationList();
                funcDef.setOldParams(oldparamlist);
                StatementNode block = parseCompoundStatement();
                funcDef.setFuncBody(block);
            }
        }

        /*(6.9.1) 
         declaration-list:
            declaration
            declaration-list declaration
        */
        //old-style function parameter defs - here for completeness
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

    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");