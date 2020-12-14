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
        public Definition[] Definitions { get; set; }

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
            string[] names, tweakDBIds, resources;
            using (var data = new MemoryStream(stringBytes, false))
            {
                names = LoadStrings(input, header.NameStringOffsets, validate, data, endian);
                tweakDBIds = LoadStrings(input, header.TweakDBIdStringOffsets, validate, data, endian);
                resources = LoadStrings(input, header.ResourceStringOffsets, validate, data, endian);
            }

            var definitionHeaders = LoadArray(input, header.Definitions, DefinitionHeader.Size, validate, DefinitionHeader.Read, endian);
            var definitions = new Definition[definitionHeaders.Length];
            for (int i = 1; i < definitionHeaders.Length; i++)
            {
                definitions[i] = DefinitionFactory.Create(definitionHeaders[i].Type);
            }
            for (int i = 1; i < definitionHeaders.Length; i++)
            {
                var definitionHeader = definitionHeaders[i];
                var definition = definitions[i];
                definition.Parent = definitions[definitionHeader.ParentIndex];
                definition.Name = names[definitionHeader.NameIndex];
            }
            var tableReader = new CacheReferenceReader(definitions, names, tweakDBIds, resources);
            for (int i = 1; i < definitionHeaders.Length; i++)
            {
                var definitionHeader = definitionHeaders[i];
                var definition = definitions[i];

                input.Position = definitionHeader.DataOffset;

                definition.LoadPosition = input.Position;

                if (validate == true)
                {
                    using (var data = input.ReadToMemoryStream((int)definitionHeader.DataSize))
                    {
                        definition.Deserialize(data, endian, tableReader);
                        if (data.Position != data.Length)
                        {
                            throw new FormatException();
                        }
                    }
                }
                else
                {
                    var expectedPosition = input.Position + definitionHeader.DataSize;
                    definition.Deserialize(input, endian, tableReader);
                    if (input.Position != expectedPosition)
                    {
                        throw new FormatException();
                    }
                }
            }

            var instance = new CacheFile();
            instance.Definitions = definitions;
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
