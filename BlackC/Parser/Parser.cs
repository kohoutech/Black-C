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

using BlackC.Lexer;
using Origami.OIL;

// the grammar this parser is pased on:
//https://en.wikipedia.org/wiki/C99
//http://www.open-std.org/jtc1/sc22/WG14/www/docs/n1256.pdf

namespace BlackC
{
    public class Parser
    {
        //public Options options;
        public Tokenizer prep;
        public Arbor arbor;

        public String filename;

        public bool preProcessOnly;
        public String preProcessFilename;

        public List<String> includePaths;
        public bool saveSpaces;

        public Parser(String _filename)
        {
            //options = _options;
            filename = _filename;

            prep = null;
            arbor = new Arbor(this);

            preProcessOnly = false;
            preProcessFilename = "";
            saveSpaces = false;

            //    includePaths = new List<string>() { "." };          //start with current dir
            //    includePaths.AddRange(options.includePaths);        //add search paths from command line         
        }

        public void handlePragma(List<Fragment> args)
        {
        }

        public Module parseFile()
        {
            Module module = null;
            prep = new Tokenizer(this, filename);

            if (preProcessOnly)
            {
                prep.preprocessFile(preProcessFilename);
            }
            else
            {
                module = parseTranslationUnit();
            }
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

        //- external definitions ----------------------------------------------

        /*(6.9) 
          translation-unit:
            (function-definition | declaration)+
          
          function-definition:
            declaration-specifiers declarator (declaration)* compound-statement
        */
        public Module parseTranslationUnit()
        {
            Module module = new Module(filename);
            do
            {
                Declaration decl = parseDeclaration();
                if (decl != null && (decl is FuncDeclNode) && ((FuncDeclNode)decl).isFuncDef)
                {
                    FuncDeclNode funcdef = (FuncDeclNode)decl;

                    List<Declaration> oldparamlist = new List<Declaration>();
                    Declaration pdecl = parseDeclaration();
                    while (pdecl != null)
                    {
                        oldparamlist.Add(pdecl);
                        pdecl = parseDeclaration();
                    }

                    CompoundStatementNode block = parseCompoundStatement();

                    arbor.completeFuncDef(funcdef, oldparamlist, block);
                    arbor.addFuncDefToModule(module, funcdef);
                }
                else
                {
                    arbor.addDeclToModule(module, decl);
                }
            }
            while (!nextTokenIs(TokenType.EOF));
            return module;
        }

        //- declarations ------------------------------------------------------

        /* 
         (6.7) 
         declaration:
            declaration-specifiers (init-declarator-list)? ;

         (6.7) 
         init-declarator-list:
            init-declarator (, init-declarator)*

         (6.7) 
         init-declarator:
            declarator (= initializer)?
         */
        public Declaration parseDeclaration()
        {
            Declaration decl = null;
            DeclSpecNode declSpecs = parseDeclarationSpecs(true, true);
            if (declSpecs != null)
            {
                //if decl spec followed by a ';' (no var list) then it's a type definition, like struct foo {...}; or enum bar {...};
                if (nextTokenIs(TokenType.SEMICOLON))
                {
                    consumeToken();                                                 //skip ';'
                    TypeDeclNode tdecl = arbor.makeTypeDeclNode(declSpecs);
                    return tdecl;
                }

                //now we have a declarator list or a func definition
                while (true)
                {
                    DeclaratorNode declarnode = parseDeclarator(false);

                    if (nextTokenIs(TokenType.LBRACE))
                    {
                        FuncDeclNode fdecl = arbor.makeFuncDeclNode(declSpecs, declarnode);         //declaration-specifiers declarator {...
                        fdecl.isFuncDef = true;
                        return fdecl;
                    }
                    else if (nextTokenIs(TokenType.EQUAL))
                    {
                        consumeToken();                                             //skip '='
                        InitializerNode initialnode = parseInitializer();
                        decl = arbor.makeVarDeclNode(decl, declSpecs, declarnode, initialnode);       //declarator = initializer
                    }
                    else
                    {
                        decl = arbor.makeVarDeclNode(decl, declSpecs, declarnode, null);             //declarator
                    }

                    if (nextTokenIs(TokenType.COMMA))           //not at end of declarator list yet
                    {
                        consumeToken();                         //skip ','
                        continue;
                    }
                    consumeToken();          //at list end - skip ';'
                    break;
                }
            }
            return decl;
        }

        /* (6.7) 
         declaration-specifiers:
           (storage-class-specifier | type-specifier | type-qualifier | function-specifier)+

         (6.7.1) 
         storage-class-specifier:
           'TYPEDEF' | 'EXTERN' | 'STATIC' | 'AUTO' | 'REGISTER'

         (6.7.2) 
         type-specifier:
           'VOID' | 'CHAR' | 'SHORT' | 'INT' | 'LONG' | 'FLOAT' | 'DOUBLE' | 'SIGNED' | 'UNSIGNED' | 
            struct-or-union-specifier | enum-specifier | typedef-name

         (6.7.3) 
         type-qualifier:
           'CONST' | 'RESTRICT' | 'VOLATILE'
         
         (6.7.4) 
         function-specifier:
            INLINE
        */
        public DeclSpecNode parseDeclarationSpecs(bool scAllowed, bool fsAllowed)
        {
            DeclSpecNode specs = null;
            List<Token> storageClassSpecs = new List<Token>();
            List<TypeDeclNode> typeDefs = new List<TypeDeclNode>();
            List<Token> typeMods = new List<Token>();
            List<Token> typeQuals = new List<Token>();
            List<Token> functionSpecs = new List<Token>();

            bool done = false;
            while (!done)
            {
                Token token = prep.getToken();
                switch (token.type)
                {
                    case TokenType.TYPEDEF:
                    case TokenType.EXTERN:
                    case TokenType.STATIC:
                    case TokenType.AUTO:
                    case TokenType.REGISTER:
                        if (scAllowed)
                        {
                            storageClassSpecs.Add(token);
                            continue;
                        }
                        else
                        {
                            prep.putTokenBack(token);
                            done = true;
                            break;
                        }

                    case TokenType.SHORT:
                    case TokenType.LONG:
                    case TokenType.SIGNED:
                    case TokenType.UNSIGNED:
                        typeMods.Add(token);
                        continue;

                    case TokenType.VOID:
                    case TokenType.CHAR:
                    case TokenType.INT:
                    case TokenType.FLOAT:
                    case TokenType.DOUBLE:
                        TypeDeclNode toktype = arbor.GetTypeDef(token);
                        typeDefs.Add(toktype);
                        continue;

                    case TokenType.STRUCT:
                    case TokenType.UNION:
                        StructDeclNode structdecl = parseStructOrUnionSpec();
                        typeDefs.Add(structdecl);
                        continue;

                    case TokenType.ENUM:
                        EnumDeclNode enumdecl = parseEnumeratorSpec();
                        typeDefs.Add(enumdecl);
                        continue;

                    case TokenType.IDENT:
                        TypeDeclNode typename = arbor.GetTypeDef(token.strval);
                        if (typename != null)
                        {
                            typeDefs.Add(typename);
                            continue;
                        }
                        else
                        {
                            prep.putTokenBack(token);
                            done = true;
                            break;
                        }

                    case TokenType.CONST:
                    case TokenType.RESTRICT:
                    case TokenType.VOLATILE:
                        typeQuals.Add(token);
                        continue;

                    case TokenType.INLINE:
                        if (scAllowed)
                        {
                            functionSpecs.Add(token);
                            continue;
                        }
                        else
                        {
                            prep.putTokenBack(token);
                            done = true;
                            break;
                        }

                    default:
                        prep.putTokenBack(token);
                        done = true;
                        break;
                }
            }
            if (storageClassSpecs.Count > 0 || typeDefs.Count > 0 || typeQuals.Count > 0 || functionSpecs.Count > 0)
            {
                specs = arbor.makeDeclSpecs(storageClassSpecs, typeDefs, typeMods, typeQuals, functionSpecs);
            }
            return specs;
        }

        // stuctures/unions -----------------------------------------

        /*(6.7.2.1) 
         struct-or-union-specifier:
           ('STRUCT' | 'UNION') ('identifier' | (('identifier')? '{' (struct-declaration)+ '}'))
        */
        // struct w/o ident is for anonymous struct (possibly part of a typedef)
        // struct w/o {list} is for a already defined struct type
        public StructDeclNode parseStructOrUnionSpec()
        {
            StructDeclNode node = null;
            Token token = prep.getToken();
            if (token.type == TokenType.STRUCT || token.type == TokenType.UNION)
            {
                bool isUnion = (token.type == TokenType.UNION);
                IdentNode idNode = null;
                token = prep.getToken();                                //either identifier or '{'
                if (token.type == TokenType.IDENT)
                {
                    idNode = arbor.getStructIdentNode(token);
                    token = prep.getToken();                            //is identifier followed by '{'
                }
                if (token.type == TokenType.LBRACE)
                {
                    StructDeclarationNode field = parseStructDeclaration();
                    node = arbor.starttructSpec(node, idNode, field, isUnion);
                    field = parseStructDeclaration();
                    while (field != null)
                    {
                        node = arbor.makeStructSpec(node, field);
                        field = parseStructDeclaration();
                    }
                    consumeToken();                                     //skip closing '}'
                }
                else
                {
                    prep.putTokenBack(token);                           //no enum definiton, just a ref
                    node = arbor.getStructDecl(idNode, isUnion);
                }
            }
            else
            {
                prep.putTokenBack(token);           //not struct declar or reference
            }
            return node;
        }

        /*(6.7.2.1) 
         struct-declaration:
            specifier-qualifier-list struct-declarator-list ;
         */
        // a single struct field def (can have mult fields, ie int a, b;)
        public StructDeclarationNode parseStructDeclaration()
        {
            StructDeclarationNode node = null;
            DeclSpecNode specqual = parseSpecQualList();          //field type
            if (specqual != null)
            {
                List<StructDeclaratorNode> fieldnames = parseStructDeclaratorList();           //list of field names 
                consumeToken();
                node = arbor.makeStructDeclarationNode(specqual, fieldnames);
            }
            return node;
        }

        /*(6.7.2.1) 
         specifier-qualifier-list:
           (type-specifier | type-qualifier)+
         */
        // struct field's type - same as declaration-specifiers, w/o the storage-class-specifier or function-specifier
        public DeclSpecNode parseSpecQualList()
        {
            return parseDeclarationSpecs(false, false);
        }

        /*(6.7.2.1) 
         struct-declarator-list:
           struct-declarator (',' struct-declarator)*
         */
        // the list of field names, fx the "a, b, c" in "int a, b, c;" that def's three fields of type int
        public List<StructDeclaratorNode> parseStructDeclaratorList()
        {
            List<StructDeclaratorNode> fieldlist = null;
            StructDeclaratorNode fieldnode = parseStructDeclarator();      //the first field name
            if (fieldnode != null)
            {
                fieldlist = new List<StructDeclaratorNode>();
                fieldlist.Add(fieldnode);
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.COMMA)
                    {
                        fieldnode = parseStructDeclarator();                //the next field name
                        fieldlist.Add(fieldnode);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return fieldlist;
        }

        /*(6.7.2.1) 
         struct-declarator:
           declarator | ((declarator)? ':' constant-expression)
         */
        //a single field, named or anonymous, possibly followed by a field width (fx foo : 4;)
        public StructDeclaratorNode parseStructDeclarator()
        {
            StructDeclaratorNode node = null;
            DeclaratorNode declarnode = parseDeclarator(false);
            Token token = prep.getToken();
            ConstExpressionNode constexpr = null;
            if (token.type == TokenType.COLON)
            {
                constexpr = parseConstantExpression();
            }
            else
            {
                prep.putTokenBack(token);
            }
            if (declarnode != null || constexpr != null)
            {
                node = arbor.makeStructDeclaractorNode(declarnode, constexpr);
            }
            return node;
        }

        // enumerations ---------------------------------------------

        /*(6.7.2.2) 
         enum-specifier:
           'ENUM' ('identifier' | (('identifier')? '{' enumerator (',' enumerator)* (',')? '}'))
         */
        public EnumDeclNode parseEnumeratorSpec()
        {
            EnumDeclNode node = null;
            Token token = prep.getToken();
            if (token.type == TokenType.ENUM)
            {
                IdentNode idNode = null;
                token = prep.getToken();                        //either identifier or '{'
                if (token.type == TokenType.IDENT)
                {
                    idNode = arbor.getEnumIdentNode(token);
                    token = prep.getToken();                    //is identifier followed by '{'
                }
                if (token.type == TokenType.LBRACE)
                {
                    EnumeratorNode enumer = parseEnumerator();
                    node = arbor.startEnumSpec(idNode, enumer);
                    while (true)
                    {
                        token = prep.getToken();
                        if (token.type == TokenType.COMMA)
                        {
                            enumer = parseEnumerator();
                            if (enumer != null)                                 //could be optional trailing ','
                            {
                                node = arbor.makeEnumSpec(node, enumer);
                            }
                            continue;
                        }
                        prep.putTokenBack(token);
                        break;
                    }
                }
                else
                {
                    prep.putTokenBack(token);           //no enum definiton, just a ref
                    node = arbor.getEnumDecl(idNode);
                }
            }
            else
            {
                prep.putTokenBack(token);               //not enum declar or reference
            }
            return node;
        }

        /*(6.7.2.2) 
         enumerator:
           enumeration-constant ('=' constant-expression)?
         */
        public EnumeratorNode parseEnumerator()
        {
            EnumeratorNode node = null;
            Token enumid = prep.getToken();
            EnumConstantNode enumconst = arbor.getEnumerationConstant(enumid);
            if (enumconst != null)
            {
                ConstExpressionNode constexpr = null;
                Token token = prep.getToken();
                if (token.type == TokenType.EQUAL)
                {
                    constexpr = parseConstantExpression();
                }
                else
                {
                    prep.putTokenBack(token);
                }
                node = arbor.makeEnumeratorNode(enumconst, constexpr);
            }
            return node;
        }

        //- declarators -------------------------------------------------------

        /*
         (6.7.5) 
         declarator:
            (pointer)? direct-declarator

         (6.7.6) 
         abstract-declarator:
            pointer | ((pointer)? direct-abstract-declarator)

         (6.7.5) 
         pointer:
            ('*' (type-qualifier-list)?)+
        */
        public DeclaratorNode parseDeclarator(bool isAbstract)
        {
            if (nextTokenIs(TokenType.STAR))
            {
                consumeToken();
                TypeQualNode qualList = parseTypeQualList();
                DeclaratorNode declar = parseDeclarator(isAbstract);
                DeclaratorNode node = arbor.makePointerNode(qualList, declar);      //chain pointer node to front of declarator
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
            DeclaratorNode tail = null;
            Token token = prep.getToken();

            //identifier - if we see this, the declatator is no longer abstract
            if (token.type == TokenType.IDENT)
            {
                isAbstract = false;
                IdentDeclaratorNode idnode = arbor.makeIdentDeclaratorNode(token.strval);
                tail = parseDirectDeclaratorTail(isAbstract);
                idnode.next = tail;
                return idnode;
            }

            //in direct-abstract-declarator[opt] [...] if the direct-abstract-declarator is omitted, the first token
            //we see is the '[' of the declarator tail, so call parseDirectDeclaratorTail() with no base declarator
            if (isAbstract && (token.type == TokenType.LBRACKET))
            {
                node = parseDirectDeclaratorTail(isAbstract);
                return node;
            }

            //similarly, in direct-abstract-declarator[opt] ( parameter-type-list[opt] ), we see the '(' if the 
            //direct-abstract-declarator is omitted, BUT this also may be ( declarator ) or ( abstract-declarator )
            //so test for param list or '()' first and if not, then its a parenthesized declarator
            if (token.type == TokenType.LPAREN)
            {
                consumeToken();             //skip '('
                if (isAbstract)
                {
                    ParamListNode paramlist = parseParameterList(true);
                    DeclaratorNode funcDeclar = arbor.makeFuncDeclarNode(paramlist);
                    tail = parseDirectDeclaratorTail(isAbstract);
                    funcDeclar.next = tail;
                    return funcDeclar;
                }

                //( declarator ) or ( abstract-declarator )
                DeclaratorNode declar = parseDeclarator(isAbstract);
                consumeToken();                                                     //skip closing ')'
                tail = parseDirectDeclaratorTail(isAbstract);
                declar.next = tail;
                return declar;
            }

            prep.putTokenBack(token);
            return node;
        }

        //parse one or more declarator clauses recursively - either array indexes or parameter lists 
        public DeclaratorNode parseDirectDeclaratorTail(bool isAbstract)
        {
            DeclaratorNode tail = null;
            //array index declarator clause
            //mode 1: [ type-qualifier-list[opt] assignment-expression[opt] ]
            //mode 2: [ static type-qualifier-list[opt] assignment-expression ]
            //mode 3: [ type-qualifier-list static assignment-expression ]
            //mode 4: [ * ]
            if (nextTokenIs(TokenType.LBRACKET))
            {
                int mode = 1;
                TypeQualNode qualList = parseTypeQualList();
                ExprNode assign = null;
                if (nextTokenIs(TokenType.STATIC))
                {
                    consumeToken();
                    mode = 3;
                    if (!qualList.isEmpty)
                    {
                        qualList = parseTypeQualList();
                        mode = 2;
                    }
                }
                if ((mode == 1) && (nextTokenIs(TokenType.STAR)))
                {
                    consumeToken();
                    mode = 4;
                }
                else
                {
                    assign = parseAssignExpression();
                }
                if (nextTokenIs(TokenType.RBRACKET))
                {
                    consumeToken();
                }
                DeclaratorNode arrayidx = arbor.makeDirectIndexNode(mode, qualList, assign);
                tail = parseDirectDeclaratorTail(isAbstract);
                arrayidx.next = tail;
                return arrayidx;
            }

            //parameter list declarator clause
            //( parameter-type-list )      --- new style param list
            //( identifier-list[opt] )     --- old style param list 
            //( parameter-type-list[opt] ) --- if abstract
            else if (nextTokenIs(TokenType.LPAREN))
            {
                consumeToken();                                                     //skip opening '('
                ParamListNode paramlist = parseParameterList(isAbstract);
                tail  = parseDirectDeclaratorTail(isAbstract);
                paramlist.next = tail;
                return paramlist;
            }
            return tail;
        }

        /*(6.7.5) 
         type-qualifier-list:
            (type-qualifier)+
         */
        public TypeQualNode parseTypeQualList()
        {
            TypeQualNode quals = null;
            List<Token> typeQuals = new List<Token>();

            while (true)
            {
                Token token = prep.getToken();
                if ((token.type == TokenType.CONST) || (token.type == TokenType.RESTRICT) || (token.type == TokenType.VOLATILE))
                {
                    typeQuals.Add(token);
                    continue;
                }
                prep.putTokenBack(token);
                break;
            }
            if (typeQuals.Count > 0)
            {
                quals = arbor.makeTypeQualNode(typeQuals);
            }
            return quals;
        }

        /*(6.7.5) 
          parameter-type-list:            parameter-declaration (',' parameter-declaration)* (, ...)?
         */
        //parse (possibly empty) list of parameters, we've already seen the opening paren
        public ParamListNode parseParameterList(bool isAbstract)
        {
            ParamListNode paramList = null;
            List<ParamDeclNode> parameters = new List<ParamDeclNode>();
            while (true)
            {
                if (nextTokenIs(TokenType.RPAREN))                      //at end of param list
                {
                    consumeToken();                                     //skip ending ')'
                    paramList = arbor.makeParamList(parameters);
                    break;
                }

                ParamDeclNode param = parseParameterDeclar(isAbstract);
                parameters.Add(param);

                if (nextTokenIs(TokenType.COMMA))
                {
                    consumeToken();                                             //skip ','
                    if (nextTokenIs(TokenType.ELLIPSIS))                        //param, ...
                    {
                        consumeToken();                                         //skip '...'
                        ParamDeclNode ellipsis = new ParamDeclNode("...", null);
                        //paramList = arbor.makeParamList(paramList, param);
                        parameters.Add(ellipsis);
                    }
                }
            }
            return paramList;
        }

        /*(6.7.5) 
         parameter-declaration:
            declaration-specifiers (declarator | (abstract-declarator)?)
         */
        public ParamDeclNode parseParameterDeclar(bool isAbstract)
        {
            ParamDeclNode node = null;
            DeclSpecNode declarspecs = parseDeclarationSpecs(true, true);
            if (declarspecs != null)
            {
                DeclaratorNode declar = parseDeclarator(true);
                node = arbor.makeParamDeclarNode(declarspecs, declar);
            }
            return node;
        }

        /*(6.7.5) 
         identifier-list:
           'identifier' (',' 'identifier')*
         */
        public List<IdentNode> parseIdentifierList()
        {
            List<IdentNode> list = null;
            Token token = prep.getToken();
            IdentNode id = arbor.getArgIdentNode(token);
            if (id != null)
            {
                list = new List<IdentNode>();
                list.Add(id);
                while (true)
                {
                    token = prep.getToken();
                    if (token.type == TokenType.COMMA)
                    {
                        token = prep.getToken();
                        id = arbor.getArgIdentNode(token);
                        list.Add(id);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            else
            {
                prep.putTokenBack(token);
            }
            return list;
        }

        /*(6.7.6) 
         type-name:
            specifier-qualifier-list (abstract-declarator)?
         */
        public TypeNameNode parseTypeName()
        {
            TypeNameNode node = null;
            DeclSpecNode list = parseSpecQualList();
            if (list != null)
            {
                DeclaratorNode declar = parseDeclarator(true);
                node = arbor.makeTypeNameNode(list, declar);
            }
            return node;
        }

        //- declaration initializers ------------------------------------

        /*(6.7.8) 
         initializer:
           assignment-expression | ('{' initializer-list (',')? '}')
         */
        public InitializerNode parseInitializer()
        {
            InitializerNode node = null;
            ExprNode expr = parseAssignExpression();
            if (expr != null)
            {
                node = arbor.makeInitializerNode(expr);
            }
            else
            {
                Token token = prep.getToken();
                if (token.type == TokenType.LBRACE)
                {
                    List<InitializerNode> list = parseInitializerList();
                    token = prep.getToken();
                    if (token.type == TokenType.COMMA)
                    {
                        token = prep.getToken();            //skip optional ','
                    }
                    node = arbor.makeInitializerNode(list);
                }
                else
                {
                    prep.putTokenBack(token);
                }
            }
            return node;
        }

        /*(6.7.8) 
         initializer-list:
           (designation)? initializer (',' (designation)? initializer)*
         */
        public List<InitializerNode> parseInitializerList()
        {
            List<InitializerNode> list = null;
            DesignationNode desinode = parseDesignation();
            InitializerNode initnode = parseInitializer();
            if (initnode != null)
            {
                list = arbor.makeInitializerList(list, desinode, initnode);
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.COMMA)
                    {
                        desinode = parseDesignation();
                        initnode = parseInitializer();
                        list = arbor.makeInitializerList(list, desinode, initnode);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return list;
        }

        /*(6.7.8) 
         (('[' constant-expression ']') | ('.' 'identifier'))+ '='
         */
        public DesignationNode parseDesignation()
        {
            DesignationNode node = null;
            while (true)
            {
                Token token = prep.getToken();
                if (token.type == TokenType.LBRACKET)
                {
                    ConstExpressionNode expr = parseConstantExpression();
                    consumeToken();
                    node = arbor.makeDesignationNode(node, expr);              //[ constant-expression ]
                }
                if (token.type == TokenType.PERIOD)
                {
                    Token idtoken = prep.getToken();
                    IdentNode ident = arbor.getFieldIdentNode(idtoken);
                    node = arbor.makeDesignationNode(node, ident);             //. identifier
                }
                prep.putTokenBack(token);
                break;
            }
            if (node != null)
            {
                consumeToken();                 //skip ending '='
            }
            return node;
        }

        //- statements --------------------------------------------------------

        /* (6.8)
           statement:
             labeled-statement | compound-statement | expression-statement | selection-statement | iteration-statement | jump-statement
        */
        public StatementNode parseStatement()
        {
            StatementNode stmt = parseLabeledStatement();
            if (stmt == null)
            {
                stmt = parseCompoundStatement();
            }
            //if (node == null)
            //{
            //    node = parseExpressionStatement();
            //}
            if (stmt == null)
            {
                stmt = parseSelectionStatement();
            }
            if (stmt == null)
            {
                stmt = parseIterationStatement();
            }
            if (stmt == null)
            {
                stmt = parseJumpStatement();
            }
            return stmt;
        }

        /*(6.8.1) 
          labeled-statement:
            ('identifier' ':' statement) | 
            ('CASE' constant-expression ':' statement) | 
            ('DEFAULT' ':' statement)
         */
        public StatementNode parseLabeledStatement()
        {
            StatementNode stmt = null;
            if (nextTokenIs(TokenType.IDENT))                       //identifier : statement
            {
                Token labelid = prep.getToken();
                if (nextTokenIs(TokenType.COLON))
                {
                    consumeToken();
                    StatementNode labelstmt = parseStatement();
                    stmt = arbor.makeLabelStatementNode(labelid, labelstmt);
                    return stmt;
                }
            }
            if (nextTokenIs(TokenType.CASE))                        //case constant-expression : statement
            {
                consumeToken();
                ExprNode expr = parseConstantExpression();
                consumeToken();                                     //skip ':'
                StatementNode casestmt = parseStatement();
                stmt = arbor.makeCaseStatementNode(expr, casestmt);
                return stmt;
            }
            if (nextTokenIs(TokenType.DEFAULT))                     //default : statement
            {
                consumeToken();                                     //skip 'default';
                consumeToken();                                     //skip ':'
                StatementNode defstmt = parseStatement();
                stmt = arbor.makeDefaultStatementNode(defstmt);
                return stmt;
            }
            return stmt;
        }

        /*(6.8.2) 
         compound-statement:
           '{' (declaration | statement)* '}'         
        */
        public CompoundStatementNode parseCompoundStatement()
        {
            CompoundStatementNode comp = null;
            if (nextTokenIs(TokenType.LBRACE))
            {
                consumeToken();                                 //skip opening '{'
                comp = new CompoundStatementNode();             //compound stmt may be empty
                while (true)
                {
                    Declaration decl = parseDeclaration();
                    if (decl != null)
                    {
                        comp = arbor.makeCompoundStatementNode(comp, decl);
                        continue;
                    }
                    StatementNode stmt = parseStatement();
                    if (stmt != null)
                    {
                        comp = arbor.makeCompoundStatementNode(comp, stmt);
                        continue;
                    }
                    break;
                }
                consumeToken();                                 //skip closing '}'
            }
            return comp;
        }

        /*(6.8.3) 
         expression-statement:
           (expression)? ;
         */
        public StatementNode parseExpressionStatement()
        {
            StatementNode stmt = null;
            ExprNode expr = parseExpression();
            if (expr != null)
            {
                consumeToken();                                     //skip ending ';'
                stmt = arbor.makeExpressionStatementNode(expr);
            }
            else if (nextTokenIs(TokenType.SEMICOLON))
            {
                consumeToken();                                     //skip ending ';'
                stmt = arbor.makeEmptyStatementNode();
            }
            return stmt;
        }

        /*(6.8.4) 
         selection-statement:
           ('IF' '(' expression ')' statement ('ELSE' statement)?) | 
           ('SWITCH' '(' expression ')' statement)
         */
        public StatementNode parseSelectionStatement()
        {
            StatementNode stmt = null;
            if (nextTokenIs(TokenType.IF))                      //if ( expression ) statement
            {
                consumeToken();                                 //skip 'if'
                consumeToken();                                 //skip opening '('
                ExprNode ifexpr = parseExpression();
                consumeToken();                                 //skip closing ')'
                StatementNode thenstmt = parseStatement();
                StatementNode elsestmt = null;
                consumeToken();
                if (nextTokenIs(TokenType.ELSE))                //if ( expression ) statement else statement
                {
                    consumeToken();                             //skip 'else'
                    elsestmt = parseStatement();
                }
                stmt = arbor.makeIfStatementNode(ifexpr, thenstmt, elsestmt);
            }
            else if (nextTokenIs(TokenType.SWITCH))             //switch ( expression ) statement
            {
                consumeToken();                                 //skip 'switch'
                consumeToken();                                 //skip opening '('
                ExprNode expr = parseExpression();
                consumeToken();                                 //skip closing ')'
                StatementNode swstmt = parseStatement();
                stmt = arbor.makeSwitchStatementNode(expr, swstmt);
            }
            return stmt;
        }

        /*(6.8.5) 
         iteration-statement:
           ('WHILE' '(' expression ')' statement) | 
           ('DO' statement 'WHILE' '(' expression ')' ';') | 
           ('FOR' '(' (declaration | ((expression)? ';')) (expression)? ';' (expression)? ')' statement)
         */
        public StatementNode parseIterationStatement()
        {
            StatementNode stmt = null;
            if (nextTokenIs(TokenType.WHILE))                   //while ( expression ) statement
            {
                consumeToken();                                 //skip 'while'
                consumeToken();                                 //skip opening '('
                ExprNode expr = parseExpression();
                consumeToken();                                 //skip opening ')'
                StatementNode body = parseStatement();
                stmt = arbor.makeWhileStatementNode(expr, body);
            }
            else if (nextTokenIs(TokenType.DO))                 //do statement while ( expression ) ;
            {
                consumeToken();                                 //skip 'do'
                StatementNode body = parseStatement();
                consumeToken();                                 //skip 'WHILE'
                consumeToken();                                 //skip opening '('
                ExprNode expr = parseExpression();
                consumeToken();                                 //skip closing ')'
                consumeToken();                                 //skip ';'
                stmt = arbor.makeDoStatementNode(body, expr);
            }
            else if (nextTokenIs(TokenType.FOR))                //for ( expression[opt] ; expression[opt] ; expression[opt] ) statement                                                        
            {
                consumeToken();                                 //skip 'for'
                consumeToken();                                 //skip opening '('
                ExprNode expr1 = parseExpression();
                Declaration decl = null;
                if (expr1 == null)                              //for ( declaration expression[opt] ; expression[opt] ) statement
                {
                    decl = parseDeclaration();
                }
                else
                {
                    consumeToken();                                 //skip ';'
                }
                ExprNode expr2 = parseExpression();
                consumeToken();                                 //skip ';'
                ExprNode expr3 = parseExpression();
                consumeToken();                                 //skip closing ')'
                StatementNode body = parseStatement();
                stmt = (expr1 != null) ? arbor.makeForStatementNode(expr1, expr2, expr3, body) :
                                         arbor.makeForStatementNode(decl, expr2, expr3, body);
            }
            return stmt;
        }

        /*(6.8.6) 
         jump-statement:
           (('GOTO' 'identifier') | 'CONTINUE' | 'BREAK' | ('RETURN' (expression)?)) ;
         */
        public StatementNode parseJumpStatement()
        {
            StatementNode stmt = null;
            if (nextTokenIs(TokenType.GOTO))                //goto identifier ;
            {
                consumeToken();                             //skip 'goto'
                Token labelid = prep.getToken();
                stmt = arbor.makeGotoStatementNode(labelid);
            }
            else if (nextTokenIs(TokenType.CONTINUE))       //continue ;
            {
                consumeToken();                             //skip 'continue'
                stmt = arbor.makeContinueStatementNode();
            }
            else if (nextTokenIs(TokenType.BREAK))          //break ;
            {
                consumeToken();                             //skip 'break'
                stmt = arbor.makeBreakStatementNode();
            }
            else if (nextTokenIs(TokenType.RETURN))         //return expression[opt] ;
            {
                consumeToken();                             //skip 'return'
                ExprNode expr = parseExpression();
                stmt = arbor.makeReturnStatementNode(expr);
            }
            if (stmt != null)
            {
                consumeToken();                                 //skip ending ';'
            }
            return stmt;
        }

        //- expressions -------------------------------------------------------

        /*(6.5.1) 
         primary-expression:
           'identifier' | 'constant' | 'string-literal' | ('(' expression ')')
         */
        public ExprNode parsePrimaryExpression()
        {
            ExprNode node = null;
            Token token = prep.getToken();
            if (token.type == TokenType.IDENT)
            {
                node = arbor.getExprIdentNode(token);
            }
            else if (token.type == TokenType.INTCONST)
            {
                node = arbor.makeIntegerConstantNode(token.intval);
            }
            else if (token.type == TokenType.FLOATCONST)
            {
                node = arbor.makeFloatConstantNode(token.floatval);
            }
            else if (token.type == TokenType.CHARCONST)
            {
                node = arbor.makeCharConstantNode(token);
            }
            else if (token.type == TokenType.STRINGCONST)
            {
                node = arbor.makeStringConstantNode(token);
            }
            else if (token.type == TokenType.LPAREN)            //( expression )
            {
                ExprNode expr = parseExpression();
                consumeToken();                                 //skip ')'
                node = arbor.makeSubexpressionNode(expr);
            }
            else
            {
                prep.putTokenBack(token);
            }
            return node;
        }

        /*(6.5.2) 
         postfix-expression:
           (primary-expression | ('(' type-name ')' '{' initializer-list (',')? '}')) 
         */
        public ExprNode parsePostfixExpression()
        {
            ExprNode expr = parsePrimaryExpression();         //primary-expression
            if (expr == null)
            {
                Token token = prep.getToken();           //( type-name ) { initializer-list }
                if (token.type == TokenType.LPAREN)
                {
                    TypeNameNode name = parseTypeName();
                    consumeToken();                                             //skip ')'
                    consumeToken();                                             //skip '{'
                    List<InitializerNode> initList = parseInitializerList();
                    token = prep.getToken();
                    if (token.type == TokenType.COMMA)             //the comma is optional
                    {
                        token = prep.getToken();
                    }
                    expr = arbor.makeTypeInitExprNode(expr);
                }
                else
                {
                    prep.putTokenBack(token);
                }
            }
            if (expr != null)
            {
                expr = parsePostfixExpressionTail(expr);
            }
            return expr;
        }

        /*
           (('[' expression ']') | ('(' (argument-expression-list)? ')') | ('.' 'identifier') | ('->' 'identifier') | '++' | '--')*
        */
        public ExprNode parsePostfixExpressionTail(ExprNode head)
        {
            while (true)
            {
                if (nextTokenIs(TokenType.LBRACKET))            //postfix-expression [ expression ]
                {
                    ExprNode expr = parseExpression();
                    consumeToken();
                    head = arbor.makeIndexExprNode(head, expr);
                    continue;
                }

                else if (nextTokenIs(TokenType.LPAREN))              //postfix-expression ( argument-expression-list[opt] )
                {
                    ExprNode argList = parseArgExpressionList();
                    consumeToken();
                    head = arbor.makeFuncCallExprNode(head, argList);
                    continue;
                }

                else if (nextTokenIs(TokenType.PERIOD))              //postfix-expression . identifier
                {
                    Token token = prep.getToken();
                    IdentNode idNode = arbor.getFieldIdentNode(token);
                    head = arbor.makeFieldExprNode(head, idNode);
                    continue;
                }

                else if (nextTokenIs(TokenType.ARROW))               //postfix-expression -> identifier
                {
                    Token idtoken = prep.getToken();
                    IdentNode idNode = arbor.getFieldIdentNode(idtoken);
                    head = arbor.makeRefFieldExprNode(head, idNode);
                    continue;
                }

                else if (nextTokenIs(TokenType.PLUSPLUS))            //postfix-expression ++
                {
                    head = arbor.makePostPlusPlusExprNode(head);
                    continue;
                }

                else if (nextTokenIs(TokenType.MINUSMINUS))          //postfix-expression --
                {
                    head = arbor.makePostMinusMinusExprNode(head);
                    continue;
                }
                break;
            }
            return head;
        }

        /*(6.5.2) 
         argument-expression-list:
           assignment-expression  (',' assignment-expression)*
         */
        public ExprNode parseArgExpressionList()
        {
            ExprNode list = parseAssignExpression();
            if (list != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.COMMA)
                    {
                        ExprNode expr = parseAssignExpression();
                        list = arbor.makeArgumentExprList(list, expr);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return list;
        }

        /*(6.5.3) 
         unary-expression:
           ('++'| '--'|'SIZEOF')* postfix-expression | (('&'|'*'|'+'|'-'|'~'|'!') cast-expression) | ('SIZEOF' '(' type-name ')')

            postfix-expression
            ++ unary-expression
            -- unary-expression
            unary-operator cast-expression
            sizeof unary-expression
            sizeof ( type-name )
         */
        public ExprNode parseUnaryExpression()
        {
            ExprNode node = node = parsePostfixExpression();            //postfix-expression
            if (node == null)
            {
                if (nextTokenIs(TokenType.PLUSPLUS))                    //++ unary-expression
                {
                    consumeToken();
                    ExprNode expr = parseUnaryExpression();
                    node = arbor.makePlusPlusExprNode(expr);
                }

                else if (nextTokenIs(TokenType.MINUSMINUS))             //-- unary-expression
                {
                    consumeToken();
                    ExprNode expr = parseUnaryExpression();
                    node = arbor.makeMinusMinusExprNode(expr);
                }

                //unary-operator cast-expression
                else if (nextTokenIs(TokenType.AMPERSAND))
                {
                    consumeToken();
                    ExprNode expr = parseCastExpression();
                    node = arbor.makeUnaryOperatorNode(expr, UnaryOperatorNode.OPERATOR.AMPERSAND);
                }

                else if (nextTokenIs(TokenType.STAR))
                {
                    consumeToken();
                    ExprNode expr = parseCastExpression();
                    node = arbor.makeUnaryOperatorNode(expr, UnaryOperatorNode.OPERATOR.STAR);
                }

                else if (nextTokenIs(TokenType.PLUS))
                {
                    consumeToken();
                    ExprNode expr = parseCastExpression();
                    node = arbor.makeUnaryOperatorNode(expr, UnaryOperatorNode.OPERATOR.PLUS);
                }

                else if (nextTokenIs(TokenType.MINUS))
                {
                    consumeToken();
                    ExprNode expr = parseCastExpression();
                    node = arbor.makeUnaryOperatorNode(expr, UnaryOperatorNode.OPERATOR.MINUS);
                }

                else if (nextTokenIs(TokenType.TILDE))
                {
                    consumeToken();
                    ExprNode expr = parseCastExpression();
                    node = arbor.makeUnaryOperatorNode(expr, UnaryOperatorNode.OPERATOR.TILDE);
                }

                else if (nextTokenIs(TokenType.EXCLAIM))
                {
                    consumeToken();
                    ExprNode expr = parseCastExpression();
                    node = arbor.makeUnaryOperatorNode(expr, UnaryOperatorNode.OPERATOR.EXCLAIM);
                }

                else if (nextTokenIs(TokenType.SIZEOF))
                {
                    if (nextTokenIs(TokenType.LPAREN))         //sizeof ( type-name )
                    {
                        consumeToken();
                        TypeNameNode name = parseTypeName();
                        consumeToken();
                        node = arbor.makeSizeofTypeExprNode(name);
                    }
                    else                                 //sizeof unary-expression                        
                    {
                        ExprNode expr = parseUnaryExpression();
                        node = arbor.makeSizeofUnaryExprNode(expr);
                    }
                }
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
            ExprNode nameList = null;
            Token token = prep.getToken();
            while (token.type == TokenType.LPAREN)
            {
                consumeToken();
                TypeNameNode name = parseTypeName();
                consumeToken();
                nameList = arbor.makeCastExprNode(nameList, name);
                token = prep.getToken();
            }
            prep.putTokenBack(token);
            ExprNode node = parseUnaryExpression();
            if (nameList != null)
            {
                node = arbor.makeCastExprNode(nameList, node);
            }
            return node;
        }

        /*(6.5.5) 
         multiplicative-expression:
           cast-expression ('*'|'/'|'%' cast-expression)*
         */
        public ExprNode parseMultExpression()
        {
            ExprNode lhs = parseCastExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.STAR)
                    {
                        ExprNode rhs = parseCastExpression();
                        lhs = arbor.makeMultiplyExprNode(lhs, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.SLASH)
                    {
                        ExprNode rhs = parseCastExpression();
                        lhs = arbor.makeDivideExprNode(lhs, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.PERCENT)
                    {
                        ExprNode rhs = parseCastExpression();
                        lhs = arbor.makeModuloExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.6) 
         additive-expression:
           multiplicative-expression ('+'|'-' multiplicative-expression)*
         */
        public ExprNode parseAddExpression()
        {
            ExprNode lhs = parseMultExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.PLUS)
                    {
                        ExprNode rhs = parseMultExpression();
                        lhs = arbor.makeAddExprNode(lhs, rhs);
                        continue;
                    }
                    if (token.type == TokenType.MINUS)
                    {
                        ExprNode rhs = parseMultExpression();
                        lhs = arbor.makeSubtractExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.7) 
         shift-expression:
           additive-expression ('<<'|'>>' additive-expression)*
         */
        public ExprNode parseShiftExpression()
        {
            ExprNode lhs = parseAddExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.LESSLESS)
                    {
                        ExprNode rhs = parseAddExpression();
                        lhs = arbor.makeShiftLeftExprNode(lhs, rhs);
                        continue;
                    }
                    if (token.type == TokenType.GTRGTR)
                    {
                        ExprNode rhs = parseAddExpression();
                        lhs = arbor.makeShiftRightExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.8) 
         relational-expression:
           shift-expression (('<'|'>'|'<='|'>=') shift-expression)*
         */
        public ExprNode parseRelationalExpression()
        {
            ExprNode lhs = parseShiftExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.LESSTHAN)
                    {
                        ExprNode rhs = parseShiftExpression();
                        lhs = arbor.makeLessThanExprNode(lhs, rhs);
                        continue;
                    }
                    if (token.type == TokenType.GTRTHAN)
                    {
                        ExprNode rhs = parseShiftExpression();
                        lhs = arbor.makeGreaterThanExprNode(lhs, rhs);
                        continue;
                    }
                    if (token.type == TokenType.LESSEQUAL)
                    {
                        ExprNode rhs = parseShiftExpression();
                        lhs = arbor.makeLessEqualExprNode(lhs, rhs);
                        continue;
                    }
                    if (token.type == TokenType.GTREQUAL)
                    {
                        ExprNode rhs = parseShiftExpression();
                        lhs = arbor.makeGreaterEqualExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.9) 
         equality-expression:
           relational-expression (('=='|'!=') relational-expression)*
         */
        public ExprNode parseEqualityExpression()
        {
            ExprNode lhs = parseRelationalExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.EQUALEQUAL)
                    {
                        ExprNode rhs = parseRelationalExpression();
                        lhs = arbor.makeEqualsExprNode(lhs, rhs);
                        continue;
                    }
                    if (token.type == TokenType.NOTEQUAL)
                    {
                        ExprNode rhs = parseRelationalExpression();
                        lhs = arbor.makeNotEqualsExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.10) 
         AND-expression:
           equality-expression ('&' equality-expression)*
         */
        public ExprNode parseANDExpression()
        {
            ExprNode lhs = parseEqualityExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.AMPERSAND)
                    {
                        ExprNode rhs = parseEqualityExpression();
                        lhs = arbor.makeANDExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.11) 
         exclusive-OR-expression:
           AND-expression ('^' AND-expression)*
         */
        public ExprNode parseXORExpression()
        {
            ExprNode lhs = parseANDExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.CARET)
                    {
                        ExprNode rhs = parseANDExpression();
                        lhs = arbor.makeXORExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.12) 
         inclusive-OR-expression:
           exclusive-OR-expression ('|' exclusive-OR-expression)*
         */
        public ExprNode parseORExpression()
        {
            ExprNode lhs = parseXORExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.BAR)
                    {
                        ExprNode rhs = parseXORExpression();
                        lhs = arbor.makeORExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.13) 
         logical-AND-expression:
           inclusive-OR-expression ('&&' inclusive-OR-expression)*
         */
        public ExprNode parseLogicalANDExpression()
        {
            ExprNode lhs = parseORExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.AMPAMP)
                    {
                        ExprNode rhs = parseORExpression();
                        lhs = arbor.makeLogicalANDExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.14) 
         logical-OR-expression:
           logical-AND-expression ('||' logical-AND-expression)*
         */
        public ExprNode parseLogicalORExpression()
        {
            ExprNode lhs = parseLogicalANDExpression();
            if (lhs != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.BARBAR)
                    {
                        ExprNode rhs = parseLogicalANDExpression();
                        lhs = arbor.makeLogicalORExprNode(lhs, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return lhs;
        }

        /*(6.5.15) 
         conditional-expression:
           logical-OR-expression ('?' expression ':' conditional-expression)?
         */
        public ExprNode parseConditionalExpression()
        {
            ExprNode lhs = parseLogicalORExpression();
            if (lhs != null)
            {
                if (nextTokenIs(TokenType.QUESTION))
                {
                    consumeToken();                                             //skip '?'
                    ExprNode expr = parseExpression();
                    consumeToken();                                             //skip ':'
                    ExprNode condit = parseConditionalExpression();
                    lhs = arbor.makeConditionalExprNode(lhs, expr, condit);
                }
            }
            return lhs;
        }

        //the last three productions in this section are referenced by productions in other sections
        //so they return specific node types instead of <ExprNode> which in internal to Expressions

        /*(6.5.16) 
         assignment-expression:
          (unary-expression ('='|'*='|'/='|'%='|'+='|'-='|'<<='|'>>='|'&='|'^='|'|='))* conditional-expression
         */
        public ExprNode parseAssignExpression()
        {
            ExprNode expr = parseConditionalExpression();
            if (expr != null)
            {
                while (true)
                {
                    Token token = prep.getToken();
                    if (token.type == TokenType.EQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.EQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.MULTEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.MULTEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.SLASHEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.SLASHEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.PERCENTEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.PERCENTEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.PLUSEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.PLUSEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.MINUSEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.MINUSEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.LESSLESSEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.LESSLESSEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.GTRGTREQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.GTRGTREQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.AMPEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.AMPEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.CARETEQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.CARETEQUAL, rhs);
                        continue;
                    }
                    else if (token.type == TokenType.BAREQUAL)
                    {
                        ExprNode rhs = parseConditionalExpression();
                        expr = arbor.makeAssignExpressionNode(expr, ASSIGNOP.BAREQUAL, rhs);
                        continue;
                    }
                    prep.putTokenBack(token);
                    break;
                }
            }
            return expr;
        }

        /*(6.5.17) 
          assignment-expression (',' assignment-expression)*
         */
        public ExprNode parseExpression()
        {
            ExprNode expr = parseAssignExpression();
            if (expr != null)
            {
                while (nextTokenIs(TokenType.COMMA))
                {
                    {
                        ExprNode rhs = parseAssignExpression();
                        expr = arbor.makeExpressionNode(expr, rhs);
                    }
                }
            }
            return expr;
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

    //-------------------------------------------------------------------------

    public class ParserException : Exception
    {
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");