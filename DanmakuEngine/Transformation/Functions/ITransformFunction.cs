namespace DanmakuEngine.Transformation.Functions;

public interface ITransformFunction
{
    /// <summary>
    /// Transformation function
    /// </summary>
    /// <param name="time">time is a value between 0 and 1</param>
    /// <returns>the scaler, between -1 and 1</returns>
    double Transform(double time);
}
