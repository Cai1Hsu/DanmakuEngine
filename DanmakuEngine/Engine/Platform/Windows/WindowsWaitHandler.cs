// from osu.Framework

using System.Runtime.CompilerServices;
using DanmakuEngine.Engine.Sleeping;

namespace DanmakuEngine.Engine.Platform.Windows;

public class WindowsWaitHandler : IWaitHandler
{
    public void Wait(double milliseconds)
        => Wait(TimeSpan.FromMilliseconds(milliseconds));

    public void Wait(TimeSpan timeSpan)
    {
        // use SDLWaitHandler as fallback since it also provides high resolution, just higher stddev.
        if (!waitWaitableTimer(timeSpan))
            IWaitHandler.SDLWaitHandler.Wait(timeSpan);
    }

    private IntPtr waitableTimer;

    public bool IsHighResolution => waitableTimer != IntPtr.Zero;

    public void Register()
    {
        try
        {
            // Attempt to use CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, only available since Windows 10, version 1803.
            waitableTimer = Execution.CreateWaitableTimerEx(IntPtr.Zero, null,
                Execution.CreateWaitableTimerFlags.CREATE_WAITABLE_TIMER_MANUAL_RESET | Execution.CreateWaitableTimerFlags.CREATE_WAITABLE_TIMER_HIGH_RESOLUTION, Execution.TIMER_ALL_ACCESS);

            if (waitableTimer == IntPtr.Zero)
            {
                // Fall back to a more supported version. This is still far more accurate than Thread.Sleep.
                waitableTimer = Execution.CreateWaitableTimerEx(IntPtr.Zero, null, Execution.CreateWaitableTimerFlags.CREATE_WAITABLE_TIMER_MANUAL_RESET, Execution.TIMER_ALL_ACCESS);
            }
        }
        catch
        {
            // Any kind of unexpected exception should fall back to Thread.Sleep.
        }
    }

    public void Unregister()
    {
        if (waitableTimer != IntPtr.Zero)
        {
            Execution.CloseHandle(waitableTimer);
            waitableTimer = IntPtr.Zero;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool waitWaitableTimer(TimeSpan timeSpan)
    {
        if (waitableTimer == IntPtr.Zero) return false;

        // Not sure if we want to fall back to Thread.Sleep on failure here, needs further investigation.
        if (Execution.SetWaitableTimerEx(waitableTimer, Execution.CreateFileTime(timeSpan), 0, null, default, IntPtr.Zero, 0))
        {
            Execution.WaitForSingleObject(waitableTimer, Execution.INFINITE);
            return true;
        }

        return false;
    }
}
