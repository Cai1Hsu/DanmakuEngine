// from osu.Framework.Tests

using DanmakuEngine.Allocations;
using NUnit.Framework;

namespace DanmakuEngine.Tests.Allocations;

public class TestMultiBuffer
{
    [SetUp]
    public void SetUp()
    {

    }

    [Test]
    public void TestWriteOnly()
    {
        var doubleBuffer = new DoubleBuffer<int>();

        for (int i = 0; i < 1000; i++)
        {
            using (doubleBuffer.GetForWrite())
            {
            }
        }

        var tripleBuffer = new TripleBuffer<int>();

        for (int i = 0; i < 1000; i++)
        {
            using (tripleBuffer.GetForWrite())
            {
            }
        }
    }

    [Test]
    public void TestReadOnly()
    {
        var doubleBuffer = new DoubleBuffer<TestObject>();

        using (var obj = doubleBuffer.GetForRead())
            Assert.That(obj, Is.Null);

        var tripleBuffer = new TripleBuffer<TestObject>();

        using (var obj = tripleBuffer.GetForRead())
            Assert.That(obj, Is.Null);
    }

    [Test]
    public void TestDoubleBuffer()
    {
        var doubleBuffer = new DoubleBuffer<int>();

        using (var obj1 = doubleBuffer.GetForWrite())
            obj1.Value = 1;

        using (var obj1 = doubleBuffer.GetForRead())
            Assert.That(obj1!.Value, Is.EqualTo(1));

        using (var obj2 = doubleBuffer.GetForWrite())
            obj2.Value = 2;

        using (var obj2 = doubleBuffer.GetForRead())
            Assert.That(obj2!.Value, Is.EqualTo(2));
    }

    [Test]
    public void TestTripleBuffer()
    {
        var tripleBuffer = new TripleBuffer<int>();

        using (var obj1 = tripleBuffer.GetForWrite())
            obj1.Value = 1;

        using (var obj1 = tripleBuffer.GetForRead())
            Assert.That(obj1!.Value, Is.EqualTo(1));

        using (var obj2 = tripleBuffer.GetForWrite())
            obj2.Value = 2;

        using (var obj2 = tripleBuffer.GetForRead())
            Assert.That(obj2!.Value, Is.EqualTo(2));

        using (var obj3 = tripleBuffer.GetForWrite())
            obj3.Value = 3;

        using (var obj3 = tripleBuffer.GetForRead())
            Assert.That(obj3!.Value, Is.EqualTo(3));
    }

    [Test]
    public void TestSameBufferIsNotWrittenTwiceInRowNoContestation()
    {
        var tripleBuffer = createWithIDsMatchingIndices();

        using (var write = tripleBuffer.GetForWrite())
            Assert.That(write.Value?.ID, Is.EqualTo(0));

        // buffer 0: waiting for read
        // buffer 1: old
        // buffer 2: old

        using (var buffer = tripleBuffer.GetForRead())
            Assert.That(buffer?.Value?.ID, Is.EqualTo(0));

        // buffer 0: last read
        // buffer 1: old
        // buffer 2: old

        using (var write = tripleBuffer.GetForWrite())
            Assert.That(write.Value?.ID, Is.EqualTo(1));

        // buffer 0: last read
        // buffer 1: waiting for read
        // buffer 2: old

        using (var write = tripleBuffer.GetForWrite())
            Assert.That(write.Value?.ID, Is.EqualTo(2));

        // buffer 0: last read
        // buffer 1: old
        // buffer 2: waiting for read

        using (var write = tripleBuffer.GetForWrite())
            Assert.That(write.Value?.ID, Is.EqualTo(1));

        // buffer 0: last read
        // buffer 1: waiting for read
        // buffer 2: old

        using (var buffer = tripleBuffer.GetForRead())
            Assert.That(buffer?.Value?.ID, Is.EqualTo(1));

        // buffer 0: old
        // buffer 1: last read
        // buffer 2: old
    }

    [Test]
    public void TestSameBufferIsNotWrittenTwiceInRowContestation()
    {
        var tripleBuffer = createWithIDsMatchingIndices();

        // Test with first write in use during second.
        using (var write = tripleBuffer.GetForWrite())
            Assert.That(write.Value?.ID, Is.EqualTo(0));

        // buffer 0: waiting for read
        // buffer 1: old
        // buffer 2: old

        using (var read = tripleBuffer.GetForRead())
        {
            Assert.That(read?.Value?.ID, Is.EqualTo(0));

            // buffer 0: reading
            // buffer 1: old
            // buffer 2: old

            using (var write = tripleBuffer.GetForWrite())
                Assert.That(write.Value?.ID, Is.EqualTo(1));

            // buffer 0: reading
            // buffer 1: waiting for read
            // buffer 2: old

            using (var write = tripleBuffer.GetForWrite())
                Assert.That(write.Value?.ID, Is.EqualTo(2));

            // buffer 0: reading
            // buffer 1: old
            // buffer 2: waiting for read
        }

        using (var read = tripleBuffer.GetForRead())
        {
            Assert.That(read?.Value?.ID, Is.EqualTo(2));

            // buffer 0: old
            // buffer 1: old
            // buffer 2: reading

            using (var write = tripleBuffer.GetForWrite())
                Assert.That(write.Value?.ID, Is.EqualTo(0));

            // buffer 0: waiting for read
            // buffer 1: old
            // buffer 2: reading

            using (var write = tripleBuffer.GetForWrite())
                Assert.That(write.Value?.ID, Is.EqualTo(1));

            // buffer 0: old
            // buffer 1: waiting for read
            // buffer 2: reading

            using (var write = tripleBuffer.GetForWrite())
                Assert.That(write.Value?.ID, Is.EqualTo(0));

            // buffer 0: waiting for read
            // buffer 1: old
            // buffer 2: reading
        }

        using (var read = tripleBuffer.GetForRead())
        {
            Assert.That(read?.Value?.ID, Is.EqualTo(0));
            // buffer 0: reading
            // buffer 1: old
            // buffer 2: old
        }
    }

