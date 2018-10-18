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

namespace BlackC
{
    class Parser
    {
        Preprocessor preprocessor;
        Scanner scanner;

        public Parser()
        {
        }

        //- expressions ------------------------------------------------------

        /*(6.5.1) 
         primary-expression:
            identifier
            constant
            string-literal
            ( expression )
         */
        public bool parsePrimaryExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.2) 
         postfix-expression:
            primary-expression
            postfix-expression [ expression ]
            postfix-expression ( argument-expression-list[opt] )
            postfix-expression . identifier
            postfix-expression -> identifier
            postfix-expression ++
            postfix-expression --
            ( type-name ) { initializer-list }
            ( type-name ) { initializer-list , }
         */
        public bool parsePostfixExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.2) 
         argument-expression-list:
            assignment-expression
            argument-expression-list , assignment-expression
         */
        public bool parseArgExpressionList()
        {
            int cuepoint = scanner.record();
            bool result = parseAssignExpression();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    result = parseAssignExpression();
                }
                scanner.rewind(cuepoint2, result);
            }
            scanner.rewind(cuepoint, empty);
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
        public bool parseUnaryExpression()
        {
            bool result = true;
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
            scanner.rewind(cuepoint, result);
            return result;
        }

        /*(6.5.4) 
         cast-expression:
            unary-expression
            ( type-name ) cast-expression
         */
        public bool parseCastExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.5) 
         multiplicative-expression:
            cast-expression
            multiplicative-expression * cast-expression
            multiplicative-expression / cast-expression
            multiplicative-expression % cast-expression
         */
        public bool parseMultExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.6) 
         additive-expression:
            multiplicative-expression
            additive-expression + multiplicative-expression
            additive-expression - multiplicative-expression
         */
        public bool parseAddExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.7) 
         shift-expression:
            additive-expression
            shift-expression << additive-expression
            shift-expression >> additive-expression
         */
        public bool parseShiftExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.8) 
         relational-expression:
            shift-expression
            relational-expression < shift-expression
            relational-expression > shift-expression
            relational-expression <= shift-expression
            relational-expression >= shift-expression
         */
        public bool parseRelationalExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.9) 
         equality-expression:
            relational-expression
            equality-expression == relational-expression
            equality-expression != relational-expression
         */
        public bool parseEqualExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.10) 
         AND-expression:
            equality-expression
            AND-expression & equality-expression
         */
        public bool parseANDExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.11) 
         exclusive-OR-expression:
            AND-expression
            exclusive-OR-expression ^ AND-expression
         */
        public bool parseXORExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.12) 
         inclusive-OR-expression:
            exclusive-OR-expression
            inclusive-OR-expression | exclusive-OR-expression
         */
        public bool parseORExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.13) 
         logical-AND-expression:
            inclusive-OR-expression
            logical-AND-expression && inclusive-OR-expression
         */
        public bool parseLogicalANDExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.14) 
         logical-OR-expression:
            logical-AND-expression
            logical-OR-expression || logical-AND-expression
         */
        public bool parseLogicalORExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.15) 
         conditional-expression:
            logical-OR-expression
            logical-OR-expression ? expression : conditional-expression
         */
        public bool parseConditionalExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.5.16) 
         assignment-expression:
            conditional-expression
            unary-expression assignment-operator assignment-expression
         */
        public bool parseAssignExpression()
        {
            bool result = true;
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
            scanner.rewind(cuepoint, result);
            return result;
        }

        /*(6.5.17) 
         expression:
            assignment-expression
            expression , assignment-expression
         */
        public bool parseExpression()
        {
            bool result = true;
            return result;
        }

        /*(6.6) 
         constant-expression:
            conditional-expression
         */
        public bool parseConstantExpression()
        {
            bool result = true;
            return result;
        }

        //- declarations ------------------------------------------------------

        /* (6.7) 
         declaration:
            declaration-specifiers init-declarator-list[opt] ;
         */
        public bool parseDeclaration()
        {
            bool result = true;
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
                parseDeclarationSpecs();
            }
            else
            {
                result = parseTypeSpec();
                if (result)
                {
                    parseDeclarationSpecs();
                }
                else
                {
                    result = parseTypeQual();
                    if (result)
                    {
                        parseDeclarationSpecs();
                    }
                    else
                    {
                        result = parseFunctionSpec();
                        if (result)
                        {
                            parseDeclarationSpecs();
                        }
                    }
                }
            }
            scanner.rewind(cuepoint, result);
            return result;
        }

        //public void parseDeclarSpecs(List<Token> specs)
        //{
        //    bool done = false;
        //    while (!done)
        //    {
        //        Token token = scanner.getToken();

        //        //type specifier
        //        else if ((token is tVoid) || (token is tChar) || (token is tShort) || (token is tInt) || (token is tLong)
        //            || (token is tFloat) || (token is tDouble) || (token is tSigned) || (token is tUnsigned))
        //        {
        //            specs.Add(token);
        //        }
        //        else if (token is tEnum)
        //        {
        //            parseEnumSpec();
        //        }
        //        else if ((token is tIdentifier) && (((tIdentifier)token).isTypeDef))
        //        {
        //            //handle typedef
        //        }

        //        //type qualifier
        //        else if ((token is tConst) || (token is tRestrict) || (token is tVolatile))
        //        {
        //            specs.Add(token);
        //        }

        //        //func specifier
        //        else if (token is tInline)
        //        {
        //            specs.Add(token);
        //        }

        //        //none of the above
        //        else
        //        {
        //            done = true;
        //        }
        //    }
        //}

        /*(6.7) 
         init-declarator-list:
            init-declarator
            init-declarator-list , init-declarator
         */
        public bool parseInitDeclaratorList()
        {
            int cuepoint = scanner.record();
            bool result = parseInitDeclarator();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    result = parseInitDeclarator();
                }
                scanner.rewind(cuepoint2, result);
            }
            scanner.rewind(cuepoint, empty);
            return empty;
        }

        /*(6.7) 
         init-declarator:
            declarator
            declarator = initializer
         */
        public bool parseInitDeclarator()
        {
            bool result = true;
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
            scanner.rewind(cuepoint, result);
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
            struct-or-union-specifier *
            enum-specifier
            typedef-name
         */
        public bool parseTypeSpec()
        {
            bool result = true;
            return result;
        }

        /*(6.7.2.1) 
         struct-or-union-specifier:
            struct-or-union identifier[opt] { struct-declaration-list }
            struct-or-union identifier
         */
        public bool parseStructOrUnionSpec()
        {
            bool result = true;
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
            scanner.rewind(cuepoint, result);
            return result;
        }

        /*(6.7.2.1) 
         struct-declaration-list:
            struct-declaration
            struct-declaration-list struct-declaration
         */
        public bool parseStructDeclarationList()
        {
            int cuepoint = scanner.record();
            bool result = parseStructDeclaration();
            bool empty = result;
            while (result)
            {
                result = parseExternalDeclaration();
            }
            scanner.rewind(cuepoint, empty);
            return empty;
        }

        /*(6.7.2.1) 
         struct-declaration:
            specifier-qualifier-list struct-declarator-list ;
         */
        public bool parseStructDeclaration()
        {
            bool result = true;
            return result;
        }

        /*(6.7.2.1) 
         specifier-qualifier-list:
            type-specifier specifier-qualifier-list[opt]
            type-qualifier specifier-qualifier-list[opt]
         */
        public bool parseStuctQualList()
        {
            bool result = true;
            return result;
        }

        /*(6.7.2.1) 
         struct-declarator-list:
            struct-declarator
            struct-declarator-list , struct-declarator
         */
        public bool parseStructDeclaratorList()
        {
            int cuepoint = scanner.record();
            bool result = parseStructDeclarator();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    result = parseStructDeclarator();
                }
                scanner.rewind(cuepoint2, result);
            }
            scanner.rewind(cuepoint, empty);
            return empty;
        }

        /*(6.7.2.1) 
         struct-declarator:
            declarator
            declarator[opt] : constant-expression
         */
        public bool parseStructDeclarator()
        {
            bool result = true;
            return result;
        }

        /*(6.7.2.2) 
         enum-specifier:
            enum identifier[opt] { enumerator-list }
            enum identifier[opt] { enumerator-list , }
            enum identifier
         */
        public bool parseEnumeratorSpec()
        {
            bool result = true;
            return result;
        }

        /*(6.7.2.2) 
         enumerator-list:
            enumerator
            enumerator-list , enumerator
         */
        public bool parseEnumeratorList()
        {
            int cuepoint = scanner.record();
            bool result = parseEnumerator();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    result = parseEnumerator();
                }
                scanner.rewind(cuepoint2, result);
            }
            scanner.rewind(cuepoint, empty);
            return empty;
        }

        /*(6.7.2.2) 
         enumerator:
            enumeration-constant
            enumeration-constant = constant-expression
         */
        public bool parseEnumerator()
        {
            bool result = true;
            return result;
        }

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
            scanner.rewind(cuepoint, result);
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
            scanner.rewind(cuepoint, result);
            return result;
        }

        /*(6.7.5) 
         declarator:
            pointer[opt] direct-declarator
         */
        public bool parseDeclarator()
        {
            bool result = true;
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
            bool result = true;
            return result;
        }

        /*(6.7.5) 
         pointer:
            * type-qualifier-list[opt]
            * type-qualifier-list[opt] pointer
         */
        public bool parsePointer()
        {
            bool result = true;
            return result;
        }

        /*(6.7.5) 
         type-qualifier-list:
            type-qualifier
            type-qualifier-list type-qualifier
         */
        public bool parseTypeQualList()
        {
            bool result = true;
            return result;
        }

        /*(6.7.5) 
         parameter-type-list:
            parameter-list
            parameter-list , ...
         */
        public bool parseParameterTypeList()
        {
            bool result = true;
            return result;
        }

        /*(6.7.5) 
         parameter-list:
            parameter-declaration
            parameter-list , parameter-declaration
         */
        public bool parseParameterList()
        {
            int cuepoint = scanner.record();
            bool result = parseParameterDeclar();
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                Token token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    result = parseParameterDeclar();
                }
                scanner.rewind(cuepoint2, result);
            }
            scanner.rewind(cuepoint, empty);
            return empty;
        }

        /*(6.7.5) 
         parameter-declaration:
            declaration-specifiers declarator
            declaration-specifiers abstract-declarator[opt]
         */
        public bool parseParameterDeclar()
        {
            bool result = true;
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
            bool result = (token is tIdentifier);
            bool empty = result;
            while (result)
            {
                int cuepoint2 = scanner.record();
                token = scanner.getToken();
                result = (token is tComma);
                if (!result)
                {
                    token = scanner.getToken();
                    result = (token is tIdentifier);
                }
                scanner.rewind(cuepoint2, result);
            }
            scanner.rewind(cuepoint, empty);
            return empty;
        }

        /*(6.7.6) 
         type-name:
            specifier-qualifier-list abstract-declarator[opt]
         */
        public bool parseTypeName()
        {
            bool result = true;
            return result;
        }

        /*(6.7.6) 
         abstract-declarator:
            pointer
            pointer[opt] direct-abstract-declarator
         */
        public bool parseAbstractDeclarator()
        {
            bool result = true;
            return result;
        }

        /*(6.7.6) 
         direct-abstract-declarator:
            ( abstract-declarator )
            direct-abstract-declarator[opt] [ type-qualifier-list[opt]
            assignment-expression[opt] ]
            direct-abstract-declarator[opt] [ static type-qualifier-list[opt]
            assignment-expression ]
            direct-abstract-declarator[opt] [ type-qualifier-list static
            assignment-expression ]
            direct-abstract-declarator[opt] [ * ]
            direct-abstract-declarator[opt] ( parameter-type-list[opt] )
         */
        public bool parseDirectAbstractDeclarator()
        {
            bool result = true;
            return result;
        }

        /*(6.7.7) 
         typedef-name:
            identifier
         */
        public bool parseTypedefName()
        {
            int cuepoint = scanner.record();
            Token token = scanner.getToken();
            bool result = (token is tIdentifier);
            scanner.rewind(cuepoint, result);
            return result;
        }

        /*(6.7.8) 
         initializer:
            assignment-expression
            { initializer-list }
            { initializer-list , }
         */
        public bool parseInitializer()
        {
            bool result = true;
            return result;
        }

        /*(6.7.8) 
         initializer-list:
            designation[opt] initializer
            initializer-list , designation[opt] initializer
         */
        public bool parseInitializerList()
        {
            bool result = true;
            return result;
        }

        /*(6.7.8) 
         designation:
            designator-list =
         */
        public bool parseDesignation()
        {
            bool result = true;
            return result;
        }

        /*(6.7.8) 
         designator-list:
            designator
            designator-list designator
         */
        public bool parseDesignatorList()
        {
            bool result = true;
            return result;
        }

        /*(6.7.8) 
         designator:
            [ constant-expression ]
            . identifier
         */
        public bool parseDesignator()
        {
            bool result = true;
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

        public bool parseLabeledStatement()
        {
            bool result = true;
            return result;
        }

        public bool parseCompoundStatement()
        {
            bool result = true;
            return result;
        }

        public bool parseBlockItemList()
        {
            bool result = true;
            return result;
        }

        public bool parseBlockItem()
        {
            bool result = true;
            return result;
        }

        public bool parseExpressionStatement()
        {
            bool result = true;
            return result;
        }

        public bool parseSelectionStatement()
        {
            bool result = true;
            return result;
        }

        public bool parseIterationStatement()
        {
            bool result = true;
            return result;
        }

        public bool parseJumpStatement()
        {
            bool result = true;
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
            int cuepoint = scanner.record();
            bool result = parseFunctionDef();
            if (!result)
            {
                scanner.rewind(cuepoint, false);
                result = parseDeclaration();
            }
            scanner.rewind(cuepoint, result);
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
                    parseDesignatorList();
                    result = parseCompoundStatement();
                }
            }
            scanner.rewind(cuepoint, result);
            return result;
        }

        /*(6.9.1) declaration-list:
declaration
declaration-list declaration
*/
        public bool parseDeclarationList()
        {
            bool result = true;
            return result;
        }

        //-----------------------------------------------------------------------------

        public void parseFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);

            preprocessor = new Preprocessor(lines);
            preprocessor.process();

            //for (int i = 0; i < lines.Length; i++)
            //{
            //    Console.WriteLine(lines[i]);
            //}

            scanner = new Scanner(lines);
            parseTranslationUnit();

            Console.WriteLine("done parsing");
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");