using System.Diagnostics;
using DanmakuEngine.Bindables;
using DanmakuEngine.Timing;
using Silk.NET.SDL;
using Vortice.DXGI;

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

    public bool Worthless
        => !IsDown.IsBound() && OnDown is null && OnUp is null && (Bindings is null || Bindings.Count == 0);

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

    public void StatusChanged(Action<ValueChangedEvent<bool>> onChange, bool runOnceImmediately = false)
    {
        IsDown.BindValueChanged(onChange, runOnceImmediately);
    }

    public void BindKeyDown(Action<KeyStatus, double> onKeyDown)
    {
        OnDown += onKeyDown;
    }

    public void BindKeyUp(Action<KeyStatus, double> onKeyUp)
    {
        OnUp += onKeyUp;
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

        if (Worthless)
            return false;

        // we shouldn't handle modifier this way
        // consider use a separate method for handling key event and pass the modifier
        Mod = (Keymod)e.Keysym.Mod;

        if (e.Type == (uint)EventType.Keydown)
        {
            IsDown.Value = true;
        }
        else if (e.Type == (uint)EventType.Keyup)
        {
            IsDown.Value = false;

            Mod = Keymod.None;
        }

        return true;
    }

    public KeyStatus(KeyCode key)
    {
        this.Key = key;

        IsDown.BindValueChanged(v =>
        {
            if (v.NewValue)
                OnDown?.Invoke(this, Time.ElapsedSeconds);
            else
                OnUp?.Invoke(this, Time.ElapsedSeconds);
        });
    }
}
