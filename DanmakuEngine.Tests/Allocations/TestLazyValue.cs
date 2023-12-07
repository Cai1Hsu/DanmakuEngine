using DanmakuEngine.Allocations;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Allocations;

public class TestLazyValue
{
    private class TestClass(int value)
    {
        public int Value { get; } = value;
    }

    [Test]
    public void TestNoParameterConstructor()
    {
        var lazy = new LazyValue<TestClass>(() => new(42));

        Assert.That(lazy.Value, Is.Not.Null);

        Assert.That(lazy.Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestParameterConstructor()
    {
        var lazy = new LazyValue<TestClass>(() => new(42));

        Assert.That(lazy.Value, Is.Not.Null);

        Assert.That(lazy.Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestValueCreatesObject()
    {
        var lazy = new LazyValue<TestClass>(() => new(42));
        var value = lazy.Value;

        Assert.That(value, Is.Not.Null);

        Assert.That(value.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestValueReturnsSameObject()
    {
        var lazy = new LazyValue<TestClass>(() => new(42));
        var value1 = lazy.Value;
        var value2 = lazy.Value;

        Assert.That(value2, Is.SameAs(value1));
    }

    [Test]
    public void ValueCreatesObjectOnFirstAccess()
    {
        var lazyValue = new LazyValue<string>(() => "Hello, world!");

        Assert.That(lazyValue.RawValue, Is.Null);
        Assert.That(lazyValue.Value, Is.EqualTo("Hello, world!"));
    }

    [Test]
    public void HasValueReturnsFalseBeforeValueIsAccessed()
    {
        var lazyValue = new LazyValue<string>(() => "Hello, world!");

        Assert.That(lazyValue.HasValue, Is.False);
    }

    [Test]
    public void HasValueReturnsTrueAfterValueIsAccessed()
    {
        var lazyValue = new LazyValue<string>(() => "Hello, world!");
        var _ = lazyValue.Value;

        Assert.That(lazyValue.HasValue, Is.True);
    }

    [Test]
    public void TestAssignsValue()
    {
        var lazyValue = new LazyValue<string>(() => "Hello, world!");

        lazyValue.AssignValue("Goodbye, world!");

        Assert.That(lazyValue.Value, Is.EqualTo("Goodbye, world!"));
    }

    [Test]
    public void TestResetsValueAndLoader()
    {
        var lazyValue = new LazyValue<string>(() => "Hello, world!");
        var _ = lazyValue.Value;

        lazyValue.Reset(() => "Goodbye, world!");

        Assert.That(lazyValue.HasValue, Is.False);

        Assert.That(lazyValue.Value, Is.EqualTo("Goodbye, world!"));
    }

    [Test]
    public void TestExplicitConvertFromTValue()
    {
        var value = @"Hello, world";

        var lazyValue = (LazyValue<string>)value;

        Assert.That(lazyValue.HasValue, Is.True);

        Assert.That(lazyValue.Value, Is.EqualTo(value));
    }

    [Test]
    public void TestImplicitConvertToTValue()
    {
        var lazyValue = new LazyValue<TestClass>(() => new(42));

        Assert.That(lazyValue.RawValue, Is.Null);

        TestClass value = lazyValue;

        Assert.That(lazyValue.Value, Is.EqualTo(value));

        Assert.That(lazyValue.Value, Is.SameAs(value));
    }

    [Test]
    public void TestImplictCreateWithLoader()
    {
        var loader = () => new TestClass(42);

        LazyValue<TestClass> lazyValue = loader;

        Assert.That(lazyValue.RawValue, Is.Null);
        Assert.That(lazyValue.Value.Value, Is.EqualTo(42));
    }
}