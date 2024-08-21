using System.Numerics;
using System.Runtime.InteropServices;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using ImGuiNET;
using SDL_KeyboardEvent = Silk.NET.SDL.KeyboardEvent;
using SDL_KeyCode = Silk.NET.SDL.KeyCode;
using SDL_Keymod = Silk.NET.SDL.Keymod;
using SDL_MouseButtonEvent = Silk.NET.SDL.MouseButtonEvent;
using SDL_MouseMotionEvent = Silk.NET.SDL.MouseMotionEvent;
using SDL_MouseWheelEvent = Silk.NET.SDL.MouseWheelEvent;
using SDL_TextEditingEvent = Silk.NET.SDL.TextEditingEvent;
using SDL_TextInputEvent = Silk.NET.SDL.TextInputEvent;

namespace DanmakuEngine.DearImgui;

public static partial class Imgui
{
#pragma warning disable
    private const uint SDL_MOUSEWHEEL_NORMAL = 0;
    private const uint SDL_MOUSEWHEEL_FLIPPED = 1;

    private const byte SDL_RELEASED = 0;
    private const byte SDL_PRESSED = 1;

    private const char NUL = '\0';
#pragma warning restore

    #region Mouse

    internal static bool EatMouseButtonEvents(SDL_MouseButtonEvent e)
    {
        // We've checked this in the main loop
        mouseButtonEvent(e);

        return _io.WantCaptureMouse;
    }

    private static void mouseButtonEvent(SDL_MouseButtonEvent e)
    {
        var buttondown = e.State == SDL_PRESSED;

        var button = e.Button switch
        {
            1 => 0, // Left
            2 => 2, // Middle
            3 => 1, // Right
            4 => 3, // X1
            5 => 4, // X2
            _ => -1
        };

        if (button == -1)
            return;

        _io.MouseDown[button] = buttondown;
    }

    // We shouldn't eat the mouse motion event
    internal static void MouseMotionEvent(SDL_MouseMotionEvent e)
    {
        // we need the mouse postion in the window
        _io.MousePos = new Vector2(e.X, e.Y);
    }

    internal static bool EatMouseWheelEvents(SDL_MouseWheelEvent e)
    {
        // We've checked this in the main loop
        mouseWheelEvent(e);

        return _io.WantCaptureMouse;
    }

    private static void mouseWheelEvent(SDL_MouseWheelEvent e)
    {
        var (offsetX, offsetY) = (e.X, e.Y);

        if (e.Direction == SDL_MOUSEWHEEL_FLIPPED)
        {
            offsetX = -offsetX;
            offsetY = -offsetY;
        }

        _io.MouseWheel = offsetY;
        _io.MouseWheelH = offsetX;
    }

    #endregion  // Mouse

    #region Keyboard

    internal static bool EatTextInputEvents(SDL_TextInputEvent e)
    {
        // We've checked this in the main loop
        textInputEvent(e);

        return _io.WantTextInput;
    }

    private static void textInputEvent(SDL_TextInputEvent e)
    {
        // pop up the keyboard
        _sdl.StartTextInput();

        unsafe
        {
            var str = ByteExtensions.ToString(e.Text);

            _io.AddInputCharactersUTF8(str);
        }
    }

    internal static bool EatKeyboardEvents(SDL_KeyboardEvent e)
    {
        // We've checked this in the main loop
        keyboardEvent(e);

        return _io.WantCaptureKeyboard;
    }

