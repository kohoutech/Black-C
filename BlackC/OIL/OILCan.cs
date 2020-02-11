/* ----------------------------------------------------------------------------
LibOriOIL - a library for working with Origami Internal Language
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

using Origami.ENAML;

namespace Origami.OIL
{
    class OILCan
    {
        public string filename;

        public static Module loadOILCan(String filename)
        {
            Module module = new Module();
            EnamlData can = EnamlData.loadFromFile(filename);

            //root.decls.Add(new VarDeclar("i", "int"));
            //root.decls.Add(new VarDeclar("j", "int"));
            //root.decls.Add(new VarDeclar("k", "int"));

            //root.statements.Add(new AssignStmt(new Identifier("i"), new PrimaryIntConst(2)));
            //root.statements.Add(new AssignStmt(new Identifier("j"), new PrimaryIntConst(3)));
            //root.statements.Add(new AssignStmt(new Identifier("k"), new AddOpNode(new PrimaryId("i"), new PrimaryId("j"))));
            //root.statements.Add(new PrintVarStmt(new Identifier("k")));

            return module;
        }

        public static void saveOILCan(Module module, String filename)
        {
        }
    }
}
