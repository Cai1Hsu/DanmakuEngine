using DanmakuEngine.Dependency;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens.LoadingScreen;

public class LoadingScreen : Screen, IInjectable
{
    [Inject]
    private ScreenStack screens = null!;
    
    public override void Load()
    {
        keyboardHandler = null!;
    }
    
    public override async void Start()
    {
        keyboardHandler = null!;
        
        Logger.Log("少女祈祷中");
        Logger.Log("    Now loading...");

        Logger.Info("Keyboard is not handled in this screen.");

        // Assume that we are loading something
        await Task.Delay(5000);

        screens.Pop();
        screens.Push(new MainScreen.MainScreen());
    }

    public override void Update(double delta)
    {
        
    }
}