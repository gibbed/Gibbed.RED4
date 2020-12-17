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
    [Instruction(Opcode.VirtualFunc)]
    public struct VirtualFunc
    {
        public const int ChainCount = -1;

        public short JumpOffset;
        public ushort SourceLine;
        public string Function;

        public VirtualFunc(short jumpOffset, ushort sourceLine, string function)
        {
            this.JumpOffset = jumpOffset;
            this.SourceLine = sourceLine;
            this.Function = function;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var jumpOffset = reader.ReadValueS16();
            var sourceLine = reader.ReadValueU16();
            var function = reader.ReadName();
            jumpOffset += 1 + 2; // make relative to the instruction
            return (new VirtualFunc(jumpOffset, sourceLine, function), 12);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var instance = (VirtualFunc)argument;
            var jumpOffset = instance.JumpOffset;
            jumpOffset -= 1 + 2; // make relative to the jump offset
            writer.WriteValueS16(jumpOffset);
            writer.WriteValueU16(instance.SourceLine);
            writer.WriteName(instance.Function);
            return 12;
        }

        public void Deconstruct(out short jumpOffset, out ushort sourceLine, out string function)
        {
            jumpOffset = this.JumpOffset;
            sourceLine = this.SourceLine;
            function = this.Function;
        }

        public override string ToString()
        {
            return $"({this.JumpOffset}, {this.SourceLine}, {this.Function})";
        }
    }
}
