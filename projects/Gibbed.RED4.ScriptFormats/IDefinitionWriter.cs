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

using System.Collections.Generic;

namespace Gibbed.RED4.ScriptFormats
{
    internal interface IDefinitionWriter
    {
        long Position { get; set; }

        void WriteValueB8(bool value);
        void WriteValueS8(sbyte value);
        void WriteValueS16(short value);
        void WriteValueS32(int value);
        void WriteValueS64(long value);
        void WriteValueU8(byte value);
        void WriteValueU16(ushort value);
        void WriteValueU32(uint value);
        void WriteValueU64(ulong value);
        void WriteValueF32(float value);
        void WriteValueF64(double value);
        ushort WriteStringU16(string value);
        uint WriteStringU32(string value);
        void WriteBytes(byte[] value);
        void WriteReference(Definition value);
        void WriteReferences(IList<Definition> list);
        void WriteReferences<T>(IList<T> list)
            where T : Definition;
        void WriteName(string value);
        void WriteTweakDBId(string value);
        void WriteResource(string value);
    }
}
