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

namespace Gibbed.RED4.ScriptFormats
{
    public abstract class ScriptedType
    {
        public abstract ScriptedTypeType Type { get; }
        public ScriptedType Parent { get; set; }
        public string Name { get; set; }
        public long LoadPosition { get; internal set; }

        internal abstract void Serialize(Stream output, Endian endian, ICacheTables tables);
        internal abstract void Deserialize(Stream input, Endian endian, ICacheTables tables);

        internal static ScriptedType[] ReadTypeArray(Stream input, Endian endian, ICacheTables tables)
        {
            var count = input.ReadValueU32(endian);
            var items = new ScriptedType[count];
            for (uint i = 0; i < count; i++)
            {
                var typeIndex = input.ReadValueU32(endian);
                items[i] = tables.GetType(typeIndex);
            }
            return items;
        }

        internal static T[] ReadTypeArray<T>(Stream input, Endian endian, ICacheTables tables)
            where T: ScriptedType
        {
            var count = input.ReadValueU32(endian);
            var items = new T[count];
            for (uint i = 0; i < count; i++)
            {
                var typeIndex = input.ReadValueU32(endian);
                items[i] = tables.GetType<T>(typeIndex);
            }
            return items;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.Name) == true)
            {
                return $"{this.Type}";
            }
            return $"{this.Type} {this.Name}";
        }
    }
}
