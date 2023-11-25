using System.Text;
using DanmakuEngine.Logging;
using DanmakuEngine.Timing;
using DanmakuEngine.Transfomation;
using DanmakuEngine.Transfomation.Functions;

namespace DanmakuEngine.Games.Screens.MainMenu;

public class MainScreen : Screen
{
    protected List<TransformSequence> transformations = new();

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
                new TransformSequence(
                    new Transformer(1000, new SineInQuad(), (percentage) =>
                        {
                            ShowBGM(percentage);
                        }
                    )
                ).Delay(2000),
                new TransformSequence(
                    new Transformer(1000, new SineOut(), (percentage) =>
                        {
                            ShowBGM(percentage);
                        }
                    )
                ).Delay(1000)
            ).LoopForever(),
            new TransformSequence(
                new Transformer(1500, new SineInQuad(), (percentage) =>
                    {
                        UseSpell(percentage);
                    }
                )
            ).Delay(1000).LoopForever(),
            new TransformSequence(
                new Transformer(1000, new SineInQuad(), (percentage) =>
                    {
                        EnemySpell(percentage);
                    }
                )
            ).Delay(1000).LoopForever(),
            new TransformSequence(
                new Transformer(1000, new SineIn(), (percentage) =>
                    {
                        PrintSlider((int)Math.Round(percentage * 100));
                    }
                ),
                new Transformer(1000, new SineOut(), (percentage) =>
                    {
                        PrintSlider((int)Math.Round(percentage * 100));
                    }
                )
            ).LoopForever(),
        });

        Console.Clear();
        Console.ResetColor();
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

        int line = height - (int)(percentage * height);

        Console.SetCursorPosition(0, 1);
        for (int i = 0; i < 26; i++)
        {
            if (i == line)
                Logger.Write("難題「仏の御石の鉢　-砕けぬ意思-」", writeLine: true);
            else
                Logger.Write((new string(' ', 50)), writeLine: true);
        }
    }

    private void UseSpell(double percentage)
    {
        const int height = 15;

        int line = (int)(percentage * height);

        Console.SetCursorPosition(0, 10);
        for (int i = 1; i <= 26; i++)
        {
            Logger.Write("\u001b[50C");

            if (i == line)
                Logger.Write("恋符「マスタースパーク」", writeLine: true);
            else
                Logger.Write((new string(' ', 100)), writeLine: true);
        }
    }

    private void ShowBGM(double percentage)
    {
        // japanese causes width issue so we uses english
        const string bgm = "BGM: Taketori Hishou ~ Lunatic Princess";

        int chars = (int)Math.Round(percentage * bgm.Length);

        string line = new(' ', 50 - chars);

        line += bgm[0..chars];

        Console.SetCursorPosition(50, 1);
        Console.Write(line);
    }

    // This method is called when the screen(or average object) is starting
    public override void Start()
    {
        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");

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