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
    internal interface IDefinitionReader
    {
        long Position { get; set; }

        bool ReadValueB8();
        sbyte ReadValueS8();
        short ReadValueS16();
        int ReadValueS32();
        long ReadValueS64();
        byte ReadValueU8();
        ushort ReadValueU16();
        uint ReadValueU32();
        ulong ReadValueU64();
        float ReadValueF32();
        double ReadValueF64();
        string ReadStringU16();
        string ReadStringU16(out ushort size);
        string ReadStringU32();
        string ReadStringU32(out uint size);
        byte[] ReadBytes();
        Definition ReadReference();
        T ReadReference<T>() where T : Definition;
        Definition[] ReadReferences();
        T[] ReadReferences<T>() where T : Definition;
        string ReadName();
        string ReadTweakDBId();
        string ReadResource();
    }
}
