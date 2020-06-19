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

using BlackC.Scan;

namespace BlackC
{
    public class ParseDeclar
    {
        //public Preprocessor prep;
        //public Arbor arbor;
        //public ParseExpr pexpr;

        //public ParseDeclar(Preprocessor _prep, Arbor _arbor)
        //{
        //    prep = _prep;
        //    arbor = _arbor;
        //    pexpr = null;
        //}

        ///* 
        // (6.7) 
        // declaration:
        //    declaration-specifiers init-declarator-list[opt] ;
         
        // (6.7) 
        // init-declarator-list:
        //    init-declarator
        //    init-declarator-list , init-declarator

        // (6.7) 
        // init-declarator:
        //    declarator
        //    declarator = initializer
        // */
        //public DeclarationNode parseDeclaration()
        //{
        //    DeclarationNode node = null;
        //    DeclarSpecNode declarspecs = parseDeclarationSpecs();

        //    //type definition, like struct foo {...}; or enum bar {...};
        //    if (prep.getToken().type == TokenType.SEMICOLON)
        //    {
        //        prep.next();
        //        node = arbor.makeTypeDefNode(declarspecs);
        //        node.isFuncDef = false;
        //        return node;
        //    }

        //    bool done = false;
        //    bool isFuncDef = true;
        //    while (!done)
        //    {
        //        DeclaratorNode declarnode = parseDeclarator(false);
        //        if (prep.getToken().type == TokenType.EQUAL)
        //        {
        //            prep.next();
        //            isFuncDef = false;
        //            InitializerNode initialnode = parseInitializer();
        //            node = arbor.makeDeclaration(declarspecs, declarnode, initialnode, node);       //declarator = initializer
        //        }
        //        else
        //        {
        //            node = arbor.makeDeclaration(declarspecs, declarnode, null, node);      //declarator
        //        }

        //        if (prep.getToken().type == TokenType.COMMA)
        //        {
        //            isFuncDef = false;
        //            prep.next();
        //        }
        //        else if (prep.getToken().type == TokenType.SEMICOLON)
        //        {
        //            isFuncDef = false;
        //            prep.next();
        //            done = true;
        //        }
        //    }
        //    node.isFuncDef = isFuncDef;
        //    return node;
        //}

        ///* (6.7) 
        // declaration-specifiers:
        //    storage-class-specifier declaration-specifiers[opt]
        //    type-specifier declaration-specifiers[opt]
        //    type-qualifier declaration-specifiers[opt]
        //    function-specifier declaration-specifiers[opt]      

        // (6.7.1) 
        // storage-class-specifier:
        //    typedef
        //    extern
        //    static
        //    auto
        //    register
         
        // (6.7.2) 
        // type-specifier:
        //    void
        //    char
        //    short
        //    int
        //    long
        //    float
        //    double
        //    signed
        //    unsigned
        //    _Bool
        //    _Complex
        //    struct-or-union-specifier
        //    enum-specifier
        //    typedef-name

        // (6.7.3) 
        // type-qualifier:
        //    const
        //    restrict
        //    volatile
         
        // (6.7.4) 
        // function-specifier:
        //    inline
        //*/
        //public DeclarSpecNode parseDeclarationSpecs()
        //{
        //    DeclarSpecNode specs = new DeclarSpecNode();
        //    bool done = false;
        //    while (!done)
        //    {
        //        Token token = prep.getToken();
        //        switch (token.type)
        //        {
        //            case TokenType.TYPEDEF:
        //            case TokenType.EXTERN:
        //            case TokenType.STATIC:
        //            case TokenType.AUTO:
        //            case TokenType.REGISTER:
        //                specs.setStorageClassSpec(token);
        //                prep.next();
        //                break;

        //            case TokenType.VOID:
        //            case TokenType.CHAR:
        //            case TokenType.INT:
        //            case TokenType.FLOAT:
        //            case TokenType.DOUBLE:
        //                specs.setBaseClassSpec(token);
        //                prep.next();
        //                break;

        //            case TokenType.SHORT:
        //            case TokenType.LONG:
        //            case TokenType.SIGNED:
        //            case TokenType.UNSIGNED:
        //                specs.setBaseClassModifier(token);
        //                prep.next();
        //                break;

        //            case TokenType.STRUCT:
        //            case TokenType.UNION:
        //                specs.typeSpec = parseStructOrUnionSpec();
        //                break;

