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
}