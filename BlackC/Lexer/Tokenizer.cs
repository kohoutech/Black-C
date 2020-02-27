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

namespace BlackC.Lexer
{
    public class Tokenizer
    {
        Parser parser;
        Preprocessor prep;

        Queue<Fragment> frags;
        Queue<Token> tokens;
        Dictionary<String, TokenType> keywords;

        public Tokenizer(Parser _parser, String filename)
        {
            parser = _parser;

            prep = new Preprocessor(parser, filename);
            frags = new Queue<Fragment>();
            tokens = new Queue<Token>();

            //build keyword list
            keywords = new Dictionary<string, TokenType>();
            keywords.Add("break", TokenType.BREAK);
            keywords.Add("case", TokenType.CASE);
            keywords.Add("char", TokenType.CHAR);
            keywords.Add("const", TokenType.CONST);
            keywords.Add("continue", TokenType.CONTINUE);
            keywords.Add("default", TokenType.DEFAULT);
            keywords.Add("do", TokenType.DO);
            keywords.Add("double", TokenType.DOUBLE);
            keywords.Add("else", TokenType.ELSE);
            keywords.Add("enum", TokenType.ENUM);
            keywords.Add("extern", TokenType.EXTERN);
            keywords.Add("float", TokenType.FLOAT);
            keywords.Add("for", TokenType.FOR);
            keywords.Add("goto", TokenType.GOTO);
            keywords.Add("if", TokenType.IF);
            keywords.Add("int", TokenType.INT);
            keywords.Add("long", TokenType.LONG);
            keywords.Add("return", TokenType.RETURN);
            keywords.Add("short", TokenType.SHORT);
            keywords.Add("signed", TokenType.SIGNED);
            keywords.Add("static", TokenType.STATIC);
            keywords.Add("struct", TokenType.STRUCT);
            keywords.Add("switch", TokenType.SWITCH);
            keywords.Add("typedef", TokenType.TYPEDEF);
            keywords.Add("union", TokenType.UNION);
            keywords.Add("unsigned", TokenType.UNSIGNED);
            keywords.Add("void", TokenType.VOID);
            keywords.Add("while", TokenType.WHILE);            
        }

        //skip tokenizing and just write preprocessor output to file
        public void preprocessFile(String filename)
        {
            prep.preprocessFile(filename);
        }

        //- fragment handling -------------------------------------------------

        public Fragment getNextFrag()
        {
            if (frags.Count != 0)
            {
                return frags.Dequeue();
            }
            Fragment frag = prep.getFrag();
            return frag;
        }

        public void putFragBack(Fragment frag)
        {
            frags.Enqueue(frag);
        }

        //the number fragment we get from the scanner should be well-formed
        public Token ParseNumber(String numstr)
        {
            Token tok = null;
            try
            {
                if (numstr.Contains('.'))
                {
                    double dval = Convert.ToDouble(numstr);
                    tok = new Token(TokenType.FLOATCONST);
                    tok.floatval = dval;
                }
                else
                {
                    int bass = 10;
                    if (numstr.StartsWith("0x"))
                    {
                        bass = 16;
                    }
                    else if (numstr.StartsWith("0"))
                    {
                        bass = 8;
                    }
                    int intval = Convert.ToInt32(numstr, bass);
                    tok = new Token(TokenType.INTCONST);
                    tok.intval = intval;
                }
            }
            catch (Exception e)
            {
                parser.error("error parsing number str " + numstr + " : " + e.Message);
            }
            return tok;
        }

        public Token ParseString(string p)
        {
            return new Token(TokenType.STRINGCONST);
        }

        public Token ParseChar(string p)
        {
            return new Token(TokenType.CHARCONST);
        }

        //- token handling ----------------------------------------------------

        public Token getToken()
        {
            if (tokens.Count > 0)
            {
                return tokens.Dequeue();
            }
            Token token = tokenizer();
            return token;
        }

        public void putTokenBack(Token tok)
        {
            tokens.Enqueue(tok);
        }

