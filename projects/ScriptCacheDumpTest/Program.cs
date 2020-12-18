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
using System.Text;
using Gibbed.RED4.ScriptFormats;
using Gibbed.RED4.ScriptFormats.Definitions;
using Gibbed.RED4.ScriptFormats.Instructions;

namespace ScriptCacheDumpTest
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

            byte[] testCacheBytes;
            using (var output = new MemoryStream())
            {
                cache.Save(output);
                output.Flush();
                testCacheBytes = output.ToArray();
            }

            File.WriteAllBytes("test_roundtrip.redscripts", testCacheBytes);
            CacheFile testCache;
            using (var input = new MemoryStream(testCacheBytes, false))
            {
                testCache = CacheFile.Load(input, validate);
            }

            DumpExecCallableFunctions(cache);
            DumpFunctions(cache, validate);
            DumpEnumerations(cache);
        }

        private static void DumpExecCallableFunctions(CacheFile scriptCacheFile)
        {
            var gameInstanceDef = scriptCacheFile.Definitions
                            .OfType<NativeDefinition>()
                            .Where(t => t.Name == "GameInstance")
                            .SingleOrDefault();
            var stringType = scriptCacheFile.Definitions
                .OfType<NativeDefinition>()
                .SingleOrDefault(t => t.Name == "String");
            var candidates = scriptCacheFile.Definitions
                .OfType<FunctionDefinition>()
                .Where(f =>
                    f.Parameters.Count >= 1 &&
                    f.Parameters.Count <= 6 &&
                    f.Parameters[0].Type == gameInstanceDef &&
                    f.Parameters.Skip(1).All(p => p.Type == stringType))
                .ToArray();
            var sb = new StringBuilder();
            foreach (var tuple in candidates
                .Select(c => (function: c, path: c.ToPath()))
                .Where(t => t.function.Parent == null)
                .OrderBy(t => t.path))
            {
                var (function, path) = tuple;
                sb.Append($"{path}(");
                for (int i = 1; i < function.Parameters.Count; i++)
                {
                    var parameter = function.Parameters[i];
                    if (i > 1)
                    {
                        sb.Append(", ");
                    }
                    sb.Append($"{parameter.Name}");
                }
                sb.AppendLine(")");
            }
            File.WriteAllText("exec_callable_script_functions.txt", sb.ToString());
        }

        private static void DumpFunctions(CacheFile cache, bool validate)
        {
            string currentSourcePath = "***DUMP FUNCTIONS***";
            FunctionDefinition previousFunction = null;
            var sb = new StringBuilder();
            foreach (var function in cache.Definitions
                .OfType<FunctionDefinition>()
                .Where(t => t.Flags.HasFlag(FunctionFlags.HasCode))
                .OrderBy(f => f.SourceFile?.Path)
                .ThenBy(f => f.SourceLine))
            {
                if (previousFunction != null)
                {
                    sb.AppendLine();
                }

                if (function.SourceFile?.Path != currentSourcePath)
                {
                    currentSourcePath = function.SourceFile?.Path;
                    if (previousFunction != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                        sb.AppendLine();
                    }
                    sb.AppendLine($"// SOURCE PATH: {currentSourcePath ?? "UNKNOWN"}");
                    sb.AppendLine();
                }

                DumpFunction(cache, function, sb, validate);
                previousFunction = function;
            }

            File.WriteAllText("test_function_dump.txt", sb.ToString(), Encoding.UTF8);
        }

        private static void DumpFunction(CacheFile cacheFile, FunctionDefinition function, StringBuilder sb, bool validate)
        {
            var functionName = function.ToName();
            if (function.Name != functionName)
            {
                sb.AppendLine($"// {function.Name}");
            }

            sb.Append("function ");

            sb.Append(function.ToPath());

            sb.Append("(");

            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var parameter = function.Parameters[i];
                if (i > 0)
                {
                    sb.Append(", ");
                }
                sb.Append($"{parameter.ToName()}: {parameter.Type.ToPath()}");
            }

            sb.Append(")");

            if (function.ReturnType != null)
            {
                sb.Append($" : {function.ReturnType.ToPath()} ");
            }

            sb.AppendLine();
            sb.AppendLine("{");

            const FunctionFlags ignoredFlags = FunctionFlags.HasReturnValue |
                FunctionFlags.HasParameters | FunctionFlags.HasLocals |
                FunctionFlags.HasCode;
            var flags = function.Flags & ~ignoredFlags;
            if (flags != FunctionFlags.None)
            {
                sb.AppendLine($"  // Flags : {flags}");
            }

            if (function.Locals != null)
            {
                foreach (var local in function.Locals)
                {
                    sb.AppendLine($"  var {local.ToName()} : {local.Type.ToPath()};");
                }
            }

            var groups = InstructionGrouper.GroupCode(cacheFile, function).ToArray();

            var groupStack = new LinkedList<(InstructionGrouper.Group group, string indent, bool isLast)>();
            for (int i = 0; i < groups.Length; i++)
            {
                groupStack.AddLast((groups[i], "", i == groups.Length - 1));
            }

            int opcodeIndex = 0;
            while (groupStack.Count > 0)
            {
                var (group, indent, isLast) = groupStack.First.Value;
                groupStack.RemoveFirst();

                var instruction = group.Instruction;

                if (instruction.LoadPosition < 0)
                {
                    sb.Append($"  (@0x?????? ????) #{opcodeIndex++,-4}");
                }
                else
                {
                    var loadPosition = instruction.LoadPosition;

                    long absolutePosition;
                    long relativePosition;
                    if (validate)
                    {
                        absolutePosition = loadPosition + function.LoadPosition;
                        relativePosition = loadPosition - function.CodeLoadPosition;
                    }
                    else
                    {
                        absolutePosition = loadPosition;
                        relativePosition = loadPosition - function.CodeLoadPosition;
                    }

                    sb.Append($"  (@0x{absolutePosition:X6} {relativePosition,4}) #{opcodeIndex++,-4}");
                }

                sb.Append(indent);
                sb.Append(isLast == true ? " └─" : " ├─");

                DumpInstruction(instruction, sb);

                var lastChildIndex = group.Children.Count - 1;
                for (int i = lastChildIndex; i >= 0; i--)
                {
                    var child = group.Children[i];
                    groupStack.AddFirst((child, indent + (isLast == true ? "   " : " │ "), i == lastChildIndex));
                }
            }

            sb.AppendLine("}");
        }

        private static void DumpInstruction(Instruction instruction, StringBuilder sb)
        {
            var opName = GetOpcodeName(instruction);

            sb.Append($"{opName}");

            if (instruction.Argument == null)
            {
                sb.AppendLine();
                return;
            }

            if (instruction.Argument is string s)
            {
                sb.AppendLine($" \"{s}\"");
            }
            else if (instruction.Argument is byte[] bytes)
            {
                sb.Append(" bytes(");
                sb.Append(string.Join(", ", bytes.Select(b => "0x" + b.ToString("X2")).ToArray()));
                sb.AppendLine(")");
            }
            else if (instruction.Opcode == Opcode.EnumConst)
            {
                var (enumeration, enumeral) = (EnumConst)instruction.Argument;
                sb.AppendLine($" ({enumeration.ToPath()}, {enumeral.ToName()})");
            }
            else if (instruction.Opcode == Opcode.LocalVar)
            {
                var local = (LocalDefinition)instruction.Argument;
                sb.AppendLine($" {local.ToName()} // {local.Type.ToPath()}");
            }
            else if (instruction.Opcode == Opcode.ParamVar)
            {
                var parameter = (ParameterDefinition)instruction.Argument;
                sb.AppendLine($" {parameter.ToName()} // {parameter.Type.ToPath()}");
            }
            else if (instruction.Opcode == Opcode.ObjectVar || instruction.Opcode == Opcode.StructMember)
            {
                var property = (PropertyDefinition)instruction.Argument;
                sb.AppendLine($" {property.ToPath()} // {property.Type.ToPath()}");
            }
            else if (instruction.Opcode == Opcode.Jump ||
                instruction.Opcode == Opcode.JumpIfFalse ||
                instruction.Opcode == Opcode.Skip ||
                instruction.Opcode == Opcode.Context)
            {
                var targetIndex = (int)instruction.Argument;
                sb.AppendLine($" =>{targetIndex}");
            }
            else if (instruction.Opcode == Opcode.Switch)
            {
                var (switchType, firstCaseIndex) = (Switch)instruction.Argument;
                sb.Append($" (=>{firstCaseIndex}, {switchType.ToPath()}");
                /*if (switchType.BaseType != null)
                {
                    sb.Append($" \/\* {switchType.BaseType.ToPath()} \*\/");
                }*/
                sb.AppendLine($")");
            }
            else if (instruction.Opcode == Opcode.SwitchLabel)
            {
                var (falseIndex, trueIndex) = (SwitchLabel)instruction.Argument;
                sb.AppendLine($" (false=>{falseIndex}, true=>{trueIndex})");
            }
            else if (instruction.Opcode == Opcode.FinalFunc)
            {
                var (nextIndex, sourceLine, function) = (FinalFunc)instruction.Argument;
                sb.Append($" (=>{nextIndex}, {sourceLine}, {function.ToPath()})");
                if (function.Parameters.Count > 0 || function.ReturnType != null)
                {
                    sb.Append(" //");
                    if (function.Parameters.Count > 0)
                    {
                        sb.Append($" parameters={function.Parameters.Count} (");
                        for (int i = 0; i < function.Parameters.Count; i++)
                        {
                            var parameter = function.Parameters[i];
                            if (i > 0)
                            {
                                sb.Append(", ");
                            }
                            sb.Append($"{parameter.Name}: {parameter.Type.ToPath()}");
                        }
                        sb.Append(')');
                    }
                    if (function.ReturnType != null)
                    {
                        sb.Append($" return={function.ReturnType.ToPath()}");
                    }
                }
                sb.AppendLine();
            }
            else if (instruction.Opcode == Opcode.VirtualFunc)
            {
                var (nextIndex, sourceLine, name) = (VirtualFunc)instruction.Argument;
                sb.AppendLine($" (=>{nextIndex}, {sourceLine}, {name})");
            }
            else
            {
                sb.AppendLine($" {instruction.Argument}");
            }

            static string GetOpcodeName(Instruction instruction)
            {
                var name = Enum.GetName(typeof(Opcode), instruction.Opcode);
                if (name == null || name.StartsWith("Unknown") == true)
                {
                    return ((byte)instruction.Opcode).ToString() + "?";
                }
                return name;
            }
        }

        private static void DumpEnumerations(CacheFile scriptCacheFile)
        {
            EnumerationDefinition previousEnumeration = null;
            var sb = new StringBuilder();
            foreach (var tuple in scriptCacheFile.Definitions
                .OfType<EnumerationDefinition>()
                .Select(e => (enumeration: e, path: e.ToPath()))
                .OrderBy(t => t.path))
            {
                var (enumeration, path) = tuple;

                if (previousEnumeration != null)
                {
                    sb.AppendLine();
                }

                sb.AppendLine($"enum {path} // {enumeration.Size}");
                sb.AppendLine("{");
                foreach (var enumeral in enumeration.Enumerals)
                {
                    sb.Append("  ");
                    if (string.IsNullOrEmpty(enumeral.Name) == true)
                    {
                        sb.Append("_ /* blank name */");
                    }
                    else
                    {
                        sb.Append(enumeral.Name);
                    }

                    sb.AppendLine($" = {enumeral.Value},");
                }
                sb.AppendLine("}");

                previousEnumeration = enumeration;
            }

            File.WriteAllText("test_enumeration_dump.txt", sb.ToString(), Encoding.UTF8);
        }
    }
}
