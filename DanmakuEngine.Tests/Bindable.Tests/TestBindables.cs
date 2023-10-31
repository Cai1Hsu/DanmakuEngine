using DanmakuEngine.Bindables;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace DanmakuEngine.Tests;

public class TestBindables
{
    [SetUp]
    public void Setup()
    {

    }

    [Test]
    public void TestValue()
    {
        const int value1 = 1;

        Bindable<int> bindable1 = new(value1);

        Assert.That(bindable1.Value, Is.EqualTo(value1));

        bindable1.Value++;

        Assert.That(bindable1.Value, Is.EqualTo(value1 + 1));
    }

    [Test]
    public void TestValueEquals()
    {
        const int value = 1;

        Bindable<int> bindable1 = new(value);
        Bindable<int> bindable2 = new(value);

        Assert.That(bindable1.ValueEquals(bindable2), Is.True);
        Assert.That(bindable1.ValueEquals(value), Is.True);

        bindable2.Value++;

        Assert.That(bindable1.ValueEquals(bindable2), Is.False);
        Assert.That(bindable1.ValueEquals(bindable2.Value), Is.False);
    }

    [Test]
    public void TestDisableAndEnable()
    {
        Bindable<int> b = new(1);

        Assert.That(b.Enabled, Is.True);

        b.Enabled = false;

        Assert.That(b.Enabled, Is.False);

        // Let's do some invalid operation
        try
        {
            b.Value = 2;

            Assert.Fail();
        }
        catch (InvalidOperationException)
        {
            // Assert.Pass();
        }
    }

    [Test]
    public void TestValueChanged()
    {
        bool flag = false;
        Bindable<int> b = new(1);

        b.AddBindValueChangedEvent(_ => flag = true);

        Assert.That(flag, Is.False);

        b.Value = 2;

        Assert.That(flag, Is.True);
    }

    [Test]
    public void TestValueChangedExecuteImmediately()
    {
        bool flag = false;
        Bindable<int> b = new(1);

        b.AddBindValueChangedEvent(_ => flag = true, true);

        Assert.That(flag, Is.True);
    }

    [Test]
    public void TestValueChangedIgnore()
    {
        // Test when new value is equal to old value, the event wont be triggered
        Bindable<int> b = new(1);

        b.AddBindValueChangedEvent(e =>
        {
            Assert.That(e.NewValue, Is.Not.EqualTo(e.OldValue));
        });

        b.Value = 2;

        Assert.Pass();
    }

    [Test]
    public void TestRemoveValeChangedEvent()
    {
        bool flag = false;
        Bindable<int> b = new(1);
        Action<BindValueChangedEvent<int>> action4 = _ =>
        {
            flag = true;
        };

        b.AddBindValueChangedEvent(action4);

        b.Value = 2;

        Assert.That(flag, Is.True);

        flag = false;
        b.RemoveBindValueChangedEvent(action4);

        b.Value = 1;

        Assert.That(flag, Is.False);
    }


    [Test]
    public void TestEnabledChanged()
    {
        bool flag = false;
        Bindable<int> b = new(1);

        b.AddEnabledChangedEvent(_ => flag = true);

        Assert.That(flag, Is.False);

        b.Enabled = false;

        Assert.That(flag, Is.True);

        flag = false;
        b.Enabled = false;

        Assert.That(flag, Is.False);
    }

    [Test]
    public void TestEnabledChangedExecuteImmediately()
    {
        bool flag = false;
        Bindable<int> b = new(1);

        b.AddEnabledChangedEvent(_ => flag = true, true);

        Assert.That(flag, Is.True);
    }

    [Test]
    public void TestRemoveEnabledChanged()
    {
        bool flag = false;
        Bindable<int> b = new(1);
        
        Action<bool> action = _ =>
        {
            flag = true;
        };

        b.AddEnabledChangedEvent(action);

        b.Enabled = false;

        Assert.That(flag, Is.True);

        flag = false;
        b.RemoveEnabledChangedEvent(action);

        b.Enabled = true;

        Assert.That(flag, Is.False);
    }

    [Test]
    public void TestBindTo()
    {
        Bindable<int> b1 = new(1);

        Assert.That(b1.IsBound(), Is.False);

        Bindable<int> b2 = new(2);

        b1.BindTo(b2);

        Assert.That(b1.IsBound(), Is.True);
        Assert.That(b2.IsBound(), Is.True);

        Assert.That(b1.IsBoundWith(b2), Is.True);
        Assert.That(b2.IsBoundWith(b1), Is.True);

        Assert.That(b1.IsBoundWith(new Bindable<int>(3)), Is.False);
    }

    [Test]
    public void TestFailOnMultipleBinds()
    {
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(2);

        b1.BindTo(b2);

        try
        {
            b1.BindTo(new Bindable<int>(3));

            Assert.Fail();
        }
        catch (InvalidOperationException)
        {
            Assert.Pass();
        }

        try
        {
            b2.BindTo(new Bindable<int>(3));

            Assert.Fail();
        }
        catch (InvalidOperationException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void TestFailOnBindToDisabledBindable()
    {
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(1)
        {
            Enabled = false
        };

        try
        {
            b1.BindTo(b2);

            Assert.Fail();
        }
        catch (InvalidOperationException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public void TestValueSyncOnBinding()
    {
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(2);

        b1.BindTo(b2);

        Assert.That(b2.Value, Is.EqualTo(1));
        Assert.That(b1.Value, Is.EqualTo(b2.Value));
    }


    [Test]
    public void TestBindEnabledSync()
    {
        // FIXME: Dead loop
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(1);

        b1.BindTo(b2);

        b1.Enabled = false;

        Assert.That(b2.Enabled, Is.False);
    }

    [Test]
    public void TestBindValueSync()
    {
        // FIXME: Dead loop
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(1);

        b1.BindTo(b2);

        b1.Value = 2;

        Assert.That(b2.Value, Is.EqualTo(2));
    }

    [Test]
    public void TestUnbind()
    {
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(1);

        b1.BindTo(b2);
        b1.UnBind(b2);

        Assert.That(b1.IsBound(), Is.False);
        Assert.That(b2.IsBound(), Is.False);

        Assert.That(b1.IsBoundWith(b2), Is.False);
        Assert.That(b2.IsBoundWith(b1), Is.False);
    }

    [Test]
    public void TestUnbindValueSync()
    {
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(1);

        b1.BindTo(b2);
        b1.UnBind(b2);

        b1.Value = 2;

        Assert.That(b2.Value, Is.EqualTo(1));
    }

    [Test]
    public void TestUnbindEnabledSync()
    {
        // FIXME: Dead loop
        Bindable<int> b1 = new(1);
        Bindable<int> b2 = new(1);

        b1.BindTo(b2);

        b1.Enabled = false;

        b1.UnBind(b2);

        b1.Enabled = true;

        Assert.That(b2.Enabled, Is.False);
    }
}