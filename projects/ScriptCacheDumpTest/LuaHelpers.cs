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

using System.Text;

namespace ScriptCacheDumpTest
{
    internal static class LuaHelpers
    {
        public static string Escape(string value)
        {
            var sb = new StringBuilder();
            foreach (var c in value)
            {
                if (c == '"' || c == '\\')
                {
                    sb.Append('\\');
                    sb.Append(c);
                }
                else if (c == '\r')
                {
                    sb.Append('\\');
                    sb.Append('r');
                }
                else if (c == '\n')
                {
                    sb.Append('\\');
                    sb.Append('n');
                }
                else if (c != '\t' && ((c >= 0 && c <= 31) || c == 127))
                {
                    sb.Append('\\');
                    sb.AppendFormat("{0:D3}", (int)c);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
