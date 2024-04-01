using System.Diagnostics;
using DanmakuEngine.DearImgui.Graphics;
using ImGuiNET;
using Moq;
using Moq.Protected;

public unsafe class TestImguiDrawDataBuffer
{
    [Test]
    public void TestConstructor()
    {
        var buffer = new ImguiDrawDataBuffer(10, 0);

        Assert.That(buffer.Capacity, Is.EqualTo(10));
        Assert.That(buffer.Count, Is.EqualTo(10));
        Assert.That(buffer.BufferIndex, Is.EqualTo(0));

        Assert.That((nint)buffer.DrawData.NativePtr, Is.Not.EqualTo(0));
        Assert.That((nint)buffer.Lists, Is.Not.EqualTo(0));

        buffer.Dispose();
    }

    [Test]
    public void TestForceGC_Collect()
    {
        var buffer = new ImguiDrawDataBuffer(20, 0);
        buffer.PreTakeSnapShot(10);
        Assert.That(buffer.Count, Is.EqualTo(10));

        buffer.Dispose();
    }

    [Test]
    public void TestForceGC_NoCollect()
    {
        var buffer = new ImguiDrawDataBuffer(20, 0);
        buffer.DoGC();
        Assert.That(buffer.Count, Is.EqualTo(20));

        buffer.Dispose();
    }

    [Test]
    public void TestPreTakeSnapShot_Enlarge()
    {
        var buffer = new ImguiDrawDataBuffer(10, 0);
        buffer.PreTakeSnapShot(20);
        Assert.That(buffer.Count, Is.EqualTo(20));
        Assert.That(buffer.Capacity, Is.EqualTo(30));
        buffer.Dispose();
    }

    [Test]
    public void TestPreTakeSnapShot_Collect()
    {
        var stub = new Mock<ImguiDrawDataBuffer>(20, 0);
        stub.Protected().Setup<bool>("QueuedForGC").Returns(true);

        var buffer = stub.Object;

        buffer.PreTakeSnapShot(9);

        Assert.That(buffer.Count, Is.EqualTo(9));
        Assert.That(buffer.Capacity, Is.EqualTo(9));

        buffer.Dispose();
    }

    [Test]
    public void TestPreTakeSnapShot_NotCollectTimeNotSatisified()
    {
        var stub = new Mock<ImguiDrawDataBuffer>(20, 0);
        stub.Protected().Setup<bool>("QueuedForGC").Returns(false);

        var buffer = stub.Object;

        buffer.PreTakeSnapShot(9);

        Assert.That(buffer.Count, Is.EqualTo(9));
        Assert.That(buffer.Capacity, Is.EqualTo(20));

        buffer.Dispose();
    }

    [Test]
    public void TestPreTakeSnapShot_NotCollectRateNotSatisfied()
    {
        var buffer = new ImguiDrawDataBuffer(20, 0);

        buffer.PreTakeSnapShot(10);

        Assert.That(buffer.Count, Is.EqualTo(10));
        Assert.That(buffer.Capacity, Is.EqualTo(20));

        buffer.Dispose();
    }

    [Test]
    public void TestRealloc_EnlargeNotOverrideWhenCountLessThanAllocated()
    {
        // This test is to ensure that the old llocated lists are not overridden
        // when the count is less than the allocated capacity
        // This used to cause memory leaks so i added this test to ensure that it doesn't happen again

        var buffer = new ImguiDrawDataBuffer(20, 0);

        buffer.PreTakeSnapShot(15); // shrink count while not triggering gc

        Debug.Assert(buffer.Count is 15);
        Debug.Assert(buffer.Capacity is 20);

        // Record addresses of the first 15 lists
        IList<nint> _15Lists = new nint[15];

        for (int i = 0; i < 15; i++)
            _15Lists[i] = (nint)buffer.Lists[i];

        buffer.PreTakeSnapShot(25); // enlarge count

        Debug.Assert(buffer.Count is 25);

        // Assert that our old lists are still there
        for (int i = 0; i < 15; i++)
            Assert.That((nint)buffer.Lists[i], Is.EqualTo(_15Lists[i]));
    }
}
