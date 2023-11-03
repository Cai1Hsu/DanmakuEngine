using System.Reflection;
using DanmakuEngine.Games;

namespace DanmakuEngine.Dependency;

public class DependencyContainer
{
    public static DependencyContainer Instance { get; private set; } = null!;
    private readonly object _lock = new();

    private readonly Dictionary<Type, object?> _cache = new();

    public void Cache<T>(T obj)
        => Cache(typeof(T), obj!);

    public void Cache(Type T, object obj)
    {
        if (!T.IsInstanceOfType(obj))
            throw new InvalidOperationException("Can not cache a instance of another type");

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

        obj.Inject(this);
    }
    
    public void CacheAndInject(Type T, object obj)
    {
        if (!typeof(IInjectable).IsAssignableFrom(T))
            throw new InvalidOperationException("Can not inject a non IInjectable object");

        Cache(T, obj);
        ((IInjectable)obj).Inject(this);
    }

    public T Get<T>() => (T)Get(typeof(T));

    public object Get(Type T)
    {
        lock (_lock)
        {
            if (!_cache.TryGetValue(T, out var value))
                return null!;

            return value!;
        }
    }

    public void AutoInject(IInjectable obj)
        => obj.AutoInject();
    
    public void Inject(IInjectable obj)
        => obj.Inject(this);

    public DependencyContainer(GameHost? h = null!)
    {
        if (Instance != null)
            throw new InvalidOperationException("Already created a dependency container");
            
        Instance = this;
        
        if (h != null)
            Cache(h);
    }
}