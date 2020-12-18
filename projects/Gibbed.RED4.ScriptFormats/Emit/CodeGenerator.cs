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
using Gibbed.RED4.ScriptFormats.Definitions;
using Gibbed.RED4.ScriptFormats.Instructions;

namespace Gibbed.RED4.ScriptFormats.Emit
{
    public class CodeGenerator
    {
        private readonly List<(Opcode opcode, object argument)> _Instructions;
        private readonly Dictionary<Label, int> _Labels;
        private readonly int _BaseIndex;

        public CodeGenerator(int baseIndex)
        {
            this._Instructions = new List<(Opcode opcode, object argument)>();
            this._Labels = new Dictionary<Label, int>();
            this._BaseIndex = baseIndex;
        }

        public CodeGenerator()
            : this(0)
        {

        }

        public void Reset()
        {
            this._Instructions.Clear();
            this._Labels.Clear();
        }

        public Instruction[] GetCode()
        {
            var baseIndex = this._BaseIndex;
            foreach (var kv in this._Labels)
            {
                kv.Key.TargetIndex = kv.Value;
            }
            var instructions = new Instruction[this._Instructions.Count];
            for (int i = 0; i < this._Instructions.Count; i++)
            {
                var (opcode, argument) = this._Instructions[i];
                instructions[i] = GetCode(baseIndex, opcode, argument);
            }
            return instructions;
        }

