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

//arbor - a place where trees are grown

namespace BlackC
{
    class Arbor
    {
        Dictionary<string, int> typepdefids;

        public Arbor()
        {
            typepdefids = new Dictionary<string, int>();

            string[] lines = File.ReadAllLines("typedefs.txt");
            foreach (String line in lines)
            {
                string[] parts = line.Split(' ');
                String id = parts[0];
                int count = Convert.ToInt32(parts[1]);
                typepdefids.Add(id, count);
            }
        }

        //temproary kludge to get around ambiguity in C99's grammar between typedef and identifier 
        //see https://en.wikipedia.org/wiki/The_lexer_hack
        //this will be removed once the rest of the semantic analysis is up & running and this is not needed anymore
        //crazy eh?

        //returns false the first/second time we see a name in a type defintion, then true once its been defined
        public bool isTypedef(String id)
        {
            bool result = false;
            if (typepdefids.ContainsKey(id))
            {
                if (typepdefids[id] > 0)
                {
                    typepdefids[id]--;
                }
                else
                {
                    result = true;
                }
            }
            return result;
        }
    }
}
