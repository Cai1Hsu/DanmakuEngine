using DanmakuEngine.Engine;

namespace DanmakuEngine.Dependency;

public class DependencyContainer
{
    public static DependencyContainer Instance { get; private set; } = null!;

    private static object _instanceLock = new();

    private readonly object _lock = new();

    private readonly Dictionary<Type, object?> _cache = new();

    public void Cache<T>(T obj)
        => Cache(typeof(T), obj!);

    public void Cache(Type T, object obj)
    {
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        if (!T.IsInstanceOfType(obj))
            throw new InvalidOperationException($"Can not cache a instance of another type, expected {T} got {obj.GetType()}");

        lock (_lock)
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

    public void CacheAndInject(Type T, object obj)
    {
        if (!typeof(IInjectable).IsAssignableFrom(T))
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
        lock (_lock)
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

    static DependencyContainer()
    {
        Instance = new DependencyContainer();
    }
}