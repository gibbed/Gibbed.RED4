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

namespace Gibbed.RED4.ScriptFormats
{
    internal class DefinitionWriter : IDefinitionWriter
    {
        private readonly Stream _Stream;
        private readonly Endian _Endian;
        private readonly long _BasePosition;
        private readonly Dictionary<Definition, List<long>> _Definitions;
        private readonly Dictionary<string, List<long>> _Names;
        private readonly Dictionary<string, List<long>> _TweakDBIds;
        private readonly Dictionary<string, List<long>> _Resources;

        public DefinitionWriter(IEnumerable<Definition> definitions, Stream stream, Endian endian, long basePosition = 0)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            this._Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this._Endian = endian;
            this._BasePosition = basePosition;
            this._Definitions = new Dictionary<Definition, List<long>>();
            this._Names = new Dictionary<string, List<long>>();
            this._TweakDBIds = new Dictionary<string, List<long>>();
            this._Resources = new Dictionary<string, List<long>>();

            foreach (var definition in definitions)
            {
                this._Definitions.Add(definition, new List<long>());
                if (this._Names.ContainsKey(definition.Name) == false)
                {
                    this._Names[definition.Name] = new List<long>();
                }
            }
        }

        public long Position
        {
            get => this._Stream.Position;
            set => this._Stream.Position = value;
        }

        public void WriteValueB8(bool value)
        {
            this._Stream.WriteValueB8(value);
        }

        public void WriteValueS8(sbyte value)
        {
            this._Stream.WriteValueS8(value);
        }

        public void WriteValueS16(short value)
        {
            this._Stream.WriteValueS16(value, this._Endian);
        }

        public void WriteValueS32(int value)
        {
            this._Stream.WriteValueS32(value, this._Endian);
        }

        public void WriteValueS64(long value)
        {
            this._Stream.WriteValueS64(value, this._Endian);
        }

        public void WriteValueU8(byte value)
        {
            this._Stream.WriteValueU8(value);
        }

        public void WriteValueU16(ushort value)
        {
            this._Stream.WriteValueU16(value, this._Endian);
        }

        public void WriteValueU32(uint value)
        {
            this._Stream.WriteValueU32(value, this._Endian);
        }

        public void WriteValueU64(ulong value)
        {
            this._Stream.WriteValueU64(value, this._Endian);
        }

        public void WriteValueF32(float value)
        {
            this._Stream.WriteValueF32(value, this._Endian);
        }

        public void WriteValueF64(double value)
        {
            this._Stream.WriteValueF64(value, this._Endian);
        }

        public ushort WriteStringU16(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "string too long");
            }
            var size = (ushort)bytes.Length;
            this.WriteValueU16(size);
            this._Stream.WriteBytes(bytes);
            return size;
        }

        public uint WriteStringU32(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            var longLength = (long)bytes.Length;
            if (longLength > uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value too long");
            }
            var size = (uint)bytes.Length;
            this.WriteValueU32(size);
            this._Stream.WriteBytes(bytes);
            return size;
        }

        public void WriteBytes(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            var longLength = (long)value.Length;
            if (longLength > uint.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "value too long");
            }
            this.WriteValueU32((uint)value.Length);
            this._Stream.WriteBytes(value);
        }

        public void WriteReference(Definition value)
        {
            if (value == null)
            {
                this.WriteValueU32(0);
                return;
            }
            List<long> positions;
            if (this._Definitions.TryGetValue(value, out positions) == false)
            {
                this._Definitions[value] = positions = new List<long>();
            }
            positions.Add(this._BasePosition + this.Position);
            this.WriteValueU32(uint.MaxValue);
        }

        public void WriteReferences(IList<Definition> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            this.WriteValueS32(list.Count);
            foreach (var instance in list)
            {
                this.WriteReference(instance);
            }
        }

        public void WriteReferences<T>(IList<T> list) where T : Definition
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            this.WriteValueS32(list.Count);
            foreach (var instance in list)
            {
                this.WriteReference(instance);
            }
        }

        public void WriteName(string value)
        {
            List<long> positions;
            if (this._Names.TryGetValue(value, out positions) == false)
            {
                this._Names[value] = positions = new List<long>();
            }
            positions.Add(this._BasePosition + this.Position);
            this.WriteValueU32(uint.MaxValue);
        }

        public void WriteTweakDBId(string value)
        {
            List<long> positions;
            if (this._TweakDBIds.TryGetValue(value, out positions) == false)
            {
                this._TweakDBIds[value] = positions = new List<long>();
            }
            positions.Add(this._BasePosition + this.Position);
            this.WriteValueU32(uint.MaxValue);
        }

        public void WriteResource(string value)
        {
            List<long> positions;
            if (this._Resources.TryGetValue(value, out positions) == false)
            {
                this._Resources[value] = positions = new List<long>();
            }
            positions.Add(this._BasePosition + this.Position);
            this.WriteValueU32(uint.MaxValue);
        }

        public (Definition definition, long[] positions)[] GetReferencePairs()
        {
            return this._Definitions
                .Select(kv => (definition: kv.Key, positions: kv.Value.ToArray()))
                .ToArray();
        }

        public (string name, long[] positions)[] GetNamePairs()
        {
            return this._Names
                .Select(kv => (name: kv.Key, positions: kv.Value.ToArray()))
                .ToArray();
        }

        public (string tweakDBId, long[] positions)[] GetTweakDBIdPairs()
        {
            return this._TweakDBIds
                .Select(kv => (name: kv.Key, positions: kv.Value.ToArray()))
                .ToArray();
        }

        public (string resource, long[] positions)[] GetResourcePairs()
        {
            return this._Resources
                .Select(kv => (name: kv.Key, positions: kv.Value.ToArray()))
                .ToArray();
        }
    }
}
