using System.Collections.Concurrent;

namespace DanmakuEngine.DearImgui;

#if DEBUG
public static partial class ImguiUtils
{
    private static long allcatedCount = 0;
    private static long freedCount = 0;
    private static long reallocCount = 0;

    public static long AllocCount => allcatedCount;
    public static long FreeCount => freedCount - reallocCount;
    public static long ReallocCount => reallocCount;

    public enum AllocCallsite
    {
        Realloc,
        Alloc,
    }

    public class AllocRecord(nint address, uint size, AllocCallsite callsite)
    {
        public readonly nint Address = address;
        public readonly uint Size = size;
        public readonly AllocCallsite Callsite = callsite;
    }

    public static ConcurrentDictionary<nint, AllocRecord> AllocRecords = new();
    public static ConcurrentDictionary<nint, string> ExactCallsites = new();

    private static void AddRecord(AllocRecord record)
        => AllocRecords.TryAdd(record.Address, record);

    private static void RemoveRecord(nint address)
        => AllocRecords.TryRemove(address, out _);
}
#endif
