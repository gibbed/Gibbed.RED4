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
using System.IO;
using System.Linq;
using Gibbed.RED4.ScriptFormats;
using Gibbed.RED4.ScriptFormats.Definitions;
using Gibbed.RED4.ScriptFormats.Emit;
using Gibbed.RED4.ScriptHelpers;

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

            RemoveStaticArraySizeUsage(cache);

            var mySourceFile = new SourceFileDefinition(@"_\gibbed\exec.script");
            cache.Definitions.Add(mySourceFile);

            AddExecCommandSTS(cache, mySourceFile);
            AddMinimapScaler(cache);

            PatchJohnnySkillChecks(cache);

            byte[] testCacheBytes;
            using (var output = new MemoryStream())
            {
                cache.Save(output);
                output.Flush();
                testCacheBytes = output.ToArray();
            }

            File.WriteAllBytes("test_patch.redscripts", testCacheBytes);
        }

        private static void RemoveStaticArraySizeUsage(CacheFile cache)
        {
            foreach (var function in cache.Definitions
                .OfType<FunctionDefinition>()
                .Where(fd => (fd.Flags & FunctionFlags.HasCode) != 0))
            {
                for (int i = 0; i < function.Code.Count;)
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
        }

        private static void AddExecCommandSTS(CacheFile cache, SourceFileDefinition mySourceFile)
        {
            var gameInstanceNative = cache.GetNative("GameInstance");
            var stringNative = cache.GetNative("String");

            var inkMenuInstanceSwitchToScenarioClass = cache.GetClass("inkMenuInstance_SwitchToScenario");

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
            var stringToNameFunction = cache.GetFunction("StringToName");
            var getUISystemFunction = cache.GetFunction("GameInstance", "GetUISystem");
            var uiSystemClass = cache.GetClass("UISystem");
            var queueEventFunction = uiSystemClass.GetFunction("QueueEvent");

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
        }

        private static void AddMinimapScaler(CacheFile cache)
        {
            var visionRadii = new(PropertyDefinition property, string propertyName, float scale)[]
            {
                (null, "visionRadiusVehicle", 3.0f),
                (null, "visionRadiusCombat", 1.5f),
                (null, "visionRadiusQuestArea", 1.5f),
                (null, "visionRadiusSecurityArea", 1.5f),
                (null, "visionRadiusInterior", 1.5f),
                (null, "visionRadiusExterior", 3.0f),
            };

            var floatNative = cache.GetNative("Float");
            var minimapContainerController = cache.GetClass("MinimapContainerController");

            for (int i = 0; i < visionRadii.Length; i++)
            {
                var visionRadius = visionRadii[i];
                if (visionRadius.scale.Equals(0.0f) == true ||
                    visionRadius.scale.Equals(1.0f) == true)
                {
                    // if the scale is 0.0 or 1.0, skip
                    continue;
                }
                visionRadius.property = CreateNativeProperty(visionRadius.propertyName, floatNative);
                visionRadius.property.Parent = minimapContainerController;
                cache.Definitions.Add(visionRadius.property);
                minimapContainerController.Properties.Add(visionRadius.property);
                visionRadii[i] = visionRadius;
            }

            var operatorMultiply = cache.GetFunction("OperatorMultiply;FloatFloat;Float");

            var minimapContainerControllerOnInitialize = minimapContainerController.GetFunction("OnInitialize");

            var code = minimapContainerControllerOnInitialize.Code;

            // remove trailing nop
            code.RemoveAt(code.Count - 1);

            /* generate code for each vision radius property:
             *   this.(property) *= scale
             */
            var cg = new CodeGenerator(code.Count);
            foreach (var visionRadius in visionRadii)
            {
                if (visionRadius.property == null)
                {
                    // property didn't get created -- skip
                    continue;
                }
                cg.Emit(Opcode.Assign);
                {
                    cg.Emit(Opcode.ObjectVar, visionRadius.property);
                    var afterMultiplyLabel = cg.DefineLabel();
                    cg.Emit(Opcode.FinalFunc, afterMultiplyLabel, 0, operatorMultiply);
                    {
                        cg.Emit(Opcode.ObjectVar, visionRadius.property);
                        cg.Emit(Opcode.FloatConst, visionRadius.scale);
                        cg.Emit(Opcode.ParamEnd);
                    }
                    cg.MarkLabel(afterMultiplyLabel);
                }
            }
            cg.Emit(Opcode.Nop);

            code.AddRange(cg.GetCode());
        }

        private static void PatchJohnnySkillChecks(CacheFile cache)
        {
            // This removes the code checks that make the "Johnny" node have the glitch out effect.

            // These are hardcoded locations of the checks in the code. This is BAD.
            // TODO(gibbed): dynamically search for the checks.

            // PerksMainGameController::OnAttributeClicked
            // Remove an entire Jump chain.
            var target1 = cache.GetFunction("PerksMainGameController", "OnAttributeClicked");
            for (int i = 0; i < 6; i++)
            {
                target1.Code[i] = new Instruction(Opcode.Nop);
            }

            // PerksMainGameController::OnProficiencyClicked
            // Remove an entire Jump chain.
            var target2 = cache.GetFunction("PerksMainGameController", "OnProficiencyClicked");
            for (int i = 0; i < 6; i++)
            {
                target2.Code[i] = new Instruction(Opcode.Nop);
            }

            // PerksMenuAttributeDisplayController::Update
            // Change a JumpIfFalse to Jump.
            var target3 = cache.GetFunction("PerksMenuAttributeDisplayController", "Update;");
            target3.Code[2] = new Instruction(Opcode.Jump, target3.Code[2].Argument);

            // PerkMenuTooltipController::SetupCustom
            // Change a switch label to jump to the same location as the false result.
            var target4 = cache.GetFunction("PerkMenuTooltipController", "SetupCustom;AttributeTooltipData");
            target4.Code[4] = new Instruction(Opcode.SwitchLabel, new Gibbed.RED4.ScriptFormats.Instructions.SwitchLabel(10, 10));

            // PerksMenuAttributeDisplayController::SetHovered
            // Change a JumpIfFalse to Jump.
            var target5 = cache.GetFunction("PerksMenuAttributeDisplayController", "SetHovered;Bool");
            target5.Code[10] = new Instruction(Opcode.Jump, target5.Code[10].Argument);
        }

        private static PropertyDefinition CreateNativeProperty(string name, NativeDefinition type)
        {
            return new PropertyDefinition()
            {
                Name = name,
                Flags = PropertyFlags.IsNative | PropertyFlags.Unknown10,
                Type = type,
                Visibility = Visibility.Protected,
            };
        }
    }
}