    private static void keyboardEvent(SDL_KeyboardEvent e)
    {
        var keydown = e.State == SDL_PRESSED;
        var key = translateKeycode(e.Keysym.Sym);
        // var mod = translateModKey(e.Keysym.Mod);

        if (key != ImGuiKey.None)
            _io.AddKeyEvent(key, keydown);

        // if (mod != ImGuiKey.None)
        //     _io.AddKeyEvent(mod, keydown);

        // handle modifiers
        _io.KeyCtrl = hasModifier(e.Keysym.Mod, SDL_Keymod.Ctrl) | hasModifier(e.Keysym.Mod, SDL_Keymod.Rctrl);
        _io.KeyShift = hasModifier(e.Keysym.Mod, SDL_Keymod.Shift) | hasModifier(e.Keysym.Mod, SDL_Keymod.Rshift);
        _io.KeyAlt = hasModifier(e.Keysym.Mod, SDL_Keymod.Alt) | hasModifier(e.Keysym.Mod, SDL_Keymod.Ralt);
        _io.KeySuper = hasModifier(e.Keysym.Mod, SDL_Keymod.Gui) | hasModifier(e.Keysym.Mod, SDL_Keymod.Rgui);

        static bool hasModifier(ushort mod, SDL_Keymod flag)
            => (mod & (int)flag) == (int)flag;
    }

    private static ImGuiKey translateModKey(ushort mod)
    {
        return mod switch
        {
            (ushort)SDL_Keymod.Ctrl => ImGuiKey.LeftCtrl,
            (ushort)SDL_Keymod.Shift => ImGuiKey.LeftShift,
            (ushort)SDL_Keymod.Alt => ImGuiKey.LeftAlt,
            (ushort)SDL_Keymod.Gui => ImGuiKey.ModSuper,
            (ushort)SDL_Keymod.Num => ImGuiKey.NumLock,
            (ushort)SDL_Keymod.Rctrl => ImGuiKey.RightCtrl,
            (ushort)SDL_Keymod.Rshift => ImGuiKey.RightShift,
            (ushort)SDL_Keymod.Ralt => ImGuiKey.RightAlt,
            (ushort)SDL_Keymod.Rgui => ImGuiKey.RightSuper,
            (ushort)SDL_Keymod.Scroll => ImGuiKey.ScrollLock,
            (ushort)SDL_Keymod.Caps => ImGuiKey.CapsLock,
            _ => ImGuiKey.None
        };
    }

