namespace DanmakuEngine.Transformation.Functions;

public class SquareRootIn : ITransformFunction
{
    /// <summary>
    /// SquareRoot transformation function
    /// Transform a value between 0 and 1 using a square root function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between 0 and 1</returns>
    public double Transform(double time)
        => Math.Sqrt(time);
}
