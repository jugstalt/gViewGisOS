using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.system
{
    public class WildcardEx : System.Text.RegularExpressions.Regex
    {
        public WildcardEx(string pattern)
            : base(WildcardToRegex(pattern))
        {
        }
        public WildcardEx(string pattern, System.Text.RegularExpressions.RegexOptions options)
            : base(WildcardToRegex(pattern), options)
        {
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + System.Text.RegularExpressions.Regex.Escape(pattern).
                        Replace("\\*", ".*").Replace("\\?", ".") + "$";
        }
    }
}
