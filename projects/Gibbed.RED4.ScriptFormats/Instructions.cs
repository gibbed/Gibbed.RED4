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

using System;
using System.Collections.Generic;
using System.Linq;
using Gibbed.RED4.ScriptFormats.Instructions;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    internal static class Instructions
    {
        internal static Instruction[] Read(IDefinitionReader reader, uint size)
        {
            var pairs = new List<(Instruction instruction, uint instructionOffset)>();
            var offsetsToIndices = new Dictionary<long, int>();
            uint instructionOffset;
            int index;
            for (instructionOffset = 0, index = 0; instructionOffset < size; index++)
            {
                offsetsToIndices[instructionOffset] = index;

                var loadPosition = reader.Position;
                var (instruction, instructionSize) = Read(reader);
                instruction.LoadPosition = loadPosition;

                pairs.Add((instruction, instructionOffset));
                instructionOffset += instructionSize;
            }
            FixupJumps(pairs, offsetsToIndices);
            return pairs.Select(p => p.instruction).ToArray();
        }

        private static void FixupJumps(List<(Instruction instruction, uint offset)> pairs, Dictionary<long, int> offsets)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                var (instruction, instructionOffset) = pairs[i];
                if (instruction.Opcode.IsJump() == false)
                {
                    continue;
                }
                switch (instruction.Opcode)
                {
                    case Opcode.Switch:
                    {
                        var (type, firstCaseOffset) = (Switch_Offset)instruction.Argument;
                        int index = ResolveJumpOffset(instructionOffset, firstCaseOffset, offsets);
                        instruction.Argument = new Switch(type, index);
                        pairs[i] = (instruction, instructionOffset);
                        break;
                    }
                    case Opcode.SwitchLabel:
                    {
                        var (falseOffset, trueOffset) = (SwitchLabel_Offset)instruction.Argument;
                        int falseIndex = ResolveJumpOffset(instructionOffset, falseOffset, offsets);
                        int trueIndex = ResolveJumpOffset(instructionOffset, trueOffset, offsets);
                        instruction.Argument = new SwitchLabel(falseIndex, trueIndex);
                        pairs[i] = (instruction, instructionOffset);
                        break;
                    }
                    case Opcode.Jump:
                    case Opcode.JumpIfFalse:
                    case Opcode.Skip:
                    case Opcode.Context:
                    {
                        var jumpOffset = (short)instruction.Argument;
                        int index = ResolveJumpOffset(instructionOffset, jumpOffset, offsets);
                        instruction.Argument = index;
                        pairs[i] = (instruction, instructionOffset);
                        break;
                    }
                    case Opcode.Conditional:
                    {
                        var (falseOffset, trueOffset) = (Conditional_Offset)instruction.Argument;
                        int falseIndex = ResolveJumpOffset(instructionOffset, falseOffset, offsets);
                        int trueIndex = ResolveJumpOffset(instructionOffset, trueOffset, offsets);
                        instruction.Argument = new Conditional(falseIndex, trueIndex);
                        pairs[i] = (instruction, instructionOffset);
                        break;
                    }
                    case Opcode.FinalFunc:
                    {
                        var (nextOffset, sourceLine, function) = (FinalFunc_Offset)instruction.Argument;
                        int nextIndex = ResolveJumpOffset(instructionOffset, nextOffset, offsets);
                        instruction.Argument = new FinalFunc(nextIndex, sourceLine, function);
                        pairs[i] = (instruction, instructionOffset);
                        break;
                    }
                    case Opcode.VirtualFunc:
                    {
                        var (nextOffset, sourceLine, name) = (VirtualFunc_Offset)instruction.Argument;
                        int nextIndex = ResolveJumpOffset(instructionOffset, nextOffset, offsets);
                        instruction.Argument = new VirtualFunc(nextIndex, sourceLine, name);
                        pairs[i] = (instruction, instructionOffset);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }

        private static int ResolveJumpOffset(uint instructionOffset, short jumpOffset, Dictionary<long, int> offsets)
        {
            var absoluteOffset = instructionOffset + jumpOffset;
            if (offsets.TryGetValue(absoluteOffset, out var index) == false)
            {
                throw new InvalidOperationException($"bad jump target ({jumpOffset:+#;-#} => {absoluteOffset})");
            }
            return index;
        }

        internal static uint Write(IDefinitionWriter writer, IEnumerable<Instruction> code)
        {
            var instructions = code.ToArray();

            var jumpInfos = StubJumps(instructions);

            uint codeOffset = 0;
            var instructionPositions = new long[instructions.Length];
            var instructionOffsets = new uint[instructions.Length + 1];
            for (int i = 0; i < instructions.Length; i++)
            {
                instructionPositions[i] = writer.Position;
                instructionOffsets[i] = codeOffset;
                var instruction = instructions[i];
                codeOffset += Write(instruction, writer);
            }
            instructionOffsets[instructions.Length] = codeOffset;

            foreach (var (instructionIndex, argument) in jumpInfos)
            {
                var instruction = instructions[instructionIndex];
                if (instruction.Opcode.IsJump() == false)
                {
                    throw new InvalidOperationException();
                }

                switch (instruction.Opcode)
                {
                    case Opcode.Switch:
                    {
                        var (type, firstCaseIndex) = (Switch)argument;
                        var firstCaseOffset = MakeJumpOffset(instructionOffsets, instructionIndex, firstCaseIndex);
                        instruction.Argument = new Switch_Offset(type, firstCaseOffset);
                        break;
                    }
                    case Opcode.SwitchLabel:
                    {
                        var (falseCaseIndex, trueCaseIndex) = (SwitchLabel)argument;
                        var falseCaseOffset = MakeJumpOffset(instructionOffsets, instructionIndex, falseCaseIndex);
                        var trueCaseOffset = MakeJumpOffset(instructionOffsets, instructionIndex, trueCaseIndex);
                        instruction.Argument = new SwitchLabel_Offset(falseCaseOffset, trueCaseOffset);
                        break;
                    }
                    case Opcode.Jump:
                    case Opcode.JumpIfFalse:
                    case Opcode.Skip:
                    case Opcode.Context:
                    {
                        var jumpIndex = (int)argument;
                        var jumpOffset = MakeJumpOffset(instructionOffsets, instructionIndex, jumpIndex);
                        instruction.Argument = jumpOffset;
                        break;
                    }
                    case Opcode.Conditional:
                    {
                        var (falseCaseIndex, trueCaseIndex) = (Conditional)argument;
                        var falseCaseOffset = MakeJumpOffset(instructionOffsets, instructionIndex, falseCaseIndex);
                        var trueCaseOffset = MakeJumpOffset(instructionOffsets, instructionIndex, trueCaseIndex);
                        instruction.Argument = new Conditional_Offset(falseCaseOffset, trueCaseOffset);
                        break;
                    }
                    case Opcode.FinalFunc:
                    {
                        var (nextIndex, sourceLine, function) = (FinalFunc)argument;
                        var nextOffset = MakeJumpOffset(instructionOffsets, instructionIndex, nextIndex);
                        instruction.Argument = new FinalFunc_Offset(nextOffset, sourceLine, function);
                        break;
                    }
                    case Opcode.VirtualFunc:
                    {
                        var (nextIndex, sourceLine, name) = (VirtualFunc)argument;
                        var nextOffset = MakeJumpOffset(instructionOffsets, instructionIndex, nextIndex);
                        instruction.Argument = new VirtualFunc_Offset(nextOffset, sourceLine, name);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }

                var oldPosition = writer.Position;
                writer.Position = instructionPositions[instructionIndex];
                Write(instruction, writer);
                writer.Position = oldPosition;
            }

            return codeOffset;
        }

        private static short MakeJumpOffset(uint[] instructionOffsets, int instructionIndex, int firstCaseIndex)
        {
            var sourceOffset = instructionOffsets[instructionIndex];
            var targetOffset = instructionOffsets[firstCaseIndex];
            var jumpOffset = (long)targetOffset - sourceOffset;
            if (jumpOffset < short.MinValue || jumpOffset > short.MaxValue)
            {
                throw new InvalidOperationException();
            }
            return (short)jumpOffset;
        }

        private static List<(int index, object argument)> StubJumps(Instruction[] instructions)
        {
            var arguments = new List<(int index, object argument)>();
            for (int i = 0; i < instructions.Length; i++)
            {
                var instruction = instructions[i];
                if (instruction.Opcode.IsJump() == false)
                {
                    continue;
                }

                arguments.Add((i, instruction.Argument));
                object argument;
                switch (instruction.Opcode)
                {
                    case Opcode.Switch:
                    {
                        var (type, _) = (Switch)instruction.Argument;
                        argument = new Switch_Offset(type, default);
                        break;
                    }
                    case Opcode.SwitchLabel:
                    {
                        argument = new SwitchLabel_Offset(default, default);
                        break;
                    }
                    case Opcode.Jump:
                    case Opcode.JumpIfFalse:
                    case Opcode.Skip:
                    case Opcode.Context:
                    {
                        argument = default(short);
                        break;
                    }
                    case Opcode.Conditional:
                    {
                        argument = new Conditional_Offset(default, default);
                        break;
                    }
                    case Opcode.FinalFunc:
                    {
                        var (_, sourceLine, function) = (FinalFunc)instruction.Argument;
                        argument = new FinalFunc_Offset(default, sourceLine, function);
                        break;
                    }
                    case Opcode.VirtualFunc:
                    {
                        var (_, sourceLine, name) = (VirtualFunc)instruction.Argument;
                        argument = new VirtualFunc_Offset(default, sourceLine, name);
                        break;
                    }
                    default:
                    {
                        throw new NotSupportedException();
                    }
                }
                instruction.Argument = argument;
                instructions[i] = instruction;
            }
            return arguments;
        }

        internal static (Instruction, uint) Read(IDefinitionReader reader)
        {
            uint size = 0;

            var opcode = (Opcode)reader.ReadValueU8();
            size++;

            var (_, handler, _) = InstructionInfo.GetInternal(opcode);

            Instruction instance;
            instance.Opcode = opcode;
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
            instance.LoadPosition = -1;
            return (instance, size);
        }

        internal static uint Write(Instruction instruction, IDefinitionWriter writer)
        {
            var opcode = instruction.Opcode;

            uint size = 0;

            writer.WriteValueU8((byte)opcode);
            size++;

            var (_, _, handler) = InstructionInfo.GetInternal(opcode);
            if (handler != null)
            {
                size += handler(instruction.Argument, writer);
            }

            return size;
        }
    }
}
