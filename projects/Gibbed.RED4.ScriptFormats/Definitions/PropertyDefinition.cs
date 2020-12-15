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
    public class PropertyDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Property;

        public PropertyDefinition()
        {
            this.Unknown58s = new List<(string, string)>();
            this.Unknown38s = new List<(string, string)>();
        }

        public Visibility Visibility { get; set; }
        public Definition Type { get; set; }
        public PropertyFlags Flags { get; set; }
        public string UnknownDiscardedString { get; set; }
        public List<(string, string)> Unknown58s { get; }
        public List<(string, string)> Unknown38s { get; }

        private static readonly PropertyFlags KnownFlags =
            PropertyFlags.Unknown0 | PropertyFlags.Unknown1 |
            PropertyFlags.Unknown2 | PropertyFlags.Unknown3 |
            PropertyFlags.Unknown4 | PropertyFlags.Unknown5 |
            PropertyFlags.Unknown6 | PropertyFlags.Unknown7 |
            PropertyFlags.Unknown8 | PropertyFlags.Unknown9 |
            PropertyFlags.Unknown10;

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8((byte)this.Visibility);
            writer.WriteReference(this.Type);
            writer.WriteValueU16((ushort)this.Flags);

            if ((this.Flags & PropertyFlags.Unknown5) != 0)
            {
                writer.WriteStringU16(this.UnknownDiscardedString);
            }

            writer.WriteValueS32(this.Unknown58s.Count);
            foreach (var unknown58 in this.Unknown58s)
            {
                writer.WriteStringU16(unknown58.Item1);
                writer.WriteStringU16(unknown58.Item2);
            }

            writer.WriteValueS32(this.Unknown38s.Count);
            foreach (var unknown38 in this.Unknown38s)
            {
                writer.WriteStringU16(unknown38.Item1);
                writer.WriteStringU16(unknown38.Item2);
            }
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var visibility = (Visibility)reader.ReadValueU8();
            var type = reader.ReadReference<NativeDefinition>();

            var flags = (PropertyFlags)reader.ReadValueU16();
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != PropertyFlags.None)
            {
                throw new FormatException();
            }

            var unknownDiscardedString = (flags & PropertyFlags.Unknown5) != 0
                ? reader.ReadStringU16()
                : null;

            var unknown58Count = reader.ReadValueU32();
            var unknown58s = new ValueTuple<string, string>[unknown58Count];
            for (uint i = 0; i < unknown58Count; i++)
            {
                var unknown58_0 = reader.ReadStringU16();
                var unknown58_1 = reader.ReadStringU16();
                unknown58s[i] = new ValueTuple<string, string>(unknown58_0, unknown58_1);
            }

            var unknown38Count = reader.ReadValueU32();
            var unknown38s = new ValueTuple<string, string>[unknown38Count];
            for (uint i = 0; i < unknown38Count; i++)
            {
                var unknown38 = reader.ReadStringU16();
                var unknown28 = reader.ReadStringU16();
                unknown38s[i] = new ValueTuple<string, string>(unknown38, unknown28);
            }

            this.Unknown58s.Clear();
            this.Unknown38s.Clear();
            this.Visibility = visibility;
            this.Type = type;
            this.Flags = flags;
            this.UnknownDiscardedString = unknownDiscardedString;
            this.Unknown58s.AddRange(unknown58s);
            this.Unknown38s.AddRange(unknown38s);
        }
    }
}
