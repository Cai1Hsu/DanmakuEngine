namespace DanmakuEngine.Transformation.Functions;

public class CubicIn : ITransformFunction
{
    /// <summary>
    /// Cubic-In transformation function
    /// Transform a value between 0 and 1 using a cubic function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => time * time * time;
}

public class CubicOut : ITransformFunction
{
    /// <summary>
    /// Cubic-Out transformation function
    /// Transform a value between 0 and 1 using a cubic function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
    {
        double f = time - 1;

        return (f * f * f) + 1;
    }
}
