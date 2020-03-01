/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2020  George E Greaney

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

using BlackC.Lexer;
using Origami.OIL;

namespace BlackC
{
    class BlackC
    {
        static void Main(string[] args)
        {
            Options options = new Options(args);                    //parse the cmd line args

            String filename = args[0];

            try
            {
                if (options.preProcessOnly)
                {
                    //Tokenizer prep = new Tokenizer(this, filename);
                    //prep.preprocessFile(options.preProcessFilename);
                }
                else
                {
                    Parser parser = new Parser();                       //create a parser
                    Module module = parser.parseFile(filename);         //parse the source file                

                    OILCan oilCan = new OILCan("module.oil.txt");
                    oilCan.save(module);                                //and write it out to OIL file
                }
            }
            catch (ParserException e)
            {
                Console.WriteLine("had fatal exception: " + e.Message);
            }
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");