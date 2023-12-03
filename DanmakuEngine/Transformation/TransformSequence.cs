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

            while (_index < transformers.Count
                // Some transformers may finish immediately
                // so we need to check if it's done
                // and if it's done, we need to reset it
                // and skip it
                && transformers[_index].IsDone)
            {
                transformers[_index].Reset();
                _index++;
            }

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

    public TransformSequence BeginLater()
    {
        _begun = false;

        return this;
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
        => this;

    public TransformSequence Delay(double duration)
        => Add(new Delayer(duration));

    public TransformSequence DelayUntil(Func<bool> condition)
        => Add(new UntilDelayer(condition));

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

    private class UntilDelayer : ITransformable
    {
        private readonly Func<bool> _condition;

        public bool IsDone => _condition();

        public UntilDelayer(Func<bool> condition)
        {
            _condition = condition;
        }

        public void Update(double _)
        {
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    private class Delayer : ITransformable
    {
        private readonly double _duration;

        private double _currentTime = 0;

        public bool IsDone => _currentTime >= _duration;

        public Delayer(double duration)
        {
            _duration = duration;
        }

        public void Update(double deltaTime)
        {
            _currentTime += deltaTime;
        }

        public void Reset()
        {
            _currentTime = 0;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}