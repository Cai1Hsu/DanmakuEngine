using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DanmakuEngine.Logging;

namespace DanmakuEngine.Arguments;

public class ArgumentParser : IDisposable
{
    private readonly string[] args;

    private Dictionary<string, Argument> avaliableArguments = new();

    private readonly List<Argument> arguments = new();

    private readonly bool executionAction;

    private readonly Paramaters argumentTemplate;

    public ArgumentParser(Paramaters template, string[] args = null!, bool executeAction = true)
    {
        this.args = args ?? Environment.GetCommandLineArgs();

        this.argumentTemplate = template;

        this.executionAction = executeAction;

        LoadTemplateArguments();

        Parse();
    }

    private void LoadTemplateArguments()
    {
        foreach (var arg in argumentTemplate.Children.Keys)
        {
            avaliableArguments.Add(arg.Key, arg);
        }
    }

    private void Parse()
    {
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (!IsSupport(arg))
            {
                // TODO: We should let the user know that they passed an unknown argument.
                // However, it's difficult to handle the first argument, since it's usually the executable file
                // And it's difficult to check wheter the file is the program(or assembly) itself or a valid file passed to the program
                // since a simple string comparison is not always reliable.
                if (i == 0 && File.Exists(arg))
                    continue;

                PrintHelp();

                throw new NotSupportedException($"Unrecognized argument: {arg}");
            }

            if (!avaliableArguments[arg].HasValue)
            {
                StoreArgument(new Argument(arg));
                continue;
            }

            // The user passed the key, but didn't provide the value
            if (i == args.Length - 1 || args[i + 1].StartsWith('-'))
            {
                var missingValueException = new Exception($"Missing value for argument: {arg}");

                if (!executionAction)
                    throw missingValueException;

                var usages = GetUsage(arg);
                usages.ForEach(s => Logger.Write(s, true, true));

                throw missingValueException;
            }

            Type valueType = avaliableArguments[arg].TValue;

            object value = args[++i];

            var argument = new Argument(arg, valueType, value);
            StoreArgument(argument);
        }
    }

    private void StoreArgument(Argument arg)
    {
        arguments.Add(arg);

        var template = avaliableArguments[arg.Key];

        if (template.Operation != null && executionAction)
            template.Operation(arg);
    }

    public TValue GetDefault<TValue>(string key) => (TValue)GetDefault(key);

    public object GetDefault(string key)
    {
        if (!IsSupport(key))
            throw new Exception($"Target argument is not supported: {key}");

        var arg = avaliableArguments[key];

        if (!arg.HasValue)
            throw new Exception($"Target argument doesn't contain a value: {key}");

        return arg.GetValue();
    }

    private const string indent = "    ";

    private static readonly List<string> help_template = new(new[] { "A STG game with magnificent *Danmaku* built with .NET",
                                           "",
                                           "Usage: ./<game> [arguments]",
                                           "",
                                           "To get help, type: ./<game> -help or ./<game> -h",
                                           ""});

    public List<string> GenerateHelp()
    {
        var helps = new List<string>(help_template);

        foreach (var arg in argumentTemplate.Children.Keys)
        {
            string keyInfo = arg.Key;

            if (arg.HasValue)
                keyInfo += $" <value:{arg.TValue}>";

            keyInfo += ":";

            helps.Add(keyInfo);

            string description = indent + argumentTemplate.Children[arg];

            helps.Add(description);

            string example = indent + "example: ./<game> " + arg.Key;

            if (arg.HasValue)
                example += $" {arg.ToString()}";

            helps.Add(example);

            helps.Add("");
        }

        return helps;
    }

    public List<string> GetUsage(string flag)
    {
        var usages = new List<string>(help_template);

        if (!IsSupport(flag))
        {
            usages.Add($"Target flag is NOT supported: {flag}");

            return usages;
        }

        bool found = false;

        foreach (var arg in argumentTemplate.Children.Keys)
        {
            if (arg.Key != flag)
                continue;

            string keyInfo = arg.Key;

            if (arg.HasValue)
                keyInfo += $" <value:{arg.TValue}>";

            keyInfo += ":";

            usages.Add(keyInfo);

            string description = indent + argumentTemplate.Children[arg];

            usages.Add(description);

            usages.AddRange(new[] { "", "Example:" });

            string example = indent + "./<game> " + arg.Key;

            if (arg.HasValue)
                example += $" {arg.ToString()}";

            usages.Add(example);

            usages.Add("");

            found = true;
        }

        Debug.Assert(found);

        return usages;
    }

    public void PrintHelp()
    {
        var helps = GenerateHelp();

        helps.ForEach(s => Logger.Write(s, true, true));
    }

    public ArgumentProvider CreateProvider()
        => new(this, arguments.ToDictionary(arg => arg.Key, arg => arg));

    public bool IsSupport(string key) => avaliableArguments.ContainsKey(key);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        avaliableArguments = null!;
    }
}
