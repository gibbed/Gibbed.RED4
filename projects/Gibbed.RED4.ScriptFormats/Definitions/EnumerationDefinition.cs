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
    public class EnumerationDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Enumeration;

        public EnumerationDefinition()
        {
            this.Enumerals = new List<EnumeralDefinition>();
        }

        public byte Unknown2A { get; set; }
        public byte Size { get; set; }
        public List<EnumeralDefinition> Enumerals { get; }
        public bool Unknown29 { get; set; }

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8(this.Unknown2A);
            writer.WriteValueU8(this.Size);
            writer.WriteReferences(this.Enumerals);
            writer.WriteValueB8(this.Unknown29);
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            // TODO(gibbed): maybe visibility?
            var unknown2A = reader.ReadValueU8();
            var size = reader.ReadValueU8();
            var enumerals = reader.ReadReferences<EnumeralDefinition>();
            var unknown29 = reader.ReadValueB8();

            this.Unknown2A = unknown2A;
            this.Size = size;
            this.Enumerals.Clear();
            this.Enumerals.AddRange(enumerals);
            this.Unknown29 = unknown29;
        }
    }
}
