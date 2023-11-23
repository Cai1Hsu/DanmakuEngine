namespace DanmakuEngine.Transfomation;

public class TransformSequence : ITransformable
{
    private readonly List<ITransformable> transformers = [];

    private int index = 0;

    public bool IsDone => index >= transformers.Count;

    private bool begun = false;

    public TransformSequence(params Transformer[] transformers)
    {
        this.transformers.AddRange(transformers);
    }

    public void Update(double deltaTime)
    {
        if (!begun)
            return;

        if (IsDone)
            return;

        transformers[index].Update(deltaTime);

        if (transformers[index].IsDone)
            index++;
    }

    public void Dispose()
    {
        foreach (var transformer in transformers)
            transformer.Dispose();
    }

    public void Begin()
    {
        index = 0;

        begun = true;    
    }

    public TransformSequence Add(Transformer transformer)
    {
        transformers.Add(transformer);

        return this;
    }

    public TransformSequence Delay(double duration)
    {
        transformers.Add(new Transformer(duration, null!));

        return this;
    }

    public TransformSequence Loop(int Count)
    {
        throw new NotImplementedException();

        return this;
    }

    public TransformSequence LoopForever()
    {
        throw new NotImplementedException();

        return this;
    }
}