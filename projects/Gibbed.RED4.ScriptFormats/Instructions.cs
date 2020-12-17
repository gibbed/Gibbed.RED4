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

using System.Collections.Generic;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    internal static class Instructions
    {
        internal static Instruction[] Read(IDefinitionReader reader, uint count)
        {
            var result = new List<Instruction>();
            for (uint i = 0; i < count;)
            {
                InstructionLoadInfo loadInfo;
                loadInfo.BasePosition = reader.Position;
                loadInfo.Offset = i;
                var (instruction, size) = Read(reader);
                instruction.LoadInfo = loadInfo;
                result.Add(instruction);
                i += size;
            }
            return result.ToArray();
        }

        internal static uint Write(IDefinitionWriter writer, IEnumerable<Instruction> code)
        {
            uint size = 0;
            foreach (var instruction in code)
            {
                size += Write(instruction, writer);
            }
            return size;
        }

        internal static (Instruction, uint) Read(IDefinitionReader reader)
        {
            uint size = 0;

            var op = (Opcode)reader.ReadValueU8();
            size++;

            var (_, handler, _) = InstructionInfo.GetInternal(op);

            Instruction instance;
            instance.Op = op;
            if (handler == null)
            {
                instance.Argument = null;
            }
            else
            {
                var (argument, argumentSize) = handler(reader);
                instance.Argument = argument;
                size += argumentSize;
            }
            instance.LoadInfo = default;
            return (instance, size);
        }

        internal static uint Write(Instruction instruction, IDefinitionWriter writer)
        {
            var op = instruction.Op;

            uint size = 0;

            writer.WriteValueU8((byte)op);
            size++;

            var (_, _, handler) = InstructionInfo.GetInternal(op);
            if (handler != null)
            {
                size += handler(instruction.Argument, writer);
            }

            return size;
        }
    }
}
