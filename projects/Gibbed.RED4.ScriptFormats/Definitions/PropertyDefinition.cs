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
using System.Text;
using Gibbed.IO;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    public class PropertyDefinition : Definition
    {
        private static readonly PropertyFlags KnownFlags =
            PropertyFlags.Unknown0 | PropertyFlags.Unknown1 |
            PropertyFlags.Unknown2 | PropertyFlags.Unknown3 |
            PropertyFlags.Unknown4 | PropertyFlags.Unknown5 |
            PropertyFlags.Unknown6 | PropertyFlags.Unknown7 |
            PropertyFlags.Unknown8 | PropertyFlags.Unknown9 |
            PropertyFlags.Unknown10;

        public Visibility Visibility { get; set; }
        public Definition Type { get; set; }
        public PropertyFlags Flags { get; set; }
        public string UnknownDiscardedString { get; set; }
        public ValueTuple<string, string>[] Unknown58s { get; set; }
        public ValueTuple<string, string>[] Unknown38s { get; set; }

        public override DefinitionType DefinitionType => DefinitionType.Property;

        internal override void Serialize(Stream output, Endian endian, ICacheReferences references)
        {
            throw new NotImplementedException();
        }

        internal override void Deserialize(Stream input, Endian endian, ICacheReferences references)
        {
            var visibility = (Visibility)input.ReadValueU8();
            
            var typeIndex = input.ReadValueU32(endian);
            var type = references.GetDefinition<NativeDefinition>(typeIndex);

            var flags = (PropertyFlags)input.ReadValueU16(endian);
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != PropertyFlags.None)
            {
                throw new FormatException();
            }

            string unknownDiscardedString;
            if ((flags & PropertyFlags.Unknown5) == 0)
            {
                unknownDiscardedString = null;
            }
            else
            {
                var unknownDiscardedStringLength = input.ReadValueU16(endian);
                unknownDiscardedString = input.ReadString(unknownDiscardedStringLength, true, Encoding.UTF8);
            }

            var unknown58Count = input.ReadValueU32(endian);
            var unknown58s = new ValueTuple<string, string>[unknown58Count];
            for (uint i = 0; i < unknown58Count; i++)
            {
                var unknown58_0Length = input.ReadValueU16(endian);
                var unknown58_0 = input.ReadString(unknown58_0Length, true, Encoding.UTF8);
                var unknown58_1Length = input.ReadValueU16(endian);
                var unknown58_1 = input.ReadString(unknown58_1Length, true, Encoding.UTF8);
                unknown58s[i] = new ValueTuple<string, string>(unknown58_0, unknown58_1);
            }

            var unknown38Count = input.ReadValueU32(endian);
            var unknown38s = new ValueTuple<string, string>[unknown38Count];
            for (uint i = 0; i < unknown38Count; i++)
            {
                var unknown38Length = input.ReadValueU16(endian);
                var unknown38 = input.ReadString(unknown38Length, true, Encoding.UTF8);
                var unknown28Length = input.ReadValueU16(endian);
                var unknown28 = input.ReadString(unknown28Length, true, Encoding.UTF8);
                unknown38s[i] = new ValueTuple<string, string>(unknown38, unknown28);
            }

            this.Visibility = visibility;
            this.Type = type;
            this.Flags = flags;
            this.UnknownDiscardedString = unknownDiscardedString;
            this.Unknown58s = unknown58s;
            this.Unknown38s = unknown38s;
        }
    }
}
