using DanmakuEngine.Bindables;
using Silk.NET.Maths;

namespace DanmakuEngine.Input;

/// <summary>
/// Represents an object that has sensitivity settings.
/// </summary>
public interface IHasSensitivity<T>
    where T : unmanaged, IFormattable, IComparable<T>, IEquatable<T>
{
    /// <summary>
    /// The horizontal sensitivity.
    /// </summary>
    public T HorizontalSensitivity
    {
        get => HorizontalSensitivityBindable.Value;
        set => HorizontalSensitivityBindable.Value = value;
    }

    /// <summary>
    /// The vertical sensitivity.
    /// </summary>
    public T VerticalSensitivity
    {
        get => VerticalSensitivityBindable.Value;
        set => VerticalSensitivityBindable.Value = value;
    }

    /// <summary>
    /// The sensitivity as a vector.
    /// </summary>
    public Vector2D<T> Sensitivity
    {
        get => new Vector2D<T>(HorizontalSensitivity, VerticalSensitivity);
        set
        {
            HorizontalSensitivity = value.X;
            VerticalSensitivity = value.Y;
        }
    }

    /// <summary>
    /// Real field that stores the horizontal sensitivity.
    /// </summary>
    public Bindable<T> HorizontalSensitivityBindable { get; }

    /// <summary>
    /// Real field that stores the vertical sensitivity.
    /// </summary>
    public Bindable<T> VerticalSensitivityBindable { get; }

    /// <summary>
    /// Sets the sensitivity for each axis.
    /// </summary>
    /// <param name="horizontal">the horizontal sensitivity to set</param>
    /// <param name="vertical">the vertical sensitivity to set</param>
    public void SetSensitivity(T horizontal, T vertical)
    {
        HorizontalSensitivity = horizontal;
        VerticalSensitivity = vertical;
    }

    /// <summary>
    /// Sets the sensitivity for both axes.
    /// </summary>
    /// <param name="sensitivity">the sensitivity to set</param>
    public void SetSensitive(T sensitivity)
        => SetSensitivity(sensitivity, sensitivity);

    /// <summary>
    /// Sets the horizontal sensitivity and keeps the vertical sensitivity the same.
    /// </summary>
    /// <param name="horizontal">the horizontal sensitivity to set</param>
    public void SetHorizontalSensitivity(T horizontal)
        => HorizontalSensitivity = horizontal;

    /// <summary>
    /// Sets the vertical sensitivity and keeps the horizontal sensitivity the same.
    /// </summary>
    /// <param name="vertical">the vertical sensitivity to set</param>
    public void SetVerticalSensitivity(T vertical)
        => VerticalSensitivity = vertical;

    /// <summary>
    /// Resets the sensitivity to the default value.
    /// </summary>
    public void ResetSensitivity()
    {
        HorizontalSensitivityBindable.SetDefault();
        VerticalSensitivityBindable.SetDefault();
    }
}