        public Token tokenizer()
        {
            Token tok = null;
            Fragment frag;
            Fragment nextfrag;

            while (true)
            {
                frag = getNextFrag();

                //ignore spaces & eolns
                if ((frag.type == FragType.SPACE) || (frag.type == FragType.EOLN))
                {
                    continue;
                }

                //check if word is keyword, typename or identifier
                if (frag.type == FragType.WORD)
                {
                    if (keywords.ContainsKey(frag.str))
                    {
                        tok = new Token(keywords[frag.str]);
                    }
                    else
                    {
                        tok = new Token(TokenType.IDENT);
                        tok.strval = frag.str;
                    }
                    break;
                }

                //convert number / string / char str into constant value
                if (frag.type == FragType.NUMBER)
                {
                    tok = ParseNumber(frag.str);
                    break;
                }

                if (frag.type == FragType.STRING)
                {
                    tok = ParseString(frag.str);
                    break;
                }

                if (frag.type == FragType.CHAR)
                {
                    tok = ParseChar(frag.str);
                    break;
                }

                //convert single punctuation chars into punctuation tokens, combining as necessary
                //need 2 lookaheads at most for '...' token
                if (frag.type == FragType.PUNCT)
                {
                    char c = frag.str[0];
                    switch (c)
                    {
                        case '[':
                            tok = new Token(TokenType.LBRACKET);
                            break;
                        case ']':
                            tok = new Token(TokenType.RBRACKET);
                            break;
                        case '(':
                            tok = new Token(TokenType.LPAREN);
                            break;
                        case ')':
                            tok = new Token(TokenType.RPAREN);
                            break;
                        case '{':
                            tok = new Token(TokenType.LBRACE);
                            break;
                        case '}':
                            tok = new Token(TokenType.RBRACE);
                            break;

                        case '+':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '+'))
                            {
                                tok = new Token(TokenType.PLUSPLUS);
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.PLUSEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.PLUS);
                            }
                            break;
                        case '-':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '-'))
                            {
                                tok = new Token(TokenType.MINUSMINUS);
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.MINUSEQUAL);
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '>'))
                            {
                                tok = new Token(TokenType.ARROW);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.MINUS);
                            }
                            break;
                        case '*':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.MULTEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.STAR);
                            }
                            break;
                        case '/':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.SLASHEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.SLASH);
                            }
                            break;
                        case '%':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.PERCENTEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.PERCENT);
                            }
                            break;
                        case '&':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '&'))
                            {
                                tok = new Token(TokenType.AMPAMP);
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.AMPEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.AMPERSAND);
                            }
                            break;
                        case '|':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '|'))
                            {
                                tok = new Token(TokenType.BARBAR);
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.BAREQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.BAR);
                            }
                            break;
                        case '~':
                            tok = new Token(TokenType.TILDE);
                            break;
                        case '^':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.CARETEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.CARET);
                            }
                            break;


                        case '=':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.EQUALEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.EQUAL);
                            }
                            break;
                        case '!':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.NOTEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.EXCLAIM);
                            }
                            break;
                        case '<':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '<'))
                            {
                                Fragment frag2 = getNextFrag();
                                if ((frag2.type == FragType.PUNCT) && (frag2.str[0] == '='))
                                {
                                    tok = new Token(TokenType.LESSLESSEQUAL);   //<<=
                                }
                                else
                                {
                                    putFragBack(frag2);
                                    tok = new Token(TokenType.LESSLESS);
                                }
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.LESSEQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.LESSTHAN);
                            }
                            break;
                        case '>':
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '>'))
                            {
                                Fragment frag2 = getNextFrag();
                                if ((frag2.type == FragType.PUNCT) && (frag2.str[0] == '='))
                                {
                                    tok = new Token(TokenType.GTRGTREQUAL);   //>>=
                                }
                                else
                                {
                                    putFragBack(frag2);
                                    tok = new Token(TokenType.GTRGTR);
                                }
                            }
                            else if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '='))
                            {
                                tok = new Token(TokenType.GTREQUAL);
                            }
                            else
                            {
                                putFragBack(nextfrag);
                                tok = new Token(TokenType.GTRTHAN);
                            }
                            break;

                        case ',':
                            tok = new Token(TokenType.COMMA);
                            break;
                        case '.':
                            bool threedots = false;
                            nextfrag = getNextFrag();
                            if ((nextfrag.type == FragType.PUNCT) && (nextfrag.str[0] == '.'))
                            {
                                Fragment frag2 = getNextFrag();
                                if ((frag2.type == FragType.PUNCT) && (frag2.str[0] == '.'))
                                {
                                    tok = new Token(TokenType.ELLIPSIS);        //...
                                    threedots = true;
                                }
                                else
                                {
                                    putFragBack(nextfrag);
                                    putFragBack(frag2);
                                }
                            }
                            else
                            {
                                putFragBack(nextfrag);
                            }
                            if (!threedots)
                            {
                                tok = new Token(TokenType.PERIOD);
                            }
                            break;
                        case '?':
                            tok = new Token(TokenType.QUESTION);
                            break;
                        case ':':
                            tok = new Token(TokenType.COLON);
                            break;
                        case ';':
                            tok = new Token(TokenType.SEMICOLON);
                            break;

                        default:
                            tok = new Token(TokenType.ERROR);
                            break;
                    }
                    break;
                }

                if (frag.type == FragType.EOF)
                {
                    tok = new Token(TokenType.EOF);
                    break;
                }
            }

            return tok;
        }
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");