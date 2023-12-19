using System.Reflection;
using Silk.NET.Core.Attributes;
using Silk.NET.SDL;

namespace DanmakuEngine.Extensions.Keys;

public static class KeyCodeExtensions
{
    public static string GetName(this KeyCode key)
        => key.ToString().Substring(1);

    public static string ToSdlName(this KeyCode key)
    {
        var fieldInfo = typeof(KeyCode).GetField(key.ToString());

        var attribute = (NativeNameAttribute)fieldInfo?.GetCustomAttribute(typeof(NativeNameAttribute))!;

        if (attribute is null)
            return key.GetName();

        return attribute.Name;
    }
}
