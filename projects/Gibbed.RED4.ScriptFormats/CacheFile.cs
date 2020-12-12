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
using Gibbed.RED4.ScriptFormats.Cache;
using CRC32 = Gibbed.RED4.FileFormats.Hashing.CRC32;

namespace Gibbed.RED4.ScriptFormats
{
    public class CacheFile
    {
        public ScriptedType[] Types { get; set; }

        private CacheFile()
        {
        }

        public static CacheFile Load(Stream input, bool validate)
        {
            var headerBytes = input.ReadBytes(FileHeader.Size + CacheHeader.Size);
            Endian endian;
            CacheHeader header;
            using (var data = new MemoryStream(headerBytes, false))
            {
                var fileHeader = FileHeader.Read(data);
                if (fileHeader.Version != 13 || fileHeader.Unknown != 0)
                {
                    throw new FormatException();
                }

                endian = fileHeader.Endian;
                header = CacheHeader.Read(data, endian);
            }

            if (validate == true)
            {
                var deadbeefBytes = BitConverter.GetBytes(0xDEADBEEFu);
                Array.Copy(deadbeefBytes, 0, headerBytes, FileHeader.Size + CacheHeader.HashOffset, 4);

                if (CRC32.Compute(headerBytes, 0, headerBytes.Length) != header.HeaderHash)
                {
                    throw new FormatException();
                }
            }

            var stringBytes = LoadArrayBytes(input, header.StringData, 1, validate);
            string[] typeNames, names, resources;
            using (var data = new MemoryStream(stringBytes, false))
            {
                typeNames = LoadStrings(input, header.TypeNameStringOffsets, validate, data, endian);
                names = LoadStrings(input, header.NameStringOffsets, validate, data, endian);
                resources = LoadStrings(input, header.ResourceStringOffsets, validate, data, endian);
            }

            var scriptedTypeHeaders = LoadArray(input, header.ScriptedTypes, ScriptedTypeHeader.Size, validate, ScriptedTypeHeader.Read, endian);
            var scriptedTypes = new ScriptedType[scriptedTypeHeaders.Length];
            for (int i = 1; i < scriptedTypeHeaders.Length; i++)
            {
                scriptedTypes[i] = ScriptedTypeFactory.Create(scriptedTypeHeaders[i].Type);
            }
            for (int i = 1; i < scriptedTypeHeaders.Length; i++)
            {
                var scriptedTypeHeader = scriptedTypeHeaders[i];
                var scriptedType = scriptedTypes[i];
                scriptedType.Parent = scriptedTypes[scriptedTypeHeader.ParentIndex];
                scriptedType.Name = typeNames[scriptedTypeHeader.NameIndex];
            }
            var scriptedTypeTableReader = new CacheTableReader(scriptedTypes, names, resources);
            for (int i = 1; i < scriptedTypeHeaders.Length; i++)
            {
                var scriptedTypeHeader = scriptedTypeHeaders[i];
                var scriptedType = scriptedTypes[i];

                input.Position = scriptedTypeHeader.DataOffset;

                scriptedType.LoadPosition = input.Position;

                if (validate == true)
                {
                    using (var data = input.ReadToMemoryStream((int)scriptedTypeHeader.DataSize))
                    {
                        scriptedType.Deserialize(data, endian, scriptedTypeTableReader);
                        if (data.Position != data.Length)
                        {
                            throw new FormatException();
                        }
                    }
                }
                else
                {
                    var expectedPosition = input.Position + scriptedTypeHeader.DataSize;
                    scriptedType.Deserialize(input, endian, scriptedTypeTableReader);
                    if (input.Position != expectedPosition)
                    {
                        throw new FormatException();
                    }
                }
            }

            var instance = new CacheFile();
            instance.Types = scriptedTypes;
            return instance;
        }

        private static byte[] LoadArrayBytes(Stream input, ArrayHeader array, int itemSize, bool validate)
        {
            byte[] bytes;

            if (array.Count == 0)
            {
                bytes = new byte[0];
            }
            else
            {
                var size = array.Count * itemSize;
                if (size > int.MaxValue)
                {
                    throw new FormatException();
                }
                input.Position = array.Offset;
                bytes = input.ReadBytes((int)size);
            }

            if (validate == true)
            {
                if (CRC32.Compute(bytes, 0, bytes.Length) != array.Hash)
                {
                    throw new FormatException();
                }
            }

            return bytes;
        }

        private static T[] LoadArray<T>(
            Stream input,
            ArrayHeader array,
            int itemSize,
            bool validate,
            Func<Stream, Endian, T> readCallback,
            Endian endian)
        {
            var bytes = LoadArrayBytes(input, array, itemSize, validate);
            var items = new T[array.Count];
            if (array.Count > 0)
            {
                using (var data = new MemoryStream(bytes, false))
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        items[i] = readCallback(data, endian);
                    }
                }
            }
            return items;
        }

        private static string[] LoadStrings(Stream input, ArrayHeader array, bool validate, Stream stringData, Endian endian)
        {
            var offsets = LoadArray(input, array, 4, validate, StreamHelpers.ReadValueU32, endian);
            var strings = new string[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
            {
                stringData.Position = offsets[i];
                strings[i] = stringData.ReadStringZ(Encoding.UTF8);
            }
            return strings;
        }
    }
}
