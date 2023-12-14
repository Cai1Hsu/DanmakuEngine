using DanmakuEngine.Logging;

namespace DanmakuEngine.Transformation;

public class TransformSequence : ITransformable
{
    private readonly List<ITransformable> transformers = new();

    private int _index = 0;

    public bool IsDone => !_loopForever && _currentLoop >= _loopCount;

    private bool _begun = true;

    private int _loopCount = 1;

    private int _currentLoop = 0;

    private bool _loopForever = false;

    public double CurrentExtraTime => transformers[_index].CurrentExtraTime;

    public double TotalDuration => _totalDuration;

    public bool IsCurrentDone => transformers[_index].IsCurrentDone;

    private double _totalDuration = 0;

    public TransformSequence()
    {
    }

    public TransformSequence(ITransformable transformable)
    {
        Add(transformable);
    }

    public TransformSequence(params ITransformable[] transformables)
    {
        Add(transformables);
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
            var doAdjust = IsCurrentDone;

            var extraTime = doAdjust ? transformers[_index].CurrentExtraTime : 0;

            // TODO: we should implement a `getNextTransformation` method
            transformers[_index].Reset();
            _index++;

            CheckAndRunNextLoop();

            // The top transformer may be a infinite transform sequence
            // and its current transformer may not be done
            if (!doAdjust)
                return;

            while (HandleImmediateTransform()
                // see the comment below and CurrentExtraTime in `ITransformable`
                || HandleTransformationFinishesWithinExtraTime())
            {
                transformers[_index].Reset();
                _index++;

                CheckAndRunNextLoop();
            }

            // This is needed because we may not(or always not) finish the transform at the exact time
            // Generally, we will finish the transform a little bit later
            // this allow us to make our transform sequence more accurate
            // we dont need to care about whether the transform is a transformer or a transform sequence
            // as transfrom sequence fixes this problem recursively
            transformers[_index].Update(extraTime);

            void CheckAndRunNextLoop()
            {
                if (_index >= transformers.Count)
                {
                    if (!_loopForever && _currentLoop < _loopCount)
                        _currentLoop++;

                    _index = 0;
                }
            }

            bool HandleImmediateTransform()
            {
                if (_index < transformers.Count
                    // Some transformers may finish immediately
                    // so we need to check if it's done
                    // and if it's done, we need to reset it
                    // and skip it
                    && transformers[_index].IsDone)
                {
                    // just finish the transform
                    transformers[_index].FinishInstantly();

                    return true;
                }

                return false;
            }

            bool HandleTransformationFinishesWithinExtraTime()
            {
                if (_index >= transformers.Count)
                    return false;

                if (extraTime <= transformers[_index].TotalDuration)
                    return false;

                // just finish the transform
                transformers[_index].FinishInstantly();

                // calculate the left extra time
                extraTime -= transformers[_index].TotalDuration;

                return true;
            }
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

        foreach (var t in transformables)
            _totalDuration += t.TotalDuration;

        return this;
    }

    public TransformSequence Add(ITransformable transformable)
    {
        transformers.Add(transformable);

        _totalDuration += transformable.TotalDuration;

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

        public double CurrentExtraTime => IsDone ? _currentTime - _duration : throw new InvalidOperationException("The transform is not done yet");

        public double TotalDuration => _duration;

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
