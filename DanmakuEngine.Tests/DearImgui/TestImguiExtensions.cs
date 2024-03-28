using System.Runtime.InteropServices;
using DanmakuEngine.DearImgui;
using ImGuiNET;

namespace DanmakuEngine.Tests.DearImgui;

public class TestImguiExtensions
{
    [Test]
    public void TestImVectorSwap()
    {
        ImVector lhs = new(1, 2, 3);
        ImVector rhs = new(4, 5, 6);

        unsafe
        {
            lhs.Swap(ref rhs);
        }

        Assert.That(lhs.Size, Is.EqualTo(4));
        Assert.That(lhs.Capacity, Is.EqualTo(5));
        Assert.That(lhs.Data, Is.EqualTo((nint)6));

        Assert.That(rhs.Size, Is.EqualTo(1));
        Assert.That(rhs.Capacity, Is.EqualTo(2));
        Assert.That(rhs.Data, Is.EqualTo((nint)3));
    }

    [Test]
    public void TestImVectorReserveLess()
    {
        var ram = Marshal.AllocHGlobal(8);
        ImVector lhs = new(4, 8, ram);

        unsafe
        {
            lhs.Reserve<int>(4);
        }

        try
        {
            Assert.That(lhs.Data, Is.EqualTo((nint)ram));
            Assert.That(lhs.Capacity, Is.EqualTo(8));
        }
        finally
        {
            Marshal.FreeHGlobal(ram);
        }
    }

    [Test]
    public void TestImVectorReserveMore()
    {
        var ram = Marshal.AllocHGlobal(4);
        ImVector lhs = new(1, 4, ram);

        unsafe
        {
            lhs.Reserve<int>(8);
        }

        try
        {
            Assert.That(lhs.Data, Is.Not.EqualTo((nint)ram));
            Assert.That(lhs.Capacity, Is.EqualTo(8));
        }
        finally
        {
            Marshal.FreeHGlobal(lhs.Data);
        }
    }
}
