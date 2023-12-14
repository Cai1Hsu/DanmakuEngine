namespace DanmakuEngine.Transformation.Functions;

public class SquareIn : ITransformFunction
{
    /// <summary>
    /// Square-In transformation function
    /// Transform a value between 0 and 1 using a square function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => -(time * (time - 2));
}

public class SquareOut : ITransformFunction
{
    /// <summary>
    /// Square-In transformation function
    /// Transform a value between 1 and 0 using a square function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => (1 - time) * (1 - time);
}
