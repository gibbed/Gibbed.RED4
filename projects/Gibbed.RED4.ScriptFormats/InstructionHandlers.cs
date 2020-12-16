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
using Gibbed.RED4.ScriptFormats.Instructions;

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    internal static class InstructionHandlers
    {
        // All of these readers are completely temporary while research is on-going.
        // TODO(gibbed): concrete types for all of this.
        internal static (Instruction, uint) Read(IDefinitionReader reader)
        {
            uint size = 0;

            var op = (Opcode)reader.ReadValueU8();
            size++;

            if (_Lookup.TryGetValue(op, out var handler) == false)
            {
                throw new FormatException($"no handler for {op}");
            }

            Instruction instance;
            instance.Op = op;
            if (handler.read == null)
            {
                instance.Argument = null;
            }
            else
            {
                var (argument, argumentSize) = handler.read(reader);
                instance.Argument = argument;
                size += argumentSize;
            }
            instance.LoadInfo = default;
            return (instance, size);
        }

        internal static uint Write(Instruction instruction, IDefinitionWriter writer)
        {
            var op = instruction.Op;

            uint size = 0;

            writer.WriteValueU8((byte)op);
            size++;

            if (_Lookup.TryGetValue(op, out var handler) == false)
            {
                throw new FormatException($"no handler for {op}");
            }

            if (handler.write != null)
            {
                size += handler.write(instruction.Argument, writer);
            }

            return size;
        }

        private delegate (object, uint) ReadDelegate(IDefinitionReader reader);
        private delegate uint WriteDelegate(object argument, IDefinitionWriter writer);

        private static readonly Dictionary<Opcode, (ReadDelegate read, WriteDelegate write)> _Lookup;

        static InstructionHandlers()
        {
            _Lookup = new Dictionary<Opcode, (ReadDelegate, WriteDelegate)>(256)
            {
                { Opcode.Nop, default },
                { Opcode.Null, default },
                { Opcode.Int32One, default },
                { Opcode.Int32Zero, default },
                { Opcode.Int8Const, (Int8Const.Read, Int8Const.Write) },
                { Opcode.Int16Const, (Int16Const.Read, Int16Const.Write) },
                { Opcode.Int32Const, (Int32Const.Read, Int32Const.Write) },
                { Opcode.Int64Const, (Int64Const.Read, Int64Const.Write) },
                { Opcode.Uint8Const, (Uint8Const.Read, Uint8Const.Write) },
                { Opcode.Uint16Const, (Uint16Const.Read, Uint16Const.Write) },
                { Opcode.Uint32Const, (Uint32Const.Read, Uint32Const.Write) },
                { Opcode.Uint64Const, (Uint64Const.Read, Uint64Const.Write) },
                { Opcode.FloatConst, (FloatConst.Read, FloatConst.Write) },
                { Opcode.DoubleConst, (DoubleConst.Read, DoubleConst.Write) },
                { Opcode.NameConst, (NameConst.Read, NameConst.Write) },
                { Opcode.EnumConst, (EnumConst.Read, EnumConst.Write) },
                { Opcode.StringConst, (StringConst.Read, StringConst.Write) },
                { Opcode.TweakDBIdConst, (TweakDBIdConst.Read, TweakDBIdConst.Write) },
                { Opcode.ResourceConst, (ResourceConst.Read, ResourceConst.Write) },
                { Opcode.BoolTrue, default },
                { Opcode.BoolFalse, default },
                { (Opcode)21, (Unknown21.Read, Unknown21.Write) },
                { Opcode.Assign, default },
                { Opcode.Target, default },
                { Opcode.LocalVar, (_DefinitionRef<LocalDefinition>.Read, _DefinitionRef<LocalDefinition>.Write) },
                { Opcode.ParamVar, (_DefinitionRef<ParameterDefinition>.Read, _DefinitionRef<ParameterDefinition>.Write) },
                { Opcode.ObjectVar, (_DefinitionRef<PropertyDefinition>.Read, _DefinitionRef<PropertyDefinition>.Write) },
                { (Opcode)27, default },
                { Opcode.Switch, (Switch.Read, Switch.Write) },
                { Opcode.SwitchLabel, (SwitchLabel.Read, SwitchLabel.Write) },
                { Opcode.SwitchDefault, default },
                { Opcode.Jump, (_Jump.Read, _Jump.Write) },
                { Opcode.JumpIfFalse, (_Jump.Read, _Jump.Write) },
                { Opcode.Skip, (_Jump.Read, _Jump.Write) },
                { Opcode.Conditional, (_Conditional.Read, _Conditional.Write) },
                { Opcode.Constructor, (Constructor.Read, Constructor.Write) },
                { Opcode.FinalFunc, (FinalFunc.Read, FinalFunc.Write) },
                { Opcode.VirtualFunc, (VirtualFunc.Read, VirtualFunc.Write) },
                { Opcode.ParamEnd, default },
                { Opcode.Return, default },
                { Opcode.StructMember, (_DefinitionRef<PropertyDefinition>.Read, _DefinitionRef<PropertyDefinition>.Write) },
                { Opcode.Context, (_Jump.Read, _Jump.Write) },
                { Opcode.TestEqual, (_DefinitionRef.Read, _DefinitionRef.Write) },
                { Opcode.TestNotEqual, (_DefinitionRef.Read, _DefinitionRef.Write) },
                { (Opcode)44, (_DefinitionRef.Read, _DefinitionRef.Write) },
                { (Opcode)45, default },
                { Opcode.This, default },
                { (Opcode)47, (Unknown47.Read, Unknown47.Write) },
                { (Opcode)48, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)49, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)50, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)51, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)52, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)53, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)54, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)55, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)56, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)57, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)58, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)59, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)60, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)61, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)62, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)63, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)64, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)65, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)66, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)67, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)68, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)69, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)70, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)71, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)72, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)73, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)74, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)75, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)76, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)77, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)78, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)79, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)80, default },
                { (Opcode)81, default },
                { Opcode.EnumToInt, (EnumCast.Read, EnumCast.Write) },
                { Opcode.IntToEnum, (EnumCast.Read, EnumCast.Write) },
                { Opcode.DynamicCast, (DynamicCast.Read, DynamicCast.Write) },
                { Opcode.StructToString, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)86, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)87, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)88, default },
                { (Opcode)89, default },
                { (Opcode)90, default },
                { (Opcode)91, default },
                { (Opcode)92, default },
                { (Opcode)93, default },
                { (Opcode)94, default },
                { (Opcode)95, default },
                { (Opcode)96, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)97, (_DefinitionRef<NativeDefinition>.Read, _DefinitionRef<NativeDefinition>.Write) },
                { (Opcode)98, default },
            };
        }
    }
}
