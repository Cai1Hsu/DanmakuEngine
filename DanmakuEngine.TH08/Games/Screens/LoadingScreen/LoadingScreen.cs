using DanmakuEngine.Logging;
using DanmakuEngine.Games.Screens.MainMenu;
using DanmakuEngine.Scheduling;

namespace DanmakuEngine.Games.Screens.Welcome;

public class LoadingScreen : Screen
{
    private Scheduler scheduler = null!;

    // All of the three methods below run in the `Update` loop

    // called by `load` in constructor for screen and manually called for other objects
    // This method is called when the screen(or average object) is loading
    // You can do some initialization for the screen here
    public override void Load()
    {
        keyboardHandler = null!;

        InternalChildren =
        [
            scheduler = new Scheduler(),
        ];
    }

    // This method is called the first frame when the screen(or average object) is completely loaded
    // If you want to do something that takes a long time without blocking the main thread, you can use `Start` method
    // make it async and use `await Task.Delay` to delay the execution
    public override void Start()
    {
        Logger.Log("少女祈祷中");
        Logger.Log("    Now loading...");

        Logger.Log("This will took about 5s");

        Logger.Info("Keyboard is not handled in this screen.");

        // Assume that we are loading something
        // you should load your resources here
        // and do it in a async way

        scheduler.ScheduleTask(new ScheduledTask(() =>
        {
            // if finished, switch to main screen
            ScreenStack.Switch(new MainScreen());
        }, () =>
        {
            // check that if our loading is finished
            // here we just check if the time is greater than 2s
            return ScreenClock.CurrentTime > 2;
        }));
    }

    // This method is called every frame, the first call is after `Start` method
    public override void Update()
    {

    }
}