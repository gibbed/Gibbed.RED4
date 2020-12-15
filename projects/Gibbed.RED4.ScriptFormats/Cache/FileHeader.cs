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
    internal struct FileHeader
    {
        public const int Size = 12;

        public const uint Signature = 0x53444552; // 'REDS'

        public Endian Endian;
        public uint Version;
        public uint Unknown;

        public static FileHeader Read(Stream input)
        {
            var magic = input.ReadValueU32(Endian.Little);
            if (magic != Signature && magic.Swap() != Signature)
            {
                throw new FormatException();
            }
            var endian = magic == Signature ? Endian.Little : Endian.Big;

            FileHeader instance;
            instance.Endian = endian;
            instance.Version = input.ReadValueU32(endian);
            instance.Unknown = input.ReadValueU32(endian);
            return instance;
        }

        public static void Write(Stream output, FileHeader instance)
        {
            var endian = instance.Endian;
            output.WriteValueU32(Signature, endian);
            output.WriteValueU32(instance.Version, endian);
            output.WriteValueU32(instance.Unknown, endian);
        }

        public void Write(Stream output)
        {
            Write(output, this);
        }
    }
}
