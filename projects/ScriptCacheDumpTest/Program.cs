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
using Gibbed.RED4.ScriptFormats.ScriptedTypes;

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

            var sb = new StringBuilder();
            foreach (var function in scriptCacheFile.Types
                .OfType<FunctionType>()
                .Where(t => t.Flags.HasFlag(FunctionFlags.HasBody)))
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

                Opcode? previousOp = null;
                foreach (var instruction in function.Body)
                {
                    if (previousOp == Opcode.End)
                    {
                        sb.AppendLine();
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

                    var opName = Enum.GetName(typeof(Opcode), instruction.Op) ?? (instruction.Op.ToString() + "?");

                    sb.Append($"  {opName,20}");

                    if (instruction.Argument == null)
                    {
                        sb.AppendLine();
                        continue;
                    }

                    if (instruction.Argument is string s)
                    {
                        sb.AppendLine($"  \"{s}\"");
                    }
                    else if (instruction.Argument is byte[] bytes)
                    {
                        sb.Append("  bytes(");
                        sb.Append(string.Join(", ", bytes.Select(b => "0x" + b.ToString("X2")).ToArray()));
                        sb.AppendLine(")");
                    }
                    else if (instruction.Op == Opcode.Jump ||
                        instruction.Op == Opcode.JumpFalse)
                    {
                        var jumpOffset = (short)instruction.Argument;
                        sb.Append($"  {jumpOffset:+#;-#}");
                        if (instruction.LoadInfo.HasValue == true)
                        {
                            sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                        }
                        sb.AppendLine();
                    }
                    else if (instruction.Op == Opcode.Call)
                    {
                        (var jumpOffset, var unknown, var functionType) = ((short, ushort, FunctionType))instruction.Argument;

                        sb.Append($"  ({jumpOffset:+#;-#}");
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

                        sb.Append($"  ({jumpOffset:+#;-#}");
                        if (instruction.LoadInfo.HasValue == true)
                        {
                            sb.Append($" => {instruction.LoadInfo.Value.Offset + jumpOffset}");
                        }
                        sb.AppendLine($", {unknown}, {unknownIndex})");
                    }
                    else
                    {
                        sb.AppendLine($"  {instruction.Argument}");
                    }

                    previousOp = instruction.Op;
                }

                sb.AppendLine();
                sb.AppendLine();
            }

            File.WriteAllText("test_function_dump.txt", sb.ToString(), Encoding.UTF8);
        }

        private static string GetPath(ScriptedType type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            var types = new List<ScriptedType>();
            while (type != null)
            {
                types.Insert(0, type);
                type = type.Parent;
            }
            return string.Join("::", types.Select(t => t.Name).ToArray());
        }
    }
}
