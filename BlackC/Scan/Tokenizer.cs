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

using BlackC.Parse;

namespace BlackC.Scan
{
    //handles translation phases 6 & 7

    /*(5.1.1.2) 
        translation phase 6 : Adjacent string literal tokens are concatenated
        translation phase 7 : Whitespace is no longer significant
                              Each preprocessing token is converted into a (C) token
    */

    public class Tokenizer
    {
        Parser parser;
        Preprocessor pp;

        List<PPToken> ppTokens;
        Dictionary<String, TokenType> keywords;

        public Tokenizer(Parser _parser, String filename)
        {
            parser = _parser;

            pp = new Preprocessor(parser, filename);
            ppTokens = new List<PPToken>();

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

        //- token handling ----------------------------------------------------

        public PPToken getPPToken()
        {
            PPToken ppTok = null;
            if (ppTokens.Count != 0)
            {
                ppTok = ppTokens[ppTokens.Count - 1];
                ppTokens.RemoveAt(ppTokens.Count - 1);
            }
            else
            {
                ppTok = pp.getPPToken();
            }
            return ppTok;
        }

        public void replacePPToken(PPToken ppTok)
        {
            ppTokens.Add(ppTok);
        }

        //convert preprocessor tokens (strings) into c tokens as input for the parser

        public Token getToken()
        {
            Token tok = null;
            PPToken ppTok;
            PPToken nextppTok;

            while (true)
            {
                ppTok = getPPToken();

                //ignore spaces, comments & eolns
                if ((ppTok.type == PPTokenType.SPACE) || (ppTok.type == PPTokenType.COMMENT) || (ppTok.type == PPTokenType.EOLN))
                {
                    continue;
                }

                //check if word is keyword or identifier
                if (ppTok.type == PPTokenType.WORD)
                {
                    if (keywords.ContainsKey(ppTok.str))
                    {
                        tok = new Token(keywords[ppTok.str]);
                    }
                    else
                    {
                        tok = new IdentToken(ppTok.str);
                    }
                    break;
                }

                //convert int / float / string / char str into constant value
                if (ppTok.type == PPTokenType.INTEGER)
                {
                    tok = ParseInteger(ppTok.str);
                    break;
                }

                if (ppTok.type == PPTokenType.FLOAT)
                {
                    tok = ParseFloat(ppTok.str);
                    break;
                }

                if (ppTok.type == PPTokenType.STRING)
                {
                    tok = ParseString(ppTok.str);
                    break;
                }

                if (ppTok.type == PPTokenType.CHAR)
                {
                    tok = ParseChar(ppTok.str);
                    break;
                }

                //convert single punctuation chars into punctuation tokens, combining as necessary
                //need 2 lookaheads at most for '...' token
                if (ppTok.type == PPTokenType.PUNCT)
                {
                    char c = ppTok.str[0];
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
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '+'))
                            {
                                tok = new Token(TokenType.PLUSPLUS);
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.PLUSEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.PLUS);
                            }
                            break;
                        case '-':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '-'))
                            {
                                tok = new Token(TokenType.MINUSMINUS);
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.MINUSEQUAL);
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '>'))
                            {
                                tok = new Token(TokenType.ARROW);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.MINUS);
                            }
                            break;
                        case '*':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.MULTEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.STAR);
                            }
                            break;
                        case '/':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.SLASHEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.SLASH);
                            }
                            break;
                        case '%':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.PERCENTEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.PERCENT);
                            }
                            break;
                        case '&':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '&'))
                            {
                                tok = new Token(TokenType.AMPAMP);
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.AMPEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.AMPERSAND);
                            }
                            break;
                        case '|':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '|'))
                            {
                                tok = new Token(TokenType.BARBAR);
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.BAREQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.BAR);
                            }
                            break;
                        case '~':
                            tok = new Token(TokenType.TILDE);
                            break;
                        case '^':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.CARETEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.CARET);
                            }
                            break;


                        case '=':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.EQUALEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.EQUAL);
                            }
                            break;
                        case '!':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.NOTEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.EXCLAIM);
                            }
                            break;
                        case '<':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '<'))
                            {
                                PPToken tok2 = getPPToken();
                                if ((tok2.type == PPTokenType.PUNCT) && (tok2.str[0] == '='))
                                {
                                    tok = new Token(TokenType.LESSLESSEQUAL);   //<<=
                                }
                                else
                                {
                                    replacePPToken(tok2);
                                    tok = new Token(TokenType.LESSLESS);
                                }
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.LESSEQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.LESSTHAN);
                            }
                            break;
                        case '>':
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '>'))
                            {
                                PPToken tok2 = getPPToken();
                                if ((tok2.type == PPTokenType.PUNCT) && (tok2.str[0] == '='))
                                {
                                    tok = new Token(TokenType.GTRGTREQUAL);   //>>=
                                }
                                else
                                {
                                    replacePPToken(tok2);
                                    tok = new Token(TokenType.GTRGTR);
                                }
                            }
                            else if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '='))
                            {
                                tok = new Token(TokenType.GTREQUAL);
                            }
                            else
                            {
                                replacePPToken(nextppTok);
                                tok = new Token(TokenType.GTRTHAN);
                            }
                            break;

                        case ',':
                            tok = new Token(TokenType.COMMA);
                            break;
                        case '.':
                            bool threedots = false;
                            nextppTok = getPPToken();
                            if ((nextppTok.type == PPTokenType.PUNCT) && (nextppTok.str[0] == '.'))
                            {
                                PPToken tok2 = getPPToken();
                                if ((tok2.type == PPTokenType.PUNCT) && (tok2.str[0] == '.'))
                                {
                                    tok = new Token(TokenType.ELLIPSIS);        //...
                                    threedots = true;
                                }
                                else
                                {
                                    replacePPToken(nextppTok);
                                    replacePPToken(tok2);
                                }
                            }
                            else
                            {
                                replacePPToken(nextppTok);
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

                //last but not least - end of file
                if (ppTok.type == PPTokenType.EOF)
                {
                    tok = new Token(TokenType.EOF);
                    break;
                }
            }

            return tok;
        }

        //- token conversion ---------------------------------------

        //the integer token we get from the scanner should be well-formed
        public Token ParseInteger(String numstr)
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

        //the number fragment we get from the scanner should be well-formed
        public Token ParseFloat(String numstr)
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
    }
}

//Console.WriteLine("There's no sun in the shadow of the wizard");