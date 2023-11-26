namespace DanmakuEngine.Transfomation;

public interface ITransformable
{
    void Reset();

    void Update(double deltaTime);

    bool IsDone => false;

    void Dispose();

    TransformSequence Add(params ITransformable[] transformables);

    TransformSequence Add(ITransformable transformable);

    TransformSequence Then();

    TransformSequence Delay(double duration);

    TransformSequence Loop(int Count);

    TransformSequence LoopForever();
}