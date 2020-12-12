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

namespace Gibbed.RED4.ScriptFormats.Cache
{
    internal struct ScriptedTypeHeader
    {
        public const int Size = 20;

        public uint NameIndex;
        public uint ParentIndex;
        public uint DataOffset;
        public uint DataSize;
        public ScriptedTypeType Type;

        // TODO(gibbed): maybe uninitialized data? padding?
        public byte Unknown11;
        public byte Unknown12;
        public byte Unknown13;

        public static ScriptedTypeHeader Read(Stream input, Endian endian)
        {
            ScriptedTypeHeader instance;
            instance.NameIndex = input.ReadValueU32(endian);
            instance.ParentIndex = input.ReadValueU32(endian);
            instance.DataOffset = input.ReadValueU32(endian);
            instance.DataSize = input.ReadValueU32(endian);
            instance.Type = (ScriptedTypeType)input.ReadValueU8();
            instance.Unknown11 = input.ReadValueU8();
            instance.Unknown12 = input.ReadValueU8();
            instance.Unknown13 = input.ReadValueU8();
            return instance;
        }

        public static void Write(Stream output, ScriptedTypeHeader instance, Endian endian)
        {
            throw new NotImplementedException();
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public static string[] Names;

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (this.ParentIndex != 0)
            {
                sb.Append($"parent={this.ParentIndex} ");
            }
            sb.Append(Enum.IsDefined(typeof(ScriptedTypeType), this.Type) == true
                ? $"type={this.Type} "
                : $"type=unknown({this.Type}) ");
            if (this.NameIndex > 0)
            {
                sb.Append(Names != null
                    ? $"name=\"{Names[this.NameIndex]}\" "
                    : $"name_index={this.NameIndex}");
            }
            sb.Append($"data=(@{this.DataOffset:X} {this.DataSize}) {this.Unknown11} {this.Unknown12} {this.Unknown13}");
            return sb.ToString();
        }
    }
}