        private static Instruction GetCode(int baseIndex, Opcode opcode, object argument)
        {
            switch (opcode)
            {
                case Opcode.Nop:
                case Opcode.Null:
                case Opcode.Int32One:
                case Opcode.Int32Zero:
                case Opcode.Int8Const:
                case Opcode.Int16Const:
                case Opcode.Int32Const:
                case Opcode.Int64Const:
                case Opcode.Uint8Const:
                case Opcode.Uint16Const:
                case Opcode.Uint32Const:
                case Opcode.Uint64Const:
                case Opcode.FloatConst:
                case Opcode.DoubleConst:
                case Opcode.NameConst:
                case Opcode.StringConst:
                case Opcode.TweakDBIdConst:
                case Opcode.ResourceConst:
                case Opcode.BoolTrue:
                case Opcode.BoolFalse:
                case Opcode.Assign:
                case Opcode.Target:
                case Opcode.LocalVar:
                case Opcode.ParamVar:
                case Opcode.ObjectVar:
                case Opcode.Unknown27:
                case Opcode.SwitchDefault:
                case Opcode.ParamEnd:
                case Opcode.Return:
                case Opcode.StructMember:
                case Opcode.TestEqual:
                case Opcode.TestNotEqual:
                case Opcode.New:
                case Opcode.Delete:
                case Opcode.This:
                case Opcode.ArrayClear:
                case Opcode.ArraySize:
                case Opcode.ArrayResize:
                case Opcode.ArrayFindFirst:
                case Opcode.ArrayFindFirstFast:
                case Opcode.ArrayFindLast:
                case Opcode.ArrayFindLastFast:
                case Opcode.ArrayContains:
                case Opcode.ArrayContainsFast:
                case Opcode.ArrayUnknown57:
                case Opcode.ArrayUnknown58:
                case Opcode.ArrayPushBack:
                case Opcode.ArrayPopBack:
                case Opcode.ArrayInsert:
                case Opcode.ArrayRemove:
                case Opcode.ArrayRemoveFast:
                case Opcode.ArrayGrow:
                case Opcode.ArrayErase:
                case Opcode.ArrayEraseFast:
                case Opcode.ArrayLast:
                case Opcode.ArrayElement:
                case Opcode.StaticArraySize:
                case Opcode.StaticArrayFindFirst:
                case Opcode.StaticArrayFindFirstFast:
                case Opcode.StaticArrayFindLast:
                case Opcode.StaticArrayFindLastFast:
                case Opcode.StaticArrayContains:
                case Opcode.StaticArrayContainsFast:
                case Opcode.StaticArrayUnknown76:
                case Opcode.StaticArrayUnknown77:
                case Opcode.StaticArrayLast:
                case Opcode.StaticArrayElement:
                case Opcode.HandleToBool:
                case Opcode.WeakHandleToBool:
                case Opcode.ToString:
                case Opcode.ToVariant:
                case Opcode.FromVariant:
                case Opcode.VariantIsValid:
                case Opcode.VariantIsHandle:
                case Opcode.VariantIsArray:
                case Opcode.Unknown91:
                case Opcode.VariantToString:
                case Opcode.WeakHandleToHandle:
                case Opcode.HandleToWeakHandle:
                case Opcode.WeakHandleNull:
                case Opcode.ToScriptRef:
                case Opcode.FromScriptRef:
                case Opcode.Unknown98:
                {
                    return new Instruction(opcode, argument);
                }
                case Opcode.EnumConst:
                {
                    var (enumeration, enumeral) = ((EnumerationDefinition, EnumeralDefinition))argument;
                    return new Instruction(opcode, new EnumConst(enumeration, enumeral));
                }
                case Opcode.Unknown21:
                {
                    throw new NotImplementedException();
                }
                case Opcode.Switch:
                {
                    var (native, firstCaseLabel) = ((NativeDefinition, Label))argument;
                    if (firstCaseLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new Instruction(opcode, new Switch(native, baseIndex + firstCaseLabel.TargetIndex));
                }
                case Opcode.SwitchLabel:
                {
                    var (falseLabel, trueLabel) = ((Label, Label))argument;
                    if (falseLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    if (trueLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new Instruction(opcode, new SwitchLabel(baseIndex + falseLabel.TargetIndex, baseIndex + trueLabel.TargetIndex));
                }
                case Opcode.Jump:
                case Opcode.JumpIfFalse:
                case Opcode.Skip:
                case Opcode.Context:
                {
                    var targetLabel = (Label)argument;
                    if (targetLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new Instruction(opcode, baseIndex + targetLabel.TargetIndex);
                }
                case Opcode.Conditional:
                {
                    var (falseLabel, trueLabel) = ((Label, Label))argument;
                    if (falseLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    if (trueLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new Instruction(opcode, new Conditional(baseIndex + falseLabel.TargetIndex, baseIndex + trueLabel.TargetIndex));
                }
                case Opcode.Constructor:
                {
                    var (parameterCount, type) = ((byte, ClassDefinition))argument;
                    return new Instruction(opcode, new Constructor(parameterCount, type));
                }
                case Opcode.FinalFunc:
                {
                    var (targetLabel, sourceLine, function) = ((Label, ushort, FunctionDefinition))argument;
                    if (targetLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new Instruction(opcode, new FinalFunc(baseIndex + targetLabel.TargetIndex, sourceLine, function));
                }
                case Opcode.VirtualFunc:
                {
                    var (targetLabel, sourceLine, name) = ((Label, ushort, string))argument;
                    if (targetLabel.TargetIndex < 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return new Instruction(opcode, new VirtualFunc(baseIndex + targetLabel.TargetIndex, sourceLine, name));
                }
                case Opcode.Unknown47:
                {
                    var (bytes, unknown) = ((byte[], byte))argument;
                    return new Instruction(opcode, new Unknown47(bytes, unknown));
                }
                case Opcode.EnumToInt32:
                case Opcode.Int32ToEnum:
                {
                    var (type, size) = ((NativeDefinition, byte))argument;
                    return new Instruction(opcode, new EnumCast(type, size));
                }
                case Opcode.DynamicCast:
                {
                    var (type, unknown) = ((ClassDefinition, byte))argument;
                    return new Instruction(opcode, new DynamicCast(type, unknown));
                }
            }
            throw new NotSupportedException();
        }

        public Label DefineLabel()
        {
            var label = new Label(this);
            this._Labels.Add(label, -1);
            return label;
        }

        public void MarkLabel(Label label)
        {
            if (this._Labels.TryGetValue(label, out var index) == false)
            {
                throw new ArgumentException("unknown label", nameof(label));
            }
            if (index >= 0)
            {
                throw new ArgumentException("label already marked", nameof(label));
            }
            this._Labels[label] = this._Instructions.Count;
        }

        public void Emit(Opcode opcode)
        {
            switch (opcode)
            {
                case Opcode.Nop:
                case Opcode.Null:
                case Opcode.Int32One:
                case Opcode.Int32Zero:
                case Opcode.BoolTrue:
                case Opcode.BoolFalse:
                case Opcode.Assign:
                case Opcode.Target:
                case Opcode.Unknown27:
                case Opcode.SwitchDefault:
                case Opcode.ParamEnd:
                case Opcode.Return:
                case Opcode.Delete:
                case Opcode.This:
                case Opcode.HandleToBool:
                case Opcode.WeakHandleToBool:
                case Opcode.VariantIsValid:
                case Opcode.VariantIsHandle:
                case Opcode.VariantIsArray:
                case Opcode.Unknown91:
                case Opcode.VariantToString:
                case Opcode.WeakHandleToHandle:
                case Opcode.HandleToWeakHandle:
                case Opcode.WeakHandleNull:
                case Opcode.Unknown98:
                {
                    this._Instructions.Add((opcode, null));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, sbyte argument)
        {
            switch (opcode)
            {
                case Opcode.Int8Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, short argument)
        {
            switch (opcode)
            {
                case Opcode.Int16Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, int argument)
        {
            switch (opcode)
            {
                case Opcode.Int32Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, long argument)
        {
            switch (opcode)
            {
                case Opcode.Int64Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, byte argument)
        {
            switch (opcode)
            {
                case Opcode.Uint8Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, ushort argument)
        {
            switch (opcode)
            {
                case Opcode.Uint16Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, uint argument)
        {
            switch (opcode)
            {
                case Opcode.Uint32Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, ulong argument)
        {
            switch (opcode)
            {
                case Opcode.Uint64Const:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, float argument)
        {
            switch (opcode)
            {
                case Opcode.FloatConst:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, double argument)
        {
            switch (opcode)
            {
                case Opcode.DoubleConst:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, string argument)
        {
            switch (opcode)
            {
                case Opcode.NameConst:
                case Opcode.StringConst:
                case Opcode.TweakDBIdConst:
                case Opcode.ResourceConst:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, EnumerationDefinition argument1, EnumeralDefinition argument2)
        {
            switch (opcode)
            {
                case Opcode.EnumConst:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void EmitUnknown21(Opcode opcode) // TODO(gibbed): fix
        {
            switch (opcode)
            {
                case Opcode.Unknown21:
                {
                    throw new NotImplementedException();
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, LocalDefinition argument)
        {
            switch (opcode)
            {
                case Opcode.LocalVar:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, ParameterDefinition argument)
        {
            switch (opcode)
            {
                case Opcode.ParamVar:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, PropertyDefinition argument)
        {
            switch (opcode)
            {
                case Opcode.ObjectVar:
                case Opcode.StructMember:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, NativeDefinition argument1, Label argument2)
        {
            switch (opcode)
            {
                case Opcode.Switch:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, Label argument1, Label argument2)
        {
            switch (opcode)
            {
                case Opcode.SwitchLabel:
                case Opcode.Conditional:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, Label argument)
        {
            switch (opcode)
            {
                case Opcode.Jump:
                case Opcode.JumpIfFalse:
                case Opcode.Skip:
                case Opcode.Context:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, byte argument1, ClassDefinition argument2)
        {
            switch (opcode)
            {
                case Opcode.Constructor:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, Label argument1, ushort argument2, FunctionDefinition argument3)
        {
            switch (opcode)
            {
                case Opcode.FinalFunc:
                {
                    this._Instructions.Add((opcode, (argument1, argument2, argument3)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, Label argument1, ushort argument2, string argument3)
        {
            switch (opcode)
            {
                case Opcode.VirtualFunc:
                {
                    this._Instructions.Add((opcode, (argument1, argument2, argument3)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, Definition argument)
        {
            switch (opcode)
            {
                case Opcode.TestEqual:
                case Opcode.TestNotEqual:
                case Opcode.New:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, byte[] argument1, byte argument2)
        {
            switch (opcode)
            {
                case Opcode.Unknown47:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, NativeDefinition argument)
        {
            switch (opcode)
            {
                case Opcode.ArrayClear:
                case Opcode.ArraySize:
                case Opcode.ArrayResize:
                case Opcode.ArrayFindFirst:
                case Opcode.ArrayFindFirstFast:
                case Opcode.ArrayFindLast:
                case Opcode.ArrayFindLastFast:
                case Opcode.ArrayContains:
                case Opcode.ArrayContainsFast:
                case Opcode.ArrayUnknown57:
                case Opcode.ArrayUnknown58:
                case Opcode.ArrayPushBack:
                case Opcode.ArrayPopBack:
                case Opcode.ArrayInsert:
                case Opcode.ArrayRemove:
                case Opcode.ArrayRemoveFast:
                case Opcode.ArrayGrow:
                case Opcode.ArrayErase:
                case Opcode.ArrayEraseFast:
                case Opcode.ArrayLast:
                case Opcode.ArrayElement:
                case Opcode.StaticArraySize:
                case Opcode.StaticArrayFindFirst:
                case Opcode.StaticArrayFindFirstFast:
                case Opcode.StaticArrayFindLast:
                case Opcode.StaticArrayFindLastFast:
                case Opcode.StaticArrayContains:
                case Opcode.StaticArrayContainsFast:
                case Opcode.StaticArrayUnknown76:
                case Opcode.StaticArrayUnknown77:
                case Opcode.StaticArrayLast:
                case Opcode.StaticArrayElement:
                case Opcode.ToString:
                case Opcode.ToVariant:
                case Opcode.FromVariant:
                case Opcode.ToScriptRef:
                case Opcode.FromScriptRef:
                {
                    this._Instructions.Add((opcode, argument));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, NativeDefinition argument1, byte argument2)
        {
            switch (opcode)
            {
                case Opcode.EnumToInt32:
                case Opcode.Int32ToEnum:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }

        public void Emit(Opcode opcode, ClassDefinition argument1, byte argument2)
        {
            switch (opcode)
            {
                case Opcode.DynamicCast:
                {
                    this._Instructions.Add((opcode, (argument1, argument2)));
                    return;
                }
            }
            throw new ArgumentException($"{opcode} not supported with these arguments", nameof(opcode));
        }
    }
}
