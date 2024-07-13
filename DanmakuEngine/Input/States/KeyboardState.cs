using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DanmakuEngine.Bindables;
using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Events;
using DanmakuEngine.Input.Handlers;
using DanmakuEngine.Input.States;
using Silk.NET.Input;
using Silk.NET.SDL;
using Veldrid.Sdl2;

namespace DanmakuEngine.Input.Keybards;

public partial class KeyboardState
{
    // protected readonly FrozenDictionary<KeyCode, KeyStatus> keyStatuses;

    // public KeyStatus this[KeyCode key] => keyStatuses[key];

    // public void BindFor(KeyCode key, Bindable<bool> bindable)
    //     => this[key].BindFor(bindable);

    // public void BindKey(KeyCode key1, KeyCode key2)
    //     => this[key1].BindTo(this[key2]);

    // public void ClearBindings(KeyCode key)
    // {
    //     var keyStatus = this[key];

    //     keyStatus.IsDown.UnbindBindings();
    // }

    // public void RemoveBinding(KeyCode key, Bindable<bool> bindable)
    // {
    //     var keyStatus = this[key];

    //     keyStatus.IsDown.UnbindFrom(bindable);
    // }

    // public void RemoveBinding(KeyCode key1, KeyCode key2)
    // {
    //     var keyStatus1 = this[key1];
    //     var keyStatus2 = this[key2];

    //     keyStatus1.IsDown.UnbindFrom(keyStatus2.IsDown);
    // }

    // public void Reset()
    // {
    //     foreach (var key in keyStatuses.Values)
    //     {
    //         key.IsDown.UnbindAll();
    //         key.IsDown.Value = false;

    //         key.OnDown = null!;
    //         key.OnUp = null!;
    //     }
    // }

    // public virtual bool HandleEvent(KeyboardEvent e)
    // {
    //     var targetKey = this[(KeyCode)e.Keysym.Sym];

    //     return targetKey.HandleEvent(e);

    //     e.Type
    // }

    // public abstract void RegisterKeys();

    // protected KeyStatus Register(KeyCode key)
    //     => keyStatuses[key];

    // protected bool IsKeyDown(KeyboardEvent e)
    //     => e.Type == (uint)EventType.Keydown;

    // protected bool IsKeyUp(KeyboardEvent e)
    //     => e.Type == (uint)EventType.Keyup;

    // public void Register()
    // {
    //     throw new NotImplementedException();
    // }

    // public KeyboardState()
    // {
    //     // TODO: generate map only after the keys are registered
    //     List<KeyStatus> keyStatuses = new();

    //     foreach (var key in Enum.GetValues<KeyCode>())
    //         keyStatuses.Add(new KeyStatus(key));

    //     this.keyStatuses = keyStatuses.ToFrozenDictionary(k => k.Key);

    //     Debug.Assert(this.keyStatuses.Count != 0);

    //     if (this is IInjectable injectable)
    //         injectable.AutoInject();
    // }

    private readonly ButtonStates<Keys> _states = new();

    public IKeyEvent? LastEvent { get; internal set; }

    public bool IsPressed(Keys key)
        => _states.IsPressed(key);

    public bool SetPressed(Keys key, bool pressed)
        => _states.SetPressed(key, pressed);

    /// <summary>
    /// Indicate whether the control key is pressed.
    /// </summary>
    public bool CtrlModifier => IsPressed(Keys.ControlLeft) || IsPressed(Keys.ControlRight);

    /// <summary>
    /// Indicate whether the alt key is pressed.
    /// </summary>
    public bool AltModifier => IsPressed(Keys.AltLeft) || IsPressed(Keys.AltRight);

    /// <summary>
    /// Indicate whether the shift key is pressed.
    /// </summary>
    public bool ShiftModifier => IsPressed(Keys.ShiftLeft) || IsPressed(Keys.ShiftRight);

    /// <summary>
    /// Indicate whether the super key is pressed.
    /// On Windows, this is the Windows key.
    /// </summary>
    public bool SuperModifier => IsPressed(Keys.SuperLeft) || IsPressed(Keys.SuperRight);

    public void Initialize()
    {
        _states.Clear();
    }
}
