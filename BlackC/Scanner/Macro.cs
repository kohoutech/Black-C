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
    public class Macro
    {
        public static Dictionary<String, Macro> macroTable;

        public String name;
        public List<Token> def;
        public bool hasParams;
        public List<Token> paramList;

        public bool invoked;
        public List<Token> invokeList;
        public int invokePos;

        public static void initMacros()
        {
            macroTable = new Dictionary<string, Macro>();
        }

        public static Macro defineMacro(Token token)
        {
            String name = token.chars;
            Console.WriteLine("defining macro " + name);

            if (macroTable.ContainsKey(name))
            {
                macroTable.Remove(name);
            }

            Macro def = new Macro(name);
            macroTable.Add(name, def);

            return def;
        }

        public static void undefineMacro(Token token)
        {
            String name = token.chars;
            Console.Write("undefining macro " + name);

            if (macroTable.ContainsKey(name))
            {
                macroTable.Remove(name);
            }
        }

        public static Macro lookupMacro(Token token)
        {
            String name = token.chars;
            Console.Write("looking up macro " + name);

            Macro def = null;
            if (macroTable.ContainsKey(name))
            {
                def = macroTable[name];
            }
            return def;
        }

        //- macro definition --------------------------------------------------

        public Macro(String _name)
        {
            name = _name;
            def = new List<Token>();
            hasParams = false;
            paramList = null;
            invoked = false;
            invokeList = null;
            invokePos = 0;
        }

        public void scanMacroDefinition(Scanner scanner)
        {
            //get tokens that make up this macro's definition
            Token token = scanner.scanToken();        
            while (token.type != TokenType.tEOLN)
            {
                def.Add(token);
                token = scanner.scanToken();
            }

            //check if macro is parameterized, if so, build param list
            if ((def.Count > 0) && (def[0].type == TokenType.tLPAREN))
            {
                hasParams = true;
                paramList = new List<Token>();
                int pos = 1;
                while (def[pos].type != TokenType.tRPAREN)
                {
                    paramList.Add(def[pos++]);
                    if (def[pos].type == TokenType.tCOMMA)
                    {
                        pos++;
                    }
                }
                def.RemoveRange(0, pos+1);        //remove the param tokens (including the ()'s) from the replacement list tokens
            }

        }

        //- macro invocation --------------------------------------------------

        public List<List<Token>> buildArgList(Scanner scanner)
        {
            List<List<Token>> argList = new List<List<Token>>();
            int paramLevel = 0;

            Token token = null;
            List<Token> argTokens = new List<Token>();
            do
            {
                token = scanner.scanToken();
                if (token.type == TokenType.tLPAREN)
                {
                    if (paramLevel > 0)
                    {
                        argTokens.Add(token);
                    }
                    paramLevel++;
                }
                else if (token.type == TokenType.tRPAREN)
                {
                    paramLevel--;
                    if (paramLevel > 0)
                    {
                        argTokens.Add(token);
                    }
                }
                else if (token.type == TokenType.tCOMMA)
                {
                    argList.Add(argTokens);
                    argTokens = new List<Token>();
                }
                else
                {
                    argTokens.Add(token);
                }
            }
            while (paramLevel > 0);
            argList.Add(argTokens);             //add last token arg list

            return argList;
        }

        public List<Token> buildInvokeList(List<List<Token>> argList)
        {
            List<Token> tokenList = new List<Token>();
            for (int i = 0; i < def.Count; i++)
            {
                Token token = def[i];
                int sub = -1;
                if (token.type == TokenType.tIDENTIFIER)
                {
                    for (int j = 0; j < paramList.Count; j++)           //find def token in param list
                    {
                        if (token.chars.Equals(paramList[j].chars))
                        {
                            sub = j;
                            break;
                        }
                    }
                }
                if (sub < 0)                    //no match
                {
                    tokenList.Add(def[i]);
                }
                else
                {
                    tokenList.AddRange(argList[sub]);       //sub arg token list for matched param
                }
            }

            return tokenList;
        }

        public List<Token> StringifyInvokeList(List<Token> inList)
        {
            List<Token> outList = new List<Token>();
            Token token = null;
            for (int i = 0; i < inList.Count; i++)
            {
                if (((i + 1) < inList.Count) && (inList[i].type == TokenType.tHASH))
                {
                    String tokenString = inList[i+1].chars;
                    token = new Token(TokenType.tSTRINGCONST, tokenString);
                    i++;
                }
                else if (((i + 2) < inList.Count) && (inList[i+1].type == TokenType.tDOUBLEHASH))
                {
                    String tokenString = inList[i].chars + inList[i+2].chars;
                    token = new Token(TokenType.tSTRINGCONST, tokenString);
                    i += 2;
                }
                else
                {
                    token = inList[i];
                }
                outList.Add(token);
            }

            return outList;
        }

        public void invokeMacro(Scanner scanner)
        {
            Console.WriteLine("expanding macro " + name);
            //if parameterized, build actual token replacement list from argument tokens
            if (hasParams)
            {
                List<List<Token>> argList = buildArgList(scanner);
                invokeList = buildInvokeList(argList);
            }
            else
            {
                invokeList = def;
            }

            invokeList = StringifyInvokeList(invokeList);       //handle token pasting

            invokePos = 0;
        }

        public Token getToken()
        {
            Token result = null;
            if (invokePos < invokeList.Count)
            {
                result = invokeList[invokePos++];
            }
            return result;
        }

        internal bool atEnd()
        {
            throw new NotImplementedException();
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");