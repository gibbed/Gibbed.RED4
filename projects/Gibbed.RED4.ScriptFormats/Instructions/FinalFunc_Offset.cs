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
    [Instruction(Opcode.FinalFunc)]
    internal struct FinalFunc_Offset
    {
        public const int ChainCount = -1;

        public short NextOffset;
        public ushort SourceLine;
        public FunctionDefinition Function;

        public FinalFunc_Offset(short nextOffset, ushort sourceLine, FunctionDefinition function)
        {
            this.NextOffset = nextOffset;
            this.SourceLine = sourceLine;
            this.Function = function;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var nextOffset = reader.ReadValueS16();
            var sourceLine = reader.ReadValueU16();
            var function = reader.ReadReference<FunctionDefinition>();
            nextOffset += 1 + 2; // make relative to the instruction
            return (new FinalFunc_Offset(nextOffset, sourceLine, function), 12);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var (jnextOffset, sourceLine, function) = (FinalFunc_Offset)argument;
            jnextOffset -= 1 + 2; // make relative to the jump offset;
            writer.WriteValueS16(jnextOffset);
            writer.WriteValueU16(sourceLine);
            writer.WriteReference(function);
            return 12;
        }

        public void Deconstruct(out short nextOffset, out ushort sourceLine, out FunctionDefinition function)
        {
            nextOffset = this.NextOffset;
            sourceLine = this.SourceLine;
            function = this.Function;
        }

        public override string ToString()
        {
            return $"({this.NextOffset}, {this.SourceLine}, {this.Function})";
        }
    }
}
