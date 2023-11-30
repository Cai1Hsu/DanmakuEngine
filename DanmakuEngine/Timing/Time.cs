using DanmakuEngine.Configuration;
using DanmakuEngine.Engine;
using OpenTK.Graphics.OpenGL;

namespace DanmakuEngine.Timing;

public class Time
{
    /// <summary>
    /// Represents and controls the update frequency of the fps in debug console
    /// it's the delay seconds between updating the fps in debug console.
    /// You shouldn't set a very low value.
    /// </summary>
    protected double UpdateFrequency = 1;
    protected double fps_debug_time = 0;

    protected double count_time = 0;
    protected int count_frame = 0;

    public double ActualFPS { get; protected set; }

    public static double CurrentTime { get; protected set; }

    public static double RenderDelta { get; protected set; } = 1 / 60;
    public static double UpdateDelta { get; protected set; } = 1 / 60;

    public void ResetTime()
    {
        var refresh_rate = ((GameHost)this).ConfigManager.RefreshRate;

        UpdateDelta = 1 / refresh_rate;
        RenderDelta = 1 / refresh_rate;

        CurrentTime = 0;
    }
}