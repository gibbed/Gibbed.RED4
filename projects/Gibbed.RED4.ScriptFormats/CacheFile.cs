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
using System.IO;
using System.Linq;
using System.Text;
using Gibbed.IO;
using Gibbed.RED4.ScriptFormats.Cache;
using CRC32 = Gibbed.RED4.Common.Hashing.CRC32;

namespace Gibbed.RED4.ScriptFormats
{
    public class CacheFile
    {
        public Endian Endian { get; set; }
        public uint Unknown00 { get; set; }
        public uint Unknown04 { get; set; }
        public uint Unknown08 { get; set; }
        public uint Unknown10 { get; set; }
        public List<Definition> Definitions { get; }

        private CacheFile()
        {
            this.Definitions = new List<Definition>();
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
            var reader = new DefinitionReader(input, endian, definitions, names, tweakDBIds, resources);
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
                        var slicedReader = new DefinitionReader(data, endian, definitions, names, tweakDBIds, resources);
                        definition.Deserialize(slicedReader);
                        if (data.Position != data.Length)
                        {
                            throw new FormatException();
                        }
                    }
                }
                else
                {
                    var expectedPosition = input.Position + definitionHeader.DataSize;
                    definition.Deserialize(reader);
                    if (input.Position != expectedPosition)
                    {
                        throw new FormatException();
                    }
                }
            }

            var instance = new CacheFile();
            instance.Unknown00 = header.Unknown00;
            instance.Unknown04 = header.Unknown04;
            instance.Unknown08 = header.Unknown08;
            instance.Unknown10 = header.Unknown10;
            instance.Definitions.AddRange(definitions.Skip(1));
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
                var actualHash = CRC32.Compute(bytes, 0, bytes.Length);
                if (actualHash != array.Hash)
                {
                    throw new FormatException($"array hash mismatch: {actualHash:X8} vs {array.Hash:X8}");
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

        public void Save(Stream output)
        {
            var endian = this.Endian;

            var definitionLocations = new (long position, long size)[this.Definitions.Count];
            byte[] definitionBytes;
            (Definition definition, long[] positions)[] referencePairs;
            (string name, long[] positions)[] namePairs;
            (string tweakDBId, long[] positions)[] tweakDBIdPairs;
            (string resource, long[] positions)[] resourcePairs;
            using (var data = new MemoryStream())
            {
                var writer = new DefinitionWriter(this.Definitions, data, endian);
                for (int i = 0; i < this.Definitions.Count; i++)
                {
                    var definition = this.Definitions[i];
                    var definitionPosition = data.Position;
                    definition.Serialize(writer);
                    var definitionSize = data.Position - definitionPosition;
                    definitionLocations[i] = (definitionPosition, definitionSize);
                }
                data.Flush();
                definitionBytes = data.ToArray();
                referencePairs = writer.GetReferencePairs();
                namePairs = writer.GetNamePairs();
                tweakDBIdPairs = writer.GetTweakDBIdPairs();
                resourcePairs = writer.GetResourcePairs();
            }

            (string name, uint offset, long[] positions)[] nameOffsets;
            (string tweakDBId, uint offset, long[] positions)[] tweakDBIdOffsets;
            (string resource, uint offset, long[] positions)[] resourceOffsets;
            byte[] stringBytes;
            using (var data = new MemoryStream())
            {
                var stringOffsetLookup = new Dictionary<string, uint>();
                foreach (var value in new[] { "" }
                    .Concat(namePairs.Select(np => np.name))
                    .Concat(tweakDBIdPairs.Select(tp => tp.tweakDBId))
                    .Concat(resourcePairs.Select(rp => rp.resource))
                    .Distinct()
                    .OrderBy(v => v))
                {
                    stringOffsetLookup[value] = (uint)data.Position;
                    data.WriteStringZ(value, Encoding.UTF8);
                }
                data.Flush();
                stringBytes = data.ToArray();
                nameOffsets = BuildStringOffsets(namePairs, stringOffsetLookup);
                tweakDBIdOffsets = BuildStringOffsets(tweakDBIdPairs, stringOffsetLookup);
                resourceOffsets = BuildStringOffsets(resourcePairs, stringOffsetLookup);
            }

            var definitionLookup = new Dictionary<Definition, int>();
            for (int i = 0; i < referencePairs.Length; i++)
            {
                definitionLookup[referencePairs[i].definition] = 1 + i;
            }
            var nameLookup = new Dictionary<string, int>();
            for (int i = 0; i < nameOffsets.Length; i++)
            {
                nameLookup[nameOffsets[i].name] = i;
            }
            var tweakDBIdLookup = new Dictionary<string, int>();
            for (int i = 0; i < tweakDBIdOffsets.Length; i++)
            {
                tweakDBIdLookup[tweakDBIdOffsets[i].tweakDBId] = i;
            }
            var resourceLookup = new Dictionary<string, int>();
            for (int i = 0; i < resourceOffsets.Length; i++)
            {
                resourceLookup[resourceOffsets[i].resource] = i;
            }

            // Resolve definition/name/tweakDBId/resource indices
            using (var data = new MemoryStream(definitionBytes))
            {
                for (int i = 0; i < referencePairs.Length; i++)
                {
                    var (_, positions) = referencePairs[i];
                    foreach (var position in positions)
                    {
                        data.Position = position;
                        data.WriteValueS32(1 + i, endian);
                    }
                }
                for (int i = 0; i < nameOffsets.Length; i++)
                {
                    var (_, _, positions) = nameOffsets[i];
                    foreach (var position in positions)
                    {
                        data.Position = position;
                        data.WriteValueS32(i, endian);
                    }
                }
                for (int i = 0; i < tweakDBIdOffsets.Length; i++)
                {
                    var (_, _, positions) = tweakDBIdOffsets[i];
                    foreach (var position in positions)
                    {
                        data.Position = position;
                        data.WriteValueS32(i, endian);
                    }
                }
                for (int i = 0; i < resourceOffsets.Length; i++)
                {
                    var (_, _, positions) = resourceOffsets[i];
                    foreach (var position in positions)
                    {
                        data.Position = position;
                        data.WriteValueS32(i, endian);
                    }
                }
            }

            var nameStringOffsetBytes = WriteStringOffsets(nameOffsets, endian);
            var tweakDBIdStringOffsetBytes = WriteStringOffsets(tweakDBIdOffsets, endian);
            var resourceStringOffsetBytes = WriteStringOffsets(resourceOffsets, endian);

            CacheHeader cacheHeader;

            output.Position = FileHeader.Size + CacheHeader.Size;

            cacheHeader.StringData.Offset = (uint)output.Position;
            cacheHeader.StringData.Count = (uint)stringBytes.Length;
            cacheHeader.StringData.Hash = CRC32.Compute(stringBytes, 0, stringBytes.Length);
            output.WriteBytes(stringBytes);

            cacheHeader.NameStringOffsets.Offset = (uint)output.Position;
            cacheHeader.NameStringOffsets.Count = (uint)nameOffsets.Length;
            cacheHeader.NameStringOffsets.Hash = CRC32.Compute(nameStringOffsetBytes, 0, nameStringOffsetBytes.Length);
            output.WriteBytes(nameStringOffsetBytes);

            cacheHeader.TweakDBIdStringOffsets.Offset = (uint)output.Position;
            cacheHeader.TweakDBIdStringOffsets.Count = (uint)tweakDBIdOffsets.Length;
            cacheHeader.TweakDBIdStringOffsets.Hash = CRC32.Compute(tweakDBIdStringOffsetBytes, 0, tweakDBIdStringOffsetBytes.Length);
            output.WriteBytes(tweakDBIdStringOffsetBytes);

            cacheHeader.ResourceStringOffsets.Offset = (uint)output.Position;
            cacheHeader.ResourceStringOffsets.Count = (uint)resourceOffsets.Length;
            cacheHeader.ResourceStringOffsets.Hash = CRC32.Compute(resourceStringOffsetBytes, 0, resourceStringOffsetBytes.Length);
            output.WriteBytes(resourceStringOffsetBytes);

            var definitionHeadersPosition = output.Position;
            output.Position += DefinitionHeader.Size * (1 + this.Definitions.Count);

            var definitionOffsetBase = (uint)output.Position;
            output.WriteBytes(definitionBytes);

            var definitionHeaders = new DefinitionHeader[this.Definitions.Count];
            byte[] definitionHeaderBytes;
            using (var data = new MemoryStream())
            {
                DefinitionHeader nullDefinition = default;
                nullDefinition.Write(data, endian);

                for (int i = 0; i < this.Definitions.Count; i++)
                {
                    var definition = this.Definitions[i];

                    int nameIndex;
                    if (definition.Name == null)
                    {
                        nameIndex = 0;
                    }
                    else
                    {
                        if (nameLookup.TryGetValue(definition.Name, out nameIndex) == false)
                        {
                            throw new InvalidOperationException();
                        }
                    }

                    int parentIndex;
                    if (definition.Parent == null)
                    {
                        parentIndex = 0;
                    }
                    else
                    {
                        if (definitionLookup.TryGetValue(definition.Parent, out parentIndex) == false)
                        {
                            throw new InvalidOperationException();
                        }
                    }

                    var (definitionOffset, definitionSize) = definitionLocations[i];

                    DefinitionHeader definitionHeader;
                    definitionHeader.NameIndex = (uint)nameIndex;
                    definitionHeader.ParentIndex = (uint)parentIndex;
                    definitionHeader.DataOffset = definitionOffsetBase + (uint)definitionOffset;
                    definitionHeader.DataSize = (uint)definitionSize;
                    definitionHeader.Type = definition.DefinitionType;
                    definitionHeader.Unknown11 = definitionHeader.Unknown12 = definitionHeader.Unknown13 = default;
                    definitionHeaders[i] = definitionHeader;
                    definitionHeader.Write(data, endian);
                }
                data.Flush();
                definitionHeaderBytes = data.ToArray();
            }

            output.Position = definitionHeadersPosition;
            output.WriteBytes(definitionHeaderBytes);

            cacheHeader.Definitions.Offset = (uint)definitionHeadersPosition;
            cacheHeader.Definitions.Count = 1 + (uint)definitionHeaders.Length;
            cacheHeader.Definitions.Hash = CRC32.Compute(definitionHeaderBytes, 0, definitionHeaderBytes.Length);

            byte[] headerBytes;
            using (var data = new MemoryStream())
            {
                FileHeader fileHeader;
                fileHeader.Endian = endian;
                fileHeader.Version = 13;
                fileHeader.Unknown = 0;

                fileHeader.Write(data);

                cacheHeader.Unknown00 = this.Unknown00;
                cacheHeader.Unknown04 = this.Unknown04;
                cacheHeader.Unknown08 = this.Unknown08;
                cacheHeader.HeaderHash = 0xDEADBEEFu;
                cacheHeader.Unknown10 = this.Unknown10;
                cacheHeader.Write(data, endian);

                data.Flush();

                cacheHeader.HeaderHash = CRC32.Compute(data.GetBuffer(), 0, (int)data.Length);
                data.Position = FileHeader.Size;
                cacheHeader.Write(data, endian);

                data.Flush();
                headerBytes = data.ToArray();
            }

            output.Position = 0;
            output.WriteBytes(headerBytes);
        }

        private static byte[] WriteStringOffsets((string value, uint offset, long[] positions)[] vops, Endian endian)
        {
            using (var data = new MemoryStream())
            {
                foreach (var vop in vops)
                {
                    data.WriteValueU32(vop.offset, endian);
                }
                data.Flush();
                return data.ToArray();
            }
        }

        private static (string name, uint offset, long[] positions)[] BuildStringOffsets(
            (string name, long[] positions)[] pairs,
            Dictionary<string, uint> stringOffsetLookup)
        {
            var result = new (string name, uint offset, long[] positions)[pairs.Length];
            for (int i = 0; i < pairs.Length; i++)
            {
                var (name, positions) = pairs[i];
                uint offset;
                if (string.IsNullOrEmpty(name) == true)
                {
                    offset = 0;
                }
                else if (stringOffsetLookup.TryGetValue(name, out offset) == false)
                {
                    throw new InvalidOperationException();
                }
                result[i] = (name, offset, positions);
            }
            return result.OrderBy(r => r.offset).ToArray();
        }
    }
}
