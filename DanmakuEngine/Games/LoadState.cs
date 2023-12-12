namespace DanmakuEngine.Games;

public enum LoadState
{
    /// <summary>
    /// The gameobject is not loaded and has not been initialized
    /// </summary>
    NotLoaded,
    /// <summary>
    /// The gameobject is loaded, but has not been initialized in the Update loop
    /// </summary>
    Ready,
    /// <summary>
    /// The gameobject is loaded and has been initialized in the Update loop
    /// which means it is ready to be used
    /// </summary>
    Complete
}
