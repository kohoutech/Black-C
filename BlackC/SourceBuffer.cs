/* ----------------------------------------------------------------------------
Black C - a frontend C parser
Copyright (C) 1997-2019  George E Greaney

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

        //public bool atBOL;

        public List<byte> buf;
        public char ch;                 //the cur char
        public int pos;                 //points to next char
        public int linenum;             //cur line num, starts with 1
        public int linestart;           //pos of first char on line in buf
        public int linepos;             //pos of cur char in line

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

        //public SourceBuffer(String _path, String _filename)
        //{
        //    filename = _filename;
        //    path = _path;
        //    fullname = path + "\\" + filename;

        //    atBOL = true;
        //    eolnCount = 0;
        //}

        public SourceBuffer(String _path, String _filename)
        {
            filename = _filename;
            path = _path;
            fullname = path + "\\" + filename;

            //read in source into byte list, fix ending
            buf = new List<byte>(File.ReadAllBytes(fullname));
            if (buf[buf.Count - 1] != '\n')
                buf.Add((byte)('\n'));
            buf.Add(0);

            pos = 0;
            linenum = 1;
            linestart = 0;
            linepos = 0;

            getChar();
        }

        /*(5.1.1.2) 
            translation phase 1 : translate trigraphs (5.2.1.1)
            translation phase 2 : handle line continuations
         */
        public void getChar()
        {
            bool done = true;
            do
            {
                linepos = pos - linestart;
                ch = (char)buf[pos++];
                done = true;

                //translate possible trigraph
                if (ch == '?')
                {
                    char c2 = (char)buf[pos];
                    if (c2 == '?')
                    {
                        char c3 = (char)buf[pos + 1];
                        switch (c3)
                        {
                            case '=': ch = '#'; pos += 2; break;
                            case '(': ch = '['; pos += 2; break;
                            case '/': ch = '\\'; pos += 2; break;
                            case ')': ch = ']'; pos += 2; break;
                            case '\'': ch = '^'; pos += 2; break;
                            case '<': ch = '{'; pos += 2; break;
                            case '!': ch = '|'; pos += 2; break;
                            case '>': ch = '}'; pos += 2; break;
                            case '-': ch = '~'; pos += 2; break;
                        }
                    }
                }

                //handle line continuations
                if ((ch == '\\') && ((char)buf[pos] == '\n') ||
                    (ch == '\\') && ((char)buf[pos] == '\r') && ((char)buf[pos+1] == '\n'))     //MS eolns
                {
                    if (((char)buf[pos] == '\r') && ((char)buf[pos + 1] == '\n'))
                        pos++;
                    pos++;
                    linenum++;
                    linestart = pos;
                    done = false;
                }
            } while (!done);
        }

        //goto next char & return cur char
        public char gotoNextChar()
        {
            char result = ch;
            if (pos != buf.Count)
            {
                getChar();
            }
            return result;
        }

        //return next char w/o advancing
        public char peekNextChar()
        {
            return (char)buf[pos];
        }

        public void onNewLine()
        {
            linenum++;
            linestart = pos;
        }

        public bool atEnd()
        {
            return (pos == buf.Count);
        }

        public SourceLocation getCurPos()
        {
            return new SourceLocation(linenum, pos - linestart - 1);
        }
    }

    //-------------------------------------------------------------------------

    public class SourceLocation
    {
        public int linenum;
        public int linepos;

        public SourceLocation(int lnum, int lpos)
        {
            linenum = lnum;
            linepos = lpos;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");