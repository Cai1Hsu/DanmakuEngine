using DanmakuEngine.Allocations;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Allocations;

public class TestLazyLoadValue
{
    private class TestClass(int value)
    {
        public int Value { get; } = value;
    }

    [Test]
    public void TestNoParameterConstructor()
    {
        var lazy = new LazyLoadValue<TestClass>(42);

        Assert.That(lazy.Value, Is.Not.Null);

        Assert.That(lazy.Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestParameterConstructor()
    {
        var lazy = new LazyLoadValue<TestClass>(42);

        Assert.That(lazy.Value, Is.Not.Null);

        Assert.That(lazy.Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestValueCreatesObject()
    {
        var lazy = new LazyLoadValue<TestClass>(42);
        var value = lazy.Value;

        Assert.That(value, Is.Not.Null);

        Assert.That(value.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestValueReturnsSameObject()
    {
        var lazy = new LazyLoadValue<TestClass>(42);
        var value1 = lazy.Value;
        var value2 = lazy.Value;

        Assert.That(value2, Is.SameAs(value1));
    }
}