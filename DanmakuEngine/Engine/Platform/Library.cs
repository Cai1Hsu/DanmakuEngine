using System.Reflection;
using System.Runtime.InteropServices;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Engine.Platform;

public class Library : IDisposable
{
    private readonly IntPtr _handle;

    public Library(string name)
    {
        _handle = NativeLibrary.Load(name);
    }

    public Library(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        _handle = NativeLibrary.Load(libraryName, assembly, searchPath);
    }

    public Library(IntPtr handle)
    {
        // Check if it's a valid handle although this is not a guarantee
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Invalid handle", nameof(handle));

        _handle = handle;
    }

    public T GetFunction<T>(string entry)
        where T : Delegate
        => new NativeFunction<T>(entry, this);

    public T GetFunction<T>()
        where T : Delegate
    {
        var type_name = typeof(T).Name;

        const string postfix = "Delegate";
        if (type_name.EndsWith(postfix))
            type_name = type_name[..^postfix.Length];
        else
            throw new ArgumentException("Delegate type to work with this method must end with 'Delegate', please provide the entry name instead.");

        return GetFunction<T>(type_name);
    }

    public void Dispose()
    {
        if (_handle != IntPtr.Zero)
        {
            NativeLibrary.Free(_handle);
        }
    }

    public static implicit operator IntPtr(Library library)
        => library._handle;

    public static Library Load(string path)
    {
        if (Path.IsPathFullyQualified(path))
            return new Library(path);

        string paths =
            // cwd
            Environment.CurrentDirectory
            + ":"
            // The directory containing the executing assembly
            + new DirectoryInfo(AppContext.BaseDirectory).FullName
            + ":"
            + (string?)AppContext.GetData("NATIVE_DLL_SEARCH_DIRECTORIES") ?? string.Empty
            + ":"
            + Environment.GetEnvironmentVariable("LD_LIBRARY_PATH") ?? string.Empty;

        var directories = paths.Split(Path.PathSeparator)
                               .Distinct()
                               .Where(Directory.Exists);

        var files = directories.Select(d => Path.Combine(d, path))
                              .Where(File.Exists);

        foreach (var file in files)
        {
            if (NativeLibrary.TryLoad(file, out var handle))
                return new Library(handle);

            Logger.Warn($"Failed to load library {file}");
        }

        throw new DllNotFoundException($"Could not find library {path}");
    }
}
