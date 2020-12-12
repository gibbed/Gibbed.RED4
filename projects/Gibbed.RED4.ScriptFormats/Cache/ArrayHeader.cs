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

using System.IO;
using Gibbed.IO;

namespace Gibbed.RED4.ScriptFormats.Cache
{
    internal struct ArrayHeader
    {
        public uint Offset;
        public uint Count;
        public uint Hash;

        public static ArrayHeader Read(Stream input, Endian endian)
        {
            ArrayHeader instance;
            instance.Offset = input.ReadValueU32(endian);
            instance.Count = input.ReadValueU32(endian);
            instance.Hash = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write(Stream output, ArrayHeader instance, Endian endian)
        {
            output.WriteValueU32(instance.Offset, endian);
            output.WriteValueU32(instance.Count, endian);
            output.WriteValueU32(instance.Hash, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }

        public override string ToString()
        {
            return $"@{this.Offset:X} {this.Count} ({this.Hash:X8})";
        }
    }
}
