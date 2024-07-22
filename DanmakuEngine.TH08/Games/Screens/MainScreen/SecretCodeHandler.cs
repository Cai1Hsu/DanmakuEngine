using DanmakuEngine.Extensions.Keys;
using DanmakuEngine.Input;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class SecretCodeHandler
{
    private readonly Keys[] secretCode = new Keys[]
    {
        Keys.Up,
        Keys.Up,
        Keys.Down,
        Keys.Down,
        Keys.Left,
        Keys.Right,
        Keys.Left,
        Keys.Right,
        Keys.B,
        Keys.A,
        Keys.B,
        Keys.A
    };

    private int secretCodeIndex = 0;

    private double lastKeyDown = -1f;

    public Action OnSecretCodeEntered { get; set; } = null!;

    public bool HandleKey(Keys key)
    {
        if (Time.ElapsedSeconds - lastKeyDown > 1000)
            secretCodeIndex = 0;

        if (key == secretCode[secretCodeIndex])
        {
            lastKeyDown = Time.ElapsedSeconds;

#if DEBUG
            // definitely we don't want to leak the secret code in release build

            Logger.Debug($"SecretCode: Handled key: {key}, Index: {secretCodeIndex}, LastKeyDown: {lastKeyDown:F2}");

#endif // DEBUG

            secretCodeIndex++;

            if (secretCodeIndex == secretCode.Length)
            {
                OnSecretCodeEntered?.Invoke();

                secretCodeIndex = 0;
            }

            return true;
        }
        else
            secretCodeIndex = 0;

        return false;
    }
}
