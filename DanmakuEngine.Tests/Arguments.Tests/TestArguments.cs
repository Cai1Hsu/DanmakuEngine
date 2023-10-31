using DanmakuEngine.Arguments;
using NUnit.Framework;

namespace DanmakuEngine.Tests;

public class TestArguments
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void TestArgumentsSupport()
    {
        using (var argParser = new ArgumentParser(Array.Empty<string>()))
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
        using (var argParser = new ArgumentParser(Array.Empty<string>()))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argParser.GetDefault<int>("-refresh"), Is.EqualTo(ArgumentTemplate.RefreshRate.GetValue<int>()));

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
        using (var argParser = new ArgumentParser(new string[] { "-refresh", "120" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<int>("-refresh"), Is.EqualTo(120));
        }

        try
        {
            using (var argParser = new ArgumentParser(new string[] { "-refresh" }, false)) // We dont want to print the usage on the screen as it may cause crashes
            using (var argProvider = argParser.CreateArgumentProvider())
            {
                Assert.Fail();
            }
        }
        catch (Exception)
        {
            Assert.Pass();
        }


        Assert.Pass();
    }


    [Test]
    public void TestArgumentProviderFind()
    {
        // pass "-help" crashed host process??????
        // Seems caused by the operation of "-help"
        using (var argParser = new ArgumentParser(new string[] { "-help" }, false))
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
        using (var argParser = new ArgumentParser(new string[] { "-refresh", "120" }))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<int>("-refresh"), Is.EqualTo(120));
        }

        using (var argParser = new ArgumentParser(Array.Empty<string>()))
        using (var argProvider = argParser.CreateArgumentProvider())
        {
            Assert.That(argProvider.GetValue<int>("-refresh"), Is.EqualTo(ArgumentTemplate.RefreshRate.GetValue<int>()));
        }

        Assert.Pass();
    }
}