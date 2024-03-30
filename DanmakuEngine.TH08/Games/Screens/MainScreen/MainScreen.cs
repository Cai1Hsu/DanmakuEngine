using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using DanmakuEngine.DearImgui;
using DanmakuEngine.DearImgui.Windowing;
using DanmakuEngine.Dependency;
using DanmakuEngine.Engine;
using DanmakuEngine.Extensions;
using DanmakuEngine.Logging;
using DanmakuEngine.Scheduling;
using DanmakuEngine.Timing;
using DanmakuEngine.Transformation;
using DanmakuEngine.Transformation.Functions;
using ImGuiNET;
using Sdl2Window = DanmakuEngine.Engine.Windowing.Sdl2Window;

namespace DanmakuEngine.Games.Screens.MainMenu;

public partial class MainScreen : Screen
{
    protected List<TransformSequence> transformations = new();

    private readonly Clock clock = new();

    [Inject]
    private GameHost _host = null!;

    // This method is called when the screen(or average object) is loading
    protected override void Load()
    {
        keyboardHandler = new MainMenuKeyBoardHandler()
        {
            secretCodeHandler = new(),
        };

        // transformations.AddRange(new TransformSequence[]
        // {
        //     new TransformSequence(
        //         new Transformer(1000, new SquareIn(), (p) =>
        //             {
        //                 ShowBGM(p);
        //             }
        //         ).Delay(2000),
        //         new Transformer(1000, new SineOut(), (p) =>
        //             {
        //                 ShowBGM(p);
        //             }
        //         ).Delay(1000)
        //     ).LoopForever(),
        //     new Transformer(1500, new SquareIn(), (p) =>
        //         {
        //             UseSpell(p);
        //         }
        //     ).Delay(1000).LoopForever(),
        //     new Transformer(600, new EaseInCubic(), (p) =>
        //         {
        //             EnemySpell(p);
        //         }
        //     ).Delay(1000).LoopForever(),
        //     new TransformSequence(
        //         new Transformer(1000, new SineIn(), (p) =>
        //             {
        //                 PrintSlider((int)Math.Round(p * 99));
        //             }
        //         ),
        //         new Transformer(1000, new SineOut(), (p) =>
        //             {
        //                 PrintSlider((int)Math.Round(p * 99));
        //             }
        //         )
        //     ).LoopForever(),
        // });
    }

    // private void PrintSlider(int percentage)
    // {
    //     StringBuilder sb = new();

    //     sb.Append('[');

    //     for (int i = 0; i < 100; i++)
    //     {
    //         if (i == percentage)
    //             sb.Append('#');
    //         else
    //             sb.Append(' ');
    //     }

    //     sb.Append(']');

    //     Console.SetCursorPosition(0, 26);
    //     Logger.Write(sb.ToString(), writeLine: true);
    // }

    // private void EnemySpell(double percentage)
    // {
    //     const int height = 25;
    //     const string spell = "難題「仏の御石の鉢　-砕けぬ意思-」";

    //     int line = height - (int)Math.Round(percentage * height);

    //     Console.SetCursorPosition(0, 1);
    //     for (int i = 0; i < 26; i++)
    //     {
    //         if (i == line)
    //             Logger.Write(spell, writeLine: true);
    //         else
    //             Logger.Write(' '.Multiply(spell.GetDisplayLength()), writeLine: true);
    //     }
    // }

    // private void UseSpell(double percentage)
    // {
    //     const int height = 15;
    //     const string spell = "恋符「マスタースパーク」";

    //     int line = (int)Math.Round(percentage * height);

    //     Console.SetCursorPosition(0, 10);
    //     for (int i = 1; i <= 26; i++)
    //     {
    //         Console.Write("\u001b[50C");

    //         if (i == line)
    //             Logger.Write(spell, writeLine: true);
    //         else
    //             Logger.Write(' '.Multiply(spell.GetDisplayLength()), writeLine: true);
    //     }
    // }

    // private void ShowBGM(double percentage)
    // {
    //     // japanese causes width issue
    //     // when we meet CJK characters, the whole character will be shown or disappear
    //     // this lead to the animation speed not constant
    //     // const string bgm = "BGM: 竹取飛翔　～ Lunatic Princess";
    //     // int wholeDisplayLength = bgm.GetDisplayLength();
    //     //
    //     // int displayChars = (int)Math.Round(percentage * wholeDisplayLength);
    //     // string displayString = "";
    //     // int currentDisplayLength = 0;
    //     //
    //     // for (int i = 0; i < bgm.Length; i++)
    //     // {
    //     //     int charDisplayLength = bgm[i] < 128 ? 1 : 2;
    //     //     if (currentDisplayLength + charDisplayLength > displayChars)
    //     //     {
    //     //         if (currentDisplayLength + 1 == displayChars)
    //     //             displayString += "\u2005";
    //     //
    //     //         break;
    //     //     }
    //     //
    //     //     displayString += bgm[i];
    //     //     currentDisplayLength += charDisplayLength;
    //     // }
    //     //
    //     // string line = ' '.Multiply(wholeDisplayLength - currentDisplayLength) + displayString

    //     // Since '\u2005' is not supported in every terminal
    //     const string bgm = "BGM: Taketori Hishou ~ Lunatic Princess";
    //     int fullDisplayLength = bgm.GetDisplayLength();

