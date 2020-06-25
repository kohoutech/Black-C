/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2020  George E Greaney

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

using BlackC.Scan;
using Kohoutech.OIL;

// the grammar this parser is pased on:
//https://en.wikipedia.org/wiki/C99
//http://www.open-std.org/jtc1/sc22/WG14/www/docs/n1256.pdf

namespace BlackC.Parse
{
    public class Parser
    {
        //public Options options;
        public Tokenizer prep;
        public Arbor arbor;

        public String filename;

        public List<String> includePaths;
        public bool saveSpaces;

        public List<Token> tokenList;
        public int tokenPos;

        public Parser()
        {
            //options = _options;

            prep = null;
            arbor = new Arbor(this);

            saveSpaces = false;

            //    includePaths = new List<string>() { "." };          //start with current dir
            //    includePaths.AddRange(options.includePaths);        //add search paths from command line         

            tokenList = new List<Token>();
            tokenPos = 0;
        }

        public void handlePragma(List<Fragment> args)
        {
        }

        public Module parseFile(String _filename)
        {
            filename = _filename;
            prep = new Tokenizer(this, filename);
            tokenList.Clear();
            tokenPos = 0;

            OILNode root = parseTranslationUnit();
            Module module = null;

            Console.WriteLine("parsed " + filename);
            return module;
        }

        //- feedback ----------------------------------------------------------

        public void fatal(string msg)
        {
            Console.WriteLine("fatal : " + msg);
            throw new ParserException();
        }

        public void error(String msg)
        {
            Console.WriteLine("error : " + msg);
        }

        public void warning(String msg)
        {
            Console.WriteLine("warning : " + msg);
        }

        //- token handling ----------------------------------------------------

        //break out to separate class?
        public Token getToken()
        {
            Token tok = null;
            if (tokenPos == tokenList.Count)
            {
                tok = prep.getToken();
                tokenList.Add(tok);
            }
            else
            {
                tok = tokenList[tokenPos];
            }
            tokenPos++;
            return tok;
        }

        public void rewindToken(int pos)
        {
            tokenPos = pos;
        }

        public bool nextTokenIs(TokenType ttype)
        {
            Token token = prep.getToken();
            bool result = token.type == ttype;
            prep.putTokenBack(token);
            return result;
        }

        public void consumeToken()
        {
            Token token = prep.getToken();
        }

        //-------------------------------------------------------------------------

        //        terminals(non keyword / punctuation)
        //---------
        //identifier				\
        //enumeration-constant:   identifier	 > arbor will distinguish between these
        //typedef-name:  identifier		/
        //constant
        //string-literal

        //- External definitions ----------------------------------------------

        //translation-unit:
        //  (function-definition | declaration)+
        public OILNode parseTranslationUnit()
        {
            int mark = tokenPos;
            List<OILNode> nodeList = new List<OILNode>();
            OILNode node = parseFunctionDefinition();
            if (node == null)
            {
                node = parseDeclaration();
            }
            if (node != null)
            {
                while (node != null) 
                {
                    nodeList.Add(node);
                    node = parseFunctionDefinition();
                    if (node == null)
                    {
                        node = parseDeclaration();
                    }
                } 

                OILNode node1 = arbor.buildTranslationUnit(nodeList);
                return node1;
            }

            rewindToken(mark);
            return null;
        }

        //function-definition:
        //  declaration-specifiers declarator (declaration)* compound-statement
        public OILNode parseFunctionDefinition()
        {
            int mark = tokenPos;
            OILNode node1 = parseDeclarationSpecifiers();
            if (node1 != null)
            {
                OILNode node2 = parseDeclarator();
                if (node2 != null)
                {
                    List<OILNode> nodelist = new List<OILNode>();
                    OILNode node3 = parseDeclaration();
                    while (node3 != null)
                    {
                        nodelist.Add(node3);
                        node3 = parseDeclaration();
                    }
                    OILNode node4 = parseCompoundStatement();
                    if (node4 != null)
                    {
                        OILNode node = arbor.buildFunctionDefinition(node1, node2, nodelist, node3);
                        return node;
                    }
                }
            }

            rewindToken(mark);
            return null;
        }

        //- Declarations ------------------------------------------------------

        //declaration:
        //  declaration-specifiers (init-declarator-list)? ;
        public OILNode parseDeclaration()
        {
            int mark = tokenPos;
            OILNode node1 = parseDeclarationSpecifiers();
            if (node1 != null)
            {
                OILNode node2 = parseInitDeclaratorList();
                Token tok = getToken();
                if (tok.type == TokenType.SEMICOLON)
                {
                    OILNode node = arbor.buildDeclaration(node1, node2);
                    return node;
                }
            }

            rewindToken(mark);
            return null;
        }

