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
    [Instruction(Opcode.Switch)]
    public struct Switch
    {
        public const int ChainCount = -1;

        public NativeDefinition Type;
        public short FirstCaseOffset;

        public Switch(NativeDefinition type, short firstCaseOffset)
        {
            this.Type = type;
            this.FirstCaseOffset = firstCaseOffset;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var type = reader.ReadReference<NativeDefinition>();
            var firstCaseOffset = reader.ReadValueS16();
            firstCaseOffset += 1 + 8 + 2; // make relative to the instruction
            return (new Switch(type, firstCaseOffset), 10);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var (type, firstCaseOffset) = (Switch)argument;
            firstCaseOffset -= 1 + 8 + 2; // make relative to the jump offset
            writer.WriteReference(type);
            writer.WriteValueS16(firstCaseOffset);
            return 10;
        }

        public void Deconstruct(out NativeDefinition type, out short firstCaseOffset)
        {
            type = this.Type;
            firstCaseOffset = this.FirstCaseOffset;
        }

        public override string ToString()
        {
            return $"({this.Type}, {this.FirstCaseOffset})";
        }
    }
}
