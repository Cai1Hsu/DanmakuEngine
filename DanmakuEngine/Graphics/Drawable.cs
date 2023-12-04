using DanmakuEngine.Dependency;
using DanmakuEngine.Timing;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public class Drawable : IDisposable
{
    private float alpha = 1;
    public float Alpha
    {
        get
        {
            return alpha;
        }
        set
        {
            if (alpha == value)
                return;

            if (alpha > 1)
                alpha = 1;
            else if (alpha < 0)
                alpha = 0;

            alpha = value;
        }
    }
    const float aplhaMin = 10E-6f * 2;

    private bool alwaysPresent = true;

    protected virtual bool AlwaysPresent => alwaysPresent;

    public virtual bool IsPresent => AlwaysPresent || Alpha > aplhaMin;

    public virtual CompositeDrawable Parent { get; private set; } = null!;

    protected LoadState LoadState { get; private set; } = LoadState.NotLoaded;

    public Drawable(CompositeDrawable parent)
    {
        Parent = parent;
    }

    /// <summary>
    /// Updates the drawable and all of its children
    /// </summary>
    /// <returns>false if continues to update, and true for stops</returns>
    public virtual bool UpdateSubTree()
    {
        if (IsDisposed)
            return true;

        if (LoadState == LoadState.NotLoaded)
            return true;

        if (LoadState == LoadState.Ready)
            start();

        // Since here the LoadState is Complete

        // TODO: Update auto transforms
        // Transforms contains the transforms that are applied to the drawable
        // and animations and movement
        // may be we can implement transform using scheduler

        if (!IsPresent)
            return false;

        // scheuler update

        Update();

        OnUpdate?.Invoke(this);

        return false;
    }

    protected virtual void start()
    {
        if (LoadState != LoadState.Ready)
            return;

        LoadState = LoadState.Complete;

        Start();

        OnStart?.Invoke(this);
        // Since we can only *start* once
        OnStart = null!;
    }

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void load()
    {
        if (LoadState != LoadState.NotLoaded)
            return;

        LoadState = LoadState.Ready;

        Load();

        OnLoad?.Invoke(this);
    }

    public virtual void Load()
    {

    }

    public event Action<Drawable> OnUpdate = null!;

    public event Action<Drawable> OnLoad = null!;

    public event Action<Drawable> OnStart = null!;

    #region IDisposable

    private bool IsDisposed = false;

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || IsDisposed)
            return;

        this.OnLoad = null!;
        this.OnStart = null!;
        this.OnUpdate = null!;

        IsDisposed = true;
    }

    #endregion
}

public enum LoadState
{
    /// <summary>
    /// The drawable is not loaded and has not been initialized
    /// </summary>
    NotLoaded,
    /// <summary>
    /// The drawable is loaded, but has not been initialized in the Update loop
    /// </summary>
    Ready,
    /// <summary>
    /// The drawable is loaded and has been initialized in the Update loop
    /// which means it is ready to be drawn(or has been drawn)
    /// </summary>
    Complete
}