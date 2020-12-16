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

namespace Gibbed.RED4.ScriptFormats.Definitions
{
    [Flags]
    public enum FunctionFlags : uint
    {
        None = 0,
        Unknown0 = 1u << 0,
        Unknown1 = 1u << 1,
        Unknown2 = 1u << 2,
        Unknown3 = 1u << 3,
        IsNative = 1u << 4,
        Unknown5 = 1u << 5,
        Unknown6 = 1u << 6,
        HasReturnValue = 1u << 7,
        Unknown8 = 1u << 8,
        HasParameters = 1 << 9,
        HasLocals = 1 << 10,
        HasCode = 1u << 11,
        Unknown12 = 1u << 12,
        Unknown13 = 1u << 13,
        IsConstant = 1u << 18,
        Unknown19 = 1u << 19,
        Unknown20 = 1u << 20,
        Unknown21 = 1u << 21,
    }
}
