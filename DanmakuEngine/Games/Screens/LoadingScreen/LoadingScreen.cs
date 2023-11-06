using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens.LoadingScreen;

public class LoadingScreen : Screen
{
    public LoadingScreen(ScreenStack parent) : base(parent)
    {
    }

    // This method is called before the screen is pushed to the stack
    // After the dependency injection is done
    // You can do some initialization here
    public override void Load()
    {
        keyboardHandler = null!;
    }

    // This method is called when the screen actually starts to run
    public override void Start()
    {
        Logger.Log("少女祈祷中");
        Logger.Log("    Now loading...");

        Logger.Info("Keyboard is not handled in this screen.");

        // Assume that we are loading something
        Task.Delay(2000).Wait();

        ScreenStack.Switch(new MainScreen(Parent));
    }

    // This method is called every frame in the Update loop
    public override void Update()
    {

    }
}