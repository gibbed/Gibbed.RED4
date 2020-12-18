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
    [Instruction(Opcode.ArrayResize)]
    [Instruction(Opcode.ArrayFindFirst)]
    [Instruction(Opcode.ArrayFindFirstFast)]
    [Instruction(Opcode.ArrayFindLast)]
    [Instruction(Opcode.ArrayFindLastFast)]
    [Instruction(Opcode.ArrayContains)]
    [Instruction(Opcode.ArrayContainsFast)]
    [Instruction(Opcode.ArrayUnknown57)]
    [Instruction(Opcode.ArrayUnknown58)]
    [Instruction(Opcode.ArrayPushBack)]
    [Instruction(Opcode.ArrayRemove)]
    [Instruction(Opcode.ArrayRemoveFast)]
    [Instruction(Opcode.ArrayGrow)]
    [Instruction(Opcode.ArrayErase)]
    [Instruction(Opcode.ArrayEraseFast)]
    [Instruction(Opcode.ArrayElement)]
    [Instruction(Opcode.StaticArrayFindFirst)]
    [Instruction(Opcode.StaticArrayFindFirstFast)]
    [Instruction(Opcode.StaticArrayFindLast)]
    [Instruction(Opcode.StaticArrayFindLastFast)]
    [Instruction(Opcode.StaticArrayContains)]
    [Instruction(Opcode.StaticArrayContainsFast)]
    [Instruction(Opcode.StaticArrayUnknown76)]
    [Instruction(Opcode.StaticArrayUnknown77)]
    [Instruction(Opcode.StaticArrayElement)]
    internal static class _NativeReference2
    {
        public const int ChainCount = 2;

        public static (object, uint) Read(IDefinitionReader reader)
        {
            var native = reader.ReadReference<NativeDefinition>();
            return (native, 8);
        }

        public static uint Write(object argument, IDefinitionWriter writer)
        {
            var native = (NativeDefinition)argument;
            writer.WriteReference(native);
            return 8;
        }
    }
}
