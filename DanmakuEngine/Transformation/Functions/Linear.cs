namespace DanmakuEngine.Transformation.Functions;

public class LinearIn : ITransformFunction
{
    /// <summary>
    /// Linear-In transformation function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => time;
}

public class LinearOut : ITransformFunction
{
    /// <summary>
    /// Linear-Out transformation function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => 1 - time;
}
