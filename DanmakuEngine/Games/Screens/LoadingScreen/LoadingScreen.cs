using DanmakuEngine.Dependency;
using DanmakuEngine.Graphics;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens.LoadingScreen;

public class LoadingScreen : Screen, IInjectable
{
    [Inject]
    private readonly ScreenStack screens = null!;

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
    public override async void Start()
    {
        Logger.Log("少女祈祷中");
        Logger.Log("    Now loading...");

        Logger.Info("Keyboard is not handled in this screen.");

        // Assume that we are loading something
        await Task.Delay(3000);

        screens.Switch(new MainScreen.MainScreen(screens));
    }

    // This method is called every frame in the Update loop
    public override void Update()
    {

    }
}