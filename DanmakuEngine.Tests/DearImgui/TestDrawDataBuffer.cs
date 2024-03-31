using DanmakuEngine.DearImgui.Graphics;
using ImGuiNET;

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
        var buffer = new ImguiDrawDataBuffer(20, 0);

        // Wait for GC timer
        Thread.Sleep(1050);

        buffer.PreTakeSnapShot(9);

        Assert.That(buffer.Count, Is.EqualTo(9));
        Assert.That(buffer.Capacity, Is.EqualTo(9));

        buffer.Dispose();
    }

    [Test]
    public void TestPreTakeSnapShot_NotCollect()
    {
        var buffer = new ImguiDrawDataBuffer(20, 0);

        buffer.PreTakeSnapShot(10);

        Assert.That(buffer.Count, Is.EqualTo(10));
        Assert.That(buffer.Capacity, Is.EqualTo(20));

        buffer.Dispose();
    }
}
