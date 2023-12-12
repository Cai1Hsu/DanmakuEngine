namespace DanmakuEngine.Allocations;

/// <summary>
/// A pool that can be used to store <paramref name="perSecond"/> elements for every second.
/// </summary>
/// <typeparam name="T">The type of the element to store</typeparam>
/// <remarks>
/// This class is useful for limiting the number of times an object can be created per second.
/// For example, the player shoots bullet at a constant rate. You can use this class to store the bullets.
/// </remarks>
/// <example>
/// <code>
/// var pool = new FrequencyPool&lt;Bullet&gt;(player.ShootRate, player.BulletType.Velocity / (Screen.Height + player.BulletType.Height));
/// 
/// void Update()
/// {
///     if (!player.IsShooting)
///         return;
/// 
///     if (lastShoot + (pool.TimeBetween / 1000) &lt;= Time.CurrentTime)
///         pool.CurrentAndNext = new Bullet(player.Position, player.BulletType);
///         
///         lastShoot = Time.CurrentTime;
///     }
/// }
/// </code>
/// </example>
public class FrequencyPool<T> : RingPool<T>
{
    /// <summary>
    /// The maximum duration in milliseconds that the element will exist.
    /// 
    /// Mainly we use this to control the size of the pool.
    /// </summary>
    public readonly double MaxDuration;

    /// <summary>
    /// The time between each use of the pool in milliseconds.
    /// </summary>
    public readonly double TimeBetween;

    /// <summary>
    /// Creates a pool that can be used at most <paramref name="perSecond"/> times per second.
    /// </summary>
    /// <param name="perSecond">the number of times the pool can be used per second</param>
    /// <param name="maxDuration">max duration in milliseconds that the element will exist.</param>
    /// <exception cref="ArgumentException">if <paramref name="perSecond"/> is less than or equal to 0</exception>
    /// <exception cref="ArgumentException">if <paramref name="maxDuration"/> is less than or equal to 0</exception>
    public FrequencyPool(int perSecond, double maxDuration)
        : base(perSecond * (int)Math.Ceiling(maxDuration / 1000))
    {
        if (perSecond <= 0)
            throw new ArgumentException($"Per second must be greater than 0, found: {perSecond}", nameof(perSecond));

        if (maxDuration <= 0)
            throw new ArgumentException($"Max duration must be greater than 0, found {maxDuration}", nameof(maxDuration));

        this.TimeBetween = 1000 / perSecond;
        this.MaxDuration = maxDuration;
    }
}
