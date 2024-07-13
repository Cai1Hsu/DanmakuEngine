using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using DanmakuEngine.Allocations.ValueAccessors;
using DanmakuEngine.Bindables;

namespace DanmakuEngine.Benchmarks;

public class AccessorBenchmarks
{
    public class TestClass
    {
        public int Value { get; set; }
        public int _value;

        public TestClass(int v1, int v2)
        {
            _value = v1;
            Value = v2;
        }
    }

    private TestClass testClass = null!;

    private Bindable<int> bindable = null!;

    private BindableAccessor<int> bindableAccessor = null!;

    private DelegateAccessor<int> delegateAccessor = null!;

    private RefAccessor<int> refAccessor1 = null!;

    private RefAccessor<int> refAccessor2 = null!;

    private EmitAccessor<int, TestClass> emitAccessorField = null!;

    private EmitAccessor<int, TestClass> emitAccessorProperty = null!;

    [GlobalSetup]
    public void Setup()
    {
        testClass = new TestClass(0, 0);
        bindable = new Bindable<int>(42);

        bindableAccessor = new BindableAccessor<int>(bindable);
        delegateAccessor = DelegateAccessor<int>.Create(() => testClass.Value, value => testClass.Value = value);
        refAccessor1 = RefAccessor<int>.Create(testClass, ReadTestClassField);
        refAccessor2 = RefAccessor<int>.Create(() => ref ReadTestClassField(testClass));
        emitAccessorField = new EmitAccessor<int, TestClass>(testClass, "_value");
        emitAccessorProperty = new EmitAccessor<int, TestClass>(testClass, "Value");
    }

    [Benchmark]
    public BindableAccessor<int> CreateBindableAccessor()
    {
        return new BindableAccessor<int>(bindable);
    }

    [Benchmark]
    public DelegateAccessor<int> CreateDelegateAccessor()
    {
        return DelegateAccessor<int>.Create(() => testClass.Value, value => testClass.Value = value);
    }

    [Benchmark]
    public RefAccessor<int> CreateRefAccessor_UnsafeAccessorStoreInstance()
    {
        return RefAccessor<int>.Create(testClass, ReadTestClassField);
    }

    [Benchmark]
    public RefAccessor<int> CreateRefAccessor_UnsafeAccessorStoreInstanceWithInClosure()
    {
        return RefAccessor<int>.Create(() => ref ReadTestClassField(testClass));
    }

    [Benchmark]
    public EmitAccessor<int, TestClass> CreateEmitAccessor_Field()
    {
        return new EmitAccessor<int, TestClass>(testClass, "_value");
    }

    [Benchmark]
    public EmitAccessor<int, TestClass> CreateEmitAccessor_Property()
    {
        return new EmitAccessor<int, TestClass>(testClass, "Value");
    }

    [Benchmark]
    public int BindableAccessor_Get()
    {
        return bindableAccessor.Value;
    }

    [Benchmark]
    public void BindableAccessor_Set()
    {
        bindableAccessor.Value = 24;
    }

    [Benchmark]
    public int DelegateAccessor_Get()
    {
        return delegateAccessor.Value;
    }

    [Benchmark]
    public void DelegateAccessor_Set()
    {
        delegateAccessor.Value = 42;
    }

    [Benchmark]
    public int RefAccessor_Get_UnsafeAccessorStoreInstance()
    {
        return refAccessor1.Value;
    }

    [Benchmark]
    public void RefAccessor_Set_UnsafeAccessorStoreInstance()
    {
        refAccessor1.Value = 42;
    }

    [Benchmark]
    public int RefAccessor_Get_UnsafeAccessorStoreInstanceWithInClosure()
    {
        return refAccessor2.Value;
    }

    [Benchmark]
    public void RefAccessor_Set_UnsafeAccessorStoreInstanceWithInClosure()
    {
        refAccessor2.Value = 42;
    }

    [Benchmark]
    public int EmitAccessor_Get_Field()
    {
        return emitAccessorField.Value;
    }

    [Benchmark]
    public void EmitAccessor_Set_Field()
    {
        emitAccessorField.Value = 42;
    }

    [Benchmark]
    public int EmitAccessor_Get_Property()
    {
        return emitAccessorProperty.Value;
    }

    [Benchmark]
    public void EmitAccessor_Set_Property()
    {
        emitAccessorProperty.Value = 42;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_value")]
    private static extern ref int ReadTestClassField(TestClass testClass);
}
