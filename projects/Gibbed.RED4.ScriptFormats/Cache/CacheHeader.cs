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

namespace Gibbed.RED4.ScriptFormats.Cache
{
    internal struct CacheHeader
    {
        public const int Size = 80;

        public const int HashOffset = 12;

        public uint Unknown00;
        public uint Unknown04;
        public uint Unknown08;
        public uint HeaderHash;
        public uint Unknown10;
        public ArrayHeader StringData;
        public ArrayHeader NameStringOffsets;
        public ArrayHeader TweakDBIdStringOffsets;
        public ArrayHeader ResourceStringOffsets;
        public ArrayHeader Definitions;

        public static CacheHeader Read(Stream input, Endian endian)
        {
            CacheHeader instance;
            instance.Unknown00 = input.ReadValueU32(endian);
            instance.Unknown04 = input.ReadValueU32(endian);
            instance.Unknown08 = input.ReadValueU32(endian);
            instance.HeaderHash = input.ReadValueU32(endian);
            instance.Unknown10 = input.ReadValueU32(endian);
            instance.StringData = ArrayHeader.Read(input, endian);
            instance.NameStringOffsets = ArrayHeader.Read(input, endian);
            instance.TweakDBIdStringOffsets = ArrayHeader.Read(input, endian);
            instance.ResourceStringOffsets = ArrayHeader.Read(input, endian);
            instance.Definitions = ArrayHeader.Read(input, endian);
            return instance;
        }

        public static void Write(Stream output, CacheHeader instance, Endian endian)
        {
            output.WriteValueU32(instance.Unknown00, endian);
            output.WriteValueU32(instance.Unknown04, endian);
            output.WriteValueU32(instance.Unknown08, endian);
            output.WriteValueU32(instance.HeaderHash, endian);
            output.WriteValueU32(instance.Unknown10, endian);
            instance.StringData.Write(output, endian);
            instance.NameStringOffsets.Write(output, endian);
            instance.TweakDBIdStringOffsets.Write(output, endian);
            instance.ResourceStringOffsets.Write(output, endian);
            instance.Definitions.Write(output, endian);
        }

        public void Write(Stream output, Endian endian)
        {
            Write(output, this, endian);
        }
    }
}
