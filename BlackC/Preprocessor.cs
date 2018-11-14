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
        public Parser parser;
        public Scanner scanner;
        public List<SourceBuffer> bufferStack;

        Token lookahead;
        List<Token> replay;
        int recpos;

        public Preprocessor(Parser _parser)
        {
            parser = _parser;
            scanner = new Scanner();
            bufferStack = new List<SourceBuffer>();

            lookahead = null;
            replay = new List<Token>();
            recpos = 0;
        }

        public void setMainSourceFile(string filename)
        {
            SourceBuffer srcbuf = new SourceBuffer(filename);
            scanner.setSource(srcbuf);
        }

        //- token stream handling ------------------------------------------

        public Token getToken()
        {
            Token token = null;

            if (recpos < replay.Count)
            {
                token = replay[recpos++];
            }
            else if (lookahead != null)
            {
                token = lookahead;
            }
            else
            {
                token = scanner.scanToken();
                lookahead = token;
                replay.Add(token);
                recpos++;
            }

            return token;
        }

        public void next()
        {
            lookahead = null;
            recpos++;
        }

        public int record()
        {
            return recpos;
        }

        //rewind one token
        public void rewind()
        {
            if (recpos > 0)
            {
                recpos--;
            }
        }

        //rewind tokens to cuepoint
        public void rewind(int cuepoint)
        {
            recpos = cuepoint;
        }

        public void reset()
        {
            recpos = 0;
        }

        public bool isNextToken(TokenType ttype)
        {
            return (getToken().type == ttype);
        }

        //- directive handling ------------------------------------------------

        public void handleDirective()
        {
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    string line = lines[i];
            //    if ((line.Length > 0) && (line[0] == '#'))
            //    {
            //        lines[i] = "";
            //    }
            //}
        }
    }

    //-------------------------------------------------------------------------

    public class Macro
    {
    }
}
