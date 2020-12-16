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
    public class FunctionDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Function;

        public FunctionDefinition()
        {
            this.Parameters = new List<ParameterDefinition>();
            this.Locals = new List<LocalDefinition>();
            this.Body = new List<Instruction>();
        }

        public Visibility Visibility { get; set; }
        public FunctionFlags Flags { get; set; }
        public SourceFileDefinition SourceFile { get; set; }
        public uint SourceLine { get; set; }
        public Definition ReturnType { get; set; }
        public bool Unknown50 { get; set; }
        public Definition Unknown58 { get; set; }
        public List<ParameterDefinition> Parameters { get; }
        public List<LocalDefinition> Locals { get; }
        public uint Unknown98 { get; set; }
        public byte UnknownA0 { get; set; }
        public long BodyLoadPosition { get; internal set; }
        public List<Instruction> Body { get; }

        private static readonly FunctionFlags KnownFlags =
            FunctionFlags.Unknown0 | FunctionFlags.Unknown1 |
            FunctionFlags.Unknown2 | FunctionFlags.Unknown3 |
            FunctionFlags.Unknown4 | FunctionFlags.Unknown5 |
            FunctionFlags.Unknown6 |
            FunctionFlags.HasReturnValue |
            FunctionFlags.Unknown8 |
            FunctionFlags.HasParameters | FunctionFlags.HasLocals |
            FunctionFlags.HasBody |
            FunctionFlags.Unknown12 | FunctionFlags.Unknown13 |
            FunctionFlags.IsConstant | FunctionFlags.Unknown19 |
            FunctionFlags.Unknown20 | FunctionFlags.Unknown21;

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8((byte)this.Visibility);
            writer.WriteValueU32((uint)this.Flags);

            if ((this.Flags & FunctionFlags.Unknown4) == 0)
            {
                writer.WriteReference(this.SourceFile);
                writer.WriteValueU32(this.SourceLine);
            }

            if ((this.Flags & FunctionFlags.HasReturnValue) != 0)
            {
                writer.WriteReference(this.ReturnType);
                writer.WriteValueB8(this.Unknown50);
            }

            if ((this.Flags & FunctionFlags.Unknown8) != 0)
            {
                writer.WriteReference(this.Unknown58);
            }

            if ((this.Flags & FunctionFlags.HasParameters) != 0)
            {
                writer.WriteReferences(this.Parameters);
            }
            if ((this.Flags & FunctionFlags.HasLocals) != 0)
            {
                writer.WriteReferences(this.Locals);
            }
            if ((this.Flags & FunctionFlags.Unknown6) != 0)
            {
                writer.WriteValueU32(this.Unknown98);
            }
            if ((this.Flags & FunctionFlags.Unknown12) != 0)
            {
                writer.WriteValueU8(this.UnknownA0);
            }
            if ((this.Flags & FunctionFlags.HasBody) != 0)
            {
                var bodySizePosition = writer.Position;
                writer.WriteValueU32(uint.MaxValue);
                var bodySize = Instructions.Write(writer, this.Body);
                var endPosition = writer.Position;
                writer.Position = bodySizePosition;
                writer.WriteValueU32(bodySize);
                writer.Position = endPosition;
            }
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var visibility = (Visibility)reader.ReadValueU8();

            var flags = (FunctionFlags)reader.ReadValueU32();
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != FunctionFlags.None)
            {
                throw new FormatException();
            }

            SourceFileDefinition sourceFile;
            uint sourceLine;
            if ((flags & FunctionFlags.Unknown4) == 0)
            {
                sourceFile = reader.ReadReference<SourceFileDefinition>();
                sourceLine = reader.ReadValueU32();
            }
            else
            {
                sourceFile = null;
                sourceLine = 0;
            }

            Definition returnType;
            bool unknown50;
            if ((flags & FunctionFlags.HasReturnValue) != 0)
            {
                returnType = reader.ReadReference();
                unknown50 = reader.ReadValueB8();
            }
            else
            {
                returnType = null;
                unknown50 = false;
            }

            var unknown58 = (flags & FunctionFlags.Unknown8) != 0 ? reader.ReadReference() : null;
            var parameters = (flags & FunctionFlags.HasParameters) != 0
                ? reader.ReadReferences<ParameterDefinition>()
                : new ParameterDefinition[0];
            var locals = (flags & FunctionFlags.HasLocals) != 0
                ? reader.ReadReferences<LocalDefinition>()
                : new LocalDefinition[0];
            var unknown98 = (flags & FunctionFlags.Unknown6) != 0
                ? reader.ReadValueU32()
                : default;
            var unknownA0 = (flags & FunctionFlags.Unknown12) != 0
                ? reader.ReadValueU8()
                : default;

            long bodyPosition = -1;
            Instruction[] instructions;
            if ((flags & FunctionFlags.HasBody) != 0)
            {
                var bodySize = reader.ReadValueU32();
                bodyPosition = reader.Position;
                instructions = Instructions.Read(reader, bodySize);
            }
            else
            {
                instructions = new Instruction[0];
            }

            this.Parameters.Clear();
            this.Locals.Clear();
            this.Body.Clear();
            this.Visibility = visibility;
            this.Flags = flags;
            this.SourceFile = sourceFile;
            this.SourceLine = sourceLine;
            this.ReturnType = returnType;
            this.Unknown50 = unknown50;
            this.Unknown58 = unknown58;
            this.Parameters.AddRange(parameters);
            this.Locals.AddRange(locals);
            this.Unknown98 = unknown98;
            this.UnknownA0 = unknownA0;
            this.BodyLoadPosition = bodyPosition;
            this.Body.AddRange(instructions);
        }
    }
}
