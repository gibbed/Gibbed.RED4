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

namespace Gibbed.RED4.ScriptFormats
{
    public static class DefinitionFactory
    {
        public static Definition Create(DefinitionType type)
        {
            Definition instance;
            if (_Lookup.TryGetValue(type, out var func) == false)
            {
                instance = null;
            }
            else
            {
                instance = func();
            }
            if (instance == null)
            {
                throw new InvalidOperationException($"failed to create instance for {type}");
            }
            return instance;
        }

        static DefinitionFactory()
        {
            _Lookup = new Dictionary<DefinitionType, Func<Definition>>()
            {
                { DefinitionType.Native, () => new Definitions.NativeDefinition() },
                { DefinitionType.Class, () => new Definitions.ClassDefinition() },
                { DefinitionType.Enumeral, () => new Definitions.EnumeralDefinition() },
                { DefinitionType.Enumeration, () => new Definitions.EnumerationDefinition() },
                { DefinitionType.Function, () => new Definitions.FunctionDefinition() },
                { DefinitionType.Parameter, () => new Definitions.ParameterDefinition() },
                { DefinitionType.Local, () => new Definitions.LocalDefinition() },
                { DefinitionType.Property, () => new Definitions.PropertyDefinition() },
                { DefinitionType.SourceFile, () => new Definitions.SourceFileDefinition() },
            };
        }

        private static readonly Dictionary<DefinitionType, Func<Definition>> _Lookup;
    }
}
