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

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    public class NativeDefinition : Definition
    {
        public override DefinitionType DefinitionType => DefinitionType.Native;

        public NativeType NativeType { get; set; }
        public Definition BaseType { get; set; }
        public uint ArraySize { get; set; }

        private static bool HasBaseType(NativeType type)
        {
            switch (type)
            {
                case NativeType.Handle:
                case NativeType.WeakHandle:
                case NativeType.Array:
                case NativeType.StaticArray:
                case NativeType.Unknown6:
                {
                    return true;
                }
            }
            return false;
        }

        internal override void Serialize(IDefinitionWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteValueU8((byte)this.NativeType);

            if (HasBaseType(this.NativeType) == true)
            {
                writer.WriteReference(this.BaseType);
            }

            if (this.NativeType == NativeType.StaticArray)
            {
                writer.WriteValueU32(this.ArraySize);
            }
        }

        internal override void Deserialize(IDefinitionReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var nativeType = (NativeType)reader.ReadValueU8();
            this.NativeType = nativeType;
            this.BaseType = HasBaseType(NativeType) == true ? reader.ReadReference() : null;
            this.ArraySize = nativeType == NativeType.StaticArray ? reader.ReadValueU32() : 0;
        }
    }
}
