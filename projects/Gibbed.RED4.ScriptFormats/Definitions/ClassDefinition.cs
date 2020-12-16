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
    public class ClassDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Class;

        public ClassDefinition()
        {
            this.Functions = new List<FunctionDefinition>();
            this.Unknown20s = new List<PropertyDefinition>();
            this.Unknown30s = new List<PropertyDefinition>();
        }

        public Visibility Visibility { get; set; }
        public ClassFlags Flags { get; set; }
        public ClassDefinition BaseClass { get; set; }
        public List<FunctionDefinition> Functions { get; }
        public List<PropertyDefinition> Unknown20s { get; }
        public List<PropertyDefinition> Unknown30s { get; }

        private static readonly ClassFlags KnownFlags =
            ClassFlags.Unknown0 | ClassFlags.IsAbstract |
            ClassFlags.Unknown2 | ClassFlags.IsClass |
            ClassFlags.HasFunctions | ClassFlags.Unknown5 |
            ClassFlags.IsImportOnly | ClassFlags.Unknown8;

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8((byte)this.Visibility);
            writer.WriteValueU16((ushort)this.Flags);
            writer.WriteReference(this.BaseClass);
            if ((this.Flags & ClassFlags.HasFunctions) != 0)
            {
                writer.WriteReferences(this.Functions);
            }
            if ((this.Flags & ClassFlags.Unknown5) != 0)
            {
                writer.WriteReferences(this.Unknown20s);
            }
            if ((this.Flags & ClassFlags.Unknown8) != 0)
            {
                writer.WriteReferences(this.Unknown30s);
            }
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var visibility = (Visibility)reader.ReadValueU8();

            var flags = (ClassFlags)reader.ReadValueU16();
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != ClassFlags.None)
            {
                throw new FormatException();
            }

            var baseClass = reader.ReadReference<ClassDefinition>();
            var functions = (flags & ClassFlags.HasFunctions) != 0
                ? reader.ReadReferences<FunctionDefinition>()
                : new FunctionDefinition[0];
            var unknown20s = (flags & ClassFlags.Unknown5) != 0
                ? reader.ReadReferences<PropertyDefinition>()
                : new PropertyDefinition[0];
            var unknown30s = (flags & ClassFlags.Unknown8) != 0
                ? reader.ReadReferences<PropertyDefinition>()
                : new PropertyDefinition[0];

            this.Functions.Clear();
            this.Unknown20s.Clear();
            this.Unknown30s.Clear();
            this.Visibility = visibility;
            this.Flags = flags;
            this.BaseClass = baseClass;
            this.Functions.AddRange(functions);
            this.Unknown20s.AddRange(unknown20s);
            this.Unknown30s.AddRange(unknown30s);
        }
    }
}
