using DanmakuEngine.Logging;

namespace DanmakuEngine.Games.Screens;

public class MainScreen : Screen
{
    public MainScreen(ScreenStack parent) : base(parent)
    {   
    }

    // This method is called when the screen(or average object) is loading
    public override void Load()
    {
        keyboardHandler = new MainMenuKeyBoardHandler()
        {
            secretCodeHandler = new(Clock),
        };
    }

    // This method is called when the screen(or average object) is starting
    public override void Start()
    {
        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");
    }

    // This method is called every frame for the screen(and it's children object)
    public override void Update()
    {
        
    }
}