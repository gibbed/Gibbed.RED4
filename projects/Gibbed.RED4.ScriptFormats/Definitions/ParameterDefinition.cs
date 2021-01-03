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
    public class ParameterDefinition : FunctionVarDefinition
    {
        public override DefinitionType DefinitionType => DefinitionType.Parameter;
        public ParameterFlags Flags { get; set; }

        private static readonly ParameterFlags KnownFlags =
            ParameterFlags.IsOptional | ParameterFlags.IsOut |
            ParameterFlags.Unknown2 | ParameterFlags.Unknown3;

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteReference(this.Type);
            writer.WriteValueU8((byte)this.Flags);
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var type = reader.ReadReference<NativeDefinition>();

            var flags = (ParameterFlags)reader.ReadValueU8();
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != ParameterFlags.None)
            {
                throw new FormatException();
            }

            this.Type = type;
            this.Flags = flags;
        }
    }
}
