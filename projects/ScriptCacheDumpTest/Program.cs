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

namespace ScriptCacheDumpTest
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            const bool validate = true;

            CacheFile scriptCacheFile;
            var fileBytes = File.ReadAllBytes(args[0]);
            using (var input = new MemoryStream(fileBytes, false))
            {
                scriptCacheFile = CacheFile.Load(input, validate);
            }

            string currentSourcePath = null;
            FunctionDefinition previousFunction = null;
            var sb = new StringBuilder();
            foreach (var function in scriptCacheFile.Definitions
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

                DumpFunction(function, sb, validate);
                previousFunction = function;
            }

            File.WriteAllText("test_function_dump.txt", sb.ToString(), Encoding.UTF8);
        }

        private static void DumpFunction(FunctionDefinition function, StringBuilder sb, bool validate)
        {
            if (function.ReturnType == null)
            {
                sb.Append("void ");
            }
            else
            {
                sb.Append($"{GetPath(function.ReturnType)} ");
            }

            sb.AppendLine(GetPath(function));

            if (function.Flags != FunctionFlags.None)
            {
                sb.AppendLine($"  // Flags : {function.Flags}");
            }

            if (function.Parameters != null)
            {
                foreach (var parameter in function.Parameters)
                {
                    sb.AppendLine($"  // {parameter} : {parameter.Type}");
                }
            }
            if (function.Locals != null)
            {
                foreach (var local in function.Locals)
                {
                    sb.AppendLine($"  // {local} : {local.Type}");
                }
            }

            var groups = InstructionGrouper.GroupBody(function).ToArray();

            var groupStack = new LinkedList<(InstructionGrouper.Group, int)>();
            foreach (var group in groups)
            {
                groupStack.AddLast((group, 0));
            }

            Opcode? previousOp = null;
            while (groupStack.Count > 0)
            {
                (var group, var depth) = groupStack.First.Value;
                groupStack.RemoveFirst();

                var instruction = group.Instruction;

                if (previousOp == Opcode.NoOperation || previousOp == Opcode.ReturnWithValue)
                {
                    if (instruction.Op != Opcode.Switch &&
                        instruction.Op != Opcode.SwitchCase &&
                        instruction.Op != Opcode.SwitchDefault)
                    {
                        //sb.AppendLine();
                    }
                }

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

                    sb.Append($"  (@0x{absolutePosition:X6} {relativePosition,4}) @{loadInfo.Offset,-4}");
                }

                sb.Append(" | ");

                DumpInstruction(instruction, sb, depth);

                foreach (var child in group.Children.Reverse<InstructionGrouper.Group>())
                {
                    groupStack.AddFirst((child, depth + 1));
                }
            }
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
            else if (instruction.Op == Opcode.Jump ||
                instruction.Op == Opcode.JumpFalse ||
                instruction.Op == (Opcode)41)
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
                (var switchType, var jumpOffset) = ((NativeDefinition, short))instruction.Argument;
                sb.Append($" ({jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.AppendLine($", {switchType})");
            }
            else if (instruction.Op == Opcode.SwitchCase)
            {
                (var defaultJumpOffset, var caseJumpOffset) = ((short, short))instruction.Argument;
                sb.Append($" (false: {defaultJumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + defaultJumpOffset}");
                }
                sb.Append($", true: {caseJumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + caseJumpOffset}");
                }
                sb.AppendLine(")");
            }
            else if (instruction.Op == Opcode.Call)
            {
                (var jumpOffset, var unknown, var functionType) = ((short, ushort, FunctionDefinition))instruction.Argument;

                sb.Append($" ({jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.Append($", {unknown}, {functionType})");

                if (functionType.Parameters != null && functionType.Parameters.Length > 0)
                {
                    sb.Append($"  [parameters={functionType.Parameters.Length}]");
                }

                sb.AppendLine();
            }
            else if (instruction.Op == (Opcode)37)
            {
                (var jumpOffset, var unknown, var unknownIndex) = ((short, ushort, uint))instruction.Argument;

                sb.Append($" ({jumpOffset:+#;-#}");
                if (instruction.LoadInfo.HasValue == true)
                {
                    sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                }
                sb.AppendLine($", {unknown}, {unknownIndex})");
            }
            else
            {
                sb.AppendLine($" {instruction.Argument}");
            }
        }

        private static string GetPath(Definition type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var types = new List<Definition>();
            while (type != null)
            {
                types.Insert(0, type);
                type = type.Parent;
            }
            return string.Join("::", types.Select(t => t.Name).ToArray());
        }
    }
}
