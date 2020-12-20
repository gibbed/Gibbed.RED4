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

namespace Gibbed.RED4.ScriptFormats.Instructions
{
    [Instruction(Opcode.Unknown27)]
    internal static class Unknown27
    {
        public const int ChainCount = 0;

        private const string _Error = nameof(Opcode.Unknown27)
            + " is generated at runtime by the game and cannot be serialized into the script cache correctly";

#pragma warning disable IDE0060 // Remove unused parameter
        public static (object, uint) Read(IDefinitionReader reader)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new NotSupportedException(_Error);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static uint Write(object argument, IDefinitionWriter writer)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            throw new NotSupportedException(_Error);
        }
    }
}
