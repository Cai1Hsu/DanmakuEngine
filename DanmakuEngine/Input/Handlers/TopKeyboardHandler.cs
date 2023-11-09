using System.Diagnostics;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Games.Screens;
using DanmakuEngine.Logging;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Handlers;

public partial class TopKeyboardHandler : IInputHandler
{
    [Inject]
    private ScreenStack screens = null!;

    public TopKeyboardHandler()
    {
        // FIXME: Causes a null reference exception when NativeAOT is enabled
        /*
            Dependency/IInjectable.cs(16): 
                Trim analysis warning IL2075: 
                    DanmakuEngine.Dependency.IInjectable.AutoInject(Boolean): 
                        'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicFields', 'DynamicallyAccessedMemberTypes.NonPublicFields' in call to 'System.Type.GetFields(BindingFlags)'. The return value of method 'System.Object.GetType()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to. 
            Dependency/IInjectable.cs(37): 
                Trim analysis warning IL2075: 
                    DanmakuEngine.Dependency.IInjectable.AutoInject(Boolean): 
                        'this' argument does not satisfy 'DynamicallyAccessedMemberTypes.PublicProperties', 'DynamicallyAccessedMemberTypes.NonPublicProperties' in call to 'System.Type.GetProperties(BindingFlags)'. The return value of method 'System.Object.GetType()' does not have matching annotations. The source value must declare at least the same requirements as those declared on the target location it is assigned to. 
        */

        ((IInjectable)this).AutoInject();

        // Failed under NativeAOT
        Debug.Assert(screens != null);
    }

    public void KeyDown(KeyboardEvent e)
    {
        Debug.Assert(e.Type == (uint)EventType.Keydown);

        var repeat = e.Repeat != 0;
        var keysym = e.Keysym;

        screens.Peek()?.keyboardHandler?.KeyDown(keysym, repeat);
    }

    public void KeyUp(KeyboardEvent e)
    {
        Debug.Assert(e.Type == (uint)EventType.Keyup);

        var repeat = e.Repeat != 0;
        var keysym = e.Keysym;

        screens.Peek()?.keyboardHandler?.KeyUp(keysym, repeat);
    }

    public void Register(GameHost host)
    {
        host.KeyDown += KeyDown;
        host.KeyUp += KeyUp;
    }
}