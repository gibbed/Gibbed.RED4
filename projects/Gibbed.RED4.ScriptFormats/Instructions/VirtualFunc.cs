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

namespace Gibbed.RED4.ScriptFormats.Instructions
{
    public struct VirtualFunc
    {
        public int NextIndex;
        public ushort SourceLine;
        public string Function;

        public VirtualFunc(int nextIndex, ushort sourceLine, string function)
        {
            this.NextIndex = nextIndex;
            this.SourceLine = sourceLine;
            this.Function = function;
        }

        public void Deconstruct(out int nextIndex, out ushort sourceLine, out string function)
        {
            nextIndex = this.NextIndex;
            sourceLine = this.SourceLine;
            function = this.Function;
        }

        public override string ToString()
        {
            return $"({this.NextIndex}, {this.SourceLine}, {this.Function})";
        }
    }
}
