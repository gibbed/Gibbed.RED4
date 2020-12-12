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
using System.IO;
using Gibbed.IO;

namespace Gibbed.RED4.ScriptFormats.ScriptedTypes
{
    public class ClassType : ScriptedType
    {
        private static readonly ClassFlags KnownFlags =
            ClassFlags.Unknown0 | ClassFlags.IsAbstract |
            ClassFlags.Unknown2 | ClassFlags.IsClass |
            ClassFlags.HasFunctions | ClassFlags.Unknown5 |
            ClassFlags.IsImportOnly | ClassFlags.Unknown8;

        public override ScriptedTypeType Type => ScriptedTypeType.Enumeral;

        internal override void Serialize(Stream output, Endian endian, ICacheTables tables)
        {
            throw new NotImplementedException();
        }

        internal override void Deserialize(Stream input, Endian endian, ICacheTables tables)
        {
            var visibility = (Visibility)input.ReadValueU8();

            var flags = (ClassFlags)input.ReadValueU16();
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != ClassFlags.None)
            {
                throw new FormatException();
            }

            var baseTypeIndex = input.ReadValueU32(endian);
            var baseType = tables.GetType<ClassType>(baseTypeIndex);

            var functions = (flags & ClassFlags.HasFunctions) != 0
                ? ReadTypeArray<FunctionType>(input, endian, tables)
                : null;

            ScriptedType[] unknown20s;
            if ((flags & ClassFlags.Unknown5) != 0)
            {
                unknown20s = ReadTypeArray<PropertyType>(input, endian, tables);
            }

            ScriptedType[] unknown30s;
            if ((flags & ClassFlags.Unknown8) != 0)
            {
                unknown30s = ReadTypeArray<PropertyType>(input, endian, tables);
            }
        }
    }
}
