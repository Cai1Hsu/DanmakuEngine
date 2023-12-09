using System.Text;

namespace DanmakuEngine.Extensions;

public unsafe static class ByteExtensions
{
    const int MaxStringLength = 1024;

    public static string BytesToString(this byte[] bytes)
        => Encoding.UTF8.GetString(bytes);

    public static string BytesToString(byte* bytes)
        => Encoding.UTF8.GetString(bytes, MaxStringLength);

}