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
    public class ParameterType : ScriptedType
    {
        public override ScriptedTypeType Type => ScriptedTypeType.Parameter;

        public ScriptedType Unknown20 { get; set; }
        public byte Unknown28 { get; set; }

        internal override void Serialize(Stream output, Endian endian, ICacheTables tables)
        {
            throw new NotImplementedException();
        }

        internal override void Deserialize(Stream input, Endian endian, ICacheTables tables)
        {
            var unknown20Index = input.ReadValueU32(endian);
            var unknown20 = tables.GetType(unknown20Index);
            var unknown28 = input.ReadValueU8();

            this.Unknown20 = unknown20;
            this.Unknown28 = unknown28;
        }
    }
}
