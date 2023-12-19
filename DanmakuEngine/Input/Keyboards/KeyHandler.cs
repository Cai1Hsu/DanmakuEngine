using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DanmakuEngine.Bindables;
using DanmakuEngine.Input.Handlers;
using Silk.NET.Input;
using Silk.NET.SDL;
using Silk.NET.Vulkan;

namespace DanmakuEngine.Input.Keybards;

public class KeyHander : IKeyboardHandler
{
    protected readonly ImmutableDictionary<KeyCode, KeyStatus> keyStatuses;

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

    public KeyHander()
    {
        List<KeyStatus> keyStatuses = new();

        foreach (var key in Enum.GetValues<KeyCode>())
            keyStatuses.Add(new KeyStatus(key));

        this.keyStatuses = keyStatuses.ToImmutableDictionary(k => k.Key);

        Debug.Assert(this.keyStatuses.Count != 0);
    }
}