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
using System.IO;
using System.Linq;
using Gibbed.RED4.ScriptFormats;
using Gibbed.RED4.ScriptFormats.Definitions;
using Gibbed.RED4.ScriptFormats.Emit;

namespace ScriptCachePatchTest
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            const bool validate = true;

            CacheFile cache;
            var fileBytes = File.ReadAllBytes(args[0]);
            using (var input = new MemoryStream(fileBytes, false))
            {
                cache = CacheFile.Load(input, validate);
            }

            // Remove StaticArraySize usage...
            foreach (var function in cache.Definitions
                .OfType<FunctionDefinition>()
                .Where(fd => (fd.Flags & FunctionFlags.HasCode) != 0))
            {
                for (int i = 0; i < function.Code.Count; )
                {
                    if (function.Code[i].Opcode != Opcode.StaticArraySize)
                    {
                        i++;
                        continue;
                    }

                    var staticArrayType = (NativeDefinition)function.Code[i].Argument;
                    if (staticArrayType.NativeType != NativeType.StaticArray)
                    {
                        throw new InvalidOperationException();
                    }

                    // replace StaticArraySize with int constant
                    var staticArraySize = staticArrayType.ArraySize;
                    function.Code[i] = staticArraySize switch
                    {
                        0 => new Instruction(Opcode.Int32Zero),
                        1 => new Instruction(Opcode.Int32One),
                        _ => new Instruction(Opcode.Int32Const, staticArraySize)
                    };
                    // replace referencing of static array with a nop
                    function.Code[i + 1] = new Instruction(Opcode.Nop);
                    i += 2;
                }
            }

            var gameInstanceNative = GetDefinition<NativeDefinition>(cache, "GameInstance");
            var stringNative = GetDefinition<NativeDefinition>(cache, "String");

            var inkMenuInstanceSwitchToScenarioClass = GetDefinition<ClassDefinition>(cache, "inkMenuInstance_SwitchToScenario");

            /* Add a new definition for a native inkMenuInstance_SwitchToScenario since it doesn't exist
             * in the cache by default. It is actually defined by the game but none of the existing game
             * code uses it, so it's missing.
             */
            var inkMenuInstanceSwitchToScenario = new NativeDefinition()
            {
                Name = "inkMenuInstance_SwitchToScenario",
                NativeType = NativeType.Complex,
            };
            cache.Definitions.Add(inkMenuInstanceSwitchToScenario);

            // Add a new ref native for the previous native.
            var inkMenuInstanceSwitchToScenarioRef = new NativeDefinition()
            {
                Name = "ref:inkMenuInstance_SwitchToScenario",
                NativeType = NativeType.Handle,
                BaseType = inkMenuInstanceSwitchToScenario,
            };
            cache.Definitions.Add(inkMenuInstanceSwitchToScenarioRef);

            // stuff we use in our custom function
            var stringToNameFunction = GetDefinition<FunctionDefinition>(cache, "StringToName");
            var getUISystemFunction = GetDefinition<FunctionDefinition>(cache, "GetUISystem");
            var uiSystemClass = GetDefinition<ClassDefinition>(cache, "UISystem");
            var queueEventFunction = GetDefinition<FunctionDefinition>(cache, "QueueEvent", fd => fd.Parent == uiSystemClass);

            var mySourceFile = new SourceFileDefinition(@"_\gibbed\exec.script");
            cache.Definitions.Add(mySourceFile);

            var myFunction = new FunctionDefinition()
            {
                Name = "STS",
                Flags =
                    FunctionFlags.Unknown0 | FunctionFlags.Unknown1 |
                    FunctionFlags.HasParameters | FunctionFlags.HasLocals |
                    FunctionFlags.HasCode,
                SourceFile = mySourceFile,
            };
            cache.Definitions.Add(myFunction);

            var myParameter0 = new ParameterDefinition()
            {
                Name = "gameInstance",
                Parent = myFunction,
                Type = gameInstanceNative,
                Unknown28 = 0,
            };
            cache.Definitions.Add(myParameter0);
            myFunction.Parameters.Add(myParameter0);

            var myParameter1 = new ParameterDefinition()
            {
                Name = "name",
                Parent = myFunction,
                Type = stringNative,
                Unknown28 = 0,
            };
            cache.Definitions.Add(myParameter1);
            myFunction.Parameters.Add(myParameter1);

            var myLocal0 = new LocalDefinition()
            {
                Name = "event",
                Parent = myFunction,
                Type = inkMenuInstanceSwitchToScenarioRef,
                Unknown28 = 0,
            };
            cache.Definitions.Add(myLocal0);
            myFunction.Locals.Add(myLocal0);

            var cg = new CodeGenerator();

            // event = new inkMenuInstance_SwitchToScenario
            cg.Emit(Opcode.Assign);
            {
                cg.Emit(Opcode.LocalVar, myLocal0);
                cg.Emit(Opcode.New, inkMenuInstanceSwitchToScenarioClass);
            }

            // event.Init(StringToName(name), 0)
            var afterEventContextLabel = cg.DefineLabel();
            cg.Emit(Opcode.Context, afterEventContextLabel);
            {
                cg.Emit(Opcode.LocalVar, myLocal0);
                cg.Emit(Opcode.FinalFunc, afterEventContextLabel, 0, inkMenuInstanceSwitchToScenarioClass.Functions[0]);
                {
                    // StringToName(name)
                    var afterStringToNameLabel = cg.DefineLabel();
                    cg.Emit(Opcode.FinalFunc, afterStringToNameLabel, 0, stringToNameFunction);
                    {
                        cg.Emit(Opcode.ParamVar, myParameter1);
                        cg.Emit(Opcode.ParamEnd);
                    }
                    cg.MarkLabel(afterStringToNameLabel);
                }
                cg.Emit(Opcode.Nop);
                cg.Emit(Opcode.ParamEnd);
            }
            cg.MarkLabel(afterEventContextLabel);

            // getUISystem(gameInstance).QueueEvent(event)
            var afterUISystemContextLabel = cg.DefineLabel();
            cg.Emit(Opcode.Context, afterUISystemContextLabel);
            {
                var afterGetUISystemLabel = cg.DefineLabel();
                cg.Emit(Opcode.FinalFunc, afterGetUISystemLabel, 0, getUISystemFunction);
                {
                    cg.Emit(Opcode.ParamVar, myParameter0);
                    cg.Emit(Opcode.ParamEnd);
                }
                cg.MarkLabel(afterGetUISystemLabel);
                cg.Emit(Opcode.FinalFunc, afterUISystemContextLabel, 0, queueEventFunction);
                {
                    cg.Emit(Opcode.LocalVar, myLocal0);
                    cg.Emit(Opcode.ParamEnd);
                }
            }
            cg.MarkLabel(afterUISystemContextLabel);

            cg.Emit(Opcode.Nop);
            myFunction.Code.AddRange(cg.GetCode());

            byte[] testCacheBytes;
            using (var output = new MemoryStream())
            {
                cache.Save(output);
                output.Flush();
                testCacheBytes = output.ToArray();
            }

            File.WriteAllBytes("test_patch.redscripts", testCacheBytes);
        }

        private static T GetDefinition<T>(CacheFile cache, string name)
            where T : Definition
        {
            return cache.Definitions
                .OfType<T>()
                .Single(d => d.Name == name);
        }

        private static T GetDefinition<T>(CacheFile cache, string name, Func<T, bool> predicate)
            where T: Definition
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return cache.Definitions
                .OfType<T>()
                .Single(d => d.Name == name && predicate(d) == true);
        }

        private static IEnumerable<T> GetDefinitions<T>(CacheFile cache, Func<T, bool> predicate)
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
    }
}
