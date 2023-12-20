using DanmakuEngine.Extensions.Keys;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using Silk.NET.SDL;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class SecretCodeHandler
{
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

    public Action OnSecretCodeEntered { get; set; } = null!;

    public bool HandleKey(KeyCode key)
    {
        if (Time.CurrentTime - lastKeyDown > 1000)
            secretCodeIndex = 0;

        if (key == secretCode[secretCodeIndex])
        {
            lastKeyDown = Time.CurrentTime;

            Logger.Debug($"SecretCode: Handled key: {key.GetName()}, Index: {secretCodeIndex}, LastKeyDown: {lastKeyDown:F2}");

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
