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

namespace Gibbed.RED4.ScriptFormats
{
    internal class CacheTableReader : ICacheTables
    {
        private readonly ScriptedType[] _Types;
        private readonly string[] _Names;
        private readonly string[] _Resources;

        public CacheTableReader(ScriptedType[] types, string[] names, string[] resources)
        {
            this._Types = types;
            this._Names = names;
            this._Resources = resources;
        }

        public ScriptedType GetType(uint index)
        {
            if (index >= this._Types.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._Types[index];
        }

        public T GetType<T>(uint index) where T : ScriptedType
        {
            var instance = GetType(index);
            if (instance == null)
            {
                return null;
            }
            if (instance is T derived)
            {
                return derived;
            }
            throw new ArgumentException($"wanted {typeof(T).Name}, got {instance.GetType().Name}");
        }

        public uint PutType(ScriptedType instance)
        {
            throw new NotSupportedException();
        }

        public string GetName(uint index)
        {
            if (index >= this._Names.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._Names[index];
        }

        public uint PutName(string value)
        {
            throw new NotSupportedException();
        }

        public string GetResource(uint index)
        {
            if (index >= this._Resources.LongLength)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return this._Resources[index];
        }

        public uint PutResource(string value)
        {
            throw new NotSupportedException();
        }
    }
}
