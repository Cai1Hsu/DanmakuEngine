using System.Diagnostics;
using DanmakuEngine.Allocations;
using DanmakuEngine.Dependency;
using DanmakuEngine.Input;
using DanmakuEngine.Input.EventReceivers;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Timing;

namespace DanmakuEngine.Games;

public class GameObject(LoadState loadState = LoadState.NotLoaded) : IDisposable
{
    protected List<GameObject>? InternalChildren = null!;

    protected LoadState LoadState { get; private set; } = loadState;

    public virtual bool CanUpdate => true;

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

        if (this is IInjectable injectable)
            injectable.AutoInject();

        if (this is IReceiveEvent eventReceiver)
        {
            eventReceiver.InputManager = InputManager.GetInputManager();
            eventReceiver.InputManager.RegisterReceiver(eventReceiver);
        }

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
    public virtual bool UpdateSubTree()
    {
        if (isDisposed)
            return true;

        if (LoadState == LoadState.NotLoaded)
            load();

        if (LoadState == LoadState.Ready)
            start();

        if (!CanUpdate)
            return true;

        preUpdate();

        update();

        lateUpdate();

        return false;
    }

    protected void preUpdate()
    {
        PreUpdate();

        OnPreUpdate?.Invoke(this);
    }

    protected virtual void PreUpdate()
    {
    }

    public virtual void FixedUpdateSubtree()
    {
        if (isDisposed)
            return;

        if (LoadState < LoadState.Complete)
            return;

        fixedUpdate();

        if (InternalChildren is not null)
        {
            foreach (var c in InternalChildren)
                c.FixedUpdateSubtree();
        }
    }

    protected virtual void fixedUpdate()
    {
        FixedUpdate();
        OnFixedUpdate?.Invoke(this);
    }

    protected virtual void FixedUpdate()
    {
    }

    protected void lateUpdate()
    {
        LateUpdate();

        OnLateUpdate?.Invoke(this);
    }

    protected virtual void LateUpdate()
    {
        if (InternalChildren is not null)
        {
            foreach (var c in InternalChildren)
                c.UpdateSubTree();
        }
    }

    public event Action<GameObject>? OnPreUpdate = null;

    public event Action<GameObject>? OnLateUpdate = null;

    public event Action<GameObject>? OnFixedUpdate = null;

    public event Action<GameObject>? OnUpdate = null;

    public event Action<GameObject>? OnLoad = null;

    public Action<GameObject>? OnStart = null;

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
        this.OnFixedUpdate = null!;
        this.OnLateUpdate = null!;
        this.OnPreUpdate = null!;

        isDisposed = true;
    }

    #endregion
}
