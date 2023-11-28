using DanmakuEngine.Logging;

namespace DanmakuEngine.Transfomation;

public class TransformSequence : ITransformable
{
    private readonly List<ITransformable> transformers = new();

    private int _index = 0;

    public bool IsDone => !_loopForever && _currentLoop >= _loopCount;

    private bool _begun = true;

    private int _loopCount = 1;

    private int _currentLoop = 0;

    private bool _loopForever = false;

    public TransformSequence()
    {
    }

    public TransformSequence(ITransformable transformable)
    {
        this.transformers.Add(transformable);
    }

    public TransformSequence(params ITransformable[] transformables)
    {
        this.transformers.AddRange(transformables);
    }

    public void Update(double deltaTime)
    {
        if (!_begun)
            return;

        if (IsDone)
            return;

        transformers[_index].Update(deltaTime);

        if (transformers[_index].IsDone)
        {
            transformers[_index].Reset();
            _index++;

            if (_index >= transformers.Count)
            {
                if (!_loopForever && _currentLoop < _loopCount)
                    _currentLoop++;

                _index = 0;
            }

            return;
        }
    }

    public void Dispose()
    {
        foreach (var transformer in transformers)
            transformer.Dispose();
    }

    public void Pause()
    {
        _begun = false;
    }

    public void Begin()
    {
        _index = 0;

        _begun = true;
    }

    public TransformSequence Add(params ITransformable[] transformables)
    {
        transformers.AddRange(transformables);

        return this;
    }

    public TransformSequence Add(ITransformable transformable)
    {
        transformers.Add(transformable);

        return this;
    }

    public TransformSequence Then()
        => Delay(0);

    public TransformSequence Delay(double duration)
    {
        transformers.Add(new Transformer(duration, null!, null!));

        return this;
    }

    public TransformSequence Loop(int Count)
    {
        _loopCount = Count;

        return this;
    }

    public TransformSequence LoopForever()
    {
        _loopForever = true;

        return this;
    }

    public void Reset()
    {
        _currentLoop = 0;

        foreach (var transformer in transformers)
            transformer.Reset();
    }
}