namespace DanmakuEngine.Allocations.ValueAccessors;

public interface IAccessor<TValue>
{
    public TValue Value { get; set; }
}
