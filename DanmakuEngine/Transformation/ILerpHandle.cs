namespace DanmakuEngine.Transformation;

public interface ILerpHandle<T>
{
    T Lerp(float t);
}
