namespace DanmakuEngine.Bindables;

public class EnabledChangedEvent : EventArgs
{
    public bool Enabled;
    public EnabledChangedEvent(bool enabled)
    {
        this.Enabled = enabled;
    }
}
