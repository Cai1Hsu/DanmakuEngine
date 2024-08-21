using System.Diagnostics;
using DanmakuEngine.Engine;

namespace DanmakuEngine.Dependency;

public class DependencyContainer
{
    public static DependencyContainer Instance { get; private set; } = null!;

    private static object _instanceLock = new();

    private readonly object _cacheLock = new();

    private readonly Dictionary<Type, object?> _cache = new();

    public void Cache<T>(T obj)
        => Cache(typeof(T), obj!);

    public void Cache(Type T, object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        Debug.Assert(T.IsInstanceOfType(obj));

        lock (_cacheLock)
        {
            var added = _cache.TryAdd(T, obj);

            if (!added)
                throw new Exception($"Unable to cache {T}");

            if (obj is ICacheHookable hookable)
                hookable.OnCache(this);
        }
    }

    public void CacheAndInject<T>(T obj)
        where T : IInjectable
    {
        Cache(obj);

        obj.AutoInject();
    }

    /// <summary>
    /// If you don't have a reason, use the generic method.
    /// </summary>
    public void CacheAndInject(Type T, object obj)
    {
        if (T is not IInjectable)
            throw new InvalidOperationException("Can not inject a non IInjectable object");

        Cache(T, obj);
        ((IInjectable)obj).AutoInject();
    }

    public static T Get<T>()
    {
        lock (_instanceLock)
        {
            return (T)Instance.Get(typeof(T));
        }
    }

    public object Get(Type T)
    {
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(T, out var obj))
            {
                if (obj == null)
                    throw new Exception($"Cached object of type {T} is null");

                return obj;
            }

            throw new Exception($"Target type not found in cache: {T}");
        }
    }

    public static void AutoInject(IInjectable obj)
        => obj.AutoInject();

    public static DependencyContainer Reset()
    {
        lock (_instanceLock)
        {
            Instance = new DependencyContainer();
        }

        return Instance;
    }

    public static void UpdateValue<T>(T obj, bool assertContain = true)
        where T : IWorkingUsage
    {
        if (obj is null)
        {
            Remove<T>(false);

            return;
        }

        lock (_instanceLock)
        {
            lock (Instance._cacheLock)
            {
                Instance._cache[typeof(T)] = obj;
            }
        }
    }

    public static void Remove<T>(bool assertContain = true)
    {
        lock (_instanceLock)
        {
            lock (Instance._cacheLock)
            {
                if (!Instance._cache.ContainsKey(typeof(T)))
                {
                    if (assertContain)
                        throw new InvalidOperationException($"There is no such dependency. Type: {typeof(T)}");

                    return;
                }

                Instance._cache.Remove(typeof(T));
            }
        }
    }

    static DependencyContainer()
    {
        Instance = new DependencyContainer();
    }
}
