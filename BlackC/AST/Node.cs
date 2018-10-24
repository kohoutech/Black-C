/* ----------------------------------------------------------------------------
LibOriAST - a library for working with abstract syntax trees
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

namespace Origami.AST
{
    //base class
    public class Node
    {
    }

//- operation nodes -----------------------------------------------------------

    public class AddOpNode : Node
    {
    }

    public class SubtractOpNode : Node
    {
    }

    public class MultiplyOpNode : Node
    {
    }

    public class DivideOpNode : Node
    {
    }

    public class ModOpNode : Node
    {
    }

    public class NotOpNode : Node
    {
    }

    public class AndOpNode : Node
    {
    }

    public class OrOpNode : Node
    {
    }

    public class XorOpNode : Node
    {
    }

    public class ShiftLeftOpNode : Node
    {
    }

    public class ShiftRightOpNode : Node
    {
    }

    public class IncrementOpNode : Node
    {
    }

    public class DecrementOpNode : Node
    {
    }


//- conditional nodes -----------------------------------------------------------

    public class EqualConditNode : Node
    {
    }

    public class NotEqualConditNode : Node
    {
    }

    public class LessThanConditNode : Node
    {
    }

    public class LessEqualConditNode : Node
    {
    }

    public class GreaterThanConditNode : Node
    {
    }

    public class GreaterEqualConditNode : Node
    {
    }

    
//- statement nodes -----------------------------------------------------------

    //base statement node
    public class StatementNode : Node
    {
        StatementNode nextStmt;
    }

    public class IfNode : StatementNode
    {
    }

    public class IfElseNode : StatementNode
    {
    }

    public class WhileNode : StatementNode
    {
    }

    public class DoWhileNode : StatementNode
    {
    }

    public class SwitchNode : StatementNode
    {
    }

    public class CaseNode : StatementNode
    {
    }

    public class ForNode : StatementNode
    {
    }

    public class BreakNode : StatementNode
    {
    }

    public class ContinueNode : StatementNode
    {
    }

    public class ReturnNode : StatementNode
    {
    }
}
