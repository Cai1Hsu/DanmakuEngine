using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens.LoadingScreen;

public class LoadingScreen : Screen
{
    public LoadingScreen(ScreenStack parent) : base(parent)
    {
    }

    // All of the three methods below run in the `Update` loop

    // This method is called when the screen(or average object) is loading
    // You can do some initialization here
    public override void Load()
    {
        keyboardHandler = null!;
    }

    // This method is called when the screen actually starts to run
    public override async void Start()
    {
        Logger.Log("少女祈祷中");
        Logger.Log("    Now loading...");

        Logger.Log("This will took about 2s");

        Logger.Info("Keyboard is not handled in this screen.");

        // Assume that we are loading something
        await Task.Delay(2000);

        ScreenStack.Switch(new MainScreen(Parent));
    }

    // This method is called every frame
    public override void Update()
    {

    }
}