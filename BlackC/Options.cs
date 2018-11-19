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
    public class Options
    {
        public List<String> filenames;
        public bool preProcessOnly;

        public List<String> includePaths;

        public Options(string[] cmdArgs)
        {

            setDefaultValues();

            //first merge any response file contents into arg list
            List<String> args = new List<string>();
            foreach (String arg in cmdArgs)
            {
                if (arg[0] == '@')
                {
                    List<String> responseArgs = parseResponseFile(arg.Substring(1));
                    args.AddRange(responseArgs);
                }
                else
                {
                    args.Add(arg);
                }
            }

            //now parse all options & filenames
            parseOptions(args);

        }

        public void setDefaultValues()
        {
            filenames = new List<string>();
            includePaths = new List<string>();

            preProcessOnly = false;
        }

        public List<String> parseResponseFile(String filename)
        {
            List<String> args = new List<string>();
            char[] sep = { ' ' };

            String[] lines = File.ReadAllLines(filename);
            foreach (String line in lines)
            {
                String[] lineargs = line.Split(sep);
                args.AddRange(lineargs);
            }
            return args;
        }

        public void parseOptions(List<String> args)
        {
            for (int i = 0; i < args.Count; i++)
            {
                if ((args[i].StartsWith("/")) || (args[i].StartsWith("-")))
                {
                    String arg = args[i].Substring(1);
                    if (arg.Length > 0)
                    {
                        switch (arg[0])
                        {
                            case 'I':
                                {
                                    String path = null;
                                    if (arg.Length > 1)
                                    {
                                        path = arg.Substring(1);       //for /Ixxx form                                        
                                    }
                                    else
                                    {
                                        path = args[++i];              //for /I xxx form
                                    }
                                    includePaths.Add(path);
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    filenames.Add(args[i]);
                }
            }
        }
    }
}
