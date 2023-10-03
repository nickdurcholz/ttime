using System;
using System.IO;

namespace ttime;

public static class Extensions
{
    public static bool EqualsOIC(this string a, string b)
    {
        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }

    public static void WriteAndPad(this TextWriter writer, string value, int length)
    {
        writer.Write(value);
        for (int i = 0; i < length - value.Length; i++)
            writer.Write(' ');
    }
}