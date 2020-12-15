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

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    public class NativeDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Native;

        public byte Unknown24 { get; set; }
        public Definition Type { get; set; }
        public uint Unknown20 { get; set; }

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8(this.Unknown24);

            if (this.Unknown24 >= 2 && this.Unknown24 <= 6)
            {
                writer.WriteReference(this.Type);
            }

            if (this.Unknown24 == 5)
            {
                writer.WriteValueU32(this.Unknown20);
            }
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var unknown24 = reader.ReadValueU8();

            this.Unknown24 = unknown24;
            this.Type = unknown24 >= 2 && unknown24 <= 6 ? reader.ReadReference() : null;
            this.Unknown20 = unknown24 == 5 ? reader.ReadValueU32() : 0;
        }
    }
}
