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

namespace BlackC.Scan
{
    public class Macro
    {
        public static Dictionary<String, Macro> macroTable;
        public static List<Macro> macroList;
        public static Macro curMacro;

        public String name;
        public List<Fragment> def;
        public bool hasParams;
        public List<Fragment> paramList;
        public List<Fragment> invokeList;
        public int curPos;

        public static void initMacros()
        {
            macroTable = new Dictionary<string, Macro>();
            macroList = new List<Macro>();
            curMacro = null;
        }

        public static Macro lookupMacro(String name)
        {
            //Console.Write("looking up macro " + name);
            
            //lookup macro name in invocation chain - if it's already there, return null so preprocessor doesn't expand it
            foreach (Macro macro in macroList)
            {
                if (name.Equals(macro.name))
                {
                    return null;
                }
            }

            //return macro obj if it's been defined, else return null
            if (macroTable.ContainsKey(name))
            {
                return macroTable[name];
            }
            return null;
        }

        public static void defineMacro(List<Fragment> frags)
        {
            //delete space(s) & get macro name
            while (frags[0].type == FragType.SPACE)  
            {
                frags.RemoveAt(0); 
            }

            String name = frags[0].str;
            Console.WriteLine("defining macro " + name);
            frags.RemoveAt(0);

            Macro macro = new Macro(name, frags);
            macroTable.Add(name, macro);
        }

        public static void undefineMacro(String name)
        {
            Console.Write("undefining macro " + name);

            if (macroTable.ContainsKey(name))
            {
                macroTable.Remove(name);
            }
        }

        public static void setCurrentMacro(Macro macro, List<Fragment> args)
        {
            //add macro to invocation chain if not empty
            if (macro.def.Count > 0)
            {
                Console.WriteLine("entring macro " + macro.name);
                macro.invoke(args);
                curMacro = macro;
                macroList.Add(macro);
            }
        }

        public static bool inMacro()
        {
            return (macroList.Count > 0);
        }

        public static Fragment getfrag()
        {
            Fragment result = curMacro.invokeList[curMacro.curPos++];
            if (curMacro.curPos == curMacro.invokeList.Count)
            {
                Console.WriteLine("leaving macro " + curMacro.name);
                macroList.RemoveAt(macroList.Count - 1);
                if (macroList.Count > 0)
                {
                    curMacro = macroList[macroList.Count - 1];
                }
                else
                {
                    curMacro = null;
                }
            }
            return result;
        }

        //- macro definition --------------------------------------------------

        public Macro(String _name, List<Fragment> frags)
        {
            name = _name;
            scanMacroDefinition(frags);
            invokeList = null;
            curPos = 0;
        }

        public void scanMacroDefinition(List<Fragment> frags)
        {
            def = frags;
            hasParams = false;
            paramList = null;

            //check if macro is parameterized, if so, build param list
            if ((def.Count > 0) && (def[0].type == FragType.PUNCT) && (def[0].str[0] == '('))
            {
                hasParams = true;
                paramList = new List<Fragment>();
                int pos = 1;
                while ((def[pos].type == FragType.PUNCT) && (def[pos].str[0] == ')'))
                {
                    paramList.Add(def[pos++]);
                    if ((def[pos].type == FragType.PUNCT) && (def[pos].str[0] == ','))
                    {
                        pos++;
                    }
                }
                def.RemoveRange(0, pos+1);        //remove the param tokens (including the ()'s) from the replacement list tokens
            }

            //trim spaces off of ends of remaining macro def list
            while ((frags.Count > 0) && (frags[0].type == FragType.SPACE))
            {
                frags.RemoveAt(0);
            }
            while ((frags.Count > 0) && (frags[frags.Count-1].type == FragType.SPACE))
            {
                frags.RemoveAt(frags.Count-1);
            }
        }

        //- macro invocation --------------------------------------------------

        //public List<List<Token>> buildArgList(Scanner scanner)
        //{
        //    List<List<Token>> argList = new List<List<Token>>();
        //    int paramLevel = 0;

        //    Token token = null;
        //    List<Token> argTokens = new List<Token>();
        //    do
        //    {
        //        token = scanner.scanToken();
        //        if (token.type == TokenType.tLPAREN)
        //        {
        //            if (paramLevel > 0)
        //            {
        //                argTokens.Add(token);
        //            }
        //            paramLevel++;
        //        }
        //        else if (token.type == TokenType.tRPAREN)
        //        {
        //            paramLevel--;
        //            if (paramLevel > 0)
        //            {
        //                argTokens.Add(token);
        //            }
        //        }
        //        else if (token.type == TokenType.tCOMMA)
        //        {
        //            argList.Add(argTokens);
        //            argTokens = new List<Token>();
        //        }
        //        else
        //        {
        //            argTokens.Add(token);
        //        }
        //    }
        //    while (paramLevel > 0);
        //    argList.Add(argTokens);             //add last token arg list

        //    return argList;
        //}

        //public List<Token> buildInvokeList(List<List<Token>> argList)
        //{
        //    List<Token> tokenList = new List<Token>();
        //    for (int i = 0; i < def.Count; i++)
        //    {
        //        Token token = def[i];
        //        int sub = -1;
        //        if (token.type == TokenType.tIDENTIFIER)
        //        {
        //            for (int j = 0; j < paramList.Count; j++)           //find def token in param list
        //            {
        //                if (token.chars.Equals(paramList[j].chars))
        //                {
        //                    sub = j;
        //                    break;
        //                }
        //            }
        //        }
        //        if (sub < 0)                    //no match
        //        {
        //            tokenList.Add(def[i]);
        //        }
        //        else
        //        {
        //            tokenList.AddRange(argList[sub]);       //sub arg token list for matched param
        //        }
        //    }

        //    return tokenList;
        //}

        //public List<Token> StringifyInvokeList(List<Token> inList)
        //{
        //    List<Token> outList = new List<Token>();
        //    //Token token = null;
        //    //for (int i = 0; i < inList.Count; i++)
        //    //{
        //    //    if (((i + 1) < inList.Count) && (inList[i].type == TokenType.tHASH))
        //    //    {
        //    //        String tokenString = inList[i+1].chars;
        //    //        token = new Token(TokenType.tSTRINGCONST, tokenString);
        //    //        i++;
        //    //    }
        //    //    else if (((i + 2) < inList.Count) && (inList[i+1].type == TokenType.tDOUBLEHASH))
        //    //    {
        //    //        String tokenString = inList[i].chars + inList[i+2].chars;
        //    //        token = new Token(TokenType.tSTRINGCONST, tokenString);
        //    //        i += 2;
        //    //    }
        //    //    else
        //    //    {
        //    //        token = inList[i];
        //    //    }
        //    //    outList.Add(token);
        //    //}

        //    return outList;
        //}

        //not handling paramaterization or stringification yet
        public void invoke(List<Fragment> args)
        {
            Console.WriteLine("expanding macro " + name);
            //if parameterized, build actual token replacement list from argument tokens
            if (hasParams)
            {
        //        List<List<Token>> argList = buildArgList(scanner);
        //        invokeList = buildInvokeList(argList);
            }
            else
            {
                invokeList = def;
            }

        //    invokeList = StringifyInvokeList(invokeList);       //handle token pasting

            curPos = 0;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");