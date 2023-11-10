using DanmakuEngine.Logging;
using DanmakuEngine.Games.Screens.MainMenu;

namespace DanmakuEngine.Games.Screens.Welcome;

public class LoadingScreen : Screen
{
    // All of the three methods below run in the `Update` loop

    // called by `load` in constructor for screen and manually called for other objects
    // This method is called when the screen(or average object) is loading
    // You can do some initialization for the screen here
    public override void Load()
    {
        keyboardHandler = null!;
    }

    // This method is called the first frame when the screen(or average object) is completely loaded
    // If you want to do something that takes a long time without blocking the main thread, you can use `Start` method
    // make it async and use `await Task.Delay` to delay the execution
    public override async void Start()
    {
        Logger.Log("少女祈祷中");
        Logger.Log("    Now loading...");

        Logger.Log("This will took about 5s");

        Logger.Info("Keyboard is not handled in this screen.");

        // Assume that we are loading something
        await Task.Delay(5000);

        ScreenStack.Switch(new MainScreen());
    }

    // This method is called every frame, the first call is after `Start` method
    public override void Update()
    {

    }
}