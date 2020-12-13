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
        NoOperation = 0,
        LoadConstantOne = 2,
        LoadConstantZero = 3,
        LoadInt8 = 4,
        LoadInt16 = 5,
        LoadInt32 = 6,
        LoadInt64 = 7,
        LoadUint8 = 8,
        LoadUint16 = 9,
        LoadUint32 = 10,
        LoadUint64 = 11,
        LoadFloat = 12,
        LoadDouble = 13,
        LoadName = 14,
        LoadEnumeral = 15,
        LoadString = 16,
        LoadTweakDBId = 17,
        LoadResource = 18,
        LoadConstantTrue = 19,
        LoadConstantFalse = 20,
        StoreRef = 22,
        RefLocal = 24,
        LoadParameter = 25,
        RefProperty = 26,
        Switch = 28,
        SwitchCase = 29,
        SwitchDefault = 30,
        Jump = 31,
        JumpFalse = 32,
        Construct = 35,
        Call = 36,
        NativeCall = 37,
        EndCall = 38,
        ReturnWithValue = 39,
        LoadProperty = 40,
        AsObject = 41,
        CompareEqual = 42,
        CompareNotEqual = 43,
    }
}
