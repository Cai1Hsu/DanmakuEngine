using System.Diagnostics.CodeAnalysis;
using System.IO;
using DanmakuEngine.Logging;

namespace DanmakuEngine.DearImgui.Fonts;

public unsafe class ImguiFontRawData : IDisposable
{
    private bool _disposed = false;
    private byte[] _rawdata = null!;

    public byte[] RawData => _rawdata;

    public void Dispose()
    {
        if (_disposed)
            return;

        _rawdata = null!;
    }

    private ImguiFontRawData(byte[] data)
    {
        _rawdata = data;
    }

    public static bool TryLoadRawData(string name, string path, [NotNullWhen(true)] out ImguiFontRawData data)
    {
        data = null!;

        if (path is null)
            return false;

        if (!File.Exists(path))
            return false;

        try
        {
            var binary = File.ReadAllBytes(path);
            data = new ImguiFontRawData(binary);

            Logger.Debug($"Loaded font {name} successfully");

            return true;
        }
        catch
        {
            Logger.Debug($"Failed to load font {name}");
            return false;
        }
    }
}
