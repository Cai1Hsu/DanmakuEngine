using System.Runtime.CompilerServices;
using DanmakuEngine.Allocations;
using DanmakuEngine.Allocations.ValueAccessors;
using DanmakuEngine.Bindables;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DanmakuEngine.Tests.Allocations;

public class TestAccessors
{
    private class TestClass
    {
        public int Value { get; set; }

        public int _value;

        public TestClass(int v1, int v2)
        {
            _value = v1;
            Value = v2;
        }
    }

    [Test]
    public void TestBindableAccessor()
    {
        var bindable = new Bindable<int>(42);
        var accessor = new BindableAccessor<int>(bindable);

        Assert.That(accessor.Value, Is.EqualTo(42));

        accessor.Value = 24;

        Assert.That(bindable.Value, Is.EqualTo(24));
    }

    [Test]
    public void TestDelegateAccessor()
    {
        var testClass = new TestClass(0, 0);
        var accessor = DelegateAccessor<int>.Create(() => testClass.Value, value => testClass.Value = value);

        Assert.That(accessor.Value, Is.EqualTo(0));

        accessor.Value = 42;

        Assert.That(testClass.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestRefAccessor_AccessFieldsThroughUnsafeAccessor()
    {
        var testClass = new TestClass(0, 0);
        var accessor = new RefAccessor<int>(() => ref ReadTestClassField(testClass));

        Assert.That(accessor.Value, Is.EqualTo(0));

        accessor.Value = 42;

        Assert.That(testClass._value, Is.EqualTo(42));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
    private static extern ref int ReadTestClassField(TestClass testClass);

    [Test]
    public void TestUnsafeAccessor_AccessPropertiesWithInstanceStored()
    {
        var testClass = new TestClass(0, 0);
        var accessor = DelegateAccessor<int>.Create(testClass, ReadTestClassProperty, WriteTestClassProperty);

        Assert.That(accessor.Value, Is.EqualTo(0));

        accessor.Value = 42;

        Assert.That(testClass.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestUnsafeAccessor_NotStoreInstance()
    {
        var testClass = new TestClass(0, 0);
        var accessor = DelegateAccessor<int>.Create(() => ReadTestClassProperty(testClass), v => WriteTestClassProperty(testClass, v));

        Assert.That(accessor.Value, Is.EqualTo(0));

        accessor.Value = 42;

        Assert.That(testClass.Value, Is.EqualTo(42));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Value")]
    private static extern int ReadTestClassProperty(TestClass testClass);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Value")]
    private static extern void WriteTestClassProperty(TestClass testClass, int value);

    [Test]
    public void TestEmitAccessor_AccessFields()
    {
        var testClass = new TestClass(0, 0);
        var accessor = new EmitAccessor<int, TestClass>(testClass, "_value");

        Assert.That(accessor.Value, Is.EqualTo(0));

        accessor.Value = 42;

        Assert.That(testClass._value, Is.EqualTo(42));
    }

    [Test]
    public void TestEmitAccessor_AccessProperties()
    {
        var testClass = new TestClass(0, 0);
        var accessor = new EmitAccessor<int, TestClass>(testClass, "Value");

        Assert.That(accessor.Value, Is.EqualTo(0));

        accessor.Value = 42;

        Assert.That(testClass.Value, Is.EqualTo(42));
    }

    [Test]
    public void TestEmitAccessor_ThrowsIfMemberNotFound()
    {
        var testClass = new TestClass(0, 0);

        Assert.That(() => new EmitAccessor<int, TestClass>(testClass, "NonExistent"), Throws.ArgumentException);
    }
}
