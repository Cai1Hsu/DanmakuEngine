using DanmakuEngine.Engine;
using DanmakuEngine.Input.Events.Keyboard;
using Silk.NET.SDL;
using SDL_KeyCode = Silk.NET.SDL.KeyCode;

namespace DanmakuEngine.Input.Handlers;

public partial class KeyboardHandler : InputHandlerBase
{
    public override void Register(GameHost host)
    {
        Enabled.BindValueChanged(v =>
        {
            if (v.NewValue)
            {
                host.KeyDown += HandleKeyDown;
                host.KeyUp += HandleKeyUp;
            }
            else
            {
                host.KeyDown -= HandleKeyDown;
                host.KeyUp -= HandleKeyUp;
            }
        }, true);
    }

    private void enqueueInput(IKeyEvent input)
        => InputQueue.Enqueue(input);

    private Keys translateKey(Keysym sym)
    {
        var keycode = sym.Sym;

        if (keycode >= (int)SDL_KeyCode.KA && keycode <= (int)SDL_KeyCode.KZ)
            return Keys.A + (keycode - (int)SDL_KeyCode.KA);

        if (keycode >= (int)SDL_KeyCode.K0 && keycode <= (int)SDL_KeyCode.K9)
            return Keys.Number0 + (keycode - (int)SDL_KeyCode.K0);

        if (keycode >= (int)SDL_KeyCode.KKP1 && keycode <= (int)SDL_KeyCode.KKP9)
            return Keys.Keypad1 + (keycode - (int)SDL_KeyCode.KKP1);

        if (keycode >= (int)SDL_KeyCode.KF1 && keycode <= (int)SDL_KeyCode.KF12)
            return Keys.F1 + (keycode - (int)SDL_KeyCode.KF1);

        return keycode switch
        {
            (int)SDL_KeyCode.KKP0 => Keys.Keypad0,
            (int)SDL_KeyCode.KKPPeriod => Keys.KeypadDecimal,
            (int)SDL_KeyCode.KTab => Keys.Tab,
            (int)SDL_KeyCode.KLeft => Keys.Left,
            (int)SDL_KeyCode.KRight => Keys.Right,
            (int)SDL_KeyCode.KUp => Keys.Up,
            (int)SDL_KeyCode.KDown => Keys.Down,
            (int)SDL_KeyCode.KPageup => Keys.PageUp,
            (int)SDL_KeyCode.KPagedown => Keys.PageDown,
            (int)SDL_KeyCode.KHome => Keys.Home,
            (int)SDL_KeyCode.KEnd => Keys.End,
            (int)SDL_KeyCode.KInsert => Keys.Insert,
            (int)SDL_KeyCode.KDelete => Keys.Delete,
            (int)SDL_KeyCode.KKPBackspace => Keys.Backspace,
            (int)SDL_KeyCode.KBackspace => Keys.Backspace,
            (int)SDL_KeyCode.KSpace => Keys.Space,
            (int)SDL_KeyCode.KReturn2 => Keys.Enter,
            (int)SDL_KeyCode.KReturn => Keys.Enter,
            (int)SDL_KeyCode.KEscape => Keys.Escape,
            (int)SDL_KeyCode.KLctrl => Keys.ControlLeft,
            (int)SDL_KeyCode.KLshift => Keys.ShiftLeft,
            (int)SDL_KeyCode.KLalt => Keys.AltLeft,
            (int)SDL_KeyCode.KLgui => Keys.SuperLeft,
            (int)SDL_KeyCode.KRctrl => Keys.ControlRight,
            (int)SDL_KeyCode.KRshift => Keys.ShiftRight,
            (int)SDL_KeyCode.KRalt => Keys.AltRight,
            (int)SDL_KeyCode.KRgui => Keys.SuperRight,
            (int)SDL_KeyCode.KMenu => Keys.Menu,
            (int)SDL_KeyCode.KComma => Keys.Comma,
            (int)SDL_KeyCode.KMinus => Keys.Minus,
            (int)SDL_KeyCode.KPeriod => Keys.Period,
            (int)SDL_KeyCode.KSlash => Keys.Slash,
            (int)SDL_KeyCode.KBackslash => Keys.BackSlash,
            (int)SDL_KeyCode.KSemicolon => Keys.Semicolon,
            (int)SDL_KeyCode.KEquals => Keys.Equal,
            (int)SDL_KeyCode.KLeftbracket => Keys.LeftBracket,
            (int)SDL_KeyCode.KRightbracket => Keys.RightBracket,
            (int)SDL_KeyCode.KQuote => Keys.Apostrophe,
            (int)SDL_KeyCode.KBackquote => Keys.GraveAccent,
            (int)SDL_KeyCode.KCapslock => Keys.CapsLock,
            (int)SDL_KeyCode.KScrolllock => Keys.ScrollLock,
            (int)SDL_KeyCode.KNumlockclear => Keys.NumLock,
            (int)SDL_KeyCode.KPrintscreen => Keys.PrintScreen,
            (int)SDL_KeyCode.KPause => Keys.Pause,
            (int)SDL_KeyCode.KKPDecimal => Keys.KeypadDecimal,
            (int)SDL_KeyCode.KKPDivide => Keys.KeypadDivide,
            (int)SDL_KeyCode.KKPMultiply => Keys.KeypadMultiply,
            (int)SDL_KeyCode.KKPMinus => Keys.KeypadSubtract,
            (int)SDL_KeyCode.KKPPlus => Keys.KeypadAdd,
            (int)SDL_KeyCode.KKPEnter => Keys.KeypadEnter,
            (int)SDL_KeyCode.KKPEquals => Keys.KeypadEqual,
            _ => Keys.Unknown
        };
    }

#pragma warning disable IDE1006
    private const uint DEVICE_ID_PLACEHOLDER = 1;
#pragma warning restore IDE1006

    public void HandleKeyDown(KeyboardEvent e)
    {
        enqueueInput(new KeyDownEvent
        {
            DeviceId = DEVICE_ID_PLACEHOLDER,
            Timestamp = e.Timestamp,
            Button = translateKey(e.Keysym),
            IsRepeatInput = e.Repeat != 0
        });
    }

    public void HandleKeyUp(KeyboardEvent e)
    {
        enqueueInput(new KeyUpEvent
        {
            DeviceId = DEVICE_ID_PLACEHOLDER,
            Timestamp = e.Timestamp,
            Button = translateKey(e.Keysym),
            IsRepeatInput = e.Repeat != 0
        });
    }
}
