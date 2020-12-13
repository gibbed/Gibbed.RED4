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
        public override DefinitionType DefinitionType => DefinitionType.Property;

        internal override void Serialize(Stream output, Endian endian, ICacheTables tables)
        {
            throw new NotImplementedException();
        }

        internal override void Deserialize(Stream input, Endian endian, ICacheTables tables)
        {
            var visibility = (Visibility)input.ReadValueU8();
            
            var unknown88Index = input.ReadValueU32(endian);
            var unknown88 = tables.GetDefinition<NativeDefinition>(unknown88Index);
            var unknown20 = input.ReadValueU16(endian);

            if ((unknown20 & 0x20) != 0)
            {
                var somethingLength = input.ReadValueU16(endian);
                var something = input.ReadString(somethingLength, true, Encoding.UTF8);
            }

            var unknown58Count = input.ReadValueU32(endian);
            var unknown58s = new Tuple<string, string>[unknown58Count];
            for (uint i = 0; i < unknown58Count; i++)
            {
                var unknown58_0Length = input.ReadValueU16(endian);
                var unknown58_0 = input.ReadString(unknown58_0Length, true, Encoding.UTF8);
                var unknown58_1Length = input.ReadValueU16(endian);
                var unknown58_1 = input.ReadString(unknown58_1Length, true, Encoding.UTF8);
                unknown58s[i] = new Tuple<string, string>(unknown58_0, unknown58_1);
            }

            var unknown38Count = input.ReadValueU32(endian);
            var unknown38s = new Tuple<string, string>[unknown38Count];
            for (uint i = 0; i < unknown38Count; i++)
            {
                var unknown38Length = input.ReadValueU16(endian);
                var unknown38 = input.ReadString(unknown38Length, true, Encoding.UTF8);
                var unknown28Length = input.ReadValueU16(endian);
                var unknown28 = input.ReadString(unknown28Length, true, Encoding.UTF8);
                unknown38s[i] = new Tuple<string, string>(unknown38, unknown28);
            }
        }
    }
}
