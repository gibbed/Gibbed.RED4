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
    [Instruction(Opcode.SwitchLabel)]
    public struct SwitchLabel
    {
        public const int ChainCount = -1;

        public short FalseOffset;
        public short TrueOffset;

        public SwitchLabel(short falseOffset, short trueOffset)
        {
            this.FalseOffset = falseOffset;
            this.TrueOffset = trueOffset;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var falseOffset = reader.ReadValueS16();
            falseOffset += 1 + 2; // make relative to the instruction
            var trueOffset = reader.ReadValueS16();
            trueOffset += 1 + 2 + 2; // make relative to the instruction
            return (new SwitchLabel(falseOffset, trueOffset), 4);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var (falseOffset, trueOffset) = (SwitchLabel)argument;
            falseOffset -= 1 + 2; // make relative to the jump offset
            trueOffset -= 1 + 2 + 2; // make relative to the jump offset
            writer.WriteValueS16(falseOffset);
            writer.WriteValueS16(trueOffset);
            return 4;
        }

        public void Deconstruct(out short falseOffset, out short trueOffset)
        {
            falseOffset = this.FalseOffset;
            trueOffset = this.TrueOffset;
        }

        public override string ToString()
        {
            return $"({this.FalseOffset}, {this.TrueOffset})";
        }
    }
}
