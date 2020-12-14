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
    public class SourceFileDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.SourceFile;

        public uint Id { get; set; }
        public ulong PathHash { get; set; }
        public string Path;

        internal override void Serialize(Stream output, Endian endian, ICacheTables tables)
        {
            throw new NotImplementedException();
        }

        internal override void Deserialize(Stream input, Endian endian, ICacheTables tables)
        {
            this.Id = input.ReadValueU32(endian);
            this.PathHash = input.ReadValueU64(endian);
            var nameLength = input.ReadValueU16(endian);
            this.Path = input.ReadString(nameLength, true, Encoding.UTF8);
        }
    }
}
