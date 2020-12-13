/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using Gibbed.RED4.ScriptFormats;
using Gibbed.RED4.ScriptFormats.Definitions;

namespace ScriptCacheDumpTest
{
    internal class InstructionGrouper
    {
        private readonly FunctionDefinition _Function;
        private int _Index;

        private InstructionGrouper(FunctionDefinition function)
        {
            this._Function = function;
        }

        public static IEnumerable<Group> GroupBody(FunctionDefinition function)
        {
            var instance = new InstructionGrouper(function);
            foreach (var group in instance.GroupBody())
            {
                yield return group;
            }
        }

        private IEnumerable<Group> GroupBody()
        {
            var function = this._Function;
            for (_Index = 0; _Index < function.Body.Length; )
            {
                foreach (var group in this.GroupInstruction())
                {
                    yield return group;
                }
            }
        }

        private IEnumerable<Group> GroupInstruction()
        {
            var body = this._Function.Body;
            if (_Index >= body.Length)
            {
                yield break;
            }

            var instruction = body[_Index++];

            IEnumerable<Group> enumerable;

            var info = OpcodeInfo.Get(instruction.Op);
            if (info.ChainCount >= 0)
            {
                enumerable = GroupBasicInstruction(info.ChainCount);
            }
            else if (instruction.Op == Opcode.Construct)
            {
                (var parameterCount, _) = ((byte, ClassDefinition))instruction.Argument;
                enumerable = GroupBasicInstruction(parameterCount);
            }
            else if (instruction.Op == Opcode.Call)
            {
                (_, _, var functionType) = ((short, ushort, FunctionDefinition))instruction.Argument;
                var parameterCount = functionType.Parameters == null
                    ? 0
                    : functionType.Parameters.Length;
                parameterCount++; // EndCall
                enumerable = GroupBasicInstruction(parameterCount);
            }
            else if (instruction.Op == Opcode.Switch)
            {
                enumerable = GroupSwitchInstruction();
            }
            else if (instruction.Op == Opcode.SwitchCase)
            {
                enumerable = GroupSwitchCaseInstruction();
            }
            else if (instruction.Op == Opcode.SwitchDefault)
            {
                enumerable = GroupSwitchDefaultInstruction();
            }
            else
            {
                throw new NotImplementedException();
            }

            var group = new Group(instruction);
            foreach (var child in enumerable)
            {
                group.Children.Add(child);
            }
            yield return group;
        }

        private IEnumerable<Group> GroupBasicInstruction(int count)
        {
            for (int i = 0; i < count; i++)
            {
                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }
            }
        }

        private IEnumerable<Group> GroupSwitchInstruction()
        {
            foreach (var group in GroupInstruction())
            {
                yield return group;
            }

            var body = _Function.Body;
            var count = _Function.Body.Length;
            while (_Index < count)
            {
                var instruction = body[_Index];
                if (instruction.Op != Opcode.SwitchCase &&
                    instruction.Op != Opcode.SwitchDefault)
                {
                    yield break;
                }

                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }

                if (instruction.Op == Opcode.SwitchDefault)
                {
                    yield break;
                }
            }
        }

        private IEnumerable<Group> GroupSwitchCaseInstruction()
        {
            foreach (var group in GroupInstruction())
            {
                yield return group;
            }

            var body = _Function.Body;
            var count = _Function.Body.Length;
            while (_Index < count)
            {
                var instruction = body[_Index];
                if (instruction.Op == Opcode.SwitchCase ||
                    instruction.Op == Opcode.SwitchDefault)
                {
                    yield break;
                }
                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }
            }
        }

        private IEnumerable<Group> GroupSwitchDefaultInstruction()
        {
            var body = _Function.Body;
            var count = _Function.Body.Length;
            while (_Index < count)
            {
                var instruction = body[_Index];
                if (instruction.Op == Opcode.SwitchDefault)
                {
                    yield break;
                }
                _Index++;
                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }
            }
        }

        public class Group
        {
            public Instruction Instruction { get; set; }
            public List<Group> Children { get; }

            public Group(Instruction instruction)
            {
                this.Instruction = instruction;
                this.Children = new List<Group>();
            }
        }
    }
}
