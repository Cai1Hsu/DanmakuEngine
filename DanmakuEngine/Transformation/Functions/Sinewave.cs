namespace DanmakuEngine.Transformation.Functions;

public class SineInQuad : ITransformFunction
{
    /// <summary>
    /// Sine-In transformation function
    /// Transform a value between 0 and 1 using a quater sine function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => Math.Sin(time * Math.PI * 0.5);
}

public class SineOutQuad : ITransformFunction
{
    /// <summary>
    /// Sine-Out transformation function
    /// Transform a value between 1 and 0 using a quater sine function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => 1 - Math.Sin(time * Math.PI * 0.5);
}

public class SineIn : ITransformFunction
{
    public double Transform(double time)
        => (Math.Sin((time - 0.5) * Math.PI) + 1) / 2;
}

public class SineOut : ITransformFunction
{
    public double Transform(double time)
        => (Math.Sin((time + 0.5) * Math.PI) + 1) / 2;
}
