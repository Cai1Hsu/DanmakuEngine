using System.Diagnostics;
using DanmakuEngine.Bindables;
using DanmakuEngine.Timing;
using Silk.NET.SDL;

namespace DanmakuEngine.Input.Keybards;

public class KeyStatus
{
    public readonly KeyCode Key;

    public Keymod Mod { get; private set; }

    public readonly Bindable<bool> IsDown = new(false);

    public Action<KeyStatus, double>? OnDown = null!;

    public Action<KeyStatus, double>? OnUp = null!;

    public void BindFor(Bindable<bool> bindable)
        => bindable.BindTo(IsDown);

    private List<KeyStatus>? Bindings;

    public void BindTo(KeyStatus other)
    {
        IsDown.BindTo(other.IsDown);

        Bindings ??= new();

        Bindings.Add(other);
    }

    public void UnbindFrom(KeyStatus other)
    {
        IsDown.UnbindFrom(other.IsDown);

        Debug.Assert(Bindings is not null);

        Bindings.Remove(other);
    }

    public void UnbindFrom(Bindable<bool> bindable)
    {
        IsDown.UnbindFrom(bindable);
    }

    public void BindEvent(Action<ValueChangedEvent<bool>> onChange, bool runOnceImmediately = false)
    {
        IsDown.BindValueChanged(onChange, runOnceImmediately);
    }

    /// <summary>
    /// Handle the event and return whether the event is handled
    /// </summary>
    /// <param name="e">the event from message loop</param>
    /// <returns>whether the event is handled</returns>
    public bool HandleEvent(KeyboardEvent e)
    {
        Debug.Assert(e.Keysym.Sym == (int)Key);

        // we don't handle repeat events
        if (e.Repeat != 0)
            return false;

        Mod = (Keymod)e.Keysym.Mod;

        // if the key is bound with bindables, we assume this action is handled
        var bound = (IsDown.BindingCount - (Bindings is null ? 0 : Bindings.Count)) > 0;

        if (e.Type == (uint)EventType.Keydown)
        {
            if (OnDown is null && !bound)
                return false;

            Debug.Assert(!IsDown.Value);

            IsDown.Value = true;
        }
        else if (e.Type == (uint)EventType.Keyup)
        {
            if (OnUp is null && !bound)
                return false;

            Debug.Assert(IsDown.Value);

            IsDown.Value = false;
        }

        return true;
    }

    public KeyStatus(KeyCode key)
    {
        this.Key = key;

        IsDown.BindValueChanged(v =>
        {
            if (v.NewValue)
                OnDown?.Invoke(this, Time.CurrentTime);
            else
                OnUp?.Invoke(this, Time.CurrentTime);
        });
    }
}
