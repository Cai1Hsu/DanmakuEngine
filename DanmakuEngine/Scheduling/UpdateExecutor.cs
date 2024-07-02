
using DanmakuEngine.Timing;
using Silk.NET.Core;

namespace DanmakuEngine.Scheduling;

public class UpdateExecutor : ThrottledExecutor
{
    public UpdateExecutor(Action task)
        : base(task)
    {
    }

    protected override double excessFrameTime
        => Math.Min(base.excessFrameTime,
            Math.Max(0, Time.LastFixedUpdateTimeWithErrors + Time.FixedUpdateDeltaNonScaled - SourceTime));
}
