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

namespace Gibbed.RED4.ScriptFormats.Instructions
{
    [Instruction(Opcode.Unknown21)]
    internal static class Unknown21
    {
        public const int ChainCount = 1;

        public static (object, uint) Read(IDefinitionReader reader)
        {
            var unknown0 = reader.ReadValueU16();
            var unknown1 = reader.ReadValueU32();
            var unknown2 = reader.ReadValueU16();
            var unknown3 = reader.ReadValueU16();
            var unknown4 = reader.ReadValueU8();
            var unknown5 = reader.ReadValueU64();
            return ((unknown0, unknown1, unknown2, unknown3, unknown4, unknown5), 19);
        }

        public static uint Write(object argument, IDefinitionWriter writer)
        {
            var (unknown0, unknown1, unknown2, unknown3, unknown4, unknown5) = ((ushort, uint, ushort, ushort, byte, ulong))argument;
            writer.WriteValueU16(unknown0);
            writer.WriteValueU32(unknown1);
            writer.WriteValueU16(unknown2);
            writer.WriteValueU16(unknown3);
            writer.WriteValueU8(unknown4);
            writer.WriteValueU64(unknown5);
            return 19;
        }
    }
}
