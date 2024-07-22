using BenchmarkDotNet.Attributes;
using DanmakuEngine.Graphics.Colors;
using DanmakuEngine.Transformation;

namespace DanmakuEngine.Benchmarks;

/*
    // Reference results

    BenchmarkDotNet v0.13.12, Arch Linux
    13th Gen Intel Core i5-13500H, 1 CPU, 16 logical and 12 physical cores
    .NET SDK 8.0.303
      [Host]     : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2
      DefaultJob : .NET 8.0.7 (8.0.724.31311), X64 RyuJIT AVX2

    | Method               | Mean        | Error     | StdDev      | Median      | Ratio  | RatioSD | Allocated | Alloc Ratio |
    |--------------------- |------------:|----------:|------------:|------------:|-------:|--------:|----------:|------------:|
    | LinearSRGBLerpHandle |    265.0 ns |   7.58 ns |    19.83 ns |    274.7 ns |   1.00 |    0.00 |         - |          NA |
    | HslLerpHandle        |    420.0 ns |   1.54 ns |     1.28 ns |    419.4 ns |   1.63 |    0.16 |         - |          NA |
    | LchLerpHandle        |  9,840.5 ns | 187.83 ns |   175.69 ns |  9,885.0 ns |  38.55 |    3.68 |         - |          NA |
    | LabLerpHandle        |  1,134.3 ns |   2.91 ns |     2.58 ns |  1,133.8 ns |   4.40 |    0.43 |         - |          NA |
    | OklabLerpHandle      | 46,561.6 ns | 919.66 ns | 1,404.42 ns | 46,808.8 ns | 185.76 |   20.57 |         - |          NA |
    | FastOklabLerpHandle  |    275.3 ns |   0.20 ns |     0.17 ns |    275.3 ns |   1.07 |    0.11 |         - |          NA |

    Note: FastOklabLerpHandle is now OklabLerpHandle
          The OklabLerpHandle in the list was the old implementation, which converts to lms, oklab then lerp and convert back to srgb.
*/

public class ColorLerpBenchmarks
{
    private static SRGBColor _start = new SRGBColor(255, 0, 0, 0);
    private static SRGBColor _end = new SRGBColor(0, 255, 0, 0);

    private static void lerpTest(ILerpHandle<SRGBColor> handle)
    {
        for (int i = 0; i < 1000; i++)
        {
            handle.Lerp(i / 999f);
        }
    }

    private SRGBLerpHandle _srgbHandle = new(_start, _end);
    private HslLerpHandle _hslHandle = new(_start, _end);
    private LchLerpHandle _lchHandle = new(_start, _end);
    private LabLerpHandle _labHandle = new(_start, _end);
    private OklabLerpHandle _oklabHandle = new(_start, _end);

    [Benchmark(Baseline = true)]
    public void LinearSRGBLerpHandle()
    {
        lerpTest(_srgbHandle);
    }

    [Benchmark]
    public void HslLerpHandle()
    {
        lerpTest(_hslHandle);
    }

    [Benchmark]
    public void LchLerpHandle()
    {
        lerpTest(_lchHandle);
    }

    [Benchmark]
    public void LabLerpHandle()
    {
        lerpTest(_labHandle);
    }

    [Benchmark]
    public void OklabLerpHandle()
    {
        lerpTest(_oklabHandle);
    }
}
