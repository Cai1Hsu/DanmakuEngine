namespace DanmakuEngine.Transformation;

public interface ITransformable
{
    void Reset();

    void Update(double deltaTime);

    bool IsDone => false;

    void Dispose();

    /// <summary>
    /// When we finish a transform, we can not stop at the exact time when the currentTime is equal to the duration.
    /// So we expose this property to allow us to begin the next transform at the exact time.
    /// 
    /// we set it to 0 by default because some transformers may not have extra time
    /// for example, you can never get the extra time of the <see cref="UntilDelayer"/> class in <see cref="TransformSequence"/>
    /// </summary>
    double CurrentExtraTime => 0;

    /// <summary>
    /// The duration of the transformer or the top transformer in the sequence
    /// 
    /// we set it to double.MaxValue by default because some transformers may not have duration
    /// for example, you can never get the duration of the <see cref="UntilDelayer"/> class in <see cref="TransformSequence"/>
    /// </summary>
    double TotalDuration => double.MaxValue;

    bool IsCurrentDone => IsDone;

    void FinishInstantly() => Update(TotalDuration);
}
