namespace DanmakuEngine.Input;

public enum Keys
{
    // An unknown key.
    Unknown = -1,
    // The spacebar key.
    Space = 32,
    // The apostrophe key.
    Apostrophe = 39,
    // The comma key.
    Comma = 44,
    // The minus key.
    Minus = 45,
    // The period key.
    Period = 46,
    // The slash key.
    Slash = 47,
    // The 0 key.
    Number0 = 48,
    // The 1 key.
    Number1 = 49,
    // The 2 key.
    Number2 = 50,
    // The 3 key.
    Number3 = 51,
    // The 4 key.
    Number4 = 52,
    // The 5 key.
    Number5 = 53,
    // The 6 key.
    Number6 = 54,
    // The 7 key.
    Number7 = 55,
    // The 8 key.
    Number8 = 56,
    // The 9 key.
    Number9 = 57,
    // The semicolon key.
    Semicolon = 59,
    // The equal key.
    Equal = 61,
    // The A key.
    A = 65,
    // The B key.
    B = 66,
    // The C key.
    C = 67,
    // The D key.
    D = 68,
    // The E key.
    E = 69,
    // The F key.
    F = 70,
    // The G key.
    G = 71,
    // The H key.
    H = 72,
    // The I key.
    I = 73,
    // The J key.
    J = 74,
    // The K key.
    K = 75,
    // The L key.
    L = 76,
    // The M key.
    M = 77,
    // The N key.
    N = 78,
    // The O key.
    O = 79,
    // The P key.
    P = 80,
    // The Q key.
    Q = 81,
    // The R key.
    R = 82,
    // The S key.
    S = 83,
    // The T key.
    T = 84,
    // The U key.
    U = 85,
    // The V key.
    V = 86,
    // The W key.
    W = 87,
    // The X key.
    X = 88,
    // The Y key.
    Y = 89,
    // The Z key.
    Z = 90,
    // The left bracket(opening bracket) key.
    LeftBracket = 91,
    // The backslash.
    BackSlash = 92,
    // The right bracket(closing bracket) key.
    RightBracket = 93,
    // The grave accent key.
    GraveAccent = 96,
    // Non US keyboard layout key 1.
    World1 = 161,
    // Non US keyboard layout key 2.
    World2 = 162,
    // The escape key.
    Escape = 256,
    // The enter key.
    Enter = 257,
    // The tab key.
    Tab = 258,
    // The backspace key.
    Backspace = 259,
    // The insert key.
    Insert = 260,
    // The delete key.
    Delete = 261,
    // The right arrow key.
    Right = 262,
    // The left arrow key.
    Left = 263,
    // The down arrow key.
    Down = 264,
    // The up arrow key.
    Up = 265,
    // The page up key.
    PageUp = 266,
    // The page down key.
    PageDown = 267,
    // The home key.
    Home = 268,
    // The end key.
    End = 269,
    // The caps lock key.
    CapsLock = 280,
    // The scroll lock key.
    ScrollLock = 281,
    // The num lock key.
    NumLock = 282,
    // The print screen key.
    PrintScreen = 283,
    // The pause key.
    Pause = 284,
    // The F1 key.
    F1 = 290,
    // The F2 key.
    F2 = 291,
    // The F3 key.
    F3 = 292,
    // The F4 key.
    F4 = 293,
    // The F5 key.
    F5 = 294,
    // The F6 key.
    F6 = 295,
    // The F7 key.
    F7 = 296,
    // The F8 key.
    F8 = 297,
    // The F9 key.
    F9 = 298,
    // The F10 key.
    F10 = 299,
    // The F11 key.
    F11 = 300,
    // The F12 key.
    F12 = 301,
    // The F13 key.
    F13 = 302,
    // The F14 key.
    F14 = 303,
    // The F15 key.
    F15 = 304,
    // The F16 key.
    F16 = 305,
    // The F17 key.
    F17 = 306,
    // The F18 key.
    F18 = 307,
    // The F19 key.
    F19 = 308,
    // The F20 key.
    F20 = 309,
    // The F21 key.
    F21 = 310,
    // The F22 key.
    F22 = 311,
    // The F23 key.
    F23 = 312,
    // The F24 key.
    F24 = 313,
    // The F25 key.
    F25 = 314,
    // The 0 key on the key pad.
    Keypad0 = 320,
    // The 1 key on the key pad.
    Keypad1 = 321,
    // The 2 key on the key pad.
    Keypad2 = 322,
    // The 3 key on the key pad.
    Keypad3 = 323,
    // The 4 key on the key pad.
    Keypad4 = 324,
    // The 5 key on the key pad.
    Keypad5 = 325,
    // The 6 key on the key pad.
    Keypad6 = 326,
    // The 7 key on the key pad.
    Keypad7 = 327,
    // The 8 key on the key pad.
    Keypad8 = 328,
    // The 9 key on the key pad.
    Keypad9 = 329,
    // The decimal key on the key pad.
    KeypadDecimal = 330,
    // The divide key on the key pad.
    KeypadDivide = 331,
    // The multiply key on the key pad.
    KeypadMultiply = 332,
    // The subtract key on the key pad.
    KeypadSubtract = 333,
    // The add key on the key pad.
    KeypadAdd = 334,
    // The enter key on the key pad.
    KeypadEnter = 335,
    // The equal key on the key pad.
    KeypadEqual = 336,
    // The left shift key.
    ShiftLeft = 340,
    // The left control key.
    ControlLeft = 341,
    // The left alt key.
    AltLeft = 342,
    // The left super key.
    SuperLeft = 343,
    // The right shift key.
    ShiftRight = 344,
    // The right control key.
    ControlRight = 345,
    // The right alt key.
    AltRight = 346,
    // The right super key.
    SuperRight = 347,
    // The menu key.
    Menu = 348
}
