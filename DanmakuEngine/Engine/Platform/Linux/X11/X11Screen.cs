namespace DanmakuEngine.Engine.Platform.Linux.X11;

internal unsafe class X11Screen : IDisposable
{
    private readonly XRRScreenResources* _resources;

    public XRRScreenResources* Resources => _resources;

    public X11Screen(X11Display display, ulong window)
    {
        _resources = Xrandr.XRRGetScreenResources((IntPtr)display, window);

        if (_resources == null)
        {
            throw new X11Exception("Failed to get screen resources");
        }
    }

    public X11Screen(X11Display display)
    {
        var rootWindow = Xlib.XDefaultRootWindow((IntPtr)display);

        _resources = Xrandr.XRRGetScreenResources((IntPtr)display, rootWindow);

        if (_resources == null)
        {
            throw new X11Exception("Failed to get screen resources");
        }
    }

    public void Dispose()
    {
        if (_resources != null)
        {
            Xrandr.XRRFreeScreenResources(_resources);
        }
    }
}