        //            case TokenType.ENUM:
        //                specs.typeSpec = parseEnumeratorSpec();
        //                prep.next();
        //                break;

        //            case TokenType.IDENTIFIER:
        //                TypeSpecNode ts = parseTypedefName();
        //                if (ts != null)
        //                {
        //                    specs.typeSpec = ts;
        //                }
        //                else
        //                {
        //                    done = true;
        //                }
        //                break;

        //            case TokenType.CONST:
        //            case TokenType.RESTRICT:
        //            case TokenType.VOLATILE:
        //                specs.setTypeQual(token);
        //                prep.next();
        //                break;

        //            case TokenType.INLINE:
        //                specs.setFunctionSpec(token);
        //                prep.next();
        //                break;

        //            default:
        //                done = true;
        //                break;
        //        }
        //    }
        //    specs.complete();
        //    return specs;
        //}

        ///*(6.7.7) 
        // typedef-name:
        //    identifier
        //*/
        //public TypeSpecNode parseTypedefName()
        //{
        //    Token token = prep.getToken();
        //    IdentNode tdnode = arbor.findIdent(token);
        //    if ((tdnode != null) && (tdnode.def != null) && (tdnode.def is TypeSpecNode))
        //    {
        //        prep.next();
        //        return (TypeSpecNode)tdnode.def;
        //    }
        //    return null;
        //}

        //// stuctures/unions -----------------------------------------

        ///*(6.7.2.1) 
        // struct-or-union-specifier:
        //    struct-or-union identifier[opt] { struct-declaration-list }
        //    struct-or-union identifier

        //*/
        //// struct w/o ident is for anonymous struct (possibly part of a typedef)
        //// struct w/o {list} is for a already defined struct type
        //public StructSpecNode parseStructOrUnionSpec()
        //{
        //    Token token = prep.getToken();
        //    StructSpecNode node = null;
        //    int cuepoint = prep.record();
        //    StructUnionNode tag = parseStuctOrUnion();
        //    bool result = (tag != null);
        //    if (result)
        //    {
        //        //Token token = prep.getToken();
        //        IdentNode name = arbor.getStructIdentNode(token);
        //        result = (name != null);
        //        if (result)
        //        {
        //            int cuepoint2 = prep.record();
        //            token = prep.getToken();
        //            bool result2 = (token.type == TokenType.LBRACE);
        //            if (!result2)
        //            {
        //                node = arbor.makeStructSpec(tag, name, null);       //struct-or-union ident
        //            }
        //            if (result2)
        //            {
        //                List<StructDeclarationNode> declarList = parseStructDeclarationList();
        //                result2 = (declarList != null);
        //                if (result2)
        //                {
        //                    token = prep.getToken();
        //                    result2 = (token.type == TokenType.RBRACE);
        //                    if (result2)
        //                    {
        //                        node = arbor.makeStructSpec(tag, name, declarList);         //struct-or-union ident struct-declar-list
        //                    }
        //                }
        //            }
        //            if (!result2)
        //            {
        //                prep.rewind(cuepoint2);
        //            }
        //        }
        //        else
        //        {
        //            result = (token.type == TokenType.LBRACE);
        //            if (result)
        //            {
        //                List<StructDeclarationNode> declarList = parseStructDeclarationList();
        //                result = (declarList != null);
        //                if (result)
        //                {
        //                    token = prep.getToken();
        //                    result = (token.type == TokenType.RBRACE);
        //                    if (result)
        //                    {
        //                        node = arbor.makeStructSpec(tag, null, declarList);         //struct-or-union struct-declar-list
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ///*(6.7.2.1) 
        // struct-or-union:
        //    struct
        //    union
        //*/
        //public StructUnionNode parseStuctOrUnion()
        //{
        //    StructUnionNode node = null;
        //    int cuepoint = prep.record();
        //    Token token = prep.getToken();
        //    switch (token.ToString())
        //    {
        //        case "STRUCT":
        //            node = new StructUnionNode(StructUnionNode.LAYOUT.STRUCT);
        //            break;

