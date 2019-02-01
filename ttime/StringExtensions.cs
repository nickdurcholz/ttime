using System;

namespace ttime
{
    public static class StringExtensions
    {
        public static bool EqualsIOC(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }
    }
}