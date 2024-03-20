using System.Runtime.InteropServices;
using System.Text;

namespace DanmakuEngine.Extensions;

public static unsafe class ByteExtensions
{
    public static string BytesToString(this byte[] bytes)
        => Encoding.UTF8.GetString(bytes);

    public static string ToString(byte* bytes)
        => Marshal.PtrToStringUTF8((IntPtr)bytes) ?? string.Empty;

    public static char* ToCharPtr(this string str)
        => (char*)Marshal.StringToHGlobalAnsi(str);
}
