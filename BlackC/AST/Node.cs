/* ----------------------------------------------------------------------------
Dynamo - a backend code generator
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

namespace Dynamo.AST
{
    //base class
    class Node
    {
    }

//- operation nodes -----------------------------------------------------------

    class AddOpNode : Node
    {
    }

    class SubtractOpNode : Node
    {
    }

    class MultiplyOpNode : Node
    {
    }

    class DivideOpNode : Node
    {
    }

    class ModOpNode : Node
    {
    }

    class NotOpNode : Node
    {
    }

    class AndOpNode : Node
    {
    }

    class OrOpNode : Node
    {
    }

    class XorOpNode : Node
    {
    }

    class ShiftLeftOpNode : Node
    {
    }

    class ShiftRightOpNode : Node
    {
    }

    class IncrementOpNode : Node
    {
    }

    class DecrementOpNode : Node
    {
    }


//- conditional nodes -----------------------------------------------------------

    class EqualConditNode : Node
    {
    }

    class NotEqualConditNode : Node
    {
    }

    class LessThanConditNode : Node
    {
    }

    class LessEqualConditNode : Node
    {
    }

    class GreaterThanConditNode : Node
    {
    }

    class GreaterEqualConditNode : Node
    {
    }

    
//- statement nodes -----------------------------------------------------------

    //base statement node
    class StatementNode : Node
    {
        StatementNode nextStmt;
    }

    class IfNode : StatementNode
    {
    }

    class IfElseNode : StatementNode
    {
    }

    class WhileNode : StatementNode
    {
    }

    class DoWhileNode : StatementNode
    {
    }

    class SwitchNode : StatementNode
    {
    }

    class CaseNode : StatementNode
    {
    }

    class ForNode : StatementNode
    {
    }

    class BreakNode : StatementNode
    {
    }

    class ContinueNode : StatementNode
    {
    }

    class ReturnNode : StatementNode
    {
    }
}
