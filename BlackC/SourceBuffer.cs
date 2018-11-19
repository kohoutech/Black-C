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
    public class SourceBuffer
    {
        public String filename;
        public String path;
        public String fullname;

        public string[] lines;
        public String curline;
        public int linenum;
        public int linepos;
        public bool atBOL;
        public int eolnCount;

        public static SourceBuffer getIncludeFile(String filename, List<String> searchPaths)
        {
            String path = null;
            String fullpath = null;

            foreach (String searchpath in searchPaths)
            {
                path = searchpath;
                fullpath = path + "\\" + filename;
                if (File.Exists(fullpath))
                {
                    break;
                }
            }
            Console.WriteLine("opening include file " + fullpath);
            return new SourceBuffer(path, filename);
        }

        public SourceBuffer(String _path, String _filename)
        {
            filename = _filename;
            path = _path;
            fullname = path + "\\" + filename;

            lines = File.ReadAllLines(fullname);
            curline = null;
            linenum = 0;
            linepos = 0;
            atBOL = true;
            eolnCount = 0;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");