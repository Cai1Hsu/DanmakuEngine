using DanmakuEngine.Bindables;
using DanmakuEngine.Extensions.Vector;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;
using DanmakuEngine.Movements;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Timing;
using DanmakuEngine.Utils;
using Silk.NET.Maths;

namespace DanmakuEngine.TH08.Games;

public class Player : CompositeDrawable
{
    public class BulletType
    {
        public char Display = '#';
        public float PerSecond = 30;
        public float Velocity = 440 * 4;
    }

    public class Bullet : Drawable
    {
        public BulletType Type;

        public readonly Bindable<Vector2D<float>> Position = new();

        private readonly LinearMovementV2F _move;

        public override bool IsPresent => !_move.IsDone;

        public Bullet(float x, float y, BulletType type, IClock clock)
            : base(null!)
        {
            this.Type = type;
            this.Position.Value = new(x, y);

            _move = new(new(0, -type.Velocity))
            {
                Condition = m => m.Value.Value.Y > 0,
            };

            _move.OnDone += _ => Dispose();

            _move.SetClock(clock)
                 .BindTo(Position);
        }

        protected override void Start()
        {
            _move.BeginMove();

            InternalChildren = [_move];
        }

        protected override void Update()
        {
            renderBullet();

            void renderBullet()
            {
                if (Position.Value.Y < 0)
                    return;

                Console.SetCursorPosition(
                    (int)(Position.Value.X * Console.BufferWidth / screen_width) - 1,
                    (int)(Position.Value.Y * Console.BufferHeight / screen_height));

                Console.Out.Write($"{Type.Display} {Type.Display}");
            }
        }
    }

    public class BulletSpewer(Player player)
        : CompositeDrawable(player)
    {
        public readonly Bindable<bool> Active = new(false);

        public readonly LinkedList<Bullet> Bullets = new();

        public readonly BulletType bulletType = new();

        public bool HasBullets => Bullets.Count > 0;

        private readonly Player _player = player;

        private double shootCooldown => 1 / bulletType.PerSecond;

        private double? lastShoot = null!;

        private RNG<Bullet> rng = new();

        protected override void Start()
        {
            Active.BindTo(_player.IsShooting);
        }

        protected override void Update()
        {
            // update the bullets
            var node = Bullets.First;

            while (node is not null)
            {
                var bullet = node.Value;

                bullet.UpdateSubTree();

                if (bullet is null || !bullet.IsPresent)
                    Bullets.Remove(node);

                node = node.Next;
            }

            if (!Active.Value)
                return;

            // spawn immediately on the first shoot
            if (lastShoot == null)
            {
                lastShoot = Time.ElapsedSeconds;
                spawnBullet();
                return;
            }

            double timeElapsed = Time.ElapsedSeconds - lastShoot.Value;

            // Avoid spawning too many particles if a long amount of time has passed.
            if (Math.Abs(timeElapsed) > shootCooldown)
            {
                lastShoot = Time.ElapsedSeconds;
                spawnBullet();
                return;
            }
        }

        private void spawnBullet()
        {
            float random_offset = 0;

            if (!_player.SlowMode.Value)
                random_offset = (rng.NextSingle() - 0.5f) * 50;

            var bullet = new Bullet(_player.Position.Value.X + random_offset,
                                    _player.Position.Value.Y, bulletType, Clock);

            Bullets.AddLast(bullet);
        }
    }

    // just for demo
    private const double screen_height = 1080;
    private const double screen_width = 1920;

    public new readonly IClock Clock;

    private const float velocity = 600;

    public float Velocity => SlowMode.Value ? velocity / 2 : velocity;

    private readonly LinearMovementV2F _move = new(new(0, 0));

    public readonly BulletSpewer bulletSpewer;

    public new Scheduler Scheduler => base.Scheduler;

    public Player(IClock clock) : base(null!)
    {
        // we need to hide the Clock declared in Drawable
        // this means our design is not good enough
        Clock = clock;
        Scheduler.ChangeClock(Clock);

        bulletSpewer = new(this);
    }

    protected override void Start()
    {
        _move.SetClock(Clock)
             .BindTo(Position)
             .BeginMove();

        Add(_move,
            bulletSpewer);

        UpPressed.BindValueChanged(OnMovementChanged);
        DownPressed.BindValueChanged(OnMovementChanged);
        LeftPressed.BindValueChanged(OnMovementChanged);
        RightPressed.BindValueChanged(OnMovementChanged);

        SlowMode.BindValueChanged(OnMovementChanged);

        void OnMovementChanged(ValueChangedEvent<bool> _)
        {
            _move.Speed = this.Direction * Velocity;
        }
    }

    protected override void Update()
    {
        // shouldn't render in the update method
        // but this is only a demo
        renderPlayer();

        void renderPlayer()
        {
            Console.SetCursorPosition(
                (int)(Position.Value.X * Console.BufferWidth / screen_width),
                (int)(Position.Value.Y * Console.BufferHeight / screen_height));

            Console.Out.Write('⬤');
        }
    }

    public Vector2D<float> Direction
    {
        get
        {
            var direction = Vector2D<float>.Zero;

            if (UpPressed.Value)
                direction.Y -= 1;

            if (DownPressed.Value)
                direction.Y += 1;

            if (LeftPressed.Value)
                direction.X -= 1;

            if (RightPressed.Value)
                direction.X += 1;

            return direction.ToNormalized();
        }
    }

    public Bindable<Vector2D<float>> Position = new(new((float)(screen_width / 2), 960));

    public readonly Bindable<bool> IsShooting = new(false);

    public readonly Bindable<bool> SlowMode = new(false);

    public readonly Bindable<bool> UpPressed = new(false);
    public readonly Bindable<bool> DownPressed = new(false);
    public readonly Bindable<bool> LeftPressed = new(false);
    public readonly Bindable<bool> RightPressed = new(false);
}
