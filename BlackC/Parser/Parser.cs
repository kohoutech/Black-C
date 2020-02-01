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
        public Options options;
        public Tokenizer scan;
        public Arbor arbor;
        public ParseDeclar pdeclar;
        public ParseExpr pexpr;
        public ParseStmt pstmt;

        public List<String> includePaths;

        public Parser(Options _options)
        {
            options = _options;

            scan = null;
            arbor = new Arbor(this);

        //    //create sub parsers
        //    pdeclar = new ParseDeclar(prep, arbor);
        //    pexpr = new ParseExpr(prep, arbor);            
        //    pstmt = new ParseStmt(prep, arbor);

        //    pdeclar.pexpr = pexpr;
        //    pexpr.pdeclar = pdeclar;
        //    pstmt.pexpr = pexpr;
        //    pstmt.pdeclar = pdeclar;

        //    includePaths = new List<string>() { "." };          //start with current dir
        //    includePaths.AddRange(options.includePaths);        //add search paths from command line
        }

        //---------------------------------------------------------------------

        public void parseFile(String filename)
        {
            scan = new Tokenizer(filename);

        //    if (options.preProcessOnly)
        //    {
        //        prep.preprocessFile();
        //    }
        //    else
        //    {
                Module module = parseTranslationUnit();
                module.write();
        //    }
            Console.WriteLine("parsed " + filename);
        }

        //- external definitions ----------------------------------------------

        /*(6.9) 
         translation-unit:
            external-declaration
            translation-unit external-declaration 
        */
        public Module parseTranslationUnit()
        {
            Module unit = new Module();
            Token tok = scan.getToken();
            while (tok.type != TokenType.EOF)
            {
                Console.WriteLine(tok.ToString());
                tok = scan.getToken();
                //parseExternalDef();
            }
            return unit;
        }

        ///* (6.9)     
        // external-declaration:
        //    declaration
        //    function-definition

        // (6.7) 
        // declaration:
        //    declaration-specifiers init-declarator-list[opt] ;

        // (6.9.1)
        // function-definition:
        //    declaration-specifiers declarator declaration-list[opt] compound-statement          
        //*/
        //public void parseExternalDef()
        //{
        //    FunctionDefNode funcDef = null;
        //    int cuepoint = prep.record();
        //    DeclarationNode declars = pdeclar.parseDeclaration();
        //    if (declars.isFuncDef)
        //    {
        //        funcDef = new FunctionDefNode(declars);
        //        List<DeclarationNode> oldparamlist = parseDeclarationList();
        //        funcDef.setOldParams(oldparamlist);
        //        StatementNode block = pstmt.parseCompoundStatement();
        //        funcDef.setFuncBody(block);
        //    }
        //}

        ///*(6.9.1) 
        // declaration-list:
        //    declaration
        //    declaration-list declaration
        //*/
        ////old-style function parameter defs - here for completeness
        //public List<DeclarationNode> parseDeclarationList()
        //{
        //    List<DeclarationNode> declarList = null;
        //    DeclarationNode declar = pdeclar.parseDeclaration();
        //    if (declar != null)
        //    {
        //        declarList = new List<DeclarationNode>();
        //        declarList.Add(declar);
        //    }
        //    while (declar != null)
        //    {
        //        declar = pdeclar.parseDeclaration();
        //        declarList.Add(declar);
        //    }
        //    return declarList;
        //}
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");