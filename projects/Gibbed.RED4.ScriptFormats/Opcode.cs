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

namespace Gibbed.RED4.ScriptFormats
{
    public enum Opcode : byte
    {
        Nop = 0,
        Null = 1,
        Int32One = 2,
        Int32Zero = 3,
        Int8Const = 4,
        Int16Const = 5,
        Int32Const = 6,
        Int64Const = 7,
        Uint8Const = 8,
        Uint16Const = 9,
        Uint32Const = 10,
        Uint64Const = 11,
        FloatConst = 12,
        DoubleConst = 13,
        NameConst = 14,
        EnumConst = 15,
        StringConst = 16,
        TweakDBIdConst = 17,
        ResourceConst = 18,
        BoolTrue = 19,
        BoolFalse = 20,
        Unknown21 = 21,
        Assign = 22,
        Target = 23,
        LocalVar = 24,
        ParamVar = 25,
        ObjectVar = 26,
        Unknown27 = 27,
        Switch = 28,
        SwitchLabel = 29,
        SwitchDefault = 30,
        Jump = 31,
        JumpIfFalse = 32,
        Skip = 33,
        Conditional = 34,
        Constructor = 35,
        FinalFunc = 36,
        VirtualFunc = 37,
        ParamEnd = 38,
        Return = 39,
        StructMember = 40,
        Context = 41,
        TestEqual = 42,
        TestNotEqual = 43,
        New = 44,
        Delete = 45,
        This = 46,
        Unknown47 = 47,
        ArrayClear = 48,
        ArraySize = 49,
        ArrayResize = 50,
        ArrayFindFirst = 51,
        ArrayFindFirstFast = 52,
        ArrayFindLast = 53,
        ArrayFindLastFast = 54,
        ArrayContains = 55,
        ArrayContainsFast = 56,
        ArrayUnknown57 = 57,
        ArrayUnknown58 = 58,
        ArrayPushBack = 59,
        ArrayPopBack = 60,
        ArrayInsert = 61,
        ArrayRemove = 62,
        ArrayRemoveFast = 63,
        ArrayGrow = 64,
        ArrayErase = 65,
        ArrayEraseFast = 66,
        ArrayLast = 67,
        ArrayElement = 68,
        StaticArraySize = 69,
        StaticArrayFindFirst = 70,
        StaticArrayFindFirstFast = 71,
        StaticArrayFindLast = 72,
        StaticArrayFindLastFast = 73,
        StaticArrayContains = 74,
        StaticArrayContainsFast = 75,
        StaticArrayUnknown76 = 76, // same code as ArrayUnknown57
        StaticArrayUnknown77 = 77, // same code as ArrayUnknown58
        StaticArrayLast = 78,
        StaticArrayElement = 79,
        HandleToBool = 80,
        WeakHandleToBool = 81,
        EnumToInt32 = 82,
        Int32ToEnum = 83,
        DynamicCast = 84,
        ToString = 85,
        // custom naming...
        ToVariant = 86,
        FromVariant = 87,
        VariantIsValid = 88,
        VariantIsHandle = 89,
        VariantIsArray = 90,
        Unknown91 = 91, // variant related
        VariantToString = 92,
        WeakHandleToHandle = 93,
        HandleToWeakHandle = 94,
        WeakHandleNull = 95,
        ToScriptRef = 96,
        FromScriptRef = 97,
        Unknown98 = 98,
    }
}
