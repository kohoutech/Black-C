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

namespace BlackC
{
    public class Preprocessor
    {
        public string[] lines;

        public Preprocessor(string[] _lines)
        {
            lines = _lines;
        }

        public void process()
        {
            handleDirectives();
            removeBlockComments();
            removeLineComments();

            //for (int i = 0; i < lines.Length; i++)
            //{
            //    Console.WriteLine(lines[i]);
            //}
        }

        public void handleDirectives()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if ((line.Length > 0) && (line[0] == '#'))
                {
                    lines[i] = "";
                }
            }
        }

        public void removeLineComments()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int cpos = line.IndexOf("//");
                if (cpos >= 0)
                {
                    line = line.Remove(cpos);
                    lines[i] = line + ' ';
                }
            }
        }

        public void removeBlockComments()
        {
            int startpos;
            int endpos;
            bool found;
            bool incomment = false;
            List<string> newlines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string newline = "";

                do
                {
                    found = false;
                    if (!incomment)
                    {
                        startpos = line.IndexOf("/*");
                        if (startpos >= 0)
                        {
                            newline = newline + line.Substring(0, startpos);
                            line = line.Substring(startpos);
                            incomment = true;
                            found = true;
                        }
                    }
                    else
                    {
                        endpos = line.IndexOf("*/");
                        if (endpos >= 0)
                        {
                            line = line.Substring(endpos);
                            incomment = false;
                            found = true;
                        }
                    }
                } while (found);
                if (!incomment)
                {
                    newline = newline + line;
                }
                lines[i] = newline;
            }
        }
    }

    public class Macro
    {
    }
}
