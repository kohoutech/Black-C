/* ----------------------------------------------------------------------------
Black C Jr - a frontend C parser
Copyright (C) 2019  George E Greaney

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

namespace BlackCJr
{
    class Preprocessor
    {
        string srcName;
        string[] srcLines;
        public int lineNum;
        public int linePos;
        public FragType fragtype;

        public Preprocessor(string _srcName)
        {
            srcName = _srcName;
            srcLines = File.ReadAllLines(srcName);
            lineNum = 0;
            linePos = 0;
        }

        public String getFrag()
        {
            //eof
            if (lineNum >= srcLines.Length)
            {
                fragtype = FragType.EOF;
                return "\0";
            }

            //spaces
            String line = srcLines[lineNum];
            if ((linePos >= line.Length) || (line[linePos] == ' '))
            {
                bool done = false;
                do
                {
                    if (!done && (linePos >= line.Length))
                    {
                        lineNum++;
                        linePos = 0;
                        if (lineNum < srcLines.Length)
                        {
                            line = srcLines[lineNum];
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    if (!done && (line[linePos] == ' '))
                    {
                        linePos++;
                    }
                    else
                    {
                        done = true;
                    }
                } while (!done);
                fragtype = FragType.SPACE;
                return " ";
            }

            //words
            if ((line[linePos] >= 'A' && line[linePos] <= 'Z') || (line[linePos] >= 'a' && line[linePos] <= 'z') || (line[linePos] == '_'))
            {
                String word = "";
                while ((linePos < line.Length) &&
                    (line[linePos] >= 'A' && line[linePos] <= 'Z') || (line[linePos] >= 'a' && line[linePos] <= 'z') ||
                    (line[linePos] >= '0' && line[linePos] <= '9') || (line[linePos] == '_'))
                {
                    word += line[linePos];
                    linePos++;
                }
                fragtype = FragType.WORD;
                return word;
            }

            //numbers
            if (line[linePos] >= '0' && line[linePos] <= '9')
            {
                String num = "";
                while ((linePos < line.Length) && (line[linePos] >= '0' && line[linePos] <= '9'))
                {
                    num += line[linePos];
                    linePos++;
                }
                fragtype = FragType.NUMBER;
                return num;
            }

            //chars
            fragtype = FragType.CHAR;
            String ch = "" + line[linePos];
            linePos++;
            return ch;
        }
    }

    enum FragType
    {
        WORD,
        NUMBER,
        CHAR,
        SPACE,
        EOF
    }
}
