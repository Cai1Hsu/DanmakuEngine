using DanmakuEngine.Arguments;
using NUnit.Framework;

namespace DanmakuEngine.Tests;

public class TestArguments
{
    private ArgumentTemplate argTemplate;

    [SetUp]
    public void Setup()
    {
        argTemplate = new();
    }

    [Test]
    public void TestArgumentsSupport()
    {
        using (var argParser = new ArgumentParser(argTemplate, Array.Empty<string>()))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argParser.IsSupport("-help"), Is.True);

            Assert.That(argParser.IsSupport("-unknown-flag"), Is.False);
        }

        Assert.Pass();
    }

    [Test]
    public void TestGetDefault()
    {
        using (var argParser = new ArgumentParser(argTemplate, Array.Empty<string>()))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argParser.GetDefault<int>("-refresh"), Is.EqualTo(argTemplate.RefreshRate.GetValue<int>()));

            try
            {
                argParser.GetDefault<double>("-refresh");

                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.Pass();
            }

            try
            {
                argParser.GetDefault<object>("-help");

                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.Pass();
            }

            try
            {
                argParser.GetDefault<object>("-unknown");

                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
                Assert.Pass();
            }
        }

        Assert.Pass();
    }

    [Test]
    public void TestArgumentParse()
    {
        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-refresh", "120" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<int>("-refresh"), Is.EqualTo(120));
        }

        try
        {
            using (var argParser = new ArgumentParser(argTemplate, new string[] { "-refresh" }, false)) // We dont want to print the usage on the screen as it may cause crashes
            using (var argProvider = argParser.CreateArgumentProvider())
            {
                Assert.Fail();
            }
        }
        catch (Exception)
        {
            Assert.Pass();
        }

        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-refresh" }, false)) // We dont want to print the usage on the screen as it may cause crashes
        {
            var fieldsInfo = argTemplate.GetType().GetFields();

            Assert.That(fieldsInfo.All(f =>
            {
                var field = f.GetValue(argTemplate);

                if (field is not Argument arg)
                    return true;

                return argParser.GenerateHelp()
                                .Any(s => s.Contains(arg.Key));
            }), Is.True);
        }

        Assert.Pass();
    }


    [Test]
    public void TestArgumentProviderFind()
    {
        // FIXME
        // pass "-help" crashed host process??????
        // Seems caused by the operation of "-help"
        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-help" }, false/*remove this will lead to hotst crash*/))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.Find("-help"), Is.True);

            Assert.That(argProvider.Find("-refresh"), Is.False);
        }

        Assert.Pass();
    }

    [Test]
    public void TestArgumentGetValue()
    {
        using (var argParser = new ArgumentParser(argTemplate, new string[] { "-refresh", "120" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<int>("-refresh"), Is.EqualTo(120));
        }

        using (var argParser = new ArgumentParser(argTemplate, Array.Empty<string>()))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<int>("-refresh"), Is.EqualTo(argTemplate.RefreshRate.GetValue<int>()));
        }

        Assert.Pass();
    }
}