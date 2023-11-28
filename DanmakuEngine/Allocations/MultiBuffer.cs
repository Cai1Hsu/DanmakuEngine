// from osu.Framework

using System.Diagnostics;

namespace DanmakuEngine.Allocations;

public abstract partial class MultiBuffer<T>
{
    // This should never happen if we write correct code.
    const string exception = "You must implement bufferCount and buffers in your own class";

    protected virtual int bufferCount
        => throw new NotImplementedException(exception);

    protected virtual UsageValue<T>[] buffers
        => throw new NotImplementedException(exception);

    /// <summary>
    /// The freshest buffer index which has finished a write, and is waiting to be read.
    /// Will be set to <c>null</c> after being read once.
    /// </summary>
    private int pendingCompletedWriteIndex = -1;

    /// <summary>
    /// The last buffer index which was obtained for writing.
    /// </summary>
    private int lastWriteIndex = -1;

    /// <summary>
    /// The last buffer index which was obtained for reading.
    /// Note that this will remain "active" even after a <see cref="GetForRead"/> ends, to give benefit of doubt that the usage may still be accessing it.
    /// </summary>
    private int lastReadIndex = -1;

    private readonly ManualResetEventSlim writeCompletedEvent = new ManualResetEventSlim();

    public MultiBuffer()
    {
        // Try to fetch bufferCount
        // to check whether the user implemented bufferCount and buffers
        Debug.Assert(bufferCount > 0);
        Debug.Assert(buffers.Length == bufferCount);

        for (int i = 0; i < bufferCount; i++)
        {
            buffers[i] = new UsageValue<T>(i, finishUsage);
        }
    }

    public UsageValue<T> GetForWrite()
    {
        // Only one write should be allowed at once
        Debug.Assert(buffers.All(b => b.UsingType != UsingType.Writing));

        UsageValue<T> buffer = getNextWriteBuffer();

        return buffer;
    }

    public UsageValue<T>? GetForRead()
    {
        // Only one read should be allowed at once
        Debug.Assert(buffers.All(b => b.UsingType != UsingType.Reading));

        writeCompletedEvent.Reset();

        var buffer = getPendingReadBuffer();

        if (buffer != null)
            return buffer;

        // A completed write wasn't available, so wait for the next to complete.
        if (!writeCompletedEvent.Wait(100))
            // Generally shouldn't happen, but this avoids spinning forever.
            return null;

        return GetForRead();
    }

    private UsageValue<T>? getPendingReadBuffer()
    {
        // Avoid locking to see if there's a pending write.
        int pendingWrite = Interlocked.Exchange(ref pendingCompletedWriteIndex, -1);

        if (pendingWrite == -1)
            return null;

        lock (buffers)
        {
            var buffer = buffers[pendingWrite];

            Debug.Assert(lastReadIndex != buffer.Index);
            lastReadIndex = buffer.Index;

            Debug.Assert(buffer.UsingType == UsingType.Avaliable);
            buffer.UsingType = UsingType.Reading;
            return buffer;
        }
    }

    private UsageValue<T> getNextWriteBuffer()
    {
        lock (buffers)
        {
            for (int i = 0; i < bufferCount; i++)
            {
                // Never write to the last read index.
                // We assume there could be some reads still occurring even after the usage is finished.
                if (i == lastReadIndex) continue;

                // Never write to the same buffer twice in a row.
                // This would defeat the purpose of having a triple buffer.
                if (i == lastWriteIndex) continue;

                lastWriteIndex = i;

                var buffer = buffers[i];

                Debug.Assert(buffer.UsingType == UsingType.Avaliable);
                buffer.UsingType = UsingType.Writing;

                return buffer;
            }
        }

        throw new InvalidOperationException("No buffer could be obtained. This should never ever happen.");
    }

    private void finishUsage(UsageValue<T> obj)
    {
        // This implementation is intentionally written this way to avoid requiring locking overhead.
        bool wasWrite = obj.UsingType == UsingType.Writing;

        obj.UsingType = UsingType.Avaliable;

        if (wasWrite)
        {
            Debug.Assert(pendingCompletedWriteIndex != obj.Index);
            Interlocked.Exchange(ref pendingCompletedWriteIndex, obj.Index);

            writeCompletedEvent.Set();
        }
    }
}