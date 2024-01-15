namespace DanmakuEngine.Games;

public class GameObject(LoadState loadState = LoadState.NotLoaded) : IDisposable
{
    protected List<GameObject>? PreUpdateChildren = null!;

    protected LoadState LoadState { get; private set; } = loadState;

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
    /// You can override this method to load your own resources
    /// But don't call this method directly
    /// </summary>
    protected virtual void Load()
    {
    }

    /// <summary>
    /// Starts the drawable and its children
    /// 
    /// You can override this method to load your own resources
    /// But don't call this method directly
    /// </summary>
    protected virtual void Start()
    {
    }

    /// <summary>
    /// Updates the drawable and its children
    /// 
    /// You can override this method to load your own resources
    /// But don't call this method directly
    /// </summary>
    protected virtual void Update()
    {
    }

    /// <summary>
    /// Updates all children game objects or it self if it is not a composite game object
    /// </summary>
    /// <returns>whether we should stop updating sub tree</returns>
    public virtual bool UpdateSubTree(bool fixedUpdate = false)
    {
        if (isDisposed)
            return true;

        if (LoadState == LoadState.NotLoaded)
            load();

        if (LoadState == LoadState.Ready)
            start();

        if (PreUpdateChildren is not null)
        {
            foreach (var c in PreUpdateChildren)
                c.UpdateSubTree(fixedUpdate);
        }

        if (!BeforeUpdate(fixedUpdate))
            return true;

        if (!fixedUpdate)
            update();
        else
            this.fixedUpdate();

        return false;
    }

    protected virtual bool BeforeUpdate(bool fixedUpdate = false) => true;

    protected virtual void fixedUpdate()
    {
        FixedUpdate();

        OnFixedUpdate?.Invoke(this);
    }

    protected virtual void FixedUpdate()
    {
    }

    public event Action<GameObject> OnFixedUpdate = null!;

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
