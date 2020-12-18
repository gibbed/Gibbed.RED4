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

namespace Gibbed.RED4.ScriptFormats.Instructions
{
    [Instruction(Opcode.New)]
    internal static class _Reference0
    {
        public const int ChainCount = 0;

        public static (object, uint) Read(IDefinitionReader reader)
        {
            var definition = reader.ReadReference();
            return (definition, 8);
        }

        public static uint Write(object argument, IDefinitionWriter writer)
        {
            var definition = (Definition)argument;
            writer.WriteReference(definition);
            return 8;
        }
    }

    internal static class _DefinitionRef<T>
        where T: Definition
    {
        public static (object, uint) Read(IDefinitionReader reader)
        {
            var definition = reader.ReadReference<T>();
            return (definition, 8);
        }

        public static uint Write(object argument, IDefinitionWriter writer)
        {
            var definition = (T)argument;
            writer.WriteReference(definition);
            return 8;
        }
    }
}
