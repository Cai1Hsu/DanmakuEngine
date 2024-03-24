using DanmakuEngine.Engine.Platform.Linux;

namespace DanmakuEngine.Engine.Platform.Environments.Xdg;

public class XdgSessionTypeEnv : IEnvironmentVariable<LinuxGraphicsPlatform>
{
#pragma warning disable IDE1006
    public const string env_var = "XDG_SESSION_TYPE";

    private const string x11 = "x11";
    private const string wayland = "wayland";
#pragma warning restore IDE1006
    public LinuxGraphicsPlatform? Value
    {
        get
        {
            // This is not reliable though, as the user can change the value of the env
            // But it seems that all code exists uses this value to determine the graphics platform
            var value = Environment.GetEnvironmentVariable(env_var);

            if (value is null)
                return LinuxGraphicsPlatform.TTY;

            if (value == x11)
                return LinuxGraphicsPlatform.X11;

            if (value == wayland)
                return LinuxGraphicsPlatform.Wayland;

            return LinuxGraphicsPlatform.Unknown;
        }
    }
}
