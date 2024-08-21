using System.Globalization;
using System.Text;

namespace DanmakuEngine.Extensions;

public static class StringExtensions
{
    public static string Multiply(this string str, int count)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < count; i++)
            sb.Append(str);

        return sb.ToString();
    }

    public static string Multiply(this char c, int count)
        => new string(c, count);

    /// <summary>
    /// Get the display length of a string
    /// This may not be accurate for some characters
    /// </summary>
    /// <param name="str">The string</param>
    /// <returns>Display Length</returns>
    public static int GetDisplayLength(this string str)
    {
        int asciiCount = str.Count(c => c < 128);
        return (str.Length * 2) - asciiCount;

        // int length = str.Length;

        // foreach (char c in str)
        // {
        //     if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter)
        //         length++;
        // }

        // return length;
    }
}
