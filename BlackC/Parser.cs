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
        public bool parsePrimaryExpression(out Node primeExpr)
        {
            primeExpr = null;
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = isIdentifier(token, out primeExpr);
            if (!result)
            {
                result = (token is tIntegerConstant);
                if (result)
                {
                    primeExpr = arbor.makeIntegerConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token is tFloatConstant);
                if (result)
                {
                    primeExpr = arbor.makeFloatConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token is tCharacterConstant);
                if (result)
                {
                    primeExpr = arbor.makeCharConstantNode(token);
                }
            }
            if (!result)
            {
                result = (token is tStringConstant);
                if (result)
                {
                    primeExpr = arbor.makeStringConstantNode(token);
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
                result = parseEnumerationConstant(out primeExpr);
            }
            if (!result)
            {
                scanner.rewind(cuepoint);                   //( expression )
                token = scanner.getToken();
                result = (token is tLParen);
                if (result)
                {
                    result = parseExpression();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }            
            return result;
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
        public bool parsePostfixExpression(out Node postfixExpr)
        {
            int cuepoint = scanner.record();
            bool result = parsePrimaryExpression();         //primary-expression
            if (!result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();           //( type-name ) { initializer-list }
                result = (token is tLParen);
                if (result)
                {
                    result = parseTypeName();
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
                                result = parseInitializerList();
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
                    result = parseExpression();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRBracket);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //postfix-expression ( argument-expression-list[opt] )
                    token = scanner.getToken();
                    result = (token is tLParen);
                    if (result)
                    {
                        parseArgExpressionList();
                        token = scanner.getToken();
                        result = (token is tRParen);
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
                        result = isIdentifier(token);
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
                        result = isIdentifier(token);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //postfix-expression ++
                    token = scanner.getToken();
                    result = (token is tPlusPlus);
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);                  //postfix-expression --
                    token = scanner.getToken();
                    result = (token is tMinusMinus);
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
            return notEmpty;
        }

        /*(6.5.2) 
         argument-expression-list:
            assignment-expression
            argument-expression-list , assignment-expression
         */
        public bool parseArgExpressionList()
        {
            bool result = parseAssignExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    result = parseAssignExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
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
        public bool parseUnaryExpression(out Node expr)
        {
            int cuepoint = scanner.record();
            bool result = parsePostfixExpression();         //postfix-expression
            if (!result)
            {
                Token token = scanner.getToken();           //++ unary-expression
                result = (token is tPlusPlus);
                if (result)
                {
                    result = parseUnaryExpression();
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
                Token token = scanner.getToken();           //-- unary-expression
                result = (token is tMinusMinus);
                if (result)
                {
                    result = parseUnaryExpression();
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);                   //unary-operator cast-expression
                result = parseUnaryOperator();
                if (result)
                {
                    result = parseCastExpression();
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
                Token token = scanner.getToken();           //sizeof unary-expression
                result = (token is tSizeof);
                if (result)
                {
                    result = parseUnaryExpression();
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
                        result = parseTypeName();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRParen);
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

        /*(6.5.3) 
         unary-operator: one of
            & * + - ~ !
         */
        public bool parseUnaryOperator()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tAmpersand) || (token is tAsterisk) || (token is tPlus) || (token is tMinus) ||
                (token is tTilde) || (token is tExclaim));
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.5.4) 
         cast-expression:
            unary-expression
            ( type-name ) cast-expression
         */
        public bool parseCastExpression(out Node expr)
        {
            bool result = parseUnaryExpression();
            if (!result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLParen);
                if (result)
                {
                    result = parseTypeName();
                    if (result)
                    {
                        token = scanner.getToken();
                        result = (token is tRParen);
                        if (result)
                        {
                            result = parseCastExpression();
                        }
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return result;
        }

        /*(6.5.5) 
         multiplicative-expression:
            cast-expression
            multiplicative-expression * cast-expression
            multiplicative-expression / cast-expression
            multiplicative-expression % cast-expression
         */
        public bool parseMultExpression(out Node expr)
        {
            bool result = parseCastExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tAsterisk);
                if (result)
                {
                    result = parseCastExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tSlash);
                    if (result)
                    {
                        result = parseCastExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tPercent);
                    if (result)
                    {
                        result = parseCastExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.6) 
         additive-expression:
            multiplicative-expression
            additive-expression + multiplicative-expression
            additive-expression - multiplicative-expression
         */
        public bool parseAddExpression(out Node addExpr)
        {
            bool result = parseMultExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tPlus);
                if (result)
                {
                    result = parseMultExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tMinus);
                    if (result)
                    {
                        result = parseMultExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.7) 
         shift-expression:
            additive-expression
            shift-expression << additive-expression
            shift-expression >> additive-expression
         */
        public bool parseShiftExpression(out Node expr)
        {
            bool result = parseAddExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLeftShift);
                if (result)
                {
                    result = parseAddExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tRightShift);
                    if (result)
                    {
                        result = parseAddExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.8) 
         relational-expression:
            shift-expression
            relational-expression < shift-expression
            relational-expression > shift-expression
            relational-expression <= shift-expression
            relational-expression >= shift-expression
         */
        public bool parseRelationalExpression(out Node expr)
        {
            bool result = parseShiftExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tLessThan);
                if (result)
                {
                    result = parseShiftExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tGtrThan);
                    if (result)
                    {
                        result = parseShiftExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tLessEqual);
                    if (result)
                    {
                        result = parseShiftExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tGtrEqual);
                    if (result)
                    {
                        result = parseShiftExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.9) 
         equality-expression:
            relational-expression
            equality-expression == relational-expression
            equality-expression != relational-expression
         */
        public bool parseEqualityExpression(out Node expr)
        {
            bool result = parseRelationalExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tEqualEqual);
                if (result)
                {
                    result = parseRelationalExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                    token = scanner.getToken();
                    result = (token is tNotEqual);
                    if (result)
                    {
                        result = parseRelationalExpression();
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.10) 
         AND-expression:
            equality-expression
            AND-expression & equality-expression
         */
        public bool parseANDExpression(out Node expr)
        {
            bool result = parseEqualityExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tAmpersand);
                if (result)
                {
                    result = parseEqualityExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.11) 
         exclusive-OR-expression:
            AND-expression
            exclusive-OR-expression ^ AND-expression
         */
        public bool parseXORExpression(out Node expr)
        {
            bool result = parseANDExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tCaret);
                if (result)
                {
                    result = parseANDExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.12) 
         inclusive-OR-expression:
            exclusive-OR-expression
            inclusive-OR-expression | exclusive-OR-expression
         */
        public bool parseORExpression(out Node expr)
        {
            bool result = parseXORExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tBar);
                if (result)
                {
                    result = parseXORExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.13) 
         logical-AND-expression:
            inclusive-OR-expression
            logical-AND-expression && inclusive-OR-expression
         */
        public bool parseLogicalANDExpression(out Node expr)
        {
            bool result = parseORExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tDoubleAmp);
                if (result)
                {
                    result = parseORExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.14) 
         logical-OR-expression:
            logical-AND-expression
            logical-OR-expression || logical-AND-expression
         */
        public bool parseLogicalORExpression(out ExprNode expr)
        {
            bool result = parseLogicalANDExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tDoubleBar);
                if (result)
                {
                    result = parseLogicalANDExpression();
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return empty;
        }

        /*(6.5.15) 
         conditional-expression:
            logical-OR-expression
            logical-OR-expression ? expression : conditional-expression
         */
        public bool parseConditionalExpression(out Node expr)
        {
            bool result = parseLogicalORExpression();
            if (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tQuestion);
                if (result2)
                {
                    result2 = parseExpression();        
                    if (result2)
                    {
                        token = scanner.getToken();
                        result2 = (token is tColon);
                        if (result2)
                        {
                            result = parseConditionalExpression();
                        }                        
                    }
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return result;
        }

        /*(6.5.16) 
         assignment-expression:
            conditional-expression
            unary-expression assignment-operator assignment-expression
         */
        //for parsing purposes, we change the second rule to:
        //conditional-expression assignment-operator assignment-expression
        public bool parseAssignExpression(out Node expr)
        {
            bool result = parseConditionalExpression();
            if (result)
            {
                int cuepoint = scanner.record();
                bool result2 = parseAssignOperator();
                if (result2)
                {
                    result2 = parseAssignExpression();
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint);
                }
            }
            return result;
        }

        /* (6.5.16) 
         assignment-operator: one of
            = *= /= %= += -= <<= >>= &= ^= |=
         */
        public bool parseAssignOperator()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tEqual) || (token is tMultEqual) || (token is tSlashEqual) || (token is tPercentEqual) ||
                (token is tPlusEqual) || (token is tMinusEqual) || (token is tLShiftEqual) || (token is tRShiftEqual) ||
                (token is tAmpEqual) || (token is tCaretEqual) || (token is tBarEqual));
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.5.17) 
         expression:
            assignment-expression
            expression , assignment-expression
         */
        public bool parseExpression(out Node expr)
        {
            expr = new ExprNode();
            bool result = parseAssignExpression(out expr);
            if (result)
            {
                exprList.Add(expr);
            }
            bool empty = !result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    result = parseAssignExpression(out expr);
                    if (result)
                    {
                        exprList.Add(expr);
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return !empty;
        }

        /*(6.6) 
         constant-expression:
            conditional-expression
         */
        public ExprNode parseConstantExpression()
        {
            ExprNode result = parseConditionalExpression();
            return result;
        }

        //- declarations ------------------------------------------------------

        /* (6.7) 
         declaration:
            declaration-specifiers init-declarator-list[opt] ;
         */
        public bool parseDeclaration()
        {
            int cuepoint = scanner.record();
            bool result = parseDeclarationSpecs();
            if (result)
            {
                parseInitDeclaratorList();
                Token token = scanner.getToken();
                result = (token is tSemicolon);
            }
            if (result)
            {
                declarCount++;
                Console.WriteLine("parsed declaration " + declarCount.ToString());
            }
            else
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /* (6.7) 
         declaration-specifiers:
            storage-class-specifier declaration-specifiers[opt]
            type-specifier declaration-specifiers[opt]
            type-qualifier declaration-specifiers[opt]
            function-specifier declaration-specifiers[opt]
         */
        public bool parseDeclarationSpecs()
        {
            int cuepoint = scanner.record();
            bool result = parseStorageClassSpec();
            if (result)
            {
                //Console.WriteLine("parsed storage class declaration-specs");
                parseDeclarationSpecs();
            }
            else
            {
                result = parseTypeSpec();
                if (result)
                {
                    //Console.WriteLine("parsed type spec declaration-specs");
                    parseDeclarationSpecs();
                }
                else
                {
                    result = parseTypeQual();
                    if (result)
                    {
                        //Console.WriteLine("parsed type qual declaration-specs");
                        parseDeclarationSpecs();
                    }
                    else
                    {
                        result = parseFunctionSpec();
                        if (result)
                        {
                            //Console.WriteLine("parsed func spec declaration-specs");
                            parseDeclarationSpecs();
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

        /*(6.7) 
         init-declarator-list:
            init-declarator
            init-declarator-list , init-declarator
         */
        public bool parseInitDeclaratorList()
        {
            bool result = parseInitDeclarator();
            if (result)
            {
                //Console.WriteLine("parsed init-declarator init-declarator-list");
            }
            bool notEmpty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    result = parseInitDeclarator();
                    if (result)
                    {
                        //Console.WriteLine("parsed another init-declarator init-declarator-list");
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return notEmpty;
        }

        /*(6.7) 
         init-declarator:
            declarator
            declarator = initializer
         */
        public bool parseInitDeclarator()
        {
            bool result = parseDeclarator();
            if (result)
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tEqual);
                if (!result2)
                {
                    //Console.WriteLine("parsed declarator init-declarator");
                }
                if (result2)
                {
                    result2 = parseInitializer();
                    if (!result2)
                    {
                        //Console.WriteLine("parsed declarator = initializer init-declarator");
                    }
                }
                if (!result2)
                {
                    scanner.rewind(cuepoint);
                }
            }
            return result;
        }

        /*(6.7.1) 
         storage-class-specifier:
            typedef
            extern
            static
            auto
            register
         */
        public bool parseStorageClassSpec()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tTypedef) || (token is tExtern) || (token is tStatic) || (token is tAuto) || (token is tRegister));
            if (result)
            {
                //Console.WriteLine("parsed storage class spec");
            }
            else 
            {
                scanner.rewind(cuepoint);
            }
            return result;
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
        public bool parseTypeSpec()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tVoid) || (token is tChar) || (token is tShort) || (token is tInt) || (token is tLong)
                || (token is tFloat) || (token is tDouble) || (token is tSigned) || (token is tUnsigned));
            if (result)
            {
                //Console.WriteLine("parsed base type-spec");
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
                result = parseStructOrUnionSpec();
                if (result)
                {
                    //Console.WriteLine("parsed struct/union type-spec");
                }
            }
            if (!result)
            {
                result = parseEnumeratorSpec();
                if (result)
                {
                    //Console.WriteLine("parsed enumeration type-spec");
                }
            }
            if (!result)
            {
                result = parseTypedefName();
                if (result)
                {
                    //Console.WriteLine("parsed typedef type-spec");
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        // stuctures/unions -----------------------------------------

        /*(6.7.2.1) 
         struct-or-union-specifier:
            struct-or-union identifier[opt] { struct-declaration-list }
            struct-or-union identifier
         */
        // struct w/o ident is for anonymous struct (possibly part of a typedef)
        // struct w/o {list} is for a already defined struct type
        public bool parseStructOrUnionSpec()
        {
            int cuepoint = scanner.record();
            bool result = parseStuctOrUnion();
            if (result)
            {
                Token token = scanner.getToken();
                result = isIdentifier(token);
                if (result)
                {
                    int cuepoint2 = scanner.record();
                    token = scanner.getToken();
                    bool result2 = (token is tLBrace);
                    if (!result2)
                    {
                        //Console.WriteLine("parsed struct-or-union ident struct-or-union-spec");
                    }
                    if (result2)
                    {
                        result2 = parseStructDeclarationList();
                        if (result2)
                        {
                            token = scanner.getToken();
                            result2 = (token is tRBrace);
                            if (result2)
                            {
                                //Console.WriteLine("parsed struct-or-union ident struct-declar-list struct-or-union-spec");
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
                    //token = scanner.getToken();
                    result = (token is tLBrace);
                    if (result)
                    {
                        result = parseStructDeclarationList();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRBrace);
                            if (result)
                            {
                                //Console.WriteLine("parsed struct-or-union struct-declar-list struct-or-union-spec");
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

        /*(6.7.2.1) 
         struct-or-union:
            struct
            union
         */
        public bool parseStuctOrUnion()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tStruct) || (token is tUnion));
            if (result)
            {
                //Console.WriteLine("parsed struct-or-union");
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.2.1) 
         struct-declaration-list:
            struct-declaration
            struct-declaration-list struct-declaration
         */
        // the list of struct field defs
        public bool parseStructDeclarationList()
        {
            bool result = parseStructDeclaration();         //the first field def
            if (result)
            {
                //Console.WriteLine("parsed struct-declar struct-declar-list");
            }
            bool notEmpty = result;
            while (result)
            {
                result = parseStructDeclaration();          //the next field def
                if (result)
                {
                    //Console.WriteLine("parsed another struct-declar struct-declar-list");
                }
            }
            return notEmpty;
        }

        /*(6.7.2.1) 
         struct-declaration:
            specifier-qualifier-list struct-declarator-list ;
         */
        // a single struct field def (can have mult fields, ie int a, b;)
        public bool parseStructDeclaration()
        {
            int cuepoint = scanner.record();
            bool result = parseSpecQualList();          //field type
            if (result)
            {
                result = parseStructDeclaratorList();           //list of field names 
                if (result)
                {
                    Token token = scanner.getToken();
                    result = (token is tSemicolon);
                    if (result)
                    {
                        //Console.WriteLine("parsed spec-qual-list struct-declar-list struct-declar");
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.2.1) 
         specifier-qualifier-list:
            type-specifier specifier-qualifier-list[opt]
            type-qualifier specifier-qualifier-list[opt]
         */
        // struct field's type - same as declaration-specifiers, w/o the storage-class-specifier or function-specifier
        public bool parseSpecQualList()
        {
            bool result = parseTypeSpec();
            if (result)
            {
                parseSpecQualList();
                if (result)
                {
                    //Console.WriteLine("parsed type-spec struct-qual-list");
                }
            }
            else
            {
                result = parseTypeQual();
                if (result)
                {
                    parseSpecQualList();
                }
                if (result)
                {
                    //Console.WriteLine("parsed type-qual struct-qual-list");
                }
            }
            return result;
        }

        /*(6.7.2.1) 
         struct-declarator-list:
            struct-declarator
            struct-declarator-list , struct-declarator
         */
        // the list of field names, fx the "a, b, c" in "int a, b, c;" that def's three fields of type int
        public bool parseStructDeclaratorList()
        {
            bool result = parseStructDeclarator();      //the first field name
            if (result)
            {
                //Console.WriteLine("parsed struct-declar struct-declar-list");
            }
            bool notEmpty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (result)
                {
                    result = parseStructDeclarator();       //the next field name
                    if (result)
                    {
                        //Console.WriteLine("parsed another struct-declar struct-declar-list");
                    }
                }
                if (!result)
                {
                    scanner.rewind(cuepoint2);
                }
            }
            return notEmpty;
        }

        /*(6.7.2.1) 
         struct-declarator:
            declarator
            declarator[opt] : constant-expression
         */
        //a single field name, possibly followed by a field width (fx foo : 4;)
        public bool parseStructDeclarator()
        {
            int cuepoint = scanner.record();
            bool result = parseDeclarator();
            if (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tColon);
                if (result2)
                {
                    result2 = parseConstantExpression();
                    if (result2)
                    {
                        //Console.WriteLine("parsed declar const-exp struct-declar");
                    }
                }
                if (!result2)
                {
                    //Console.WriteLine("parsed declar struct-declar");
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
                    result = parseConstantExpression();
                    if (result)
                    {
                        //Console.WriteLine("parsed const-exp struct-declar");
                    }
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        // enumerations ---------------------------------------------

        /*(6.7.2.2) 
         enum-specifier:
            enum identifier[opt] { enumerator-list }
            enum identifier[opt] { enumerator-list , }
            enum identifier
         */
        public bool parseEnumeratorSpec()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tEnum);             //enum identifier
            if (result)
            {
                token = scanner.getToken();
                result = isIdentifier(token);
                if (result)
                {
                    int cuepoint2 = scanner.record();
                    token = scanner.getToken();
                    bool result2 = (token is tLBrace);             //enum identifier[opt] { enumerator-list }
                    if (result2)
                    {
                        result2 = parseEnumeratorList();
                        if (result2)
                        {
                            token = scanner.getToken();
                            result2 = (token is tRBrace);
                            if (result2)
                            {
                                //Console.WriteLine("parsed ident enumerator-list enum-spec");
                            }
                            if (!result2)
                            {
                                result2 = (token is tComma);            //enum identifier[opt] { enumerator-list , }
                                if (result2)
                                {
                                    token = scanner.getToken();
                                    result2 = (token is tRBrace);
                                    if (result2)
                                    {
                                        //Console.WriteLine("parsed ident enumerator-list comma enum-spec");
                                    }
                                }
                            }
                        }
                    }
                    if (!result2)
                    {
                        //Console.WriteLine("parsed ident enum-spec");
                    }
                    if (!result2)
                    {
                        scanner.rewind(cuepoint2);
                    }
                }
                else
                {
                    token = scanner.getToken();
                    result = (token is tLBrace);             //enum identifier[opt] { enumerator-list }
                    if (result)
                    {
                        result = parseEnumeratorList();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRBrace);
                            if (result)
                            {
                                //Console.WriteLine("parsed enumerator-list enum-spec");
                            }
                            if (!result)
                            {
                                result = (token is tComma);            //enum identifier[opt] { enumerator-list , }
                                if (result)
                                {
                                    token = scanner.getToken();
                                    result = (token is tRBrace);
                                    if (result)
                                    {
                                        //Console.WriteLine("parsed enumerator-list comma enum-spec");
                                    }
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
            ConstExprNode constexpr = null;
            bool result = (enumconst != null);
            if (result)
            {
                int cuepoint = scanner.record();
                Token token = scanner.getToken();
                bool result2 = (token is tEqual);
                if (result2)
                {
                    ConstExprNode constexpr = parseConstantExpression();
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
        public bool parseEnumerationConstant()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = isIdentifier(token);
            if (result)
            {
                //Console.WriteLine("parsed enum const");
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        //- end of enumerations -------------------------------------

        /*(6.7.3) 
         type-qualifier:
            const
            restrict
            volatile
         */
        public bool parseTypeQual()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tConst) || (token is tRestrict) || (token is tVolatile));
            if (result)
            {
                //Console.WriteLine("parsed type qual");
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.4) 
         function-specifier:
            inline
         */
        public bool parseFunctionSpec()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tInline);
            if (result)
            {
                //Console.WriteLine("parsed func spec");
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        //- declarator ----------------------------------------------

        /*(6.7.5) 
         declarator:
            pointer[opt] direct-declarator
         */
        public bool parseDeclarator()
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
        public bool parsePointer()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tAsterisk);
            if (result)
            {
                parseTypeQualList();
                parsePointer();
                if (result)
                {
                    //Console.WriteLine("parsed * type-qual pointer");
                }
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        /*(6.7.5) 
         type-qualifier-list:
            type-qualifier
            type-qualifier-list type-qualifier
         */
        public bool parseTypeQualList()
        {
            bool result = parseTypeQual();
            if (result)
            {
                //Console.WriteLine("parsed type-qual type-qual-list");
            }
            bool notEmpty = result;
            while (result)
            {
                result = parseTypeQual();
                if (result)
                {
                    //Console.WriteLine("parsed another type-qual type-qual-list");
                }
            }
            return notEmpty;
        }

        /*(6.7.5) 
         parameter-type-list:
            parameter-list
            parameter-list , ...
         */
        public bool parseParameterTypeList()
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
        public bool parseIdentifierList()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = isIdentifier(token);
            if (result)
            {
                //Console.WriteLine("parsed ident ident-list");
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
                    result = isIdentifier(token);
                    if (result)
                    {
                        //Console.WriteLine("parsed another ident ident-list");
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
            return !empty;
        }

        /*(6.7.6) 
         type-name:
            specifier-qualifier-list abstract-declarator[opt]
         */
        public bool parseTypeName()
        {
            bool result = parseSpecQualList();
            if (result)
            {
                parseAbstractDeclarator();
                if (result)
                {
                    //Console.WriteLine("parsed spec-qual abstractor-declarator type-name");
                }
            }
            return result;
        }

        /*(6.7.6) 
         abstract-declarator:
            pointer
            pointer[opt] direct-abstract-declarator
         */
        public bool parseAbstractDeclarator()
        {
            bool result = parsePointer();
            if (result)
            {
                //Console.WriteLine("parsed pointer abstractor-declarator");
            }
            if (result)
            {
                parseDirectAbstractDeclarator();        //don't need result of this call, the base case <pointer> is true in either case
                if (result)
                {
                    //Console.WriteLine("parsed pointer direct-abs abstractor-declarator");
                }
            }
            else
            {
                result = parseDirectAbstractDeclarator();
                if (result)
                {
                    //Console.WriteLine("parsed direct-abs abstractor-declarator");
                }
            }
            return result;
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
        public bool parseDirectAbstractDeclarator()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tLParen);
            if (result)
            {
                result = parseAbstractDeclarator();         //( abstract-declarator )
                if (result)
                {
                    token = scanner.getToken();
                    result = (token is tRParen);
                    if (result)
                    {
                        //Console.WriteLine("parsed abstract-declarator direct-abstractor-declarator");
                    }
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
                    parseTypeQualList();
                    parseAssignExpression();
                    token = scanner.getToken();
                    result = (token is tRBracket);
                    if (result)
                    {
                        //Console.WriteLine("parsed assignment expr direct-abstractor-declarator");
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
                            parseTypeQualList();
                            result = parseAssignExpression();
                            if (result)
                            {
                                token = scanner.getToken();
                                result = (token is tRBracket);
                                if (result)
                                {
                                    //Console.WriteLine("parsed static assignment expr direct-abstractor-declarator");
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
                                        //Console.WriteLine("parsed type qual assignment expr direct-abstractor-declarator");
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
                                //Console.WriteLine("parsed pointer direct-abstractor-declarator");
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
                        result = parseParameterTypeList();
                        if (result)
                        {
                            token = scanner.getToken();
                            result = (token is tRParen);
                            if (result)
                            {
                                //Console.WriteLine("parsed param type list direct-abstractor-declarator");
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

        //- end of declarators ------------------------------------

        /*(6.7.7) 
         typedef-name:
            identifier
         */
        public bool parseTypedefName()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = ((token is tIdentifier) && arbor.isTypedef(((tIdentifier)token).ident));
            if (result)
            {
                tIdentifier id = (tIdentifier)token;
                //Console.WriteLine("parsed typedef name = " + id.ident);
            }
            if (!result)
            {
                scanner.rewind(cuepoint);
            }
            return result;
        }

        //- declaration initializers ------------------------------------

        /*(6.7.8) 
         initializer:
            assignment-expression
            { initializer-list }
            { initializer-list , }
         */
        public bool parseInitializer()
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
        public bool parseInitializerList()
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