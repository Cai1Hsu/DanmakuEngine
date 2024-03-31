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
}
