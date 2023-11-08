using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class SecretCodeHandler
{
    private readonly Clock Clock;

    public SecretCodeHandler(Clock clock)
    {
        this.Clock = clock;
    }

    private readonly KeyCode[] secretCode = new KeyCode[]
    {
        KeyCode.KUp,
        KeyCode.KUp,
        KeyCode.KDown,
        KeyCode.KDown,
        KeyCode.KLeft,
        KeyCode.KRight,
        KeyCode.KLeft,
        KeyCode.KRight,
        KeyCode.KB,
        KeyCode.KA,
        KeyCode.KB,
        KeyCode.KA
    };

    private int secretCodeIndex = 0;

    private double lastKeyDown = -1f;

    public bool HandleKey(Keysym key)
    {
        if (Clock.CurrentTime - lastKeyDown > 1000)
            secretCodeIndex = 0;

        if (key.Sym == (int)secretCode[secretCodeIndex])
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