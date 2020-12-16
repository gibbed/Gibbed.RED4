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

using Gibbed.RED4.ScriptFormats.Definitions;

namespace Gibbed.RED4.ScriptFormats.Instructions
{
    public struct EnumCast
    {
        public NativeDefinition Type;
        public byte Size;

        public EnumCast(NativeDefinition type, byte size)
        {
            this.Type = type;
            this.Size = size;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var type = reader.ReadReference<NativeDefinition>();
            var size = reader.ReadValueU8();
            return (new EnumCast(type, size), 9);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var (type, size) = (EnumCast)argument;
            writer.WriteReference(type);
            writer.WriteValueU8(size);
            return 9;
        }

        public void Deconstruct(out NativeDefinition type, out byte size)
        {
            type = this.Type;
            size = this.Size;
        }

        public override string ToString()
        {
            return $"({this.Type}, {this.Size})";
        }
    }
}
