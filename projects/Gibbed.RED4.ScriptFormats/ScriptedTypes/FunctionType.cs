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
    public class FunctionType : ScriptedType
    {
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

        public override ScriptedTypeType Type => ScriptedTypeType.Function;

        public Visibility Visibility { get; set; }
        public FunctionFlags Flags { get; set; }
        public ScriptFile SourceFile { get; set; }
        public uint SourceLine { get; set; }
        public ScriptedType ReturnType { get; set; }
        public bool Unknown50 { get; set; }
        public ScriptedType Unknown58 { get; set; }
        public ParameterType[] Parameters { get; set; }
        public LocalType[] Locals { get; set; }
        public uint Unknown98 { get; set; }
        public byte UnknownA0 { get; set; }
        public long BodyLoadPosition { get; internal set; }
        public Instruction[] Body { get; set; }

        internal override void Serialize(Stream output, Endian endian, ICacheTables tables)
        {
            throw new NotImplementedException();
        }

        internal override void Deserialize(Stream input, Endian endian, ICacheTables tables)
        {
            var visibility = (Visibility)input.ReadValueU8();
            
            var flags = (FunctionFlags)input.ReadValueU32(endian);
            var unknownFlags = flags & ~KnownFlags;
            if (unknownFlags != FunctionFlags.None)
            {
                throw new FormatException();
            }

            ScriptFile scriptFile;
            uint unknownC0;
            if ((flags & FunctionFlags.Unknown4) == 0)
            {
                var scriptFileIndex = input.ReadValueU32(endian);
                scriptFile = tables.GetType<ScriptFile>(scriptFileIndex);
                unknownC0 = input.ReadValueU32(endian);
            }
            else
            {
                scriptFile = null;
                unknownC0 = 0;
            }

            ScriptedType returnType;
            bool unknown50;
            if ((flags & FunctionFlags.HasReturnValue) == 0)
            {
                returnType = null;
                unknown50 = false;
            }
            else
            {
                var returnTypeIndex = input.ReadValueU32(endian);
                returnType = tables.GetType(returnTypeIndex);
                unknown50 = input.ReadValueB8();
            }

            ScriptedType unknown58;
            if ((flags & FunctionFlags.Unknown8) == 0)
            {
                unknown58 = null;
            }
            else
            {
                var unknown58Index = input.ReadValueU32(endian);
                unknown58 = tables.GetType(unknown58Index);
            }

            ParameterType[] parameters;
            if ((flags & FunctionFlags.HasParameters) == 0)
            {
                parameters = null;
            }
            else
            {
                parameters = ReadTypeArray<ParameterType>(input, endian, tables);
            }

            LocalType[] locals;
            if ((flags & FunctionFlags.HasLocals) == 0)
            {
                locals = null;
            }
            else
            {
                locals = ReadTypeArray<LocalType>(input, endian, tables);
            }

            uint unknown98 = (flags & FunctionFlags.Unknown6) != 0
                ? input.ReadValueU32(endian)
                : default;
            byte unknownA0 = (flags & FunctionFlags.Unknown12) != 0
                ? input.ReadValueU8()
                : default;

            long bytecodePosition = -1;
            Instruction[] instructions;
            if ((flags & FunctionFlags.HasBody) == 0)
            {
                instructions = null;
            }
            else
            {
                var bytecodeSize = input.ReadValueU32(endian);
                bytecodePosition = input.Position;
                instructions = Instructions.Read(input, bytecodeSize, endian, tables);
            }

            this.Visibility = visibility;
            this.Flags = flags;
            this.SourceFile = scriptFile;
            this.SourceLine = unknownC0;
            this.ReturnType = returnType;
            this.Unknown50 = unknown50;
            this.Unknown58 = unknown58;
            this.Parameters = parameters;
            this.Locals = locals;
            this.Unknown98 = unknown98;
            this.UnknownA0 = unknownA0;
            this.BodyLoadPosition = bytecodePosition;
            this.Body = instructions;
        }
    }
}
