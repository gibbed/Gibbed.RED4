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

using Gibbed.RED4.ScriptFormats.Definitions;

namespace Gibbed.RED4.ScriptFormats.Instructions
{
    [Instruction(Opcode.Unknown50)]
    [Instruction(Opcode.Unknown51)]
    [Instruction(Opcode.Unknown52)]
    [Instruction(Opcode.Unknown53)]
    [Instruction(Opcode.Unknown54)]
    [Instruction(Opcode.ArrayContains)]
    [Instruction(Opcode.Unknown56)]
    [Instruction(Opcode.Unknown57)]
    [Instruction(Opcode.Unknown58)]
    [Instruction(Opcode.Unknown59)]
    [Instruction(Opcode.Unknown62)]
    [Instruction(Opcode.Unknown63)]
    [Instruction(Opcode.Unknown64)]
    [Instruction(Opcode.Unknown65)]
    [Instruction(Opcode.Unknown66)]
    [Instruction(Opcode.ArrayElement)]
    [Instruction(Opcode.Unknown70)]
    [Instruction(Opcode.Unknown71)]
    [Instruction(Opcode.Unknown72)]
    [Instruction(Opcode.Unknown73)]
    [Instruction(Opcode.Unknown74)]
    [Instruction(Opcode.Unknown75)]
    [Instruction(Opcode.Unknown76)]
    [Instruction(Opcode.Unknown77)]
    [Instruction(Opcode.Unknown79)]
    internal static class _NativeReference2
    {
        public const int ChainCount = 2;

        public static (object, uint) Read(IDefinitionReader reader)
        {
            var definition = reader.ReadReference<NativeDefinition>();
            return (definition, 8);
        }

        public static uint Write(object argument, IDefinitionWriter writer)
        {
            var definition = (NativeDefinition)argument;
            writer.WriteReference(definition);
            return 8;
        }
    }
}
