using System.Runtime.InteropServices;

namespace DanmakuEngine.Engine.Platform;

public class NativeFunction<T>
    where T : Delegate
{
    private readonly T _function;

    public NativeFunction(string entry, Library library)
    {
        var ptr = NativeLibrary.GetExport(library, entry);

        _function = Marshal.GetDelegateForFunctionPointer<T>(ptr);
    }

    public static implicit operator T(NativeFunction<T> function)
        => function._function;
}
