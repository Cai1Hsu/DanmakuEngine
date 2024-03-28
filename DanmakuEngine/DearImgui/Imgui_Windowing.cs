using System.Numerics;
using DanmakuEngine.Arguments;
using ImGuiNET;
using Silk.NET.Maths;

namespace DanmakuEngine.DearImgui;

public static partial class Imgui
{
    /// <summary>
    /// Scale factor for the display framebuffer. Larger values will make the UI larger.
    /// </summary>
    private static Vector2D<float> _scale = Vector2D<float>.One;
    public static Vector2D<float> Scale
    {
        get => _scale;
        set
        {
            assertValid(value.X, nameof(Scale.X));
            assertValid(value.Y, nameof(Scale.Y));

            _scale = value;

            if (!_initialized)
                return;

            _io.DisplayFramebufferScale = new Vector2(1 / value.X, 1 / value.Y);
            OnWindowResized(_io.DisplaySize.X, _io.DisplaySize.Y);

            static void assertValid(float value, string name)
            {
                ArgumentOutOfRangeException
                    .ThrowIfNegativeOrZero(value, name);

                ArgumentOutOfRangeException
                    .ThrowIfEqual(value, float.PositiveInfinity, name);

                ArgumentOutOfRangeException
                    .ThrowIfEqual(value, float.NaN, name);
            }
        }
    }

    public static void OnWindowResized(float width, float height)
    {
        if (!_initialized)
            return;

        _io.DisplaySize = new Vector2(width * _scale.X, height * _scale.Y);
    }
}