    //     int displayChars = (int)Math.Round(percentage * fullDisplayLength);
    //     string displayString = bgm[..displayChars];
    //     int displayLength = displayString.GetDisplayLength();

    //     string line = ' '.Multiply(fullDisplayLength - displayLength) + displayString;

    //     Console.SetCursorPosition(50, 1);
    //     Console.Write(line);
    // }

    // This method is called when the screen(or average object) is starting
    [Inject]
    private Sdl2Window _window = null!;
    protected override void Start()
    {
        Logger.Info("Keyboard is handled in this screen.");
        Logger.Info("You can exit the game by pressing ESC.");

        clock.Start();

        // Console.Clear();
        // Console.ResetColor();
        // foreach (var transformation in transformations)
        // {
        //     if (transformation.IsDone)
        //         transformation.Dispose();
        //     else
        //         transformation.Begin();
        // }

        // Logger.Info("Scheduled to exit in 5 seconds.");
        // Scheduler.ScheduleTaskDelay(() =>
        // {
        //     _host.RequestClose();

        //     Logger.Info("exiting");
        // }, 5);

        Debug.Assert(_host is not null, "Host is not injected");
        Debug.Assert(_window is not null, "Window is not injected");

        var _mainThread = _host.MainThread;
        var _updateThread = _host.UpdateThread;
        var _renderThread = _host.RenderThread;
        _debugWindow.OnUpdate += delegate
        {
            ImGui.Text("Hello, world!");
            // Imgui's framerate is actually our UpdateThread's framerate
            // We generate vertices in UpdateThread and cache them using TripleBuffer
            // Then we render the latest frame in RenderThread
            ImGui.Text(@"Framerate:");
            ImGui.Text($"    Main: {_mainThread.AverageFramerate:F2}({1000 / _mainThread.AverageFramerate:F2}±{_mainThread.Jitter:F2}ms)");
            ImGui.Text($"    Update: {_updateThread.AverageFramerate:F2}({1000 / _updateThread.AverageFramerate:F2}±{_updateThread.Jitter:F2}ms)");
            ImGui.Text($"    Render: {_renderThread.AverageFramerate:F2}({1000 / _renderThread.AverageFramerate:F2}±{_renderThread.Jitter:F2}ms)");

            if (ImGui.Button("Close Window"))
                _window.RequestClose();

            if (ImGui.Button("Exit Game"))
                _host.RequestClose();

            ImGui.Text(_string_with_cjk);
        };
        _debugWindow.Register();
        _demoWindow.Register();

#if DEBUG
        _allocViewer.OnUpdate += delegate
        {
            var records = ImguiUtils.AllocRecords;

            long allocated = 0;

            foreach (var (_, record) in records)
                allocated += record.Size;

            ImGui.Text($"Alloc count: {ImguiUtils.AllocCount}");
            ImGui.Text($"Free Count: {ImguiUtils.FreeCount}");
            ImGui.Text($"Realloc Count: {ImguiUtils.ReallocCount}");

            ImGui.Text($"{records.Count} records");
            ImGui.Text($"Total Allocated {allocated} bytes");

            ImGui.Separator();

            foreach (var (address, record) in records)
            {
                ImGui.Text($"Address {address:X}, CallSite: {record.Callsite}, Size {record.Size}");
            }
        };
        _allocViewer.Register();
#endif
    }

#if DEBUG
    private ImguiWindow _allocViewer = new ImguiWindow("Imgui Alloc Viewer");
#endif

    private const string _string_with_cjk = @"CJK character test
楽園の素敵な巫女
博麗霊夢（はくれいれいむ）
Reimu Hakurei

種族：人間
能力：主に空を飛ぶ程度の能力


毎度お馴染みの巫女さん。幻想郷の境にある博麗神社の巫女さん。
博麗神社自体は、幻想郷と人間界の両方に位置する。

何者に対しても平等に見る性格は、妖怪の様な普段畏れられている者
からも好かれる。逆にいうと、誰に対しても仲間として見ない。周り
に沢山人間や妖怪が居たり、一緒に行動を行っても、常に自分一人で
ある。実は冷たい人間なのかも知れない。

普通的黑魔术师
雾雨魔理沙
Marisa Kirisame

种族：人类
能力：使用魔法程度的能力

居住在幻想乡，有点普通的魔法使。有收集癖。
住在几乎不会有人类到访的魔法森林中，一边研究魔法，
一边自由地生活着。

虽然魔法使给人的印像总是室内（足不出户），但她却是经常出门。
魔理沙在研究魔法的时候，觉得没有人比较好，但除
此以外，她则是一个很喜欢热闹的人。
因为森林不会吸引人来，所以正好。不过，这并非是为了不
想让人见到研究中途的东西，才隐藏起来的（魔理沙语）
";

    private ImguiDemoWindow _demoWindow = new ImguiDemoWindow();
    private ImguiWindow _debugWindow = new ImguiWindow("Debug");

    // This method is called every frame for the screen(and it's children object)
    protected override void Update()
    {
        // foreach (var transformation in transformations)
        //     transformation.Update(clock.DeltaTime * 1000);
    }
}
