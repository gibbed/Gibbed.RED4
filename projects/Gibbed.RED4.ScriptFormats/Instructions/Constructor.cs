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
    public struct Constructor
    {
        public byte ParameterCount;
        public ClassDefinition Type;

        public Constructor(byte parameterCount, ClassDefinition type)
        {
            this.ParameterCount = parameterCount;
            this.Type = type;
        }

        internal static (object, uint) Read(IDefinitionReader reader)
        {
            var parameterCount = reader.ReadValueU8();
            var type = reader.ReadReference<ClassDefinition>();
            return (new Constructor(parameterCount, type), 9);
        }

        internal static uint Write(object argument, IDefinitionWriter writer)
        {
            var (parameterCount, definition) = (Constructor)argument;
            writer.WriteValueU8(parameterCount);
            writer.WriteReference(definition);
            return 9;
        }

        public void Deconstruct(out byte parameterCount, out ClassDefinition type)
        {
            parameterCount = this.ParameterCount;
            type = this.Type;
        }

        public override string ToString()
        {
            return $"({this.ParameterCount}, {this.Type})";
        }
    }
}
