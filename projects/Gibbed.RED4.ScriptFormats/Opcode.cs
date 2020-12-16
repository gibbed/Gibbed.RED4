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
        // 21
        Assign = 22,
        Target = 23,
        LocalVar = 24,
        ParamVar = 25,
        ObjectVar = 26,
        // 27
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
        // 44
        // 45
        This = 46,
        // 47
        // 48
        ArraySize = 49,
        // 50
        // 51
        // 52
        // 53
        // 54
        ArrayContains = 55,
        // 56
        // 57
        // 58
        // 59
        // 60
        ArrayInsert = 61,
        // 62
        // 63
        // 64
        // 65
        // 66
        // 67
        ArrayElement = 68,
        // 69
        // 70
        // 71
        // 72
        // 73
        // 74
        // 75
        // 76
        // 77
        // 78
        // 79
        // 80
        // 81
        EnumToInt = 82,
        IntToEnum = 83,
        DynamicCast = 84,
        StructToString = 85,
        // 86
        // 87
        // 88
        // 89
        // 90
        // 91
        // 92
        // 93
        // 94
        // 95
        // 96
        // 97
        // 98
    }
}
