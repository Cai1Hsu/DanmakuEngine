using DanmakuEngine.Extensions;
using DanmakuEngine.Utils;

namespace DanmakuEngine.Tests.Utils;

#pragma warning disable NUnit2005

public class TestRNG
{
    // Believe it or not, this is already a very conservative value
    private const double accepted_percentage = 0.005;
    private const int loop_count = 1000;
    private Random NET_RNG;

    [SetUp]
    public void SetUp()
    {
        NET_RNG = new Random();
    }

    [TearDown]
    public void TearDown()
    {
    }

    [Test]
    public void TestWithNonParamCtor()
    {
        var rng = new RNG();

        Assert.That(new RNG().GetHash(), Is.Not.EqualTo(rng.GetHashCode()));

        testConsitentBehaviour(rng, rng.Copy());

        testNotConsitentBehaviour(rng, new RNG());
    }

    [Test]
    public void TestWithSeed()
    {
        const int rand = 42;

        var rng = new RNG(rand);

        testConsitentBehaviour(rng, new RNG(rand));
        testNotConsitentBehaviour(rng, new RNG(-rand));
    }

    [Test]
    public void TestWithSeedAndWorker()
    {
        const int rand = 42;
        string name = "Reimu";

        var rng = new RNG(rand, name);

        testConsitentBehaviour(rng, new RNG(rand, name));

        testNotConsitentBehaviour(rng, new RNG(-rand, name));
        testNotConsitentBehaviour(rng, new RNG(rand, "Marisa"));
    }

    private void testNotConsitentBehaviour(RNG rng1, RNG rng2)
    {
        int same = 0;

        for (int i = 0; i < 100_000; i++)
            same += rng1.Next() == rng2.Next() ? 1 : 0;

        Assert.That(same, Is.LessThan(5));

    }

    private void testConsitentBehaviour(RNG rng1, RNG rng2)
    {
        for (int i = 0; i < loop_count; i++)
        {
            testConsitentInt(rng1, rng2);
            testConsitentLong(rng1, rng2);
            testConsitentFloat(rng1, rng2);
            testConsitentDouble(rng1, rng2);
        }

        for (int i = 0; i < loop_count; i++)
            testRadomOperation(rng1, rng2);

        // Since all tests above has passed, we can assume that rng1 and rng2 share the same behaviour.
        testFrequency(rng1);
    }

    private void testConsitentInt(RNG rng1, RNG rng2)
    {
        Assert.That(rng1.Next(), Is.EqualTo(rng2.Next()));
    }

    private void testConsitentLong(RNG rng1, RNG rng2)
    {
        Assert.That(rng1.NextLong(), Is.EqualTo(rng2.NextLong()));
    }

    private void testConsitentFloat(RNG rng1, RNG rng2)
    {
        Assert.That(rng1.NextSingle(), Is.EqualTo(rng2.NextSingle()));
    }

    private void testConsitentDouble(RNG rng1, RNG rng2)
    {
        Assert.That(rng1.NextDouble(), Is.EqualTo(rng2.NextDouble()));
    }

    private void testRadomOperation(RNG rng1, RNG rng2)
    {
        int op = NET_RNG.Next(0, 4000_000) / 1000_000;

        switch (op)
        {
            case 0:
            {
                testConsitentInt(rng1, rng2);
            }
            break;

            case 1:
            {
                testConsitentLong(rng1, rng2);
            }
            break;

            case 2:
            {
                testConsitentFloat(rng1, rng2);
            }
            break;

            case 3:
            {
                testConsitentDouble(rng1, rng2);
            }
            break;
        }
    }

    private void testFrequency(RNG rng)
    {
        const int count = 1_000_000;
        const int max_value = 10;

        int[] ints = new int[max_value];

        for (int i = 0; i < count; i++)
        {
            ints[rng.Next(max_value)]++;
        }

        double avg = ints.Average();

        double max_distance = ints.Select(i => Math.Abs(i - avg)).Max();

        double max_percentage = max_distance / count;

        Assert.That(max_percentage, Is.LessThan(accepted_percentage));
    }
}

#pragma warning restore NUnit2005