    private static ImGuiKey translateKeycode(int keycode)
    {
        if (keycode >= (int)SDL_KeyCode.K0 && keycode <= (int)SDL_KeyCode.K9)
            return ImGuiKey._0 + (keycode - (int)SDL_KeyCode.K0);

        if (keycode >= (int)SDL_KeyCode.KA && keycode <= (int)SDL_KeyCode.KZ)
            return ImGuiKey.A + (keycode - (int)SDL_KeyCode.KA);

        if (keycode >= (int)SDL_KeyCode.KF1 && keycode <= (int)SDL_KeyCode.KF24)
            return ImGuiKey.F1 + (keycode - (int)SDL_KeyCode.KF1);

        if (keycode >= (int)SDL_KeyCode.KKP0 && keycode <= (int)SDL_KeyCode.KKP9)
            return ImGuiKey.Keypad0 + (keycode - (int)SDL_KeyCode.KKP0);

        return keycode switch
        {
            (int)SDL_KeyCode.KTab => ImGuiKey.Tab,
            (int)SDL_KeyCode.KLeft => ImGuiKey.LeftArrow,
            (int)SDL_KeyCode.KRight => ImGuiKey.RightArrow,
            (int)SDL_KeyCode.KUp => ImGuiKey.UpArrow,
            (int)SDL_KeyCode.KDown => ImGuiKey.DownArrow,
            (int)SDL_KeyCode.KPageup => ImGuiKey.PageUp,
            (int)SDL_KeyCode.KPagedown => ImGuiKey.PageDown,
            (int)SDL_KeyCode.KHome => ImGuiKey.Home,
            (int)SDL_KeyCode.KEnd => ImGuiKey.End,
            (int)SDL_KeyCode.KInsert => ImGuiKey.Insert,
            (int)SDL_KeyCode.KDelete => ImGuiKey.Delete,
            (int)SDL_KeyCode.KKPBackspace => ImGuiKey.Backspace,
            (int)SDL_KeyCode.KBackspace => ImGuiKey.Backspace,
            (int)SDL_KeyCode.KSpace => ImGuiKey.Space,
            (int)SDL_KeyCode.KReturn2 => ImGuiKey.Enter,
            (int)SDL_KeyCode.KReturn => ImGuiKey.Enter,
            (int)SDL_KeyCode.KEscape => ImGuiKey.Escape,
            (int)SDL_KeyCode.KLctrl => ImGuiKey.LeftCtrl,
            (int)SDL_KeyCode.KLshift => ImGuiKey.LeftShift,
            (int)SDL_KeyCode.KLalt => ImGuiKey.LeftAlt,
            (int)SDL_KeyCode.KLgui => ImGuiKey.LeftSuper,
            (int)SDL_KeyCode.KRctrl => ImGuiKey.RightCtrl,
            (int)SDL_KeyCode.KRshift => ImGuiKey.RightShift,
            (int)SDL_KeyCode.KRalt => ImGuiKey.RightAlt,
            (int)SDL_KeyCode.KRgui => ImGuiKey.RightSuper,
            (int)SDL_KeyCode.KMenu => ImGuiKey.Menu,
            (int)SDL_KeyCode.KComma => ImGuiKey.Comma,
            (int)SDL_KeyCode.KMinus => ImGuiKey.Minus,
            (int)SDL_KeyCode.KPeriod => ImGuiKey.Period,
            (int)SDL_KeyCode.KSlash => ImGuiKey.Slash,
            (int)SDL_KeyCode.KBackslash => ImGuiKey.Backslash,
            (int)SDL_KeyCode.KSemicolon => ImGuiKey.Semicolon,
            (int)SDL_KeyCode.KEquals => ImGuiKey.Equal,
            (int)SDL_KeyCode.KLeftbracket => ImGuiKey.LeftBracket,
            (int)SDL_KeyCode.KRightbracket => ImGuiKey.RightBracket,
            (int)SDL_KeyCode.KQuote => ImGuiKey.Apostrophe,
            (int)SDL_KeyCode.KBackquote => ImGuiKey.GraveAccent,
            (int)SDL_KeyCode.KCapslock => ImGuiKey.CapsLock,
            (int)SDL_KeyCode.KScrolllock => ImGuiKey.ScrollLock,
            (int)SDL_KeyCode.KNumlockclear => ImGuiKey.NumLock,
            (int)SDL_KeyCode.KPrintscreen => ImGuiKey.PrintScreen,
            (int)SDL_KeyCode.KPause => ImGuiKey.Pause,
            (int)SDL_KeyCode.KKPDecimal => ImGuiKey.KeypadDecimal,
            (int)SDL_KeyCode.KKPDivide => ImGuiKey.KeypadDivide,
            (int)SDL_KeyCode.KKPMultiply => ImGuiKey.KeypadMultiply,
            (int)SDL_KeyCode.KKPMinus => ImGuiKey.KeypadSubtract,
            (int)SDL_KeyCode.KKPPlus => ImGuiKey.KeypadAdd,
            (int)SDL_KeyCode.KKPEnter => ImGuiKey.KeypadEnter,
            (int)SDL_KeyCode.KKPEquals => ImGuiKey.KeypadEqual,
            (int)SDL_KeyCode.KACForward => ImGuiKey.AppForward,
            (int)SDL_KeyCode.KACBack => ImGuiKey.AppBack,
            _ => ImGuiKey.None
        };
    }


    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SetClipboardDelegate(IntPtr userData, [MarshalAs(UnmanagedType.LPUTF8Str)] string text);
    private static readonly SetClipboardDelegate set_clipboard_text_fn_delegate = setClipboardTextFn;

    private static void setClipboardTextFn(IntPtr userData, string text)
    {
        _sdl.SetClipboardText(text);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr GetClipboardDelegate(IntPtr userData);
    private static readonly GetClipboardDelegate get_clipboard_text_fn_delegate = getClipboardTextFn;

    private static IntPtr getClipboardTextFn(IntPtr userData)
    {
        Marshal.FreeCoTaskMem(_clipboardTextMemory);
        _clipboardTextMemory = Marshal.StringToCoTaskMemUTF8(_sdl.GetClipboardTextS());
        return _clipboardTextMemory;
    }

    private static IntPtr _clipboardTextMemory = IntPtr.Zero;

    #endregion // Keyboard
}
