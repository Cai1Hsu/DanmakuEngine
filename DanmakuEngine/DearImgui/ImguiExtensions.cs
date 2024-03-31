using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Logging;
using ImGuiNET;

namespace DanmakuEngine.DearImgui;

public static unsafe class ImguiExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap(this ref ImVector lhs, ref ImVector rhs)
    {
        var _lhs = (ImguiVector*)Unsafe.AsPointer(ref lhs);
        var _rhs = (ImguiVector*)Unsafe.AsPointer(ref rhs);

        int rhs_size = _rhs->Size;
        _rhs->Size = _lhs->Size;
        _lhs->Size = rhs_size;

        int rhs_capacity = _rhs->Capacity;
        _rhs->Capacity = _lhs->Capacity;
        _lhs->Capacity = rhs_capacity;

        nint rhs_data = _rhs->Data;
        _rhs->Data = _lhs->Data;
        _lhs->Data = rhs_data;
    }
}

