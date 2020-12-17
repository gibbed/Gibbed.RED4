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
    [Instruction(Opcode.Unknown48)]
    [Instruction(Opcode.ArraySize)]
    [Instruction(Opcode.Unknown60)]
    [Instruction(Opcode.Unknown67)]
    [Instruction(Opcode.Unknown69)]
    [Instruction(Opcode.Unknown78)]
    [Instruction(Opcode.StructToString)]
    [Instruction(Opcode.Unknown86)]
    [Instruction(Opcode.Unknown87)]
    [Instruction(Opcode.Unknown96)]
    [Instruction(Opcode.Unknown97)]
    internal static class _NativeReference1
    {
        public const int ChainCount = 1;

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
