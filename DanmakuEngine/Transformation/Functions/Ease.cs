namespace DanmakuEngine.Transformation.Functions;

public class EaseIn : ITransformFunction
{
    public double Transform(double time)
        => time < 0.5 ?
            2 * time * time :
            1 - (Math.Pow((-2 * time) + 2, 2) / 2);
}

public class EaseOut : ITransformFunction
{
    public double Transform(double time)
    {
        double t = 1 - time;

        return t < 0.5 ?
            2 * t * t :
            1 - (Math.Pow((-2 * t) + 2, 2) / 2);
    }
}

public class EaseInCubic : ITransformFunction
{
    public double Transform(double t)
    {
        return t < 0.5 ? 4 * Math.Pow(t, 3) : 1 - (Math.Pow((-2 * t) + 2, 3) / 2);
    }
}
