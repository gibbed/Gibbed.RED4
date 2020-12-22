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
using System.Linq;
using Gibbed.RED4.ScriptFormats;
using Gibbed.RED4.ScriptFormats.Definitions;

namespace Gibbed.RED4.ScriptHelpers
{
    public static class CacheHelpers
    {
        public static T GetDefinition<T>(this CacheFile cache, string name)
            where T : Definition
        {
            return cache.Definitions
                .OfType<T>()
                .Single(d => d.Name == name);
        }

        public static T GetDefinition<T>(this CacheFile cache, string name, Func<T, bool> predicate)
            where T : Definition
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return cache.Definitions
                .OfType<T>()
                .Single(d => d.Name == name && predicate(d) == true);
        }

        public static IEnumerable<T> GetDefinitions<T>(this CacheFile cache, Func<T, bool> predicate)
           where T : Definition
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return cache.Definitions
                .OfType<T>()
                .Where(d => predicate(d) == true);
        }

        public static NativeDefinition GetNative(this CacheFile cache, string name)
        {
            return cache.Definitions
                .OfType<NativeDefinition>()
                .Single(d => d.Name == name);
        }

        public static ClassDefinition GetClass(this CacheFile cache, string name)
        {
            return cache.Definitions
                .OfType<ClassDefinition>()
                .Single(d => d.Name == name);
        }

        public static ClassDefinition TryGetClass(this CacheFile cache, string name)
        {
            return cache.Definitions
                .OfType<ClassDefinition>()
                .SingleOrDefault(d => d.Name == name);
        }

        public static FunctionDefinition GetFunction(this CacheFile cache, string name)
        {
            return cache.Definitions
                .OfType<FunctionDefinition>()
                .Single(d => d.Parent == null && d.Name == name);
        }

        public static FunctionDefinition GetFunction(this CacheFile cache, string className, string functionName)
        {
            return cache.GetClass(className)
                .Functions
                .Single(d => d.Name == functionName);
        }

        public static FunctionDefinition GetFunction(this ClassDefinition @class, string name)
        {
            return @class
                .Functions
                .Single(d => d.Name == name);
        }
    }
}