        //declaration-specifiers:
        //  (storage-class-specifier | type-specifier | type-qualifier | function-specifier)+
        public OILNode parseDeclarationSpecifiers()
        {
            int mark = tokenPos;
            List<OILNode> nodeList = new List<OILNode>();
            OILNode node1 = parseStorageClassSpecifier();
            if (node1 == null)
            {
                node1 = parseTypeSpecifier();
            }
            if (node1 == null)
            {
                node1 = parseTypeQualifier();
            }
            if (node1 == null)
            {
                node1 = parseFunctionSpecifier();
            }
            if (node1 != null)
            {
                while (node1 != null)
                {
                    nodeList.Add(node1);
                    node1 = parseStorageClassSpecifier();
                    if (node1 == null)
                    {
                        node1 = parseTypeSpecifier();
                    }
                    if (node1 == null)
                    {
                        node1 = parseTypeQualifier();
                    }
                    if (node1 == null)
                    {
                        node1 = parseFunctionSpecifier();
                    }
                }
                OILNode node = arbor.buildDeclarationSpecifiers(nodeList);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //init-declarator-list:
        //  init-declarator (',' init-declarator)*
        public OILNode parseInitDeclaratorList()
        {
            int mark = tokenPos;
            OILNode node = parseInitDeclarator();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.COMMA)
                {
                    OILNode node1 = parseInitDeclarator();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildInitDeclaratorList(nodeList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //init-declarator:
        //  declarator ('=' initializer)?
        public OILNode parseInitDeclarator()
        {
            int mark = tokenPos;
            OILNode node1 = parseDeclarator();
            OILNode node2 = null;
            if (node1 != null)
            {
                int mark1 = tokenPos;
                Token tok = getToken();
                if (tok.type == TokenType.EQUAL)
                {
                    node2 = parseInitializer();
                }
                else
                {
                    rewindToken(mark1);
                }
                OILNode node = arbor.buildInitDeclarator(node1, node2);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //---------------------------------------

        //storage-class-specifier:
        //  'TYPEDEF' | 'EXTERN' | 'STATIC' | 'AUTO' | 'REGISTER'
        public OILNode parseStorageClassSpecifier()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if ((tok.type == TokenType.TYPEDEF) ||
                (tok.type == TokenType.EXTERN) ||
                (tok.type == TokenType.STATIC) ||
                (tok.type == TokenType.AUTO) ||
                (tok.type == TokenType.REGISTER))
            {
                OILNode node = arbor.buildStorageClassSpecifier(tok);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //type-specifier:
        //  'VOID' | 'CHAR' | 'SHORT' | 'INT' | 'LONG' | 'FLOAT' | 'DOUBLE' | 'SIGNED' | 'UNSIGNED' | 
        //  struct-or-union-specifier | 
        //  enum-specifier | 
        //  typedef-name
        public OILNode parseTypeSpecifier()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if ((tok.type == TokenType.VOID) ||
                (tok.type == TokenType.CHAR) ||
                (tok.type == TokenType.SHORT) ||
                (tok.type == TokenType.INT) ||
                (tok.type == TokenType.LONG) ||
                (tok.type == TokenType.FLOAT) ||
                (tok.type == TokenType.DOUBLE) ||
                (tok.type == TokenType.SIGNED) ||
                (tok.type == TokenType.UNSIGNED))
            {
                OILNode node = arbor.buildTypeSpecifier(tok);
                return node;
            }
            if (tok.type == TokenType.IDENT)
            {
                OILNode node1 = arbor.isTypedefName(tok);
                if (node1 != null)
                {
                    OILNode node = arbor.buildTypeSpecifier(node1);
                    return node;
                }
            }
            rewindToken(mark);

            OILNode node2 = parseStructOrUnionSpecifier();
            if (node2 == null)
            {
                node2 = parseEnumSpecifier();
            }
            if (node2 != null)
            {
                OILNode node = arbor.buildTypeSpecifier(node2);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //struct-or-union-specifier:
        //  ('STRUCT' | 'UNION') ('identifier' | (('identifier')? '{' (struct-declaration)+ '}'))
        public OILNode parseStructOrUnionSpecifier()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if ((tok.type == TokenType.STRUCT) || (tok.type == TokenType.UNION))
            {
                bool isStruct = (tok.type == TokenType.STRUCT);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                Token id = null;
                if (tok1.type == TokenType.IDENT)
                {
                    id = tok1;
                }
                else
                {
                    rewindToken(mark1);
                }
                mark1 = tokenPos;
                tok1 = getToken();
                List<OILNode> nodeList = new List<OILNode>();
                if (tok1.type == TokenType.LBRACE)
                {
                    OILNode node1 = parseStructDeclaration();
                    while (node1 != null)
                    {
                        nodeList.Add(node1);
                        node1 = parseStructDeclaration();
                    }
                }
                else
                {
                    rewindToken(mark1);
                }
                OILNode node = arbor.buildStructOrUnionSpecifier(isStruct, id, nodeList);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //struct-declaration:
        //  specifier-qualifier-list struct-declarator-list ';'
        public OILNode parseStructDeclaration()
        {
            int mark = tokenPos;
            OILNode node1 = parseSpecifierQualifierList();
            if (node1 != null)
            {
                OILNode node2 = parseStructDeclaratorList();
                Token tok = getToken();
                if (tok.type == TokenType.SEMICOLON)
                {
                    OILNode node = arbor.buildStructDeclaration(node1, node2);
                    return node;
                }
            }
            rewindToken(mark);
            return null;
        }

        //specifier-qualifier-list:
        //  (type-specifier | type-qualifier)+
        public OILNode parseSpecifierQualifierList()
        {
            int mark = tokenPos;
            OILNode node1 = parseTypeSpecifier();
            if (node1 == null)
            {
                node1 = parseTypeQualifier();
            }
            if (node1 != null)
            {
                List<OILNode> nodelist = new List<OILNode>();
                while (node1 != null)
                {
                    nodelist.Add(node1);
                    node1 = parseTypeSpecifier();
                    if (node1 == null)
                    {
                        node1 = parseTypeQualifier();
                    }
                }
                OILNode node = arbor.buildSpecifierQualifierList(nodelist);
                return node;
            }
            rewindToken(mark);
            return null;
        }

        //struct-declarator-list:
        //  struct-declarator(',' struct-declarator)*
        public OILNode parseStructDeclaratorList()
        {
            int mark = tokenPos;
            OILNode node = parseStructDeclarator();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.COMMA)
                {
                    OILNode node1 = parseStructDeclarator();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildStructDeclaratorList();
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //struct-declarator:
        //  declarator | ((declarator)? ':' constant-expression)
        public OILNode parseStructDeclarator()
        {
            int mark = tokenPos;
            OILNode node1 = parseDeclarator();
            int mark1 = tokenPos;
            Token tok1 = getToken();
            OILNode node2 = null;
            if (tok1.type == TokenType.COLON)
            {
                node2 = parseConstantExpression();
            }
            else
            {
                rewindToken(mark1);
            }
            if (node1 != null || node2 != null)
            {
                OILNode node = arbor.buildStructDeclarator();
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //enum-specifier:
        //  'ENUM' ('identifier' | (('identifier')? '{' enumerator(',' enumerator)* (',')? '}'))
        public OILNode parseEnumSpecifier()
        {
            int mark = tokenPos;
            Token tok1 = getToken();
            if (tok1.type == TokenType.ENUM)
            {
                Token id = null;
                tok1 = getToken();
                if (tok1.type == TokenType.IDENT)
                {
                    id = tok1;
                    tok1 = getToken();
                }
                List<OILNode> nodelist = new List<OILNode>();
                if (tok1.type == TokenType.LBRACE)
                {
                    OILNode node1 = parseEnumerator();
                    nodelist.Add(node1);
                    tok1 = getToken();
                    while (tok1.type == TokenType.COMMA)
                    {
                        node1 = parseEnumerator();
                        if (node1 != null)
                        {
                            nodelist.Add(node1);
                        }
                        tok1 = getToken();
                    }
                }
                OILNode node = arbor.buildEnumSpecifier(id, nodelist);
            }
            rewindToken(mark);
            return null;
        }

        //enumerator:
        //  enumeration-constant ('=' constant-expression)?
        public OILNode parseEnumerator()
        {
            int mark = tokenPos;
            Token tok = getToken();
            Token enumc = arbor.isEnumerationConstant(tok);
            if (enumc != null)
            {
                int mark1 = tokenPos;
                Token tok1 = getToken();
                OILNode node1 = null;
                if (tok1.type == TokenType.EQUAL)
                {
                    node1 = parseConstantExpression();
                }
                else
                {
                    rewindToken(mark1);
                }
                OILNode node = arbor.buildEnumerator(enumc, node1);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //type-qualifier:
        //  'CONST' | 'RESTRICT' | 'VOLATILE'
        public OILNode parseTypeQualifier()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if ((tok.type == TokenType.CONST) ||
                (tok.type == TokenType.RESTRICT) ||
                (tok.type == TokenType.VOLATILE))
            {
                OILNode node = arbor.buildTypeQualifier(tok);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //function-specifier:
        //  INLINE
        public OILNode parseFunctionSpecifier()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.INLINE)
            {
                OILNode node = arbor.buildFunctionSpecifier(tok);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //---------------------------------------

        //declarator:
        //  (pointer)? direct-declarator
        public OILNode parseDeclarator()
        {
            int mark = tokenPos;
            OILNode node1 = parsePointer();
            OILNode node2 = parseDirectDeclarator();
            if (node2 != null)
            {
                OILNode node = arbor.buildDeclarator(node1, node2);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //direct-declarator:
        //  ('identifier' | ('(' declarator ')')) (('[' ((type-qualifier-list)? (assignment-expression)?) | (static (type-qualifier-list)? assignment-expression) |
        //  ((type-qualifier-list)? '*') | (type-qualifier-list static assignment-expression)']') | ('(' parameter-type-list | (identifier-list)? ')'))* 
        public OILNode parseDirectDeclarator()
        {
            int mark = tokenPos;
            Token tok = getToken();
            Token id = null;
            OILNode node1 = null;
            if (tok.type == TokenType.IDENT)
            {
                id = tok;
            }
            else if (tok.type == TokenType.LPAREN)
            {
                node1 = parseDeclarator();
                tok = getToken();
            }
            if (id != null || node1 != null)
            {
                OILNode node = arbor.buildBaseDirectDeclarator(id, node1);
                int mark1 = tokenPos;
                Token tok1 = getToken();

                //List<OILNode> nodeList = new List<OILNode>();

                while (tok1.type == TokenType.LBRACKET || tok1.type == TokenType.LPAREN)
                {
                    if (tok1.type == TokenType.LBRACKET)
                    {
                        int mark2 = tokenPos;
                        tok1 = getToken();
                        bool isStatic1 = false;
                        if (tok1.type == TokenType.STATIC)
                        {
                            isStatic1 = true;
                        }
                        else
                        {
                            rewindToken(mark2);
                        }
                        OILNode node2 = parseTypeQualifierList();
                        mark2 = tokenPos;
                        tok1 = getToken();
                        bool isStatic2 = false;
                        if (tok1.type == TokenType.STATIC)
                        {
                            isStatic2 = true;
                        }
                        else
                        {
                            rewindToken(mark2);
                        }
                        OILNode node3 = parseAssignmentExpression();
                        mark2 = tokenPos;
                        tok1 = getToken();
                        bool isPointer = false;
                        if (tok1.type == TokenType.STAR)
                        {
                            isPointer = true;
                        }
                        else
                        {
                            rewindToken(mark2);
                        }
                        tok1 = getToken();
                        node = arbor.buildArrayDirectDeclarator(node, isStatic1, node2, isStatic2, node3, isPointer);
                    }
                    else if (tok1.type == TokenType.LPAREN)
                    {
                        OILNode node4 = parseParameterTypeList();
                        if (node4 == null)
                        {
                            node4 = parseIdentifierList();
                        }
                        tok1 = getToken();
                        if (tok1.type == TokenType.RPAREN)
                        {
                            node = arbor.buildParamDirectDeclarator(node, node4);
                        }
                    }
                    tok1 = getToken();
                }
                rewindToken(mark1);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //pointer:
        //  ('*' (type-qualifier-list)?)+
        public OILNode parsePointer()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.STAR)
            {
                OILNode node1 = parseTypeQualifierList();
                OILNode node = arbor.buildPointer(null, node1);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.STAR)
                {
                    node1 = parseTypeQualifierList();
                    node = arbor.buildPointer(null, node1);
                }
                rewindToken(mark1);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //type-qualifier-list:
        //  (type-qualifier)+
        public OILNode parseTypeQualifierList()
        {
            int mark = tokenPos;
            OILNode node1 = parseParameterDeclaration();
            if (node1 != null)
            {
                List<OILNode> nodeList = new List<OILNode>();
                while (node1 != null)
                {
                    nodeList.Add(node1);
                    node1 = parseParameterDeclaration();
                }
                OILNode node = arbor.buildTypeQualifierList(nodeList);
                return node;
            }
            rewindToken(mark);
            return null;
        }

        //parameter-type-list:
        //  parameter-declaration (',' parameter-declaration)* (, ...)?
        public OILNode parseParameterTypeList()
        {
            int mark = tokenPos;
            OILNode node1 = parseParameterDeclaration();
            if (node1 != null)
            {
                int mark1 = tokenPos;
                Token tok1 = getToken();
                List<OILNode> nodeList = new List<OILNode>();
                while (tok1.type == TokenType.COMMA)
                {
                    OILNode node2 = parseParameterDeclaration();
                    nodeList.Add(node2);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                bool hasElipsis = false;
                mark1 = tokenPos;
                tok1 = getToken();
                if (tok1.type == TokenType.COMMA)
                {
                    tok1 = getToken();
                    if (tok1.type == TokenType.ELLIPSIS)
                    {
                        hasElipsis = true;
                    }
                    else
                    {
                        rewindToken(mark1);
                    }
                    OILNode node = arbor.buildParameterTypeList(node1, nodeList, hasElipsis);
                    return node;
                }
            }
            rewindToken(mark);
            return null;
        }

        //parameter-declaration:
        //  declaration-specifiers (declarator | (abstract-declarator)?)
        public OILNode parseParameterDeclaration()
        {
            int mark = tokenPos;
            OILNode node1 = parseDeclarationSpecifiers();
            if (node1 != null)
            {
                OILNode node2 = parseDeclarator();
                if (node2 == null)
                {
                    node2 = parseAbstractDeclarator();
                }
                OILNode node = arbor.buildParameterDeclaration();
                return node;
            }
            rewindToken(mark);
            return null;
        }

        //identifier-list:
        //  'identifier' (',' 'identifier')*
        public OILNode parseIdentifierList()
        {
            int mark = tokenPos;
            Token tok1 = getToken();
            List<Token> tokenList = new List<Token>();
            if (tok1.type == TokenType.IDENT)
            {
                tokenList.Add(tok1);
                int mark1 = tokenPos;
                tok1 = getToken();
                while (tok1.type == TokenType.COMMA)
                {
                    tok1 = getToken();
                    tokenList.Add(tok1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node = arbor.buildIdentifierList(tokenList);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //type-name:
        //  specifier-qualifier-list (abstract-declarator)?
        public OILNode parseTypeName()
        {
            int mark = tokenPos;
            OILNode node1 = parseSpecifierQualifierList();
            if (node1 != null)
            {
                OILNode node2 = parseAbstractDeclarator();
                OILNode node = arbor.buildTypeName(node1, node2);
                return node;
            }
            rewindToken(mark);
            return null;
        }

        //abstract-declarator:
        //  pointer | ((pointer)? direct-abstract-declarator)
        public OILNode parseAbstractDeclarator()
        {
            int mark = tokenPos;
            OILNode node1 = parsePointer();
            OILNode node2 = parseDirectDeclarator();
            if (node1 != null | node2 != null)
            {
                OILNode node = arbor.buildAbstractDeclarator(node1, node2);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //direct-abstract-declarator:
        //  ('(' abstract-declarator ')') | (('(' abstract-declarator ')')? 
        //   ('['((type-qualifier-list)?  (assignment-expression)?) | (STATIC (type-qualifier-list)? assignment-expression) |
        //      (type-qualifier-list static assignment-expression) | ('*')']') | 
        //  ('(' (parameter-type-list)? ')'))*)
        public OILNode parseDirectAbstractDeclarator()
        {
            int mark = tokenPos;
            Token tok = getToken();
            OILNode node1 = null;
            if (tok.type == TokenType.LPAREN)
            {
                node1 = parseAbstractDeclarator();
                tok = getToken();
            }
            if (node1 != null)
            {
                OILNode node = arbor.buildBaseAbstractDeclarator(node1);
                int mark1 = tokenPos;
                Token tok1 = getToken();

                while (tok1.type == TokenType.LBRACKET || tok1.type == TokenType.LPAREN)
                {
                    if (tok1.type == TokenType.LBRACKET)
                    {
                        int mark2 = tokenPos;
                        tok1 = getToken();
                        bool isStatic1 = false;
                        if (tok1.type == TokenType.STATIC)
                        {
                            isStatic1 = true;
                        }
                        else
                        {
                            rewindToken(mark2);
                        }
                        OILNode node2 = parseTypeQualifierList();
                        mark2 = tokenPos;
                        tok1 = getToken();
                        bool isStatic2 = false;
                        if (tok1.type == TokenType.STATIC)
                        {
                            isStatic2 = true;
                        }
                        else
                        {
                            rewindToken(mark2);
                        }
                        OILNode node3 = parseAssignmentExpression();
                        mark2 = tokenPos;
                        tok1 = getToken();
                        bool isPointer = false;
                        if (tok1.type == TokenType.STAR)
                        {
                            isPointer = true;
                        }
                        else
                        {
                            rewindToken(mark2);
                        }
                        tok1 = getToken();
                        node = arbor.buildArrayAbstractDeclarator(node, isStatic1, node2, isStatic2, node3, isPointer);
                    }
                    else if (tok1.type == TokenType.LPAREN)
                    {
                        OILNode node4 = parseParameterTypeList();
                        tok1 = getToken();
                        if (tok1.type == TokenType.RPAREN)
                        {
                            node = arbor.buildParamAbstractDeclarator(node, node4);
                        }
                    }
                    tok1 = getToken();
                }
                rewindToken(mark);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //---------------------------------------

        //initializer:
        //  assignment-expression | ('{' initializer-list(',')? '}')
        public OILNode parseInitializer()
        {
            int mark = tokenPos;
            rewindToken(mark);
            OILNode node1 = parseAssignmentExpression();
            OILNode node2 = null;
            if (node1 == null)
            {
                int mark1 = tokenPos;
                Token tok1 = getToken();
                if (tok1.type == TokenType.LBRACE)
                {
                    node2 = parseInitializerList();
                    int mark2 = tokenPos;
                    Token tok2 = getToken();
                    if (tok2.type != TokenType.COMMA)
                    {
                        rewindToken(mark2);
                    }
                    tok2 = getToken();
                }
            }
            if (node1 != null | node2 != null)
            {
                OILNode node = arbor.buildInitializer(node1, node2);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //initializer-list:
        //  (designation)? initializer (',' (designation)? initializer)*
        public OILNode parseInitializerList()
        {
            int mark = tokenPos;
            OILNode node1 = parseDesignation();
            OILNode node2 = parseAssignmentExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node2 != null)
            {
                if (node1 != null)
                {
                    nodeList.Add(node1);
                }
                nodeList.Add(node2);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.COMMA)
                {
                    node1 = parseDesignation();
                    node2 = parseAssignmentExpression();
                    if (node1 != null)
                    {
                        nodeList.Add(node1);
                    }
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node = arbor.buildInitializerList(nodeList);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //designation:
        //  (('[' constant-expression ']') | ('.' 'identifier'))+ '='
        public OILNode parseDesignation()
        {
            int mark = tokenPos;
            Token tok = getToken();
            OILNode node = null;
            while ((tok.type == TokenType.LBRACKET) || (tok.type == TokenType.PERIOD))
            {
                if (tok.type == TokenType.LBRACKET)
                {
                    OILNode node1 = parseConstantExpression();
                    Token tok1 = getToken();
                    if (tok.type == TokenType.RBRACKET)
                    {
                        node = arbor.buildArrayDesignation(node, node1);
                        mark = tokenPos;
                        tok = getToken();
                    }
                }
                if (tok.type == TokenType.PERIOD)
                {
                    Token tok1 = getToken();
                    if (tok1.type == TokenType.IDENT)
                    {
                        node = arbor.buildFieldDesignation(node, tok1);
                        mark = tokenPos;
                        tok = getToken();
                    }
                }
            }
            if (node != null)
            {
                node = arbor.buildArrayDesignation(node);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //- Statements --------------------------------------------------------

        //statement:
        //  labeled-statement | compound-statement | expression-statement | selection-statement | iteration-statement | jump-statement
        public OILNode parseStatement()
        {
            int mark = tokenPos;
            OILNode node = parseLabeledStatement();
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
            if (node != null)
            {
                node = arbor.buildStatement(node);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //labeled-statement:
        //  ('identifier' ':' statement) | 
        //  ('CASE' constant-expression ':' statement) | 
        //  ('DEFAULT' ':' statement)
        public OILNode parseLabeledStatement()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.IDENT)
            {
                Token tok2 = getToken();
                if (tok2.type == TokenType.COLON)
                {
                    OILNode node1 = parseStatement();
                    if (node1 != null)
                    {
                        OILNode node = arbor.buildLabeledStatement(tok, node1);
                        return node;
                    }
                }
            }
            else if (tok.type == TokenType.CASE)
            {
                OILNode node1 = parseConstantExpression();
                if (node1 != null)
                {
                    Token tok2 = getToken();
                    if (tok2.type == TokenType.COLON)
                    {
                        OILNode node2 = parseStatement();
                        if (node2 != null)
                        {
                            OILNode node = arbor.buildCaseStatement(node1, node2);
                            return node;
                        }
                    }
                }

            }
            else if (tok.type == TokenType.DEFAULT)
            {
                Token tok2 = getToken();
                if (tok2.type == TokenType.COLON)
                {
                    OILNode node1 = parseStatement();
                    if (node1 != null)
                    {
                        OILNode node = arbor.buildDefaultStatement(tok, node1);
                        return node;
                    }
                }
            }

            rewindToken(mark);
            return null;
        }

        //compound-statement:
        //  '{' (declaration | statement)* '}'
        public OILNode parseCompoundStatement()
        {
            int mark = tokenPos;
            Token tok = getToken();
            List<OILNode> nodeList = new List<OILNode>();
            if (tok.type == TokenType.LBRACE)
            {
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type != TokenType.RBRACE)
                {
                    rewindToken(mark1);
                    OILNode node1 = parseDeclaration();
                    if (node1 == null)
                    {
                        node1 = parseStatement();
                    }
                    if (node1 != null)
                    {
                        nodeList.Add(node1);
                    }
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                OILNode node = arbor.buildCompoundStatement(nodeList);
                return node;
            }
            rewindToken(mark);
            return null;
        }

        //expression-statement:
        //  (expression)? ;
        public OILNode parseExpressionStatement()
        {
            int mark = tokenPos;
            OILNode node = parseExpression();
            Token tok = getToken();
            if (tok.type == TokenType.SEMICOLON)
            {
                OILNode node1 = arbor.buildExpressionStatement(node);
                return node1;
            }

            rewindToken(mark);
            return null;
        }

        //selection-statement:
        //  ('IF' '(' expression ')' statement('ELSE' statement)?) | 
        //  ('SWITCH' '(' expression ')' statement)
        public OILNode parseSelectionStatement()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.IF)
            {
                Token tok1 = getToken();
                if (tok1.type == TokenType.LPAREN)
                {
                    OILNode node1 = parseExpression();
                    tok1 = getToken();
                    if (tok1.type == TokenType.RPAREN)
                    {
                        OILNode node2 = parseStatement();
                        int mark1 = tokenPos;
                        tok1 = getToken();
                        OILNode node3 = null;
                        if (tok1.type == TokenType.ELSE)
                        {
                            node3 = parseStatement();
                        }
                        else
                        {
                            rewindToken(mark1);
                        }
                        OILNode node = arbor.buildIfStatement(node1, node2, node3);
                        return node;
                    }
                }
            }
            if (tok.type == TokenType.SWITCH)
            {
                Token tok1 = getToken();
                if (tok1.type == TokenType.LPAREN)
                {
                    OILNode node1 = parseExpression();
                    tok1 = getToken();
                    if (tok1.type == TokenType.RPAREN)
                    {
                        OILNode node2 = parseStatement();
                        OILNode node = arbor.buildSwitchStatement(node1, node2);
                        return node;
                    }
                }
            }
            rewindToken(mark);
            return null;
        }

        //iteration-statement:
        //  ('WHILE' '(' expression ')' statement) | 
        //  ('DO' statement 'WHILE' '(' expression ')' ';') | 
        //  ('FOR' '(' (declaration | ((expression)? ';')) (expression)? ';' (expression)? ')' statement)
        public OILNode parseIterationStatement()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.WHILE)
            {
                Token tok1 = getToken();
                if (tok1.type == TokenType.LPAREN)
                {
                    OILNode node1 = parseExpression();
                    tok1 = getToken();
                    if (tok1.type == TokenType.RPAREN)
                    {
                        OILNode node2 = parseStatement();
                        OILNode node = arbor.buildWhileStatement(node1, node2);
                        return node;
                    }
                }
            }
            if (tok.type == TokenType.DO)
            {
                OILNode node1 = parseStatement();
                Token tok1 = getToken();
                if (tok1.type == TokenType.WHILE)
                {
                    tok1 = getToken();
                    if (tok1.type == TokenType.LPAREN)
                    {
                        OILNode node2 = parseExpression();
                        tok1 = getToken();
                        if (tok1.type == TokenType.RPAREN)
                        {
                            tok1 = getToken();
                            if (tok1.type == TokenType.SEMICOLON)
                            {
                                OILNode node = arbor.buildDoWhileStatement(node1, node2);
                                return node;
                            }
                        }
                    }
                }
            }
            if (tok.type == TokenType.FOR)
            {
                Token tok1 = getToken();
                if (tok1.type == TokenType.LPAREN)
                {
                    OILNode node1 = parseDeclaration();
                    if (node1 == null)
                    {
                        node1 = parseExpression();
                        tok1 = getToken();
                    }
                    OILNode node2 = parseExpression();
                    tok1 = getToken();
                    if (tok1.type == TokenType.SEMICOLON)
                    {
                        OILNode node3 = parseExpression();
                        tok1 = getToken();
                        if (tok1.type == TokenType.SEMICOLON)
                        {
                            tok1 = getToken();
                            if (tok1.type == TokenType.RPAREN)
                            {
                                OILNode node4 = parseStatement();
                                OILNode node = arbor.buildForStatement(node1, node2, node3, node4);
                                return node;
                            }
                        }
                    }
                }
            }

            rewindToken(mark);
            return null;
        }

        //jump-statement:
        //  (('GOTO' 'identifier') | 
        //  'CONTINUE' | 
        //  'BREAK' | 
        //  ('RETURN' (expression)?)) ;
        public OILNode parseJumpStatement()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.GOTO)
            {
                Token tok1 = getToken();
                if (tok1.type == TokenType.IDENT)
                {
                    OILNode node = arbor.buildGotoStatement(tok1);
                    return node;
                }
            }
            else if (tok.type == TokenType.CONTINUE)
            {
                OILNode node = arbor.buildContinueStatement();
                return node;
            }
            else if (tok.type == TokenType.BREAK)
            {
                OILNode node = arbor.buildBreakStatement();
                return node;
            }
            else if (tok.type == TokenType.RETURN)
            {
                OILNode node1 = parseExpression();
                OILNode node = arbor.buildReturnStatement(node1);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //- Expressions -------------------------------------------------------

        //primary-expression:
        //  'identifier' | 'constant' | 'string-literal' | ('(' expression ')')
        public OILNode parsePrimaryExpression()
        {
            int mark = tokenPos;
            Token tok = getToken();
            if (tok.type == TokenType.IDENT)
            {
                OILNode node = arbor.buildIdentExpression(tok);
                return node;
            }
            if (tok.type == TokenType.INTCONST)
            {
                OILNode node = arbor.buildIntConstExpression(tok);
                return node;
            }
            if (tok.type == TokenType.FLOATCONST)
            {
                OILNode node = arbor.buildFloatConstExpression(tok);
                return node;
            }
            if (tok.type == TokenType.CHARCONST)
            {
                OILNode node = arbor.buildCharConstExpression(tok);
                return node;
            }
            if (tok.type == TokenType.STRINGCONST)
            {
                OILNode node = arbor.buildStringConstExpression(tok);
                return node;
            }
            if (tok.type == TokenType.LPAREN)
            {
                OILNode node1 = parseExpression();
                Token tok2 = getToken();
                if (tok2.type == TokenType.RPAREN)
                {
                    OILNode node = arbor.buildSubExpression(node1);
                    return node;
                }
            }

            rewindToken(mark);
            return null;
        }

        //postfix-expression:
        //  (primary-expression | ('(' type-name ')' '{' initializer-list(',')? '}')) 
        //  (('[' expression ']') | ('(' (argument-expression-list)? ')') | ('.' 'identifier') | ('->' 'identifier') | '++' | '--')*
        public OILNode parsePostfixExpression()
        {
            int mark = tokenPos;
            OILNode node1 = parsePrimaryExpression();
            if (node1 == null)
            {
                int mark1 = tokenPos;
                Token tok1 = getToken();
                if (tok1.type == TokenType.LPAREN)
                {
                    OILNode node2 = parseTypeName();
                    if (node2 != null)
                    {
                        tok1 = getToken();
                        if (tok1.type == TokenType.RPAREN)
                        {
                            tok1 = getToken();
                            if (tok1.type == TokenType.LBRACE)
                            {
                                OILNode node3 = parseInitializerList();
                                tok1 = getToken();
                                if (tok1.type == TokenType.COMMA)
                                {
                                    tok1 = getToken();
                                }
                                tok1 = getToken();
                                if (tok1.type == TokenType.RBRACE)
                                {
                                    node1 = arbor.buildTypeNameInitializerList(node2, node3);
                                }
                            }
                        }
                    }
                }
                else
                {
                    rewindToken(mark1);
                }
            }
            if (node1 != null)
            {
                int mark3 = tokenPos;
                Token tok1 = getToken();
                while ((tok1.type == TokenType.LBRACKET) ||
                    (tok1.type == TokenType.LPAREN) ||
                    (tok1.type == TokenType.PERIOD) ||
                    (tok1.type == TokenType.ARROW) ||
                    (tok1.type == TokenType.PLUSPLUS) ||
                    (tok1.type == TokenType.MINUSMINUS))
                {
                    if (tok1.type == TokenType.LBRACKET)
                    {
                        OILNode node2 = parseExpression();
                        Token tok2 = getToken();
                        if (tok2.type == TokenType.RBRACKET)
                        {
                            node1 = arbor.buildArrayIndexExpression(node1, node2);
                        }
                    }
                    if (tok1.type == TokenType.LPAREN)
                    {
                        OILNode node2 = parseArgumentExpressionList();
                        Token tok2 = getToken();
                        if (tok2.type == TokenType.RPAREN)
                        {
                            node1 = arbor.buildArgumentListExpression(node1, node2);
                        }
                    }
                    if (tok1.type == TokenType.PERIOD)
                    {
                        Token tok3 = getToken();
                        if (tok3.type == TokenType.IDENT)
                        {
                            node1 = arbor.buildFieldReference(node1, tok3);
                        }
                    }
                    if (tok1.type == TokenType.ARROW)
                    {
                        Token tok3 = getToken();
                        if (tok3.type == TokenType.IDENT)
                        {
                            node1 = arbor.buildIndirectFieldReference(node1, tok3);
                        }
                    }
                    if ((tok1.type == TokenType.PLUSPLUS) || (tok1.type == TokenType.MINUSMINUS))
                    {
                        node1 = arbor.buildPostIncDecExpression(tok1);
                    }

                    mark3 = tokenPos;
                    tok1 = getToken();
                }

                rewindToken(mark3);
                return node1;
            }

            rewindToken(mark);
            return null;
        }

        //argument-expression-list:
        //  assignment-expression(',' assignment-expression)*
        public OILNode parseArgumentExpressionList()
        {
            int mark = tokenPos;
            List<OILNode> nodeList = new List<OILNode>();
            OILNode node1 = parseAssignmentExpression();
            if (node1 != null)
            {
                nodeList.Add(node1);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.COMMA)
                {
                    OILNode node2 = parseAssignmentExpression();
                    nodeList.Add(node2);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node = arbor.buildeArgumentExpressionList(nodeList);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //unary-expression:
        //  postfix-expression 
        //  ('++'| '--'|'SIZEOF') unary-expression | 
        //  (('&'|'*'|'+'|'-'|'~'|'!') cast-expression) | 
        // ('SIZEOF' '(' type-name ')')
        public OILNode parseUnaryExpression()
        {
            int mark = tokenPos;
            int mark1 = tokenPos;
            Token tok1 = getToken();
            if (tok1.type == TokenType.SIZEOF)
            {
                int mark2 = tokenPos;
                Token tok2 = getToken();
                if (tok2.type == TokenType.LPAREN)
                {
                    OILNode node1 = parseTypeName();
                    tok2 = getToken();
                    if (tok2.type == TokenType.RPAREN)
                    {
                        OILNode node2 = arbor.buildSizeOfExpression(node1);
                        return node2;
                    }
                }
                else
                {
                    rewindToken(mark2);
                    OILNode node1 = parseUnaryExpression();
                    OILNode node3 = arbor.buildSizeOfExpression(node1);
                    return node3;
                }
            }
            if ((tok1.type == TokenType.PLUSPLUS) ||
                (tok1.type == TokenType.MINUSMINUS))
            {
                OILNode node1 = parseUnaryExpression();
                OILNode node4 = arbor.buildIncDecExpression(tok1, node1);
                return node4;
            }
            if ((tok1.type == TokenType.AMPERSAND) ||
                (tok1.type == TokenType.STAR) ||
                (tok1.type == TokenType.PLUS) ||
                (tok1.type == TokenType.MINUS) ||
                (tok1.type == TokenType.TILDE) ||
                (tok1.type == TokenType.EXCLAIM))
            {
                OILNode node1 = parseCastExpression();
                OILNode node4 = arbor.buildUnaryExpression(tok1, node1);
                return node4;
            }
            rewindToken(mark1);
            OILNode node = parsePostfixExpression();
            if (node != null)
            {
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //cast-expression:  
        //  ('(' type-name ')')* unary-expression
        public OILNode parseCastExpression()
        {
            int mark = tokenPos;
            int mark1 = tokenPos;
            Token tok1 = getToken();
            List<OILNode> nodeList = new List<OILNode>();
            while (tok1.type == TokenType.LPAREN)
            {
                OILNode node1 = parseTypeName();
                Token tok2 = getToken();
                if (tok2.type == TokenType.RPAREN)
                {
                    nodeList.Add(node1);
                }
                mark1 = tokenPos;
                tok1 = getToken();
            }
            rewindToken(mark1);
            OILNode node2 = parseCastExpression();
            if (node2 != null)
            {
                OILNode node = arbor.buildCastExpression(nodeList, node2);
                return node;

            }

            rewindToken(mark);
            return null;
        }

        //multiplicative-expression:
        //  cast-expression ('*'|'/'|'%' cast-expression)*
        public OILNode parseMultiplicativeExpression()
        {
            int mark = tokenPos;
            OILNode node = parseCastExpression();
            List<OILNode> nodeList = new List<OILNode>();
            List<Token> tokenList = new List<Token>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.STAR ||
                    tok1.type == TokenType.SLASH ||
                    tok1.type == TokenType.PERCENT)
                {
                    tokenList.Add(tok1);
                    OILNode node1 = parseCastExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildMultiplicativeExpression(nodeList, tokenList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //additive-expression:
        //  multiplicative-expression('+'|'-' multiplicative-expression)*
        public OILNode parseAdditiveExpression()
        {
            int mark = tokenPos;
            OILNode node = parseMultiplicativeExpression();
            List<OILNode> nodeList = new List<OILNode>();
            List<Token> tokenList = new List<Token>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.PLUS ||
                    tok1.type == TokenType.MINUS)
                {
                    tokenList.Add(tok1);
                    OILNode node1 = parseMultiplicativeExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildAdditiveExpression(nodeList, tokenList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //shift-expression:
        //  additive-expression('<<'|'>>' additive-expression)*
        public OILNode parseShiftExpression()
        {
            int mark = tokenPos;
            OILNode node = parseShiftExpression();
            List<OILNode> nodeList = new List<OILNode>();
            List<Token> tokenList = new List<Token>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.LESSLESS ||
                    tok1.type == TokenType.GTRGTR)
                {
                    tokenList.Add(tok1);
                    OILNode node1 = parseShiftExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildShiftExpression(nodeList, tokenList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //relational-expression:
        //  shift-expression(('<'|'>'|'<='|'>=') shift-expression)*
        public OILNode parseRelationalExpression()
        {
            int mark = tokenPos;
            OILNode node = parseShiftExpression();
            List<OILNode> nodeList = new List<OILNode>();
            List<Token> tokenList = new List<Token>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.LESSTHAN ||
                    tok1.type == TokenType.GTRTHAN ||
                    tok1.type == TokenType.LESSEQUAL ||
                    tok1.type == TokenType.GTREQUAL)
                {
                    tokenList.Add(tok1);
                    OILNode node1 = parseShiftExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildRelationalExpression(nodeList, tokenList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //equality-expression:
        //  relational-expression(('=='|'!=') relational-expression)*
        public OILNode parseEqualityExpression()
        {
            int mark = tokenPos;
            OILNode node = parseRelationalExpression();
            List<OILNode> nodeList = new List<OILNode>();
            List<Token> tokenList = new List<Token>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.EQUALEQUAL ||
                    tok1.type == TokenType.NOTEQUAL)
                {
                    tokenList.Add(tok1);
                    OILNode node1 = parseRelationalExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildEqualityExpression(nodeList, tokenList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //AND-expression:
        //  equality-expression('&' equality-expression)*
        public OILNode parseAndExpression()
        {
            int mark = tokenPos;
            OILNode node = parseEqualityExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.AMPERSAND)
                {
                    OILNode node1 = parseEqualityExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildAndExpression(nodeList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //exclusive-OR-expression:
        //  AND-expression('^' AND-expression)*
        public OILNode parseeExclusiveOrExpression()
        {
            int mark = tokenPos;
            OILNode node = parseAndExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.CARET)
                {
                    OILNode node1 = parseAndExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildExclusiveOrExpression(nodeList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //inclusive-OR-expression:
        //  exclusive-OR-expression('|' exclusive-OR-expression)*
        public OILNode parseInclusiveOrExpression()
        {
            int mark = tokenPos;
            OILNode node = parseeExclusiveOrExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.BAR)
                {
                    OILNode node1 = parseeExclusiveOrExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildInclusiveOrExpression(nodeList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //logical-AND-expression:
        //  inclusive-OR-expression('&&' inclusive-OR-expression)*
        public OILNode parseLogicalAndExpression()
        {
            int mark = tokenPos;
            OILNode node = parseInclusiveOrExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.AMPAMP)
                {
                    OILNode node1 = parseInclusiveOrExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildLogicalAndExpression(nodeList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //logical-OR-expression:
        //  logical-AND-expression('||' logical-AND-expression)*
        public OILNode parseLogicalOrExpression()
        {
            int mark = tokenPos;
            OILNode node = parseLogicalAndExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.BARBAR)
                {
                    OILNode node1 = parseLogicalAndExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildeLogicalOrExpression(nodeList);
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //conditional-expression:
        //  logical-OR-expression ('?' expression ':' conditional-expression)?
        public OILNode parseConditionalExpression()
        {
            int mark = tokenPos;
            OILNode node1 = parseLogicalOrExpression();
            if (node1 != null)
            {
                int mark1 = tokenPos;
                Token tok1 = getToken();
                OILNode node2 = null;
                OILNode node3 = null;
                if (tok1.type == TokenType.QUESTION)
                {
                    node2 = parseExpression();
                    Token tok2 = getToken();
                    node3 = parseConditionalExpression();
                }
                else
                {
                    rewindToken(mark1);
                }
                OILNode node = arbor.buildConditionalExpression(node1, node2, node3);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //assignment-expression:
        //  (unary-expression('='|'*='|'/='|'%='|'+='|'-='|'<<='|'>>='|'&='|'^='|'|='))* conditional-expression
        public OILNode parseAssignmentExpression()
        {
            int mark = tokenPos;
            OILNode node1 = parseUnaryExpression();
            List<OILNode> nodelist = new List<OILNode>();
            List<Token> tokenList = new List<Token>();
            while (node1 != null)
            {
                nodelist.Add(node1);
                Token tok1 = getToken();
                if ((tok1.type == TokenType.EQUAL) ||
                    (tok1.type == TokenType.MULTEQUAL) ||
                    (tok1.type == TokenType.SLASHEQUAL) ||
                    (tok1.type == TokenType.PERCENTEQUAL) ||
                    (tok1.type == TokenType.PLUSEQUAL) ||
                    (tok1.type == TokenType.MINUSEQUAL) ||
                    (tok1.type == TokenType.LESSLESSEQUAL) ||
                    (tok1.type == TokenType.GTRGTREQUAL) ||
                    (tok1.type == TokenType.AMPEQUAL) ||
                    (tok1.type == TokenType.CARETEQUAL) ||
                    (tok1.type == TokenType.BAREQUAL))
                {
                    tokenList.Add(tok1);
                    node1 = parseUnaryExpression();
                }
            }
            OILNode node2 = parseConditionalExpression();
            if (node2 != null)
            {
                OILNode node = arbor.buildAssignmentExpression(nodelist, tokenList, node2);
                return node;
            }

            rewindToken(mark);
            return null;
        }

        //expression:
        //  assignment-expression(',' assignment-expression)*
        public OILNode parseExpression()
        {
            int mark = tokenPos;
            OILNode node = parseAssignmentExpression();
            List<OILNode> nodeList = new List<OILNode>();
            if (node != null)
            {
                nodeList.Add(node);
                int mark1 = tokenPos;
                Token tok1 = getToken();
                while (tok1.type == TokenType.COMMA)
                {
                    OILNode node1 = parseAssignmentExpression();
                    nodeList.Add(node1);
                    mark1 = tokenPos;
                    tok1 = getToken();
                }
                rewindToken(mark1);
                OILNode node2 = arbor.buildExpression();
                return node2;
            }

            rewindToken(mark);
            return null;
        }

        //constant-expression:
        //  conditional-expression
        public OILNode parseConstantExpression()
        {
            int mark = tokenPos;
            OILNode node = parseConditionalExpression();

            if (node != null)
            {
                OILNode node1 = arbor.buildConstantExpression();
                return node1;
            }

            rewindToken(mark);
            return null;
        }
    }

    //-------------------------------------------------------------------------

    public class ParserException : Exception
    {
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");