    [Test]
    public void TestSameBufferIsNotWrittenTwiceInRowContestation2()
    {
        var tripleBuffer = createWithIDsMatchingIndices();

        using (var write = tripleBuffer.GetForWrite())
            Assert.That(write.Value?.ID, Is.EqualTo(0));

        // buffer 0: waiting for read
        // buffer 1: old
        // buffer 2: old

        using (var read = tripleBuffer.GetForRead())
        {
            Assert.That(read?.Value?.ID, Is.EqualTo(0));

            // buffer 0: reading
            // buffer 1: old
            // buffer 2: old

            using (var write = tripleBuffer.GetForWrite())
            {
                Assert.That(write.Value?.ID, Is.EqualTo(1));

                // buffer 0: reading
                // buffer 1: writing
                // buffer 2: old
            }
        }

        using (var read = tripleBuffer.GetForRead())
        {
            Assert.That(read?.Value?.ID, Is.EqualTo(1));

            // buffer 0: old
            // buffer 1: reading
            // buffer 2: old
        }

        using (var write = tripleBuffer.GetForWrite())
        {
            Assert.That(write.Value?.ID, Is.EqualTo(0));

            // buffer 0: writing
            // buffer 1: last read
            // buffer 2: old
        }

        using (var read = tripleBuffer.GetForRead())
        {
            Assert.That(read?.Value?.ID, Is.EqualTo(0));

            // buffer 0: reading
            // buffer 1: old
            // buffer 2: old

            using (var write = tripleBuffer.GetForWrite())
            {
                Assert.That(write.Value?.ID, Is.EqualTo(1));

                // buffer 0: reading
                // buffer 1: writing
                // buffer 2: old
            }

            using (var write = tripleBuffer.GetForWrite())
            {
                Assert.That(write.Value?.ID, Is.EqualTo(2));

                // buffer 0: reading
                // buffer 1: waiting for read
                // buffer 2: writing
            }
        }
    }

    [Test]
    public void TestWriteThenRead()
    {
        var tripleBuffer = new TripleBuffer<TestObject>();

        for (int i = 0; i < 1000; i++)
        {
            var obj = new TestObject(i);

            using (var write = tripleBuffer.GetForWrite())
                write.Value = obj;

            using (var buffer = tripleBuffer.GetForRead())
                Assert.That(buffer?.Value, Is.EqualTo(obj));
        }

        using (var buffer = tripleBuffer.GetForRead())
            Assert.That(buffer, Is.Null);
    }

    [Test]
    public void TestReadSaturated()
    {
        var tripleBuffer = new TripleBuffer<TestObject>();

        for (int i = 0; i < 10; i++)
        {
            var obj = new TestObject(i);
            ManualResetEventSlim resetEventSlim = new ManualResetEventSlim();

            var readTask = Task.Factory.StartNew(() =>
            {
                resetEventSlim.Set();
                using (var buffer = tripleBuffer.GetForRead())
                    Assert.That(buffer?.Value, Is.EqualTo(obj));
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
            {
                resetEventSlim.Wait(1000);
                Thread.Sleep(10);

                using (var write = tripleBuffer.GetForWrite())
                    write.Value = obj;
            }, TaskCreationOptions.LongRunning);

            readTask.Wait();
        }
    }

    private static TripleBuffer<TestObject> createWithIDsMatchingIndices()
    {
        var tripleBuffer = new TripleBuffer<TestObject>();

        // Setup the triple buffer with correctly indexed objects.
        List<int> initialisedBuffers = new List<int>();

        initialiseBuffer();

        using (var _ = tripleBuffer.GetForRead())
        {
            initialiseBuffer();
            initialiseBuffer();
        }

        Assert.That(initialisedBuffers, Is.EqualTo(new[] { 0, 1, 2 }));

        // Read remaining buffers to reset things to a sane state (next write will be at index 0).
        using (var _ = tripleBuffer.GetForRead()) { }

        using (var _ = tripleBuffer.GetForRead()) { }

        void initialiseBuffer()
        {
            using (var write = tripleBuffer.GetForWrite())
            {
                write.Value = new TestObject(write.Index);
                initialisedBuffers.Add(write.Index);
            }
        }

        return tripleBuffer;
    }

    private class TestObject
    {
        public readonly int ID;

        public TestObject(int id)
        {
            ID = id;
        }

        public override string ToString()
        {
            return $"{base.ToString()} ID: {ID}";
        }
    }
}
