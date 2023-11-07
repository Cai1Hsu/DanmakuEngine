using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.Input;

namespace DanmakuEngine.Games.Screens;

public class SecretCodeHandler
{
    private Clock Clock;

    public SecretCodeHandler(Clock clock)
    {
        this.Clock = clock;
    }

    private readonly Key[] secretCode = new Key[]
    {
        Key.Up,
        Key.Up,
        Key.Down,
        Key.Down,
        Key.Left,
        Key.Right,
        Key.Left,
        Key.Right,
        Key.B,
        Key.A,
        Key.B,
        Key.A
    };

    private int secretCodeIndex = 0;

    private double lastKeyDown = -1f;

    public bool HandleKey(Key key)
    {
        if (Clock.CurrentTime - lastKeyDown > 1000)
            secretCodeIndex = 0;

        if (key == secretCode[secretCodeIndex])
        {
            lastKeyDown = Clock.CurrentTime;

            Logger.Debug($"SecretCode: Handled key {key}, Index: {secretCodeIndex}, LastKeyDown: {lastKeyDown:F2}");
            secretCodeIndex++;
        }
        else
            secretCodeIndex = 0;

        if (secretCodeIndex == secretCode.Length)
        {
            secretCodeIndex = 0;

            return true;
        }

        return false;
    }
}