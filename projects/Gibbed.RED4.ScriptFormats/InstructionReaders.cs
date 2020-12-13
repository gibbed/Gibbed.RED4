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
using System.IO;
using System.Text;
using Gibbed.IO;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    internal static class InstructionReaders
    {
        // All of these readers are completely temporary while research is on-going.
        // TODO(gibbed): concrete types for all of this.

        internal static (Instruction, uint) Read(Stream input, Endian endian, ICacheTables tables)
        {
            uint size = 0;

            var op = (Opcode)input.ReadValueU8();
            size++;

            if (_Lookup.TryGetValue(op, out var reader) == false)
            {
                throw new FormatException($"no handler for {op}");
            }

            Instruction instance;
            instance.Op = op;
            if (reader == null)
            {
                instance.Argument = null;
            }
            else
            {
                (var argument, var argumentSize) = reader(input, endian, tables);
                instance.Argument = argument;
                size += argumentSize;
            }
            instance.LoadInfo = default;
            return (instance, size);
        }

        private static (object, uint) ReadValueU8(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueU8(), 1);
        }

        private static (object, uint) ReadValueU16(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueU16(endian), 2);
        }

        private static (object, uint) ReadValueU32(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueU32(endian), 4);
        }

        private static (object, uint) ReadValueU64(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueU64(endian), 8);
        }

        private static (object, uint) ReadValueS8(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueS8(), 1);
        }

        private static (object, uint) ReadValueS16(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueS16(endian), 2);
        }

        private static (object, uint) ReadValueS32(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueS32(endian), 4);
        }

        private static (object, uint) ReadValueS64(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueS64(endian), 8);
        }

        private static (object, uint) ReadValueF32(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueF32(endian), 4);
        }

        private static (object, uint) ReadValueF64(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueF64(endian), 8);
        }

        private static (object, uint) ReadString(Stream input, Endian endian, ICacheTables tables)
        {
            var size = input.ReadValueU32(endian);
            return (input.ReadString((int)size, true, Encoding.UTF8), 4 + size);
        }

        private static (object, uint) ReadJump(Stream input, Endian endian, ICacheTables tables)
        {
            var jumpOffset = input.ReadValueS16(endian);
            jumpOffset += 1 + 2; // make relative to the instruction
            return (jumpOffset, 2);
        }

        private static (object, uint) ReadType(Stream input, Endian endian, ICacheTables tables)
        {
            var definitionIndex = input.ReadValueU32(endian);
            return (new ValueTuple<Definition>(tables.GetDefinition(definitionIndex)), 8);
        }

        private static (object, uint) ReadType<T>(Stream input, Endian endian, ICacheTables tables)
            where T: Definition
        {
            var definitionIndex = input.ReadValueU32(endian);
            var type = tables.GetDefinition<T>(definitionIndex);
            return (new ValueTuple<T>(type), 8);
        }

        private static (object, uint) ReadName(Stream input, Endian endian, ICacheTables tables)
        {
            var nameIndex = input.ReadValueU32(endian);
            return (tables.GetName(nameIndex), 8);
        }

        private static (object, uint) ReadTweakDBId(Stream input, Endian endian, ICacheTables tables)
        {
            var tweakDBIdIndex = input.ReadValueU32(endian);
            return (tables.GetTweakDBId(tweakDBIdIndex), 8);
        }

        private static (object, uint) ReadResource(Stream input, Endian endian, ICacheTables tables)
        {
            var resourceIndex = input.ReadValueU32(endian);
            return (tables.GetResource(resourceIndex), 8);
        }

        private static (object, uint) ReadNativeWithU8(Stream input, Endian endian, ICacheTables tables)
        {
            var definitionIndex = input.ReadValueU32(endian);
            var unknown = input.ReadValueU8();
            return ((tables.GetDefinition<NativeDefinition>(definitionIndex), unknown), 9);
        }

        private static (object, uint) ReadClassWithU8(Stream input, Endian endian, ICacheTables tables)
        {
            var definitionIndex = input.ReadValueU32(endian);
            var unknown = input.ReadValueU8();
            return ((tables.GetDefinition<ClassDefinition>(definitionIndex), unknown), 9);
        }

        private static (object, uint) ReadEnumAssign(Stream input, Endian endian, ICacheTables tables)
        {
            var enumerationIndex = input.ReadValueU32(endian);
            var enumeration = tables.GetDefinition<EnumerationDefinition>(enumerationIndex);
            var enumeralIndex = input.ReadValueU32(endian);
            var enumeral = tables.GetDefinition<EnumeralDefinition>(enumeralIndex);
            return ((enumeration, enumeral), 16);
        }

        private static (object, uint) ReadConstruct(Stream input, Endian endian, ICacheTables tables)
        {
            var parameterCount = input.ReadValueU8();
            var definitionIndex = input.ReadValueU32(endian);
            return ((parameterCount, tables.GetDefinition<ClassDefinition>(definitionIndex)), 9);
        }

        private static (object, uint) ReadCall(Stream input, Endian endian, ICacheTables tables)
        {
            var jumpOffset = input.ReadValueS16(endian);
            var unknown = input.ReadValueU16(endian);
            var definitionIndex = input.ReadValueU32(endian);
            jumpOffset += 1 + 2; // make relative to the instruction
            return ((jumpOffset, unknown, tables.GetDefinition<FunctionDefinition>(definitionIndex)), 12);
        }

        private static (object, uint) ReadUnknown37(Stream input, Endian endian, ICacheTables tables)
        {
            var jumpOffset = input.ReadValueS16(endian);
            var unknown = input.ReadValueU16(endian);
            var unknownIndex = input.ReadValueU32(endian);
            jumpOffset += 1 + 2; // make relative to the instruction
            // Clearly some sort of call, but unknownIndex is not an index into the type table.
            return ((jumpOffset, unknown, unknownIndex), 12);
        }

        private static (object, uint) ReadUnknown21(Stream input, Endian endian, ICacheTables tables)
        {
            var unknown0 = input.ReadValueU16(endian);
            var unknown1 = input.ReadValueU32(endian);
            var unknown2 = input.ReadValueU16(endian);
            var unknown3 = input.ReadValueU16(endian);
            var unknown4 = input.ReadValueU8();
            var unknown5 = input.ReadValueU64(endian);
            return ((unknown0, unknown1, unknown2, unknown3, unknown4, unknown5), 19);
        }

        private static (object, uint) ReadSwitch(Stream input, Endian endian, ICacheTables tables)
        {
            var definitionIndex = input.ReadValueU32(endian);
            var jumpOffset = input.ReadValueS16(endian);
            jumpOffset += 1 + 8 + 2; // make relative to the instruction
            return ((tables.GetDefinition<NativeDefinition>(definitionIndex), jumpOffset), 10);
        }

        private static (object, uint) ReadSwitchCase(Stream input, Endian endian, ICacheTables tables)
        {
            var jumpOffset1 = input.ReadValueS16(endian);
            jumpOffset1 += 1 + 2;
            var jumpOffset2 = input.ReadValueS16(endian);
            jumpOffset2 += 1 + 2 + 2;
            return ((jumpOffset1, jumpOffset2), 4);
        }

        private static (object, uint) ReadUnknown47(Stream input, Endian endian, ICacheTables tables)
        {
            var bytesSize = input.ReadValueU32(endian);
            var bytes = input.ReadBytes((int)bytesSize);
            var unknown = input.ReadValueU8();
            return ((bytes, unknown), bytesSize + 5);
        }

        private delegate (object, uint) ReadDelegate(Stream input, Endian endian, ICacheTables tables);

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
                { (Opcode)33, ReadValueU16 },
                { (Opcode)34, ReadSwitchCase },
                { Opcode.Construct, ReadConstruct },
                { Opcode.Call, ReadCall },
                { Opcode.NativeCall, ReadUnknown37 },
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
