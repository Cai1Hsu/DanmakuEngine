using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DanmakuEngine.Bindables;
using DanmakuEngine.Dependency;
using DanmakuEngine.Input.Handlers;
using Silk.NET.Input;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Keybards;

public abstract partial class KeyboardHandler : IKeyboardHandler
{
    protected readonly FrozenDictionary<KeyCode, KeyStatus> keyStatuses;

    public KeyStatus this[KeyCode key] => keyStatuses[key];

    public void BindFor(KeyCode key, Bindable<bool> bindable)
        => this[key].BindFor(bindable);

    public void BindKey(KeyCode key1, KeyCode key2)
        => this[key1].BindTo(this[key2]);

    public void ClearBindings(KeyCode key)
    {
        var keyStatus = this[key];

        keyStatus.IsDown.UnbindBindings();
    }

    public void RemoveBinding(KeyCode key, Bindable<bool> bindable)
    {
        var keyStatus = this[key];

        keyStatus.IsDown.UnbindFrom(bindable);
    }

    public void RemoveBinding(KeyCode key1, KeyCode key2)
    {
        var keyStatus1 = this[key1];
        var keyStatus2 = this[key2];

        keyStatus1.IsDown.UnbindFrom(keyStatus2.IsDown);
    }

    public void Reset()
    {
        foreach (var key in keyStatuses.Values)
        {
            key.IsDown.UnbindAll();
            key.IsDown.Value = false;

            key.OnDown = null!;
            key.OnUp = null!;
        }
    }

    public virtual bool HandleEvent(KeyboardEvent e)
    {
        var targetKey = this[(KeyCode)e.Keysym.Sym];

        return targetKey.HandleEvent(e);
    }

    public abstract void RegisterKeys();

    protected KeyStatus Register(KeyCode key)
        => keyStatuses[key];

    protected bool IsKeyDown(KeyboardEvent e)
        => e.Type == (uint)EventType.Keydown;

    protected bool IsKeyUp(KeyboardEvent e)
        => e.Type == (uint)EventType.Keyup;

    public KeyboardHandler()
    {
        // TODO: generate map only after the keys are registered
        List<KeyStatus> keyStatuses = new();

        foreach (var key in Enum.GetValues<KeyCode>())
            keyStatuses.Add(new KeyStatus(key));

        this.keyStatuses = keyStatuses.ToFrozenDictionary(k => k.Key);

        Debug.Assert(this.keyStatuses.Count != 0);

        if (this is IInjectable injectable)
            injectable.AutoInject();
    }
}
