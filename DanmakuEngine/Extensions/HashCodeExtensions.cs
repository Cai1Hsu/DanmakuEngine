using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;

namespace DanmakuEngine.Extensions;

public static class HashCodeExtensions
{
    public static int GetHash(this object obj)
        => HashCode.Combine(obj);

    public static int CombineHashWith(this object obj, params object[] them)
    {
        HashCode hash = new HashCode();
        hash.Add(obj);

        foreach (var t in them)
            hash.Add(t);

        return hash.ToHashCode();
    }
}
