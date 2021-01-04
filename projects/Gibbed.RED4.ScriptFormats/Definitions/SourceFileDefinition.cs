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
using FNV1a64 = Gibbed.RED4.Common.Hashing.FNV1a64;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    public class SourceFileDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.SourceFile;

        public SourceFileDefinition()
        {
            this.Path = "";
        }

        public SourceFileDefinition(string path)
        {
            this.Path = path ?? throw new ArgumentNullException(nameof(path));
            this.PathHash = FNV1a64.Compute(path);
        }

        public SourceFileDefinition(string path, ulong pathHash)
        {
            this.Path = path ?? throw new ArgumentNullException(nameof(path));
            this.PathHash = pathHash;
        }

        public uint Id { get; set; }
        public ulong PathHash { get; set; }
        public string Path;

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU32(this.Id);
            writer.WriteValueU64(this.PathHash);
            writer.WriteStringU16(this.Path);
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.Id = reader.ReadValueU32();
            this.PathHash = reader.ReadValueU64();
            this.Path = reader.ReadStringU16();
        }
    }
}