        //        case "UNION":
        //            node = new StructUnionNode(StructUnionNode.LAYOUT.UNION);
        //            break;
        //    }
        //    if (node == null)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ///*(6.7.2.1) 
        // struct-declaration-list:
        //    struct-declaration
        //    struct-declaration-list struct-declaration
        // */
        //// the list of struct field defs
        //public List<StructDeclarationNode> parseStructDeclarationList()
        //{
        //    List<StructDeclarationNode> fieldlist = null;
        //    StructDeclarationNode fieldnode = parseStructDeclaration();         //the first field def
        //    if (fieldnode != null)
        //    {
        //        fieldlist = new List<StructDeclarationNode>();
        //        fieldlist.Add(fieldnode);
        //    }
        //    while (fieldnode != null)
        //    {
        //        fieldnode = parseStructDeclaration();          //the next field def
        //        if (fieldnode != null)
        //        {
        //            fieldlist.Add(fieldnode);
        //        }
        //    }
        //    return fieldlist;
        //}

        ///*(6.7.2.1) 
        // struct-declaration:
        //    specifier-qualifier-list struct-declarator-list ;
        // */
        //// a single struct field def (can have mult fields, ie int a, b;)
        //public StructDeclarationNode parseStructDeclaration()
        //{
        //    StructDeclarationNode node = null;
        //    int cuepoint = prep.record();
        //    List<DeclarSpecNode> specqual = parseSpecQualList();          //field type
        //    bool result = (specqual != null);
        //    if (result)
        //    {
        //        List<StructDeclaratorNode> fieldnames = parseStructDeclaratorList();           //list of field names 
        //        result = (fieldnames != null);
        //        if (result)
        //        {
        //            Token token = prep.getToken();
        //            result = (token.type == TokenType.SEMICOLON);
        //            if (result)
        //            {
        //                node = arbor.makeStructDeclarationNode(specqual, fieldnames);
        //            }
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ///*(6.7.2.1) 
        // specifier-qualifier-list:
        //    type-specifier specifier-qualifier-list[opt]
        //    type-qualifier specifier-qualifier-list[opt]
        // */
        //// struct field's type - same as declaration-specifiers, w/o the storage-class-specifier or function-specifier
        //public List<DeclarSpecNode> parseSpecQualList()
        //{
        //    List<DeclarSpecNode> speclist = null;
        //    //DeclarSpecNode specnode = parseTypeSpec();
        //    //if (specnode == null)
        //    //{
        //    //    specnode = parseTypeQual();
        //    //}
        //    //if (specnode != null)
        //    //{
        //    //    speclist = new List<DeclarSpecNode>();
        //    //    speclist.Add(specnode);
        //    //    List<DeclarSpecNode> taillist = parseSpecQualList();
        //    //    if (taillist != null)
        //    //    {
        //    //        speclist.AddRange(taillist);
        //    //    }
        //    //}
        //    return speclist;
        //}

        ///*(6.7.2.1) 
        // struct-declarator-list:
        //    struct-declarator
        //    struct-declarator-list , struct-declarator
        // */
        //// the list of field names, fx the "a, b, c" in "int a, b, c;" that def's three fields of type int
        //public List<StructDeclaratorNode> parseStructDeclaratorList()
        //{
        //    List<StructDeclaratorNode> fieldlist = null;
        //    StructDeclaratorNode fieldnode = parseStructDeclarator();      //the first field name
        //    bool result = (fieldnode != null);
        //    if (result)
        //    {
        //        fieldlist = new List<StructDeclaratorNode>();
        //        fieldlist.Add(fieldnode);
        //    }
        //    while (result)
        //    {
        //        int cuepoint2 = prep.record();
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.COMMA);
        //        if (result)
        //        {
        //            fieldnode = parseStructDeclarator();       //the next field name
        //            result = (fieldnode != null);
        //            if (result)
        //            {
        //                fieldlist.Add(fieldnode);
        //            }
        //        }
        //        if (!result)
        //        {
        //            prep.rewind(cuepoint2);
        //        }
        //    }
        //    return fieldlist;
        //}

