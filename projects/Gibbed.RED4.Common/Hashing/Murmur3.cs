/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
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
using System.Text;

namespace Gibbed.RED4.Common.Hashing
{
    public static class Murmur3
    {
        private const uint SeedBase = 0x5EEDBA5E;

        private const uint C1 = 0xCC9E2D51u;
        private const uint C2 = 0x1B873593u;

        public static uint Compute(string value)
        {
            return Compute(value, Encoding.UTF8);
        }

        public static uint Compute(string value, Encoding encoding)
        {
            return Compute(encoding.GetBytes(value));
        }

        public static uint Compute(byte[] bytes)
        {
            return Compute(bytes, 0, bytes.Length, SeedBase);
        }

        public static uint Compute(byte[] bytes, uint seed)
        {
            return Compute(bytes, 0, bytes.Length, seed);
        }

        public static uint Compute(byte[] buffer, int offset, int count)
        {
            return Compute(buffer, offset, count, SeedBase);
        }

        public static uint Compute(byte[] buffer, int offset, int count, uint seed = 0)
        {
            uint hash = seed;

            // https://github.com/aappleby/smhasher/blob/92cf3702fcfaadc84eb7bef59825a23e0cd84f56/src/MurmurHash3.cpp#L110
            int headCount = count / 4;
            for (int block = 0; block < headCount; block++, offset += 4)
            {
                var value = ((uint)buffer[offset + 0] << 0) |
                    ((uint)buffer[offset + 1] << 8) |
                    ((uint)buffer[offset + 2] << 16) |
                    ((uint)buffer[offset + 3] << 24);
                value *= C1;
                value = (value >> 17) | (value << 15);
                value *= C2;
                hash ^= value;
                hash = (hash >> 19) | (hash << 13);
                hash = (hash * 5) + 0xE6546B64;
            }

            // https://github.com/aappleby/smhasher/blob/92cf3702fcfaadc84eb7bef59825a23e0cd84f56/src/MurmurHash3.cpp#L126
            int tailCount = count % 4;
            if (tailCount > 0)
            {
                uint value = tailCount switch
                {
                    3 => ((uint)buffer[offset + 0] << 0) |
                        ((uint)buffer[offset + 1] << 8) |
                        ((uint)buffer[offset + 2] << 16),
                    2 => ((uint)buffer[offset + 0] << 0) |
                        ((uint)buffer[offset + 1] << 8),
                    1 => (uint)buffer[offset + 0] << 0,
                    _ => throw new InvalidOperationException(),
                };
                value *= C1;
                value = (value >> 17) | (value << 15);
                value *= C2;
                hash ^= value;
            }

            // https://github.com/aappleby/smhasher/blob/92cf3702fcfaadc84eb7bef59825a23e0cd84f56/src/MurmurHash3.cpp#L70
            hash ^= (uint)count;
            hash ^= hash >> 16;
            hash *= 0x85EBCA6B;
            hash ^= hash >> 13;
            hash *= 0xC2B2AE35;
            hash ^= hash >> 16;
            return hash;
        }
    }
}
