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
                .Where(t => t.Flags.HasFlag(FunctionFlags.HasBody))
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
                FunctionFlags.HasBody;
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

            var groups = InstructionGrouper.GroupBody(cacheFile, function).ToArray();

            var groupStack = new LinkedList<(InstructionGrouper.Group, int)>();
            foreach (var group in groups)
            {
                groupStack.AddLast((group, 0));
            }

            int opcodeIdx = 0;
            Opcode? previousOp = null;
            while (groupStack.Count > 0)
            {
                var (group, depth) = groupStack.First.Value;
                groupStack.RemoveFirst();

                var instruction = group.Instruction;

                if (instruction.LoadInfo == null)
                {
                    sb.Append($"  (@0x?????? ????) @????");
                }
                else
                {
                    var loadInfo = instruction.LoadInfo.Value;

                    long absolutePosition;
                    long relativePosition;
                    if (validate)
                    {
                        absolutePosition = loadInfo.BasePosition + function.LoadPosition;
                        relativePosition = loadInfo.BasePosition - function.BodyLoadPosition;
                    }
                    else
                    {
                        absolutePosition = loadInfo.BasePosition;
                        relativePosition = loadInfo.BasePosition - function.BodyLoadPosition;
                    }

                    sb.Append($"  (@0x{absolutePosition:X6} {relativePosition,4}) @{loadInfo.Offset,-4} OP{opcodeIdx++,-4}");
                }

                sb.Append(" | ");

                DumpInstruction(instruction, sb, depth);

                foreach (var child in group.Children.Reverse<InstructionGrouper.Group>())
                {
                    groupStack.AddFirst((child, depth + 1));
                }
            }

            sb.AppendLine("}");
        }

        private static void DumpInstruction(Instruction instruction, StringBuilder sb, int depth)
        {
            var opName = Enum.GetName(typeof(Opcode), instruction.Op) ?? (instruction.Op.ToString() + "?");

            if (depth > 0)
            {
                sb.Append(new string(' ', depth * 2));
            }

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
            else if (instruction.Op == Opcode.EnumConst)
            {
                var (enumeration, enumeral) = (EnumConst)instruction.Argument;
                sb.AppendLine($" ({enumeration.ToPath()}, {enumeral.ToName()})");
            }
            else if (instruction.Op == Opcode.LocalVar)
            {
                var local = (LocalDefinition)instruction.Argument;
                sb.AppendLine($" {local.ToName()} // {local.Type.ToPath()}");
            }
            else if (instruction.Op == Opcode.ParamVar)
            {
                var parameter = (ParameterDefinition)instruction.Argument;
                sb.AppendLine($" {parameter.ToName()} // {parameter.Type.ToPath()}");
            }
            else if (instruction.Op == Opcode.ObjectVar)
            {
                var property = (PropertyDefinition)instruction.Argument;
                sb.AppendLine($" {property.ToPath()} // {property.Type.ToPath()}");
            }
            else if (instruction.Op == Opcode.Jump ||
                instruction.Op == Opcode.JumpIfFalse ||
                instruction.Op == Opcode.Context)
            {
                var jumpOffset = (short)instruction.Argument;
                sb.Append($" {jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.AppendLine();
            }
            else if (instruction.Op == Opcode.Switch)
            {
                var (switchType, firstCaseOffset) = (Switch)instruction.Argument;
                sb.Append($" ({firstCaseOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + firstCaseOffset}");
                }
                sb.Append($", {switchType.ToPath()}");
                /*if (switchType.BaseType != null)
                {
                    sb.Append($" \/\* {switchType.BaseType.ToPath()} \*\/");
                }*/
                sb.AppendLine($")");
            }
            else if (instruction.Op == Opcode.SwitchLabel)
            {
                var (falseOffset, trueOffset) = (SwitchLabel)instruction.Argument;
                sb.Append($" (false: {falseOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + falseOffset}");
                }
                sb.Append($", true: {trueOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + trueOffset}");
                }
                sb.AppendLine(")");
            }
            else if (instruction.Op == Opcode.Skip)
            {
                var jumpOffset = (short)instruction.Argument;
                sb.Append($" {jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.AppendLine();
            }
            else if (instruction.Op == Opcode.FinalFunc)
            {
                var (jumpOffset, sourceLine, function) = (FinalFunc)instruction.Argument;

                sb.Append($" ({jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.Append($", {sourceLine}, {function.ToPath()})");

                if (function.Parameters.Count > 0)
                {
                    sb.Append($" // parameters={function.Parameters.Count} (");
                    for (int i = 0; i < function.Parameters.Count; i++)
                    {
                        var parameter = function.Parameters[i];
                        if (i > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append($"{parameter.Name}: {parameter.Type.ToPath()}");
                    }
                    sb.Append(")");
                }

                sb.AppendLine();
            }
            else if (instruction.Op == Opcode.VirtualFunc)
            {
                var (jumpOffset, unknown, name) = (VirtualFunc)instruction.Argument;

                sb.Append($" ({jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.AppendLine($", {unknown}, {name})");
            }
            else
            {
                sb.AppendLine($" {instruction.Argument}");
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
