using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace DanmakuEngine.DearImgui;

public static unsafe class ImguiExtensions
{
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Swap(this ref ImVector lhs, ref ImVector rhs)
    {
        fixed (void* p_lhs = &lhs)
        fixed (void* p_rhs = &rhs)
        {
            ImguiVector* _lhs = (ImguiVector*)p_lhs;
            ImguiVector* _rhs = (ImguiVector*)p_rhs;

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

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reserve<T>(this ref ImVector lhs, int new_capacity)
        where T : struct
    {
        int sizeofT = Marshal.SizeOf<T>();

        Reserve(ref lhs, new_capacity, sizeofT);
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reserve(this ref ImVector lhs, int new_capacity, int sizeofT)
    {
        Debug.Assert(new_capacity >= 0, $"new_capacity: {new_capacity}");

        fixed (void* p_lhs = &lhs)
        {
            if (new_capacity <= lhs.Capacity)
                return;

            nint new_data = Marshal.AllocHGlobal(new_capacity * sizeofT);

            if (lhs.Data != 0)
            {
                long size = lhs.Size * sizeofT;
                // memcpy(new_data, Data, (size_t)Size * sizeof(T));
                Buffer.MemoryCopy((void*)lhs.Data, (void*)new_data, size, size);
                // IM_FREE(Data);
                Marshal.FreeHGlobal(lhs.Data);
            }

            ImguiVector* _lhs = (ImguiVector*)p_lhs;
            // lhs->Data = new_data;
            _lhs->Data = new_data;
            // lhs->Capacity = new_capacity;
            _lhs->Capacity = new_capacity;
        }
    }
}

