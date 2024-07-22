using System.Collections;

namespace DanmakuEngine.Input.States;

public class ButtonStates<T> : IEnumerable<T>
    where T : notnull
{
    private readonly Dictionary<T, bool> _states = new();

    public bool IsPressed(T button) => _states.TryGetValue(button, out var pressed) && pressed;

    /// <summary>
    /// Set a button to desired pressed state.
    /// </summary>
    /// <param name="button">the button to set</param>
    /// <param name="pressed">whether the button is pressed</param>
    /// <returns>whether the state was changed</returns>
    public bool SetPressed(T button, bool pressed)
    {
        bool hasValue = _states.TryGetValue(button, out var current);

        if (!hasValue && !pressed)
            return false;

        if (current == pressed)
            return false;

        if (hasValue)
            _states[button] = pressed;
        else
            _states.Add(button, pressed);

        return true;
    }

    /// <summary>
    /// Return a enumerator of all buttons that are pressed.
    /// </summary>
    /// <returns>The enumerator</returns>
    public IEnumerator<T> GetEnumerator()
        => _states.Where(kv => kv.Value).Select(kv => kv.Key).GetEnumerator();

    /// <summary>
    /// Return a enumerator of all buttons that are pressed.
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
        => _states.Where(kv => kv.Value).Select(kv => kv.Key).GetEnumerator();

    public void Clear()
        => _states.Clear();
}
