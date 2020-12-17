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
    [Instruction(Opcode.DynamicCast)]
    public struct DynamicCast
    {
        public const int ChainCount = 1;

        public ClassDefinition Type;
        public byte Unknown;

        public DynamicCast(ClassDefinition type, byte unknown)
        {
            this.Type = type;
            this.Unknown = unknown;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var type = reader.ReadReference<ClassDefinition>();
            var unknown = reader.ReadValueU8();
            return (new DynamicCast(type, unknown), 9);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var (type, unknown) = (DynamicCast)argument;
            writer.WriteReference(type);
            writer.WriteValueU8(unknown);
            return 9;
        }

        public void Deconstruct(out ClassDefinition type, out byte unknown)
        {
            type = this.Type;
            unknown = this.Unknown;
        }

        public override string ToString()
        {
            return $"({this.Type}, {this.Unknown})";
        }
    }
}
