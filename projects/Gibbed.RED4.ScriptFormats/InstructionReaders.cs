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

namespace Gibbed.RED4.ScriptFormats.ScriptedTypes
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

        private static (object, uint) ReadValueS16(Stream input, Endian endian, ICacheTables tables)
        {
            return (input.ReadValueS16(endian), 2);
        }

        private static (object, uint) ReadString(Stream input, Endian endian, ICacheTables tables)
        {
            var size = input.ReadValueU32(endian);
            return (input.ReadString((int)size, true, Encoding.UTF8), 4 + size);
        }

        private static (object, uint) ReadType(Stream input, Endian endian, ICacheTables tables)
        {
            var typeIndex = input.ReadValueU32(endian);
            return (tables.GetType(typeIndex), 8);
        }

        private static (object, uint) ReadType<T>(Stream input, Endian endian, ICacheTables tables)
            where T: ScriptedType
        {
            var typeIndex = input.ReadValueU32(endian);
            var type = tables.GetType<T>(typeIndex);
            return (type, 8);
        }

        private static (object, uint) ReadLocal(Stream input, Endian endian, ICacheTables tables)
        {
            var typeIndex = input.ReadValueU32(endian);
            return (tables.GetType<LocalType>(typeIndex), 8);
        }

        private static (object, uint) ReadName(Stream input, Endian endian, ICacheTables tables)
        {
            var nameIndex = input.ReadValueU32(endian);
            return (("<name>", tables.GetName(nameIndex)), 8);
        }

        private static (object, uint) ReadResource(Stream input, Endian endian, ICacheTables tables)
        {
            var resourceIndex = input.ReadValueU32(endian);
            return (("<resource>", tables.GetResource(resourceIndex)), 8);
        }

        private static (object, uint) ReadNativeWithU8(Stream input, Endian endian, ICacheTables tables)
        {
            var typeIndex = input.ReadValueU32(endian);
            var unknown = input.ReadValueU8();
            return ((tables.GetType<NativeType>(typeIndex), unknown), 9);
        }

        private static (object, uint) ReadClassWithU8(Stream input, Endian endian, ICacheTables tables)
        {
            var typeIndex = input.ReadValueU32(endian);
            var unknown = input.ReadValueU8();
            return ((tables.GetType<ClassType>(typeIndex), unknown), 9);
        }

        private static (object, uint) ReadEnumAssign(Stream input, Endian endian, ICacheTables tables)
        {
            var enumerationTypeIndex = input.ReadValueU32(endian);
            var enumerationType = tables.GetType<EnumerationType>(enumerationTypeIndex);
            var enumeralTypeIndex = input.ReadValueU32(endian);
            var enumeralType = tables.GetType<EnumeralType>(enumeralTypeIndex);
            return ((enumerationType, enumeralType), 16);
        }

        private static (object, uint) ReadCall(Stream input, Endian endian, ICacheTables tables)
        {
            var jumpOffset = input.ReadValueU16(endian);
            var unknown = input.ReadValueU16(endian);
            var typeIndex = input.ReadValueU32(endian);
            return ((jumpOffset, unknown, tables.GetType<FunctionType>(typeIndex)), 12);
        }

        private static (object, uint) ReadUnknown37(Stream input, Endian endian, ICacheTables tables)
        {
            var jumpOffset = input.ReadValueU16(endian);
            var unknown = input.ReadValueU16(endian);
            var unknownIndex = input.ReadValueU32(endian);
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

        private static (object, uint) ReadUnknown28(Stream input, Endian endian, ICacheTables tables)
        {
            var typeIndex = input.ReadValueU32(endian);
            var unknown = input.ReadValueU16(endian);
            return ((tables.GetType<NativeType>(typeIndex), unknown), 10);
        }

        private static (object, uint) ReadUnknown29_34(Stream input, Endian endian, ICacheTables tables)
        {
            var unknown0 = input.ReadValueU16(endian);
            var unknown1 = input.ReadValueU16(endian);
            return ((unknown0, unknown1), 4);
        }

        private static (object, uint) ReadUnknown35(Stream input, Endian endian, ICacheTables tables)
        {
            var unknown = input.ReadValueU8();
            var typeIndex = input.ReadValueU32(endian);
            return ((unknown, tables.GetType<ClassType>(typeIndex)), 9);
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
                { Opcode.End, null },
                { (Opcode)1, null },
                { Opcode.ResultConstantOne, null },
                { Opcode.ResultConstantZero, null },
                { (Opcode)4, ReadValueU8 },
                { (Opcode)5, ReadValueU16 },
                { (Opcode)6, ReadValueU32 },
                { (Opcode)7, ReadValueU64 },
                { (Opcode)8, ReadValueU8 },
                { (Opcode)9, ReadValueU16 },
                { (Opcode)10, ReadValueU32 },
                { (Opcode)11, ReadValueU64 },
                { (Opcode)12, ReadValueU32 },
                { (Opcode)13, ReadValueU64 },
                { (Opcode)14, ReadType },
                { Opcode.EnumAssign, ReadEnumAssign },
                { (Opcode)16, ReadString },
                { (Opcode)17, ReadName },
                { (Opcode)18, ReadResource },
                { Opcode.ResultConstantTrue, null },
                { Opcode.ResultConstantFalse, null },
                { (Opcode)21, ReadUnknown21 },
                { Opcode.LocalAssign, null },
                { (Opcode)23, null },
                { Opcode.LocalRef, ReadLocal },
                { (Opcode)25, ReadType },
                { (Opcode)26, ReadType },
                { (Opcode)27, null },
                { (Opcode)28, ReadUnknown28 },
                { (Opcode)29, ReadUnknown29_34 },
                { (Opcode)30, null },
                { Opcode.Jump, ReadValueS16 },
                { Opcode.JumpFalse, ReadValueS16 },
                { (Opcode)33, ReadValueU16 },
                { (Opcode)34, ReadUnknown29_34 },
                { (Opcode)35, ReadUnknown35 },
                { Opcode.Call, ReadCall },
                { (Opcode)37, ReadUnknown37 },
                { (Opcode)38, null },
                { (Opcode)39, null },
                { (Opcode)40, ReadType },
                { (Opcode)41, ReadValueU16 },
                { (Opcode)42, ReadType },
                { (Opcode)43, ReadType },
                { (Opcode)44, ReadType },
                { (Opcode)45, null },
                { (Opcode)46, null },
                { (Opcode)47, ReadUnknown47 },
                { (Opcode)48, ReadType<NativeType> },
                { (Opcode)49, ReadType<NativeType> },
                { (Opcode)50, ReadType<NativeType> },
                { (Opcode)51, ReadType<NativeType> },
                { (Opcode)52, ReadType<NativeType> },
                { (Opcode)53, ReadType<NativeType> },
                { (Opcode)54, ReadType<NativeType> },
                { (Opcode)55, ReadType<NativeType> },
                { (Opcode)56, ReadType<NativeType> },
                { (Opcode)57, ReadType<NativeType> },
                { (Opcode)58, ReadType<NativeType> },
                { (Opcode)59, ReadType<NativeType> },
                { (Opcode)60, ReadType<NativeType> },
                { (Opcode)61, ReadType<NativeType> },
                { (Opcode)62, ReadType<NativeType> },
                { (Opcode)63, ReadType<NativeType> },
                { (Opcode)64, ReadType<NativeType> },
                { (Opcode)65, ReadType<NativeType> },
                { (Opcode)66, ReadType<NativeType> },
                { (Opcode)67, ReadType<NativeType> },
                { (Opcode)68, ReadType<NativeType> },
                { (Opcode)69, ReadType<NativeType> },
                { (Opcode)70, ReadType<NativeType> },
                { (Opcode)71, ReadType<NativeType> },
                { (Opcode)72, ReadType<NativeType> },
                { (Opcode)73, ReadType<NativeType> },
                { (Opcode)74, ReadType<NativeType> },
                { (Opcode)75, ReadType<NativeType> },
                { (Opcode)76, ReadType<NativeType> },
                { (Opcode)77, ReadType<NativeType> },
                { (Opcode)78, ReadType<NativeType> },
                { (Opcode)79, ReadType<NativeType> },
                { (Opcode)80, null },
                { (Opcode)81, null },
                { (Opcode)82, ReadNativeWithU8 },
                { (Opcode)83, ReadNativeWithU8 },
                { (Opcode)84, ReadClassWithU8 },
                { (Opcode)85, ReadType<NativeType> },
                { (Opcode)86, ReadType<NativeType> },
                { (Opcode)87, ReadType<NativeType> },
                { (Opcode)88, null },
                { (Opcode)89, null },
                { (Opcode)90, null },
                { (Opcode)91, null },
                { (Opcode)92, null },
                { (Opcode)93, null },
                { (Opcode)94, null },
                { (Opcode)95, null },
                { (Opcode)96, ReadType<NativeType> },
                { (Opcode)97, ReadType<NativeType> },
                { (Opcode)98, null },
            };
        }
    }
}
