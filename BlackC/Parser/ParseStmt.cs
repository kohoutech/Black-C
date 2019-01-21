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
    public class ParseStmt
    {
        public Preprocessor prep;
        public Arbor arbor;
        public ParseDeclar pdeclar;
        public ParseExpr pexpr;

        public ParseStmt(Preprocessor _prep, Arbor _arbor)
        {
            prep = _prep;
            arbor = _arbor;
            pexpr = null;
        }

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
                    ConstExpressionNode expr = pexpr.parseConstantExpression();
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
            BlockItemNode item = pdeclar.parseDeclaration();
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
            ExpressionNode expr = pexpr.parseExpression();
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
                    ExpressionNode expr = pexpr.parseExpression();
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
                        ExpressionNode expr = pexpr.parseExpression();
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
                    ExpressionNode expr = pexpr.parseExpression();
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
                                ExpressionNode expr = pexpr.parseExpression();
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
                        ExpressionNode expr1 = pexpr.parseExpression();
                        token = prep.getToken();
                        result = (token.type == TokenType.tSEMICOLON);
                        if (result)
                        {
                            ExpressionNode expr2 = pexpr.parseExpression();
                            token = prep.getToken();
                            result = (token.type == TokenType.tSEMICOLON);
                            if (result)
                            {
                                ExpressionNode expr3 = pexpr.parseExpression();
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
                        DeclarationNode declar = pdeclar.parseDeclaration();
                        result = (declar != null);
                        if (result)
                        {
                            ExpressionNode expr2 = pexpr.parseExpression();
                            token = prep.getToken();
                            result = (token.type == TokenType.tSEMICOLON);
                            if (result)
                            {
                                ExpressionNode expr3 = pexpr.parseExpression();
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
                    ExpressionNode expr = pexpr.parseExpression();
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
    }
}
