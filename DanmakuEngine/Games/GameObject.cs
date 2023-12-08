namespace DanmakuEngine.Games;

public class GameObject : IDisposable
{
    protected LoadState LoadState { get; private set; } = LoadState.NotLoaded;

    /// <summary>
    /// Loads the drawable and its children
    /// 
    /// Most of the time, you should not override this method or call this method directly
    /// </summary>
    protected virtual void load()
    {
        if (LoadState != LoadState.NotLoaded)
            return;

        LoadState = LoadState.Ready;

        Load();

        OnLoad?.Invoke(this);

        OnLoad = null!;
    }

    /// <summary>
    /// Starts the drawable and its children
    /// 
    /// Most of the time, you should not override this method or call this method directly
    /// </summary>
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

    protected virtual void update()
    {
        Update();

        OnUpdate?.Invoke(this);
    }

    /// <summary>
    /// Loads the drawable and its children
    /// 
    /// Most of the time, you should not override this method or call this method directly
    /// </summary>
    protected virtual void Load()
    {
    }

    /// <summary>
    /// Starts the drawable and its children
    /// 
    /// Most of the time, you should not override this method or call this method directly
    /// </summary>
    protected virtual void Start()
    {
    }

    /// <summary>
    /// Updates the drawable and its children
    /// 
    /// Most of the time, you should not override this method or call this method directly
    /// </summary>
    protected virtual void Update()
    {
    }

    /// <summary>
    /// Updates all children game objects or it self if it is not a composite game object
    /// </summary>
    /// <returns>whether we should stop updating sub tree</returns>
    public virtual bool updateSubTree()
    {
        if (isDisposed)
            return true;

        if (LoadState == LoadState.NotLoaded)
            load();

        if (LoadState == LoadState.Ready)
            start();

        if (!UpdateCheck)
            return true;

        BeforeUpdate();

        update();

        return false;
    }

    protected virtual bool UpdateCheck => true;

    protected virtual void BeforeUpdate()
    {
    }

    public event Action<GameObject> OnUpdate = null!;

    public event Action<GameObject> OnLoad = null!;

    public event Action<GameObject> OnStart = null!;

    #region IDisposable

    protected bool isDisposed = false;

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing || isDisposed)
            return;

        this.OnLoad = null!;
        this.OnStart = null!;
        this.OnUpdate = null!;

        isDisposed = true;
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