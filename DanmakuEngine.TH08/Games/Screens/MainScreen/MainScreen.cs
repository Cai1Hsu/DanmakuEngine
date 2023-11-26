using System.Text;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using DanmakuEngine.Transfomation;
using DanmakuEngine.Transfomation.Functions;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class MainScreen : Screen
{
    protected List<TransformSequence> transformations = [];

    // This method is called when the screen(or average object) is loading
    public override void Load()
    {
        keyboardHandler = new MainMenuKeyBoardHandler()
        {
            secretCodeHandler = new(Clock),
        };

        transformations.AddRange(new TransformSequence[]
        {
            new TransformSequence(
                new Transformer(1000, new SquareIn(), (p) =>
                    {
                        ShowBGM(p);
                    }
                ).Delay(2000),
                new Transformer(1000, new SineOut(), (p) =>
                    {
                        ShowBGM(p);
                    }
                ).Delay(1000)
            ).LoopForever(),
            new Transformer(1500, new SquareIn(), (p) =>
                {
                    UseSpell(p);
                }
            ).Delay(1000).LoopForever(),
            new Transformer(600, new EaseInCubic(), (p) =>
                {
                    EnemySpell(p);
                }
            ).Delay(1000).LoopForever(),
            new TransformSequence(
                new Transformer(1000, new SineIn(), (p) =>
                    {
                        PrintSlider((int)Math.Round(p * 99));
                    }
                ),
                new Transformer(1000, new SineOut(), (p) =>
                    {
                        PrintSlider((int)Math.Round(p * 99));
                    }
                )
            ).LoopForever(),
        });
    }

    private void PrintSlider(int percentage)
    {
        StringBuilder sb = new();

        sb.Append('[');

        for (int i = 0; i < 100; i++)
        {
            if (i == percentage)
                sb.Append('#');
            else
                sb.Append(' ');
        }

        sb.Append(']');

        Console.SetCursorPosition(0, 26);
        Logger.Write(sb.ToString(), writeLine: true);
    }

    private void EnemySpell(double percentage)
    {
        const int height = 25;
        const string spell = "難題「仏の御石の鉢　-砕けぬ意思-」";

        int line = height - (int)Math.Round(percentage * height);

        Console.SetCursorPosition(0, 1);
        for (int i = 0; i < 26; i++)
        {
            if (i == line)
                Logger.Write(spell, writeLine: true);
            else
                Logger.Write(' '.Multiply(spell.GetDisplayLength()), writeLine: true);
        }
    }

    private void UseSpell(double percentage)
    {
        const int height = 15;
        const string spell = "恋符「マスタースパーク」";

        int line = (int)Math.Round(percentage * height);

        Console.SetCursorPosition(0, 10);
        for (int i = 1; i <= 26; i++)
        { 
            Console.Write("\u001b[50C");

            if (i == line)
                Logger.Write(spell, writeLine: true);
            else
                Logger.Write(' '.Multiply(spell.GetDisplayLength()), writeLine: true);
        }
    }

    private void ShowBGM(double percentage)
    {
        // japanese causes width issue
        // when we meet CJK characters, the whole character will be shown or disappear
        // this lead to the animation speed not constant
        // const string bgm = "BGM: 竹取飛翔　～ Lunatic Princess";
        // int wholeDisplayLength = bgm.GetDisplayLength();
        // 
        // int displayChars = (int)Math.Round(percentage * wholeDisplayLength);
        // string displayString = "";
        // int currentDisplayLength = 0;
        // 
        // for (int i = 0; i < bgm.Length; i++)
        // {
        //     int charDisplayLength = bgm[i] < 128 ? 1 : 2;
        //     if (currentDisplayLength + charDisplayLength > displayChars)
        //     {
        //         if (currentDisplayLength + 1 == displayChars)
        //             displayString += "\u2005";
        // 
        //         break;
        //     }
        // 
        //     displayString += bgm[i];
        //     currentDisplayLength += charDisplayLength;
        // }
        // 
        // string line = ' '.Multiply(wholeDisplayLength - currentDisplayLength) + displayString

        // Since '\u2005' is not supported in every terminal
        const string bgm = "BGM: Taketori Hishou ~ Lunatic Princess";
        int fullDisplayLength = bgm.GetDisplayLength();

        int displayChars = (int)Math.Round(percentage * fullDisplayLength);
        string displayString = bgm[..displayChars];
        int displayLength = displayString.GetDisplayLength();

        string line = ' '.Multiply(fullDisplayLength - displayLength) + displayString;

        Console.SetCursorPosition(50, 1);
        Console.Write(line);
    }

    // This method is called when the screen(or average object) is starting
    public override void Start()
    {
        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");

        Console.Clear();
        Console.ResetColor();
        foreach (var transformation in transformations)
        {
            if (transformation.IsDone)
                transformation.Dispose();
            else
                transformation.Begin();
        }
    }

    // This method is called every frame for the screen(and it's children object)
    public override void Update()
    {
        foreach (var transformation in transformations)
            transformation.Update(Time.UpdateDelta * 1000);
    }
}
