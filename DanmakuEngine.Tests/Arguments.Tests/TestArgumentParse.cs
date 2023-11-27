using DanmakuEngine.Arguments;
using NUnit.Framework;

namespace DanmakuEngine.Test.Arguments;

public class TestArgumentParse
{
    private readonly TestArgumentTemplate argTemplate = new();

    [SetUp]
    public void SetUp()
    {

    }

    [Test]
    public void TestInvalidParse()
    {
        try
        {
            using var argParser = new ArgumentParser(argTemplate, new string[] { "-int" }, false); // We dont want to print the usage on the screen as it may cause crashes
            using var argProvider = argParser.CreateArgumentProvider();

            Assert.Fail();
        }
        catch (Exception)
        {
            Assert.Pass();
        }

        try
        {
            using var argParser = new ArgumentParser(argTemplate, new string[] { "-unknown" }, false); // We dont want to print the usage on the screen as it may cause crashes
            using var argProvider = argParser.CreateArgumentProvider();

            Assert.Fail();
        }
        catch (Exception)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void TestParseInt()
    {
        using var argParser = new ArgumentParser(argTemplate, new string[] { "-int", "2" });
        using var argProvider = argParser.CreateArgumentProvider();

        Assert.That(argProvider.GetValue<int>("-int"), Is.EqualTo(2));
    }

    [Test]
    public void TestParseBoolean()
    {
        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-bool", "false" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<bool>("-bool"), Is.False);
        }

        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-bool", "False" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<bool>("-bool"), Is.False);
        }
    }

    [Test]
    public void TestParseDouble()
    {
        using var argParser = new ArgumentParser(argTemplate, new string[] { "-double", "2.0" });
        using var argProvider = argParser.CreateArgumentProvider();

        Assert.That(argProvider.GetValue<double>("-double"), Is.EqualTo(2.0d));
    }

    [Test]
    public void TestParseFloat()
    {
        using var argParser = new ArgumentParser(argTemplate, new string[] { "-float", "2.0" });
        using var argProvider = argParser.CreateArgumentProvider();

        Assert.That(argProvider.GetValue<float>("-float"), Is.EqualTo(2.0f));
    }

    [Test]
    public void TestParseString()
    {
        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-string", "\"/home/directory with spaces/\"" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<string>("-string"), Is.EqualTo("/home/directory with spaces/"));
        }
    }

    // [Test]
    // public void TestCustomParse()
    // {
    //     using (var argParser = new ArgumentParser(argTemplate, new string[] { "-string", "\"/\"" }))
    //     using (var argProvider = argParser.CreateArgumentProvider())
    //     {
    //         Assert.That(argProvider.GetValue<DirectoryInfo>("-directory").FullName, Is.EqualTo(new DirectoryInfo("/").FullName));
    //     }
    // }

    [Test]
    public void TestParseUnsupported()
    {
        try
        {
            using var argParser = new ArgumentParser(argTemplate, new string[] { "-unsupported", "wtf" });

            Assert.Fail();
        }
        catch (NotSupportedException)
        {
            Assert.Pass();
        }
    }

    private class TestArgumentTemplate : Paramaters
    {
        public Argument argument_int =
            new("-int", typeof(int), 1);

        public Argument argument_bool =
            new("-bool", typeof(bool), true);

        public Argument argument_float =
            new("-float", typeof(float), 1.0f);

        public Argument argument_double =
            new("-double", typeof(double), 1.0d);

        public Argument argument_string =
            new("-string", typeof(string), "str");

        public Argument argument_unsupported =
            new("-unsupported", typeof(UnsupportedType), new UnsupportedType());

        public TestArgumentTemplate()
        {
            Children = new()
            {
                { argument_int, "int" },
                { argument_bool, "bool" },
                { argument_float, "float" },
                { argument_double, "double" },
                { argument_string, "string" },
                { argument_unsupported, "unsupported" },
            };
        }
    }

    private class UnsupportedType
    {

    }
}