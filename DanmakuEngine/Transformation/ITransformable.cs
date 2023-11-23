namespace DanmakuEngine.Transfomation;

public interface ITransformable
{
    void Update(double deltaTime);

    bool IsDone => false;

    void Dispose();
}