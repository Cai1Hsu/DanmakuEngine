using DanmakuEngine.Allocations;
using DanmakuEngine.Dependency;
using DanmakuEngine.Games;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Timing;
using Silk.NET.Maths;

namespace DanmakuEngine.Graphics;

public class Drawable : GameObject, IDisposable
{
    private float alpha = 1;
    public float Alpha
    {
        get => alpha;
        set
        {
            if (alpha == value)
                return;

            alpha = Math.Clamp(value, 0, 1);
        }
    }

    private const float alphaMin = 10E-6f * 2;

    private bool alwaysPresent = true;

    protected virtual bool AlwaysPresent => alwaysPresent;

    public virtual bool IsPresent => AlwaysPresent || Alpha > alphaMin;

    public virtual CompositeDrawable Parent { get; private set; } = null!;

    #region Clock

    private readonly LazyValue<Clock> lazyClock = new(() => new Clock(true));

    protected Clock Clock
    {
        get => lazyClock.Value;
        set => lazyClock.AssignValue(value, true);
    }

    #endregion

    #region Scheduler

    private LazyValue<Scheduler> lazyScheduler;

    /// <summary>
    /// A lazily-initialized scheduler used to schedule tasks to be invoked in future <see cref="Update"/>s calls.
    /// The tasks are invoked at the beginning of the <see cref="Update"/> method before anything else.
    /// </summary>
    protected internal Scheduler Scheduler => lazyScheduler.Value;

    #endregion

    protected override bool BeforeUpdate()
    {
        if (!IsPresent)
            return false;
        // TODO: Update auto transforms
        // Transforms contains the transforms that are applied to the drawable
        // and animations and movement
        // may be we can implement transform using scheduler

        // scheuler update
        lazyScheduler.RawValue?.updateSubTree();

        return true;
    }

    public Drawable(CompositeDrawable parent)
    {
        Parent = parent;

        lazyScheduler = new(() => Scheduler.Create(lazyClock));
    }
}
