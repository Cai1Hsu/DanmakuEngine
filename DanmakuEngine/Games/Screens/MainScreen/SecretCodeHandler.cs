using DanmakuEngine.Logging;
using Silk.NET.Input;

namespace DanmakuEngine.Games.Screens.MainScreen;

public class SecretCodeHandler
{
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

    private DateTime lastKeyDown = DateTime.MinValue;

    public bool HandleKey(Key key)
    {
        if (DateTime.UtcNow - lastKeyDown > TimeSpan.FromSeconds(1))
            secretCodeIndex = 0;

        lastKeyDown = DateTime.UtcNow;

        if (key == secretCode[secretCodeIndex])
        {
            // Logger.Debug($"SecretCode: Handled key {key}, Index: {secretCodeIndex}");
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