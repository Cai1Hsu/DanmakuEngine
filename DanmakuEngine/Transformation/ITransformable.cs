namespace DanmakuEngine.Transfomation;

public interface ITransformable
{
    void Reset();

    void Update(double deltaTime);

    bool IsDone => false;

    void Dispose();
}