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
using Gibbed.RED4.ScriptFormats.Instructions;

namespace ScriptCacheDumpTest
{
    internal class InstructionGrouper
    {
        private readonly CacheFile _Cache;
        private readonly FunctionDefinition _Function;
        private int _Index;

        private InstructionGrouper(CacheFile cache, FunctionDefinition function)
        {
            this._Cache = cache;
            this._Function = function;
        }

        public static IEnumerable<Group> GroupCode(CacheFile cache, FunctionDefinition function)
        {
            var instance = new InstructionGrouper(cache, function);
            foreach (var group in instance.GroupCode())
            {
                yield return group;
            }
        }

        private IEnumerable<Group> GroupCode()
        {
            var function = this._Function;
            for (_Index = 0; _Index < function.Code.Count; )
            {
                foreach (var group in this.GroupInstruction())
                {
                    yield return group;
                }
            }
        }

        private IEnumerable<Group> GroupInstruction()
        {
            var code = this._Function.Code;
            if (_Index >= code.Count)
            {
                yield break;
            }

            var instruction = code[_Index++];

            IEnumerable<Group> enumerable;

            var chainCount = InstructionInfo.GetChainCount(instruction.Op);
            if (chainCount >= 0)
            {
                enumerable = GroupBasicInstruction(chainCount);
            }
            else if (instruction.Op == Opcode.Constructor)
            {
                var (parameterCount, _) = (Constructor)instruction.Argument;
                enumerable = GroupBasicInstruction(parameterCount);
            }
            else if (instruction.Op == Opcode.FinalFunc)
            {
                var (_, _, function) = (FinalFunc)instruction.Argument;
                var parameterCount = function.Parameters.Count;
                parameterCount++; // EndCall
                enumerable = GroupBasicInstruction(parameterCount);
            }
            else if (instruction.Op == Opcode.VirtualFunc)
            {
                // TODO(gibbed): dodgy af
                /*var (_, _, name) = ((short, ushort, string))instruction.Argument;
                var candidates = this._Cache.Definitions.Where(d => d.Name == name).ToArray();
                if (candidates.Length != 1)
                {
                    enumerable = GroupBasicInstruction(1);
                }
                else
                {
                    var function = (FunctionDefinition)candidates[0];
                    var parameterCount = function.Parameters.Count;
                    parameterCount++; // EndCall
                    enumerable = GroupBasicInstruction(parameterCount);
                }*/
                enumerable = GroupCallNameInstruction();
            }
            else if (instruction.Op == Opcode.Switch)
            {
                enumerable = GroupSwitchInstruction();
            }
            else if (instruction.Op == Opcode.SwitchLabel)
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

        private IEnumerable<Group> GroupCallNameInstruction()
        {
            var code = _Function.Code;
            var count = _Function.Code.Count;
            while (_Index < count)
            {
                var instruction = code[_Index];
                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }
                if (instruction.Op == Opcode.ParamEnd)
                {
                    yield break;
                }
            }
        }

        private IEnumerable<Group> GroupSwitchInstruction()
        {
            foreach (var group in GroupInstruction())
            {
                yield return group;
            }

            var code = _Function.Code;
            var count = _Function.Code.Count;
            while (_Index < count)
            {
                var instruction = code[_Index];
                if (instruction.Op != Opcode.SwitchLabel &&
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

            var code = _Function.Code;
            var count = _Function.Code.Count;
            while (_Index < count)
            {
                var instruction = code[_Index];
                if (instruction.Op == Opcode.SwitchLabel ||
                    instruction.Op == Opcode.SwitchDefault)
                {
                    yield break;
                }
                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }
                if (instruction.Op == Opcode.Return)
                {
                    yield break;
                }
            }
        }

        private IEnumerable<Group> GroupSwitchDefaultInstruction()
        {
            var code = _Function.Code;
            var count = _Function.Code.Count;
            while (_Index < count)
            {
                var instruction = code[_Index];
                if (instruction.Op == Opcode.SwitchDefault)
                {
                    yield break;
                }
                _Index++;
                foreach (var group in GroupInstruction())
                {
                    yield return group;
                }
                if (instruction.Op == Opcode.Return)
                {
                    yield break;
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
