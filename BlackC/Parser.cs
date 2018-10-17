/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2018  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

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


        /* (6.7) 
         declaration-specifiers:
            storage-class-specifier declaration-specifiersopt
            type-specifier declaration-specifiersopt
            type-qualifier declaration-specifiersopt
            function-specifier declaration-specifiersopt
         */
        public void parseDeclarSpecs(List<Token> specs)
        {
            bool done = false;
            while (!done)
            {
                Token token = scanner.getToken();

                //storage class specifer
                if ((token is tTypedef) || (token is tExtern) || (token is tStatic) || (token is tAuto) || (token is tRegister))
                {
                    specs.Add(token);
                }

                //type specifier
                else if ((token is tVoid) || (token is tChar) || (token is tShort) || (token is tInt) || (token is tLong)
                    || (token is tFloat) || (token is tDouble) || (token is tSigned) || (token is tUnsigned))
                {
                    specs.Add(token);
                }
                else if ((token is tStruct) || (token is tUnion))
                {
                    parseStuctOrUnionSpec();
                }
                else if (token is tEnum)
                {
                    parseEnumSpec();
                }
                else if ((token is tIdentifier) && (((tIdentifier)token).isTypeDef))
                {
                    //handle typedef
                }

                //type qualifier
                else if ((token is tConst) || (token is tRestrict) || (token is tVolatile))
                {
                    specs.Add(token);
                }

                //func specifier
                else if (token is tInline)
                {
                    specs.Add(token);
                }

                //none of the above
                else
                {
                    done = true;
                }
            }
        }

        private void parseEnumSpec()
        {
            throw new NotImplementedException();
        }

        private void parseStuctOrUnionSpec()
        {
            throw new NotImplementedException();
        }

        /* (6.7) 
         declaration:
            declaration-specifiers init-declarator-listopt ;
         */
        public void parseDeclaration()
        {
            
        }

        /* (6.9.1) 
        function-definition:
            declaration-specifiers declarator declaration-list[opt] compound-statement
         */
        public bool parseFunctionDef()
        {
            bool isFunc = true;
            List<Token> specs = new List<Token>();
            parseDeclarSpecs(specs);
            return isFunc;
        }

        /* (6.9) 
        external-declaration:
            function-definition
            declaration
        */
        public void parseExternalDeclaration()
        {
            if (!parseFunctionDef())
            {
                parseDeclaration();
            }
            //Console.WriteLine(first.ToString());
        }

        /*(6.9) 
        translation-unit:
            external-declaration
            translation-unit external-declaration 
        */
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
            Token token = scanner.getToken();
            while (!(token is tEOF))
            {
                scanner.putBack(token);
                parseExternalDeclaration();
                token = scanner.getToken();
            }

            Console.WriteLine("done parsing");
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");