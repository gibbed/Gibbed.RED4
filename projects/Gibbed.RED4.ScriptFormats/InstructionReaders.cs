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
    internal static class InstructionReaders
    {
        // All of these readers are completely temporary while research is on-going.
        // TODO(gibbed): concrete types for all of this.

        internal static (Instruction, uint) Read(IDefinitionReader reader)
        {
            uint size = 0;

            var op = (Opcode)reader.ReadValueU8();
            size++;

            if (_Lookup.TryGetValue(op, out var opcodeReader) == false)
            {
                throw new FormatException($"no handler for {op}");
            }

            Instruction instance;
            instance.Op = op;
            if (opcodeReader == null)
            {
                instance.Argument = null;
            }
            else
            {
                var (argument, argumentSize) = opcodeReader(reader);
                instance.Argument = argument;
                size += argumentSize;
            }
            instance.LoadInfo = default;
            return (instance, size);
        }

        private static (object, uint) ReadValueU8(IDefinitionReader reader)
        {
            return (reader.ReadValueU8(), 1);
        }

        private static (object, uint) ReadValueU16(IDefinitionReader reader)
        {
            return (reader.ReadValueU16(), 2);
        }

        private static (object, uint) ReadValueU32(IDefinitionReader reader)
        {
            return (reader.ReadValueU32(), 4);
        }

        private static (object, uint) ReadValueU64(IDefinitionReader reader)
        {
            return (reader.ReadValueU64(), 8);
        }

        private static (object, uint) ReadValueS8(IDefinitionReader reader)
        {
            return (reader.ReadValueS8(), 1);
        }

        private static (object, uint) ReadValueS16(IDefinitionReader reader)
        {
            return (reader.ReadValueS16(), 2);
        }

        private static (object, uint) ReadValueS32(IDefinitionReader reader)
        {
            return (reader.ReadValueS32(), 4);
        }

        private static (object, uint) ReadValueS64(IDefinitionReader reader)
        {
            return (reader.ReadValueS64(), 8);
        }

        private static (object, uint) ReadValueF32(IDefinitionReader reader)
        {
            return (reader.ReadValueF32(), 4);
        }

        private static (object, uint) ReadValueF64(IDefinitionReader reader)
        {
            return (reader.ReadValueF64(), 8);
        }

        private static (object, uint) ReadString(IDefinitionReader reader)
        {
            return (reader.ReadStringU32(out var size), 4 + size);
        }

        private static (object, uint) ReadJump(IDefinitionReader reader)
        {
            var jumpOffset = reader.ReadValueS16();
            jumpOffset += 1 + 2; // make relative to the instruction
            return (jumpOffset, 2);
        }

        private static (object, uint) ReadType(IDefinitionReader reader)
        {
            var definition = reader.ReadReference();
            return (new ValueTuple<Definition>(definition), 8);
        }

        private static (object, uint) ReadType<T>(IDefinitionReader reader)
            where T: Definition
        {
            var definition = reader.ReadReference<T>();
            return (new ValueTuple<T>(definition), 8);
        }

        private static (object, uint) ReadName(IDefinitionReader reader)
        {
            return (reader.ReadName(), 8);
        }

        private static (object, uint) ReadTweakDBId(IDefinitionReader reader)
        {
            return (reader.ReadTweakDBId(), 8);
        }

        private static (object, uint) ReadResource(IDefinitionReader reader)
        {
            return (reader.ReadResource(), 8);
        }

        private static (object, uint) ReadNativeWithU8(IDefinitionReader reader)
        {
            var definition = reader.ReadReference<NativeDefinition>();
            var unknown = reader.ReadValueU8();
            return ((definition, unknown), 9);
        }

        private static (object, uint) ReadClassWithU8(IDefinitionReader reader)
        {
            var definition = reader.ReadReference<ClassDefinition>();
            var unknown = reader.ReadValueU8();
            return ((definition, unknown), 9);
        }

        private static (object, uint) ReadEnumAssign(IDefinitionReader reader)
        {
            var enumeration = reader.ReadReference<EnumerationDefinition>();
            var enumeral = reader.ReadReference<EnumeralDefinition>();
            return ((enumeration, enumeral), 16);
        }

        private static (object, uint) ReadConstruct(IDefinitionReader reader)
        {
            var parameterCount = reader.ReadValueU8();
            return ((parameterCount, reader.ReadReference<ClassDefinition>()), 9);
        }

        private static (object, uint) ReadCall(IDefinitionReader reader)
        {
            var jumpOffset = reader.ReadValueS16();
            var unknown = reader.ReadValueU16();
            var definition = reader.ReadReference<FunctionDefinition>();
            jumpOffset += 1 + 2; // make relative to the instruction
            return ((jumpOffset, unknown, definition), 12);
        }

        private static (object, uint) ReadCallName(IDefinitionReader reader)
        {
            var jumpOffset = reader.ReadValueS16();
            var unknown = reader.ReadValueU16();
            var name = reader.ReadName();
            jumpOffset += 1 + 2; // make relative to the instruction
            return ((jumpOffset, unknown, name), 12);
        }

        private static (object, uint) ReadUnknown21(IDefinitionReader reader)
        {
            var unknown0 = reader.ReadValueU16();
            var unknown1 = reader.ReadValueU32();
            var unknown2 = reader.ReadValueU16();
            var unknown3 = reader.ReadValueU16();
            var unknown4 = reader.ReadValueU8();
            var unknown5 = reader.ReadValueU64();
            return ((unknown0, unknown1, unknown2, unknown3, unknown4, unknown5), 19);
        }

        private static (object, uint) ReadSwitch(IDefinitionReader reader)
        {
            var definition = reader.ReadReference<NativeDefinition>();
            var jumpOffset = reader.ReadValueS16();
            jumpOffset += 1 + 8 + 2; // make relative to the instruction
            return ((definition, jumpOffset), 10);
        }

        private static (object, uint) ReadSwitchCase(IDefinitionReader reader)
        {
            var jumpOffset1 = reader.ReadValueS16();
            jumpOffset1 += 1 + 2;
            var jumpOffset2 = reader.ReadValueS16();
            jumpOffset2 += 1 + 2 + 2;
            return ((jumpOffset1, jumpOffset2), 4);
        }

        private static (object, uint) ReadUnknown47(IDefinitionReader reader)
        {
            var bytes = reader.ReadBytes();
            var unknown = reader.ReadValueU8();
            return ((bytes, unknown), (uint)bytes.Length + 5);
        }

        private delegate (object, uint) ReadDelegate(IDefinitionReader reader);

        private static readonly Dictionary<Opcode, ReadDelegate> _Lookup;

        static InstructionReaders()
        {
            _Lookup = new Dictionary<Opcode, ReadDelegate>()
            {
                { Opcode.NoOperation, null },
                { (Opcode)1, null },
                { Opcode.LoadConstantOne, null },
                { Opcode.LoadConstantZero, null },
                { Opcode.LoadInt8, ReadValueS8 },
                { Opcode.LoadInt16, ReadValueS16 },
                { Opcode.LoadInt32, ReadValueS32 },
                { Opcode.LoadInt64, ReadValueS64 },
                { Opcode.LoadUint8, ReadValueU8 },
                { Opcode.LoadUint16, ReadValueU16 },
                { Opcode.LoadUint32, ReadValueU32 },
                { Opcode.LoadUint64, ReadValueU64 },
                { Opcode.LoadFloat, ReadValueF32 },
                { Opcode.LoadDouble, ReadValueF64 },
                { Opcode.LoadName, ReadName },
                { Opcode.LoadEnumeral, ReadEnumAssign },
                { Opcode.LoadString, ReadString },
                { Opcode.LoadTweakDBId, ReadTweakDBId },
                { Opcode.LoadResource, ReadResource },
                { Opcode.LoadConstantTrue, null },
                { Opcode.LoadConstantFalse, null },
                { (Opcode)21, ReadUnknown21 },
                { Opcode.StoreRef, null },
                { (Opcode)23, null },
                { Opcode.RefLocal, ReadType<LocalDefinition> },
                { Opcode.LoadParameter, ReadType<ParameterDefinition> },
                { Opcode.RefProperty, ReadType<PropertyDefinition> },
                { (Opcode)27, null },
                { Opcode.Switch, ReadSwitch },
                { Opcode.SwitchCase, ReadSwitchCase },
                { Opcode.SwitchDefault, null },
                { Opcode.Jump, ReadJump },
                { Opcode.JumpFalse, ReadJump },
                { (Opcode)33, ReadJump },
                { (Opcode)34, ReadSwitchCase },
                { Opcode.Construct, ReadConstruct },
                { Opcode.Call, ReadCall },
                { Opcode.CallName, ReadCallName },
                { Opcode.EndCall, null },
                { Opcode.ReturnWithValue, null },
                { Opcode.LoadProperty, ReadType<PropertyDefinition> },
                { Opcode.AsObject, ReadJump },
                { (Opcode)42, ReadType },
                { (Opcode)43, ReadType },
                { (Opcode)44, ReadType },
                { (Opcode)45, null },
                { (Opcode)46, null },
                { (Opcode)47, ReadUnknown47 },
                { (Opcode)48, ReadType<NativeDefinition> },
                { (Opcode)49, ReadType<NativeDefinition> },
                { (Opcode)50, ReadType<NativeDefinition> },
                { (Opcode)51, ReadType<NativeDefinition> },
                { (Opcode)52, ReadType<NativeDefinition> },
                { (Opcode)53, ReadType<NativeDefinition> },
                { (Opcode)54, ReadType<NativeDefinition> },
                { (Opcode)55, ReadType<NativeDefinition> },
                { (Opcode)56, ReadType<NativeDefinition> },
                { (Opcode)57, ReadType<NativeDefinition> },
                { (Opcode)58, ReadType<NativeDefinition> },
                { (Opcode)59, ReadType<NativeDefinition> },
                { (Opcode)60, ReadType<NativeDefinition> },
                { (Opcode)61, ReadType<NativeDefinition> },
                { (Opcode)62, ReadType<NativeDefinition> },
                { (Opcode)63, ReadType<NativeDefinition> },
                { (Opcode)64, ReadType<NativeDefinition> },
                { (Opcode)65, ReadType<NativeDefinition> },
                { (Opcode)66, ReadType<NativeDefinition> },
                { (Opcode)67, ReadType<NativeDefinition> },
                { (Opcode)68, ReadType<NativeDefinition> },
                { (Opcode)69, ReadType<NativeDefinition> },
                { (Opcode)70, ReadType<NativeDefinition> },
                { (Opcode)71, ReadType<NativeDefinition> },
                { (Opcode)72, ReadType<NativeDefinition> },
                { (Opcode)73, ReadType<NativeDefinition> },
                { (Opcode)74, ReadType<NativeDefinition> },
                { (Opcode)75, ReadType<NativeDefinition> },
                { (Opcode)76, ReadType<NativeDefinition> },
                { (Opcode)77, ReadType<NativeDefinition> },
                { (Opcode)78, ReadType<NativeDefinition> },
                { (Opcode)79, ReadType<NativeDefinition> },
                { (Opcode)80, null },
                { (Opcode)81, null },
                { (Opcode)82, ReadNativeWithU8 },
                { (Opcode)83, ReadNativeWithU8 },
                { (Opcode)84, ReadClassWithU8 },
                { (Opcode)85, ReadType<NativeDefinition> },
                { (Opcode)86, ReadType<NativeDefinition> },
                { (Opcode)87, ReadType<NativeDefinition> },
                { (Opcode)88, null },
                { (Opcode)89, null },
                { (Opcode)90, null },
                { (Opcode)91, null },
                { (Opcode)92, null },
                { (Opcode)93, null },
                { (Opcode)94, null },
                { (Opcode)95, null },
                { (Opcode)96, ReadType<NativeDefinition> },
                { (Opcode)97, ReadType<NativeDefinition> },
                { (Opcode)98, null },
            };
        }
    }
}
