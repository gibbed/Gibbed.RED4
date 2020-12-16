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

namespace Gibbed.RED4.ScriptFormats
{
    internal class DefinitionReader : IDefinitionReader
    {
        private readonly Stream _Stream;
        private readonly Endian _Endian;
        private readonly Definition[] _Definitions;
        private readonly string[] _Names;
        private readonly string[] _TweakDBIds;
        private readonly string[] _Resources;

        public DefinitionReader(
            Stream stream,
            Endian endian,
            Definition[] definitions,
            string[] names,
            string[] tweakDBIds,
            string[] resources)
        {
            this._Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            this._Endian = endian;
            this._Definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
            this._Names = names ?? throw new ArgumentNullException(nameof(names));
            this._TweakDBIds = tweakDBIds ?? throw new ArgumentNullException(nameof(tweakDBIds));
            this._Resources = resources ?? throw new ArgumentNullException(nameof(resources));
        }

        public long Position
        {
            get => this._Stream.Position;
            set => this._Stream.Position = value;
        }

        public bool ReadValueB8()
        {
            return this._Stream.ReadValueB8();
        }

        public sbyte ReadValueS8()
        {
            return this._Stream.ReadValueS8();
        }

        public short ReadValueS16()
        {
            return this._Stream.ReadValueS16(this._Endian);
        }

        public int ReadValueS32()
        {
            return this._Stream.ReadValueS32(this._Endian);
        }

        public long ReadValueS64()
        {
            return this._Stream.ReadValueS64(this._Endian);
        }

        public byte ReadValueU8()
        {
            return this._Stream.ReadValueU8();
        }

        public ushort ReadValueU16()
        {
            return this._Stream.ReadValueU16(this._Endian);
        }

        public uint ReadValueU32()
        {
            return this._Stream.ReadValueU32(this._Endian);
        }

        public ulong ReadValueU64()
        {
            return this._Stream.ReadValueU64(this._Endian);
        }

        public float ReadValueF32()
        {
            return this._Stream.ReadValueF32(this._Endian);
        }

        public double ReadValueF64()
        {
            return this._Stream.ReadValueF64(this._Endian);
        }

        public string ReadStringU16()
        {
            return this.ReadStringU16(out _);
        }

        public string ReadStringU16(out ushort size)
        {
            size = this.ReadValueU16();
            return this._Stream.ReadString(size, true, Encoding.UTF8);
        }

        public string ReadStringU32()
        {
            return this.ReadStringU32(out _);
        }

        public string ReadStringU32(out uint size)
        {
            size = this.ReadValueU32();
            if (size > int.MaxValue)
            {
                throw new FormatException();
            }
            return this._Stream.ReadString((int)size, true, Encoding.UTF8);
        }

        public byte[] ReadBytes()
        {
            var size = this.ReadValueU32();
            if (size > int.MaxValue)
            {
                throw new FormatException();
            }
            return this._Stream.ReadBytes((int)size);
        }

        public Definition ReadReference()
        {
            var index = this.ReadValueU32();
            if (index >= this._Definitions.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._Definitions[index];
        }

        public T ReadReference<T>() where T : Definition
        {
            var instance = this.ReadReference();
            if (instance == null)
            {
                return null;
            }
            if (instance is T derived)
            {
                return derived;
            }
            throw new ArgumentException($"wanted {typeof(T).Name} but got {instance.GetType().Name}");
        }

        public Definition[] ReadReferences()
        {
            var count = this.ReadValueU32();
            var items = new Definition[count];
            for (uint i = 0; i < count; i++)
            {
                items[i] = this.ReadReference();
            }
            return items;
        }

        public T[] ReadReferences<T>() where T : Definition
        {
            var count = this.ReadValueU32();
            var items = new T[count];
            for (uint i = 0; i < count; i++)
            {
                items[i] = this.ReadReference<T>();
            }
            return items;
        }

        public string ReadName()
        {
            var index = this.ReadValueU32();
            if (index >= this._Names.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._Names[index];
        }

        public string ReadTweakDBId()
        {
            var index = this.ReadValueU32();
            if (index >= this._TweakDBIds.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._TweakDBIds[index];
        }

        public string ReadResource()
        {
            var index = this.ReadValueU32();
            if (index >= this._Resources.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._Resources[index];
        }
    }
}
