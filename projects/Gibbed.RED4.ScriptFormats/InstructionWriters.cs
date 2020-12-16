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

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    internal static class InstructionWriters
    {
        internal static uint Write(Instruction instruction, IDefinitionWriter writer)
        {
            var op = instruction.Op;

            uint size = 0;

            writer.WriteValueU8((byte)op);
            size++;

            if (_Lookup.TryGetValue(op, out var opcodeWriter) == false)
            {
                throw new FormatException($"no handler for {op}");
            }

            if (opcodeWriter != null)
            {
                size += opcodeWriter(instruction.Argument, writer);
            }

            return size;
        }

        private static uint WriteValueU8(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueU8((byte)argument);
            return 1;
        }

        private static uint WriteValueU16(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueU16((ushort)argument);
            return 2;
        }

        private static uint WriteValueU32(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueU32((uint)argument);
            return 4;
        }

        private static uint WriteValueU64(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueU64((ulong)argument);
            return 8;
        }

        private static uint WriteValueS8(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueS8((sbyte)argument);
            return 1;
        }

        private static uint WriteValueS16(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueS16((short)argument);
            return 2;
        }

        private static uint WriteValueS32(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueS32((int)argument);
            return 4;
        }

        private static uint WriteValueS64(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueS64((long)argument);
            return 8;
        }

        private static uint WriteValueF32(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueF32((float)argument);
            return 4;
        }

        private static uint WriteValueF64(object argument, IDefinitionWriter writer)
        {
            writer.WriteValueF64((double)argument);
            return 8;
        }

        private static uint WriteString(object argument, IDefinitionWriter writer)
        {
            var size = writer.WriteStringU32((string)argument);
            return 4 + size;
        }

        private static uint WriteJump(object argument, IDefinitionWriter writer)
        {
            var jumpOffset = (short)argument;
            jumpOffset -= 1 + 2; // make relative to the jump offset;
            writer.WriteValueS16(jumpOffset);
            return 2;
        }

        private static uint WriteType(object argument, IDefinitionWriter writer)
        {
            var definition = ((ValueTuple<Definition>)argument).Item1;
            writer.WriteReference(definition);
            return 8;
        }

        private static uint WriteType<T>(object argument, IDefinitionWriter writer)
            where T : Definition
        {
            var definition = ((ValueTuple<T>)argument).Item1;
            writer.WriteReference(definition);
            return 8;
        }

        private static uint WriteName(object argument, IDefinitionWriter writer)
        {
            writer.WriteName((string)argument);
            return 8;
        }

        private static uint WriteTweakDBId(object argument, IDefinitionWriter writer)
        {
            writer.WriteTweakDBId((string)argument);
            return 8;
        }

        private static uint WriteResource(object argument, IDefinitionWriter writer)
        {
            writer.WriteResource((string)argument);
            return 8;
        }

        private static uint WriteNativeWithU8(object argument, IDefinitionWriter writer)
        {
            var (definition, unknown) = ((NativeDefinition, byte))argument;
            writer.WriteReference(definition);
            writer.WriteValueU8(unknown);
            return 9;
        }

        private static uint WriteClassWithU8(object argument, IDefinitionWriter writer)
        {
            var (definition, unknown) = ((ClassDefinition, byte))argument;
            writer.WriteReference(definition);
            writer.WriteValueU8(unknown);
            return 9;
        }

        private static uint WriteEnumAssign(object argument, IDefinitionWriter writer)
        {
            var (enumeration, enumeral) = ((EnumerationDefinition, EnumeralDefinition))argument;
            writer.WriteReference(enumeration);
            writer.WriteReference(enumeral);
            return 16;
        }

        private static uint WriteConstruct(object argument, IDefinitionWriter writer)
        {
            var (parameterCount, definition) = ((byte, ClassDefinition))argument;
            writer.WriteValueU8(parameterCount);
            writer.WriteReference(definition);
            return 9;
        }

        private static uint WriteCall(object argument, IDefinitionWriter writer)
        {
            var (jumpOffset, unknown, definition) = ((short, ushort, FunctionDefinition))argument;
            jumpOffset -= 1 + 2; // make relative to the jump offset;
            writer.WriteValueS16(jumpOffset);
            writer.WriteValueU16(unknown);
            writer.WriteReference(definition);
            return 12;
        }

        private static uint WriteCallName(object argument, IDefinitionWriter writer)
        {
            var (jumpOffset, unknown, name) = ((short, ushort, string))argument;
            jumpOffset -= 1 + 2; // make relative to the jump offset
            writer.WriteValueS16(jumpOffset);
            writer.WriteValueU16(unknown);
            writer.WriteName(name);
            return 12;
        }

        private static uint WriteUnknown21(object argument, IDefinitionWriter writer)
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

        private static uint WriteSwitch(object argument, IDefinitionWriter writer)
        {
            var (definition, jumpOffset) = ((NativeDefinition, short))argument;
            jumpOffset -= 1 + 8 + 2; // make relative to the jump offset
            writer.WriteReference(definition);
            writer.WriteValueS16(jumpOffset);
            return 10;
        }

        private static uint WriteSwitchCase(object argument, IDefinitionWriter writer)
        {
            var (jumpOffset1, jumpOffset2) = ((short, short))argument;
            jumpOffset1 -= 1 + 2; // make relative to the jump offset
            jumpOffset2 -= 1 + 2 + 2; // make relative to the jump offset
            writer.WriteValueS16(jumpOffset1);
            writer.WriteValueS16(jumpOffset2);
            return 4;
        }

        private static uint WriteUnknown47(object argument, IDefinitionWriter writer)
        {
            var (bytes, unknown) = ((byte[], byte))argument;
            writer.WriteBytes(bytes);
            writer.WriteValueU8(unknown);
            return (uint)bytes.Length + 5;
        }

        private delegate uint WriteDelegate(object argument, IDefinitionWriter writer);

        private static readonly Dictionary<Opcode, WriteDelegate> _Lookup;

        static InstructionWriters()
        {
            _Lookup = new Dictionary<Opcode, WriteDelegate>()
            {
                { Opcode.NoOperation, null },
                { (Opcode)1, null },
                { Opcode.LoadConstantOne, null },
                { Opcode.LoadConstantZero, null },
                { Opcode.LoadInt8, WriteValueS8 },
                { Opcode.LoadInt16, WriteValueS16 },
                { Opcode.LoadInt32, WriteValueS32 },
                { Opcode.LoadInt64, WriteValueS64 },
                { Opcode.LoadUint8, WriteValueU8 },
                { Opcode.LoadUint16, WriteValueU16 },
                { Opcode.LoadUint32, WriteValueU32 },
                { Opcode.LoadUint64, WriteValueU64 },
                { Opcode.LoadFloat, WriteValueF32 },
                { Opcode.LoadDouble, WriteValueF64 },
                { Opcode.LoadName, WriteName },
                { Opcode.LoadEnumeral, WriteEnumAssign },
                { Opcode.LoadString, WriteString },
                { Opcode.LoadTweakDBId, WriteTweakDBId },
                { Opcode.LoadResource, WriteResource },
                { Opcode.LoadConstantTrue, null },
                { Opcode.LoadConstantFalse, null },
                { (Opcode)21, WriteUnknown21 },
                { Opcode.StoreRef, null },
                { (Opcode)23, null },
                { Opcode.RefLocal, WriteType<LocalDefinition> },
                { Opcode.LoadParameter, WriteType<ParameterDefinition> },
                { Opcode.RefProperty, WriteType<PropertyDefinition> },
                { (Opcode)27, null },
                { Opcode.Switch, WriteSwitch },
                { Opcode.SwitchCase, WriteSwitchCase },
                { Opcode.SwitchDefault, null },
                { Opcode.Jump, WriteJump },
                { Opcode.JumpFalse, WriteJump },
                { (Opcode)33, WriteJump },
                { (Opcode)34, WriteSwitchCase },
                { Opcode.Construct, WriteConstruct },
                { Opcode.Call, WriteCall },
                { Opcode.CallName, WriteCallName },
                { Opcode.EndCall, null },
                { Opcode.ReturnWithValue, null },
                { Opcode.LoadProperty, WriteType<PropertyDefinition> },
                { Opcode.AsObject, WriteJump },
                { (Opcode)42, WriteType },
                { (Opcode)43, WriteType },
                { (Opcode)44, WriteType },
                { (Opcode)45, null },
                { (Opcode)46, null },
                { (Opcode)47, WriteUnknown47 },
                { (Opcode)48, WriteType<NativeDefinition> },
                { (Opcode)49, WriteType<NativeDefinition> },
                { (Opcode)50, WriteType<NativeDefinition> },
                { (Opcode)51, WriteType<NativeDefinition> },
                { (Opcode)52, WriteType<NativeDefinition> },
                { (Opcode)53, WriteType<NativeDefinition> },
                { (Opcode)54, WriteType<NativeDefinition> },
                { (Opcode)55, WriteType<NativeDefinition> },
                { (Opcode)56, WriteType<NativeDefinition> },
                { (Opcode)57, WriteType<NativeDefinition> },
                { (Opcode)58, WriteType<NativeDefinition> },
                { (Opcode)59, WriteType<NativeDefinition> },
                { (Opcode)60, WriteType<NativeDefinition> },
                { (Opcode)61, WriteType<NativeDefinition> },
                { (Opcode)62, WriteType<NativeDefinition> },
                { (Opcode)63, WriteType<NativeDefinition> },
                { (Opcode)64, WriteType<NativeDefinition> },
                { (Opcode)65, WriteType<NativeDefinition> },
                { (Opcode)66, WriteType<NativeDefinition> },
                { (Opcode)67, WriteType<NativeDefinition> },
                { (Opcode)68, WriteType<NativeDefinition> },
                { (Opcode)69, WriteType<NativeDefinition> },
                { (Opcode)70, WriteType<NativeDefinition> },
                { (Opcode)71, WriteType<NativeDefinition> },
                { (Opcode)72, WriteType<NativeDefinition> },
                { (Opcode)73, WriteType<NativeDefinition> },
                { (Opcode)74, WriteType<NativeDefinition> },
                { (Opcode)75, WriteType<NativeDefinition> },
                { (Opcode)76, WriteType<NativeDefinition> },
                { (Opcode)77, WriteType<NativeDefinition> },
                { (Opcode)78, WriteType<NativeDefinition> },
                { (Opcode)79, WriteType<NativeDefinition> },
                { (Opcode)80, null },
                { (Opcode)81, null },
                { (Opcode)82, WriteNativeWithU8 },
                { (Opcode)83, WriteNativeWithU8 },
                { (Opcode)84, WriteClassWithU8 },
                { (Opcode)85, WriteType<NativeDefinition> },
                { (Opcode)86, WriteType<NativeDefinition> },
                { (Opcode)87, WriteType<NativeDefinition> },
                { (Opcode)88, null },
                { (Opcode)89, null },
                { (Opcode)90, null },
                { (Opcode)91, null },
                { (Opcode)92, null },
                { (Opcode)93, null },
                { (Opcode)94, null },
                { (Opcode)95, null },
                { (Opcode)96, WriteType<NativeDefinition> },
                { (Opcode)97, WriteType<NativeDefinition> },
                { (Opcode)98, null },
            };
        }
    }
}