        ///*(6.7.2.1) 
        // struct-declarator:
        //    declarator
        //    declarator[opt] : constant-expression
        // */
        ////a single field name, possibly followed by a field width (fx foo : 4;)
        //public StructDeclaratorNode parseStructDeclarator()
        //{
        //    StructDeclaratorNode node = null;
        //    int cuepoint = prep.record();
        //    DeclaratorNode declarnode = parseDeclarator(false);
        //    bool result = (declarnode != null);
        //    if (result)
        //    {
        //        int cuepoint2 = prep.record();
        //        Token token = prep.getToken();
        //        bool result2 = (token.type == TokenType.COLON);
        //        if (result2)
        //        {
        //            ConstExpressionNode constexpr = pexpr.parseConstantExpression();
        //            result2 = (constexpr != null);
        //            if (result2)
        //            {
        //                node = arbor.makeStructDeclaractorNode(declarnode, constexpr);      //declarator : constant-expression
        //            }
        //        }
        //        if (!result2)
        //        {
        //            node = arbor.makeStructDeclaractorNode(declarnode, null);       //declarator
        //        }
        //        if (!result2)
        //        {
        //            prep.rewind(cuepoint2);
        //        }
        //    }
        //    if (!result)
        //    {
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.COLON);
        //        if (result)
        //        {
        //            ConstExpressionNode constexpr = pexpr.parseConstantExpression();
        //            result = (constexpr != null);
        //            if (result)
        //            {
        //                //Console.WriteLine("parsed const-exp struct-declar");
        //                node = arbor.makeStructDeclaractorNode(null, constexpr);      // : constant-expression
        //            }
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        //// enumerations ---------------------------------------------

        ///*(6.7.2.2) 
        // enum-specifier:
        //    enum identifier[opt] { enumerator-list }
        //    enum identifier[opt] { enumerator-list , }
        //    enum identifier
        // */
        //public EnumSpecNode parseEnumeratorSpec()
        //{
        //    EnumSpecNode node = null;
        //    int cuepoint = prep.record();
        //    Token token = prep.getToken();
        //    bool result = (token.type == TokenType.ENUM);
        //    if (result)
        //    {
        //        token = prep.getToken();
        //        IdentNode idNode = arbor.getEnumIdentNode(token);
        //        result = (idNode != null);
        //        if (result)
        //        {
        //            int cuepoint2 = prep.record();
        //            token = prep.getToken();
        //            bool result2 = (token.type == TokenType.LBRACE);             //enum identifier { enumerator-list }
        //            if (result2)
        //            {
        //                List<EnumeratorNode> enumList = parseEnumeratorList();
        //                result2 = (enumList != null);
        //                if (result2)
        //                {
        //                    token = prep.getToken();
        //                    result2 = (token.type == TokenType.RBRACE);
        //                    if (!result2)
        //                    {
        //                        result2 = (token.type == TokenType.COMMA);            //enum identifier { enumerator-list , }
        //                        if (result2)
        //                        {
        //                            token = prep.getToken();
        //                            result2 = (token.type == TokenType.RBRACE);
        //                        }
        //                    }
        //                    if (result2)
        //                    {
        //                        node = arbor.makeEnumSpec(idNode, enumList);
        //                    }
        //                }
        //            }
        //            if (!result2)
        //            {
        //                node = arbor.makeEnumSpec(idNode, null);        //enum identifier
        //            }
        //            if (!result2)
        //            {
        //                prep.rewind(cuepoint2);
        //            }
        //        }
        //        else
        //        {
        //            token = prep.getToken();
        //            result = (token.type == TokenType.LBRACE);             //enum { enumerator-list }
        //            if (result)
        //            {
        //                List<EnumeratorNode> enumList = parseEnumeratorList();
        //                result = (enumList != null);
        //                if (result)
        //                {
        //                    token = prep.getToken();
        //                    result = (token.type == TokenType.RBRACE);
        //                    if (!result)
        //                    {
        //                        result = (token.type == TokenType.COMMA);            //enum { enumerator-list , }
        //                        if (result)
        //                        {
        //                            token = prep.getToken();
        //                            result = (token.type == TokenType.RBRACE);
        //                        }
        //                    }
        //                    if (result)
        //                    {
        //                        node = arbor.makeEnumSpec(null, enumList);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ///*(6.7.2.2) 
        // enumerator-list:
        //    enumerator
        //    enumerator-list , enumerator
        // */
        //public List<EnumeratorNode> parseEnumeratorList()
        //{
        //    List<EnumeratorNode> enumlistnode = null;
        //    EnumeratorNode enumnode = parseEnumerator();
        //    bool result = (enumnode != null);
        //    if (result)
        //    {
        //        enumlistnode = new List<EnumeratorNode>();
        //        enumlistnode.Add(enumnode);
        //    }
        //    while (result)
        //    {
        //        int cuepoint2 = prep.record();
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.COMMA);
        //        if (result)
        //        {
        //            enumnode = parseEnumerator();
        //            result = (enumnode != null);
        //            if (result)
        //            {
        //                enumlistnode.Add(enumnode);
        //            }
        //        }
        //        if (!result)
        //        {
        //            prep.rewind(cuepoint2);
        //        }
        //    }
        //    return enumlistnode;
        //}

        ///*(6.7.2.2) 
        // enumerator:
        //    enumeration-constant
        //    enumeration-constant = constant-expression
        // */
        //public EnumeratorNode parseEnumerator()
        //{
        //    EnumeratorNode node = null;
        //    EnumConstantNode enumconst = parseEnumerationConstant();
        //    ConstExpressionNode constexpr = null;
        //    bool result = (enumconst != null);
        //    if (result)
        //    {
        //        int cuepoint = prep.record();
        //        Token token = prep.getToken();
        //        bool result2 = (token.type == TokenType.EQUAL);
        //        if (result2)
        //        {
        //            constexpr = pexpr.parseConstantExpression();
        //            result2 = (constexpr != null);
        //        }
        //        if (!result2)
        //        {
        //            prep.rewind(cuepoint);
        //        }
        //        node = arbor.makeEnumeratorNode(enumconst, constexpr);
        //    }
        //    return node;
        //}

        ///*(6.4.4.3) 
        // enumeration-constant:
        //    identifier
        // */
        //public EnumConstantNode parseEnumerationConstant()
        //{
        //    EnumConstantNode node = null;
        //    int cuepoint = prep.record();
        //    Token token = prep.getToken();
        //    if (token != null)
        //    {
        //        node = arbor.makeEnumConstNode(token);
        //    }
        //    if (node == null)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ////- declarators -------------------------------------------------------

        ///*
        // (6.7.5) 
        // declarator:
        //    pointer[opt] direct-declarator
         
        // (6.7.5) 
        // pointer:
        //    * type-qualifier-list[opt]
        //    * type-qualifier-list[opt] pointer         

        // (6.7.6) 
        // abstract-declarator:
        //    pointer
        //    pointer[opt] direct-abstract-declarator
        //*/
        //public DeclaratorNode parseDeclarator(bool isAbstract)
        //{
        //    if (prep.getToken().type == TokenType.STAR)
        //    {
        //        prep.next();
        //        TypeQualNode qualList = parseTypeQualList();
        //        DeclaratorNode declar = parseDeclarator(isAbstract);
        //        DeclaratorNode node = arbor.makePointerNode(qualList, declar);
        //        return node;
        //    }
        //    return parseDirectDeclarator(isAbstract);
        //}

        ///*(6.7.5) 
        // direct-declarator:
        //    identifier
        //    ( declarator )
        //    direct-declarator [ type-qualifier-list[opt] assignment-expression[opt] ]
        //    direct-declarator [ static type-qualifier-list[opt] assignment-expression ]
        //    direct-declarator [ type-qualifier-list static assignment-expression ]
        //    direct-declarator [ type-qualifier-list[opt] * ]
        //    direct-declarator ( parameter-type-list )
        //    direct-declarator ( identifier-list[opt] )
          
        // (6.7.6) 
        // direct-abstract-declarator:
        //    ( abstract-declarator )
        //    direct-abstract-declarator[opt] [ type-qualifier-list[opt] assignment-expression[opt] ]
        //    direct-abstract-declarator[opt] [ static type-qualifier-list[opt] assignment-expression ]
        //    direct-abstract-declarator[opt] [ type-qualifier-list static assignment-expression ]
        //    direct-abstract-declarator[opt] [ * ]
        //    direct-abstract-declarator[opt] ( parameter-type-list[opt] )
        // */
        ////this handles the base cases of both direct-declarator and direct-abstract-declarator
        ////the trailing clauses are handled in <parseDirectDeclaratorTail>
        //public DeclaratorNode parseDirectDeclarator(bool isAbstract)
        //{
        //    DeclaratorNode node = null;
        //    Token token = prep.getToken();

        //    //identifier
        //    if (!isAbstract && (token.type == TokenType.IDENTIFIER))
        //    {
        //        prep.next();
        //        IdentDeclaratorNode idnode = new IdentDeclaratorNode(token);
        //        node = parseDirectDeclaratorTail(idnode, isAbstract);
        //        return node;
        //    }

        //    //in direct-abstract-declarator[opt] [...] if the direct-abstract-declarator is omitted, the first token
        //    //we see is the '[' of the declarator tail, so call parseDirectDeclaratorTail() with no base declarator
        //    if (isAbstract && (token.type == TokenType.LBRACKET))
        //    {
        //        node = parseDirectDeclaratorTail(null, isAbstract);
        //        return node;
        //    }

        //    //similarly, in direct-abstract-declarator[opt] ( parameter-type-list[opt] ), we see the '(' if the 
        //    //direct-abstract-declarator is omitted, BUT this also may be ( declarator ) or ( abstract-declarator )
        //    //so test for param list or '()' first and if not, then its a parenthesized declarator
        //    if (token.type == TokenType.LPAREN)
        //    {
        //        prep.next();
        //        if (isAbstract)
        //        {
        //            ParamTypeListNode paramlist = parseParameterTypeList();
        //            if ((paramlist != null) || (prep.getToken().type == TokenType.RPAREN))
        //            {
        //                DeclaratorNode funcDeclar = arbor.makeFuncDeclarNode(null, paramlist);
        //                node = parseDirectDeclaratorTail(funcDeclar, isAbstract);
        //                return node;
        //            }
        //        }

        //        //( declarator ) or ( abstract-declarator )
        //        DeclaratorNode declar = parseDeclarator(isAbstract);
        //        if (prep.getToken().type == TokenType.RPAREN)
        //        {
        //            prep.next();
        //            node = parseDirectDeclaratorTail(declar, isAbstract);
        //            return node;
        //        }
        //    }

        //    return node;
        //}

        ////parse one or more declarator clauses recursively
        //public DeclaratorNode parseDirectDeclaratorTail(DeclaratorNode head, bool isAbstract)
        //{
        //    DeclaratorNode node = null;
        //    Token token = prep.getToken();

        //    //array index declarator clause
        //    //mode 1: [ type-qualifier-list[opt] assignment-expression[opt] ]
        //    //mode 2: [ static type-qualifier-list[opt] assignment-expression ]
        //    //mode 3: [ type-qualifier-list static assignment-expression ]
        //    //mode 4: [ * ]
        //    if (prep.getToken().type == TokenType.LBRACKET)
        //    {
        //        int mode = 1;
        //        TypeQualNode qualList = parseTypeQualList();
        //        AssignExpressionNode assign = null;
        //        bool isStatic = (prep.getToken().type == TokenType.STATIC);
        //        if (isStatic)
        //        {
        //            prep.next();
        //            mode = 3;
        //            if (qualList.isEmpty)
        //            {
        //                qualList = parseTypeQualList();
        //                mode = 2;
        //            }
        //        }
        //        if ((mode == 1) && (prep.getToken().type == TokenType.STAR))
        //        {
        //            prep.next();
        //            mode = 4;
        //        }
        //        else
        //        {
        //            assign = pexpr.parseAssignExpression();
        //        }
        //        if (prep.getToken().type == TokenType.RBRACKET)
        //        {
        //            prep.next();
        //        }
        //        DeclaratorNode index = arbor.makeDirectIndexNode(head, mode, qualList, assign);
        //        node = parseDirectDeclaratorTail(index, isAbstract);
        //        return node;
        //    }

        //    //parameter list declarator clause
        //    else if (prep.getToken().type == TokenType.LPAREN)
        //    {
        //        ParamTypeListNode paramlist = parseParameterTypeList();
        //        DeclaratorNode funcDeclar = arbor.makeFuncDeclarNode(head, paramlist);
        //        node = parseDirectDeclaratorTail(funcDeclar, isAbstract);
        //        return node;
        //    }
        //    return head;
        //}

        ///*(6.7.5) 
        // type-qualifier-list:
        //    type-qualifier
        //    type-qualifier-list type-qualifier
        // */
        //public TypeQualNode parseTypeQualList()
        //{
        //    TypeQualNode specs = new TypeQualNode();
        //    bool done = false;
        //    while (!done)
        //    {
        //        Token token = prep.getToken();
        //        switch (token.type)
        //        {
        //            case TokenType.CONST:
        //            case TokenType.RESTRICT:
        //            case TokenType.VOLATILE:
        //                specs.setQualifer(token);
        //                prep.next();
        //                break;

        //            default:
        //                done = true;
        //                break;
        //        }
        //    }
        //    return specs;
        //}

        ///*(6.7.5) 
        // parameter-type-list:
        //    parameter-list
        //    parameter-list , ...
        // */
        //public ParamTypeListNode parseParameterTypeList()
        //{
        //    ParamTypeListNode node = null;
        //    List<ParamDeclarNode> list = parseParameterList();
        //    bool result = (list != null);
        //    if (result)
        //    {
        //        int cuepoint = prep.record();
        //        Token token = prep.getToken();
        //        bool result2 = (token.type == TokenType.COMMA);
        //        if (result2)
        //        {
        //            token = prep.getToken();
        //            result2 = (token.type == TokenType.ELLIPSIS);
        //            if (result2)
        //            {
        //                node = arbor.ParamTypeListNode(list, true);
        //            }
        //        }
        //        else
        //        {
        //            node = arbor.ParamTypeListNode(list, false);
        //        }
        //        if (!result2)
        //        {
        //            prep.rewind(cuepoint);
        //        }
        //    }
        //    return node;
        //}

        ///*(6.7.5) 
        // parameter-list:
        //    parameter-declaration
        //    parameter-list , parameter-declaration
        // */
        //public List<ParamDeclarNode> parseParameterList()
        //{
        //    List<ParamDeclarNode> list = null;
        //    ParamDeclarNode param = parseParameterDeclar();
        //    bool result = (param != null);
        //    if (result)
        //    {
        //        list = new List<ParamDeclarNode>();
        //        list.Add(param);
        //    }
        //    bool notEmpty = result;
        //    while (result)
        //    {
        //        int cuepoint2 = prep.record();
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.COMMA);
        //        if (result)
        //        {
        //            param = parseParameterDeclar();
        //            result = (param != null);
        //            if (result)
        //            {
        //                list.Add(param);
        //            }
        //        }
        //        if (!result)
        //        {
        //            prep.rewind(cuepoint2);
        //        }
        //    }
        //    return list;
        //}

        ///*(6.7.5) 
        // parameter-declaration:
        //    declaration-specifiers declarator
        //    declaration-specifiers abstract-declarator[opt]
        // */
        //public ParamDeclarNode parseParameterDeclar()
        //{
        //    ParamDeclarNode node = null;
        //    DeclaratorNode absdeclar = null;
        //    int cuepoint = prep.record();
        //    DeclarSpecNode declarspecs = parseDeclarationSpecs();
        //    bool result = (declarspecs != null);
        //    if (result)
        //    {
        //        DeclaratorNode declar = parseDeclarator(false);
        //        bool result2 = (declar != null);
        //        if (result2)
        //        {
        //            //node = arbor.makeParamDeclarNode(declarspecs, declar, absdeclar);
        //        }
        //        else
        //        {
        //            absdeclar = parseDeclarator(true);
        //            //node = arbor.makeParamDeclarNode(declarspecs, null, absdeclar);
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ///*(6.7.5) 
        // identifier-list:
        //    identifier
        //    identifier-list , identifier
        // */
        //public List<IdentNode> parseIdentifierList()
        //{
        //    List<IdentNode> list = null;
        //    int cuepoint = prep.record();
        //    Token token = prep.getToken();
        //    IdentNode id = arbor.getArgIdentNode(token);
        //    bool result = (id != null);
        //    if (result)
        //    {
        //        list = new List<IdentNode>();
        //        list.Add(id);
        //    }
        //    bool empty = !result;
        //    while (result)
        //    {
        //        int cuepoint2 = prep.record();
        //        token = prep.getToken();
        //        result = (token.type == TokenType.COMMA);
        //        if (!result)
        //        {
        //            token = prep.getToken();
        //            id = arbor.getArgIdentNode(token);
        //            result = (id != null);
        //            if (result)
        //            {
        //                list.Add(id);
        //            }
        //        }
        //        if (!result)
        //        {
        //            prep.rewind(cuepoint2);
        //        }
        //    }
        //    if (empty)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return list;
        //}

        ///*(6.7.6) 
        // type-name:
        //    specifier-qualifier-list abstract-declarator[opt]
        // */
        //public TypeNameNode parseTypeName()
        //{
        //    TypeNameNode node = null;
        //    List<DeclarSpecNode> list = parseSpecQualList();
        //    bool result = (list != null);
        //    if (result)
        //    {
        //        DeclaratorNode declar = parseDeclarator(true);
        //        result = (declar != null);
        //        if (result)
        //        {
        //            //Console.WriteLine("parsed spec-qual abstractor-declarator type-name");
        //            //node = arbor.makeTypeNameNode(list, declar);
        //        }
        //    }
        //    return node;
        //}

        ////- declaration initializers ------------------------------------

        ///*(6.7.8) 
        // initializer:
        //    assignment-expression
        //    { initializer-list }
        //    { initializer-list , }
        // */
        //public InitializerNode parseInitializer()
        //{
        //    InitializerNode node = null;
        //    AssignExpressionNode expr = pexpr.parseAssignExpression();
        //    bool result = (expr != null);
        //    if (result)
        //    {
        //        node = arbor.makeInitializerNode(expr);
        //    }
        //    else
        //    {
        //        int cuepoint = prep.record();
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.LBRACE);
        //        if (result)
        //        {
        //            List<InitializerNode> list = parseInitializerList();
        //            result = (list != null);
        //            if (result)
        //            {
        //                token = prep.getToken();
        //                result = (token.type == TokenType.RBRACE);
        //                if (!result)
        //                {
        //                    result = (token.type == TokenType.COMMA);
        //                    if (result)
        //                    {
        //                        token = prep.getToken();
        //                        result = (token.type == TokenType.RBRACE);
        //                    }
        //                }
        //                if (result)
        //                {
        //                    node = arbor.makeInitializerNode(list);
        //                }
        //            }
        //        }
        //        if (!result)
        //        {
        //            prep.rewind(cuepoint);
        //        }
        //    }
        //    return node;
        //}

        ///*(6.7.8) 
        // initializer-list:
        //    designation[opt] initializer
        //    initializer-list , designation[opt] initializer
        // */
        //public List<InitializerNode> parseInitializerList()
        //{
        //    List<InitializerNode> list = null;
        //    DesignationNode desinode = parseDesignation();
        //    InitializerNode initnode = parseInitializer();
        //    bool result = (initnode != null);
        //    if (result)
        //    {
        //        list = new List<InitializerNode>();
        //        initnode.addDesignation(desinode);
        //        list.Add(initnode);
        //    }
        //    while (result)
        //    {
        //        int cuepoint = prep.record();
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.COMMA);
        //        if (result)
        //        {
        //            desinode = parseDesignation();
        //            initnode = parseInitializer();
        //            result = (initnode != null);
        //            if (result)
        //            {
        //                initnode.addDesignation(desinode);
        //                list.Add(initnode);
        //            }
        //        }
        //        if (!result)
        //        {
        //            prep.rewind(cuepoint);
        //        }
        //    }
        //    return list;
        //}

        ///*(6.7.8) 
        // designation:
        //    designator-list =
        // */
        //public DesignationNode parseDesignation()
        //{
        //    DesignationNode node = null;
        //    int cuepoint = prep.record();
        //    List<DesignatorNode> list = parseDesignatorList();
        //    bool result = (list != null);
        //    if (result)
        //    {
        //        Token token = prep.getToken();
        //        result = (token.type == TokenType.EQUAL);
        //        if (result)
        //        {
        //            node = arbor.makeDesignationNode(list);
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}

        ///*(6.7.8) 
        // designator-list:
        //    designator
        //    designator-list designator
        // */
        //public List<DesignatorNode> parseDesignatorList()
        //{
        //    List<DesignatorNode> list = null;
        //    DesignatorNode node = parseDesignator();
        //    if (node != null)
        //    {
        //        list = new List<DesignatorNode>();
        //        list.Add(node);
        //    }
        //    while (node != null)
        //    {
        //        node = parseDesignator();
        //        if (node != null)
        //        {
        //            list.Add(node);
        //        }
        //    }
        //    return list;
        //}

        ///*(6.7.8) 
        // designator:
        //    [ constant-expression ]
        //    . identifier
        // */
        //public DesignatorNode parseDesignator()
        //{
        //    DesignatorNode node = null;
        //    int cuepoint = prep.record();
        //    Token token = prep.getToken();
        //    bool result = (token.type == TokenType.LBRACKET);
        //    if (result)
        //    {
        //        ConstExpressionNode expr = pexpr.parseConstantExpression();
        //        if (result)
        //        {
        //            token = prep.getToken();
        //            result = (token.type == TokenType.RBRACKET);
        //            if (result)
        //            {
        //                node = arbor.makeDesignatorNode(expr);              //[ constant-expression ]
        //            }
        //        }
        //    }
        //    if (!result)
        //    {
        //        result = (token.type == TokenType.PERIOD);
        //        if (result)
        //        {
        //            token = prep.getToken();
        //            IdentNode ident = arbor.getFieldInitializerNode(token);
        //            result = (ident != null);
        //            if (result)
        //            {
        //                node = arbor.makeDesignatorNode(ident);             //. identifier
        //            }
        //        }
        //    }
        //    if (!result)
        //    {
        //        prep.rewind(cuepoint);
        //    }
        //    return node;
        //}
    }
}
