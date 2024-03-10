using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DanmakuEngine.Extensions;

namespace DanmakuEngine.Utils;

/// <summary>
/// Represent a not cryptographically secure ramdom number generator using xoroshiro128plusplus
///
/// Based on code from: https://prng.di.unimi.it/xoroshiro128plusplus.c
/// and .NET's implementation(https://github.com/dotnet/runtime/blob/b1e85b8fa386e4534eed0ce4dd394b57b146575f/src/libraries/System.Private.CoreLib/src/System/Random.Xoshiro256StarStarImpl.cs)
/// 
/// Written in 2019 by David Blackman and Sebastiano Vigna (vigna@acm.org)
/// 
/// To the extent possible under law, the author has dedicated all copyright
/// and related and neighboring rights to this software to the public domain
/// worldwide. This software is distributed without any warranty.
/// 
/// See <http://creativecommons.org/publicdomain/zero/1.0/>.
/// </summary>
public class RNG
{
    private ulong _s0, _s1;

    public (ulong s0, ulong s1) Seed => (_s0, _s1);

    public RNG Copy()
        => new(_s0, _s1);

    public RNG()
        : this(Guid.NewGuid().GetHash())
    {
    }

    public RNG(string worker)
        : this(worker.GetHash())
    {
    }

    public RNG(int seed, string worker)
    {
        do
        {
            _s1 = (uint)worker.GetHash() << 32
                | (uint)seed.CombineHashWith(worker);

            _s1 = (uint)seed << 32
                | (uint)worker.CombineHashWith(_s0);
        } while (_s0 is 0 && _s1 is 0);
    }

    public RNG(int seed)
    {
        initWithSeed(seed);

        while (_s0 is 0 && _s1 is 0)
            initWithSeed(_s0.CombineHashWith(_s1));

        void initWithSeed(int seed)
        {
            int s = seed;

            _s0 = (uint)s << 32
                | (uint)(s = s.CombineHashWith(seed));

            _s1 = (uint)(s = s.CombineHashWith(_s0)) << 32
                | (uint)s.GetHash();
        }
    }

    public RNG(ulong s0, ulong s1)
    {
        if (s0 is 0 && s1 is 0)
            throw new ArgumentException($"Must not pass two zero.");

        _s0 = s0;
        _s1 = s1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint nextUInt32()
        => (uint)(nextUInt64() >> 32);

    /// <summary>
    /// Core of this algorithm
    /// </summary>
    /// <returns>A *random* ulong number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private ulong nextUInt64()
    {
        ulong s0 = _s0;
        ulong s1 = _s1;

        ulong result = BitOperations.RotateLeft(s0 + s1, 17) + s0;

        s1 ^= s0;
        _s0 = BitOperations.RotateLeft(s0, 49) ^ s1 ^ (s1 << 21); // a, b
        _s1 = BitOperations.RotateLeft(s1, 28); // c

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal uint _nextUInt32(uint maxValue)
    {
        ulong randomProduct = (ulong)maxValue * nextUInt32();
        uint lowPart = (uint)randomProduct;

        if (lowPart < maxValue)
        {
            uint remainder = (0u - maxValue) % maxValue;

            while (lowPart < remainder)
            {
                randomProduct = (ulong)maxValue * nextUInt32();
                lowPart = (uint)randomProduct;
            }
        }

        return (uint)(randomProduct >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    internal ulong _nextUInt64(ulong maxValue)
    {
        ulong product = Math.BigMul(maxValue, nextUInt64(), out ulong lowPart);

        if (lowPart < maxValue)
        {
            ulong remainder = (0ul - maxValue) % maxValue;

            while (lowPart < remainder)
            {
                product = Math.BigMul(maxValue, nextUInt64(), out lowPart);
            }
        }

        return product;
    }

    public void NextUInt64()
        => nextUInt64();

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Next()
    {
        while (true)
        {
            // Get top 31 bits to get a value in the range [0, int.MaxValue], but try again
            // if the value is actually int.MaxValue, as the method is defined to return a value
            // in the range [0, int.MaxValue).
            ulong result = nextUInt64() >> 33;

            if (result != int.MaxValue)
                return (int)result;
        }
    }

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">
    /// The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to 0.
    /// </param>
    /// <returns>
    /// A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily
    /// includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxValue"/> is less than 0.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Next(int maxValue)
    {
        ArgumentOutOfRangeException
            .ThrowIfNegative(maxValue);

        return (int)_nextUInt32((uint)maxValue);
    }

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
    /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="minValue"/> is greater than <paramref name="maxValue"/>.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int Next(int minValue, int maxValue)
    {
        ArgumentOutOfRangeException
            .ThrowIfLessThan(maxValue, minValue,
                $"Max value({maxValue}) can NOT be less than min value({minValue}).");

        return (int)_nextUInt32((uint)(maxValue - minValue)) + minValue;
    }

    /// <summary>
    /// Returns a non-negative random integer.
    /// </summary>
    /// <returns>A 64-bit signed integer that is greater than or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public long NextLong()
    {
        while (true)
        {
            // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
            // if the value is actually long.MaxValue, as the method is defined to return a value
            // in the range [0, long.MaxValue).
            ulong result = nextUInt64() >> 1;

            if (result != long.MaxValue)
                return (long)result;
        }
    }

    /// <summary>
    /// Returns a non-negative random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to 0.</param>
    /// <returns>
    /// A 64-bit signed integer that is greater than or equal to 0, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily
    /// includes 0 but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals 0, <paramref name="maxValue"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public long NextLong(long maxValue)
    {
        ArgumentOutOfRangeException
            .ThrowIfNegative(maxValue);

        return (long)_nextUInt64((ulong)maxValue);
    }

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
    /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
    /// <returns>
    /// A 64-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/>
    /// but not <paramref name="maxValue"/>. If minValue equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public long NextLong(long minValue, long maxValue)
    {
        ArgumentOutOfRangeException
            .ThrowIfLessThan(maxValue, minValue,
                $"Max value({maxValue}) can NOT be less than min value({minValue}).");

        return (long)_nextUInt64((ulong)(maxValue - minValue)) + minValue;
    }

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public double NextDouble()
        // As described in http://prng.di.unimi.it/:
        // "A standard double (64-bit) floating-point number in IEEE floating point format has 52 bits of significand,
        //  plus an implicit bit at the left of the significand. Thus, the representation can actually store numbers with
        //  53 significant binary digits. Because of this fact, in C99 a 64-bit unsigned integer x should be converted to
        //  a 64-bit double using the expression
        //  (x >> 11) * 0x1.0p-53"
        => (nextUInt64() >> 11) * (1.0 / (1ul << 53));

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
    /// </summary>
    /// <returns>A single-precision floating point number that is greater than or equal to 0.0, and less than 1.0.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public float NextSingle()
        // Same as above, but with 24 bits instead of 53.
        => (nextUInt64() >> 40) * (1.0f / (1u << 24));

    /// <summary>
    /// Fills the elements of a specified array of bytes with random numbers.
    /// </summary>
    /// <param name="buffer">The array to be filled with random numbers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
    public void NextBytes(byte[] buffer)
        => nextBytes((Span<byte>)buffer);

    /// <summary>
    /// Fills the elements of a specified span of bytes with random numbers.
    /// </summary>
    /// <param name="buffer">The array to be filled with random numbers.</param>
    public void NextBytes(Span<byte> buffer)
        => nextBytes(buffer);


    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private unsafe void nextBytes(Span<byte> buffer)
    {
        ulong s0 = _s0, s1 = _s1;

        while (buffer.Length > sizeof(ulong))
        {
            Unsafe.WriteUnaligned(
                ref MemoryMarshal.GetReference(buffer),
                BitOperations.RotateLeft(s0 + s1, 17) + s0
            );

            // Update PRNG state.
            s1 ^= s0;
            s0 = BitOperations.RotateLeft(s0, 49) ^ s1 ^ (s1 << 21); // a, b
            s1 = BitOperations.RotateLeft(s1, 28); // c

            buffer = buffer.Slice(sizeof(ulong));
        }

        if (!buffer.IsEmpty)
        {
            ulong next = BitOperations.RotateLeft(s0 + s1, 17) + s0;
            byte* remainingBytes = (byte*)&next;

            Debug.Assert(buffer.Length < sizeof(ulong));

            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = remainingBytes[i];
            }

            // Update PRNG state.
            s1 ^= s0;
            s0 = BitOperations.RotateLeft(s0, 49) ^ s1 ^ (s1 << 21); // a, b
            s1 = BitOperations.RotateLeft(s1, 28); // c
        }

        _s0 = s0;
        _s1 = s1;
    }

    /// <summary>
    /// Equivalent to 2^64 calls to <see cref="NextUInt64"/>; it can be used to generate 2^64
    /// non-overlapping subsequences for parallel computations.
    /// </summary>
    public void Jump()
        => jump(0x2bd7a6a6e99c2ddc, 0x0992ccaf6a6fca05);

    /// <summary>
    /// Equivalent to 2^96 calls to <see cref="NextUInt64"/>; it can be used to generate 
    /// 2^32 starting points, from each of which <seealso cref="Jump"/> will generate 
    /// 2^32 non-overlapping subsequences for parallel distributed computations.
    /// </summary>
    public void LongJump()
        => jump(0x360fd5f2cf8d5d99, 0x9c6e6877736c46e3);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private void jump(params ulong[] jumps)
    {
        ulong s0 = 0;
        ulong s1 = 0;

        Debug.Assert(jumps.Length == 2);

        for (int i = 0; i < jumps.Length; i++)
        {
            for (int b = 0; b < 64; b++)
            {
                if ((jumps[i] & 1ul << b) != 0ul)
                {
                    s0 ^= _s0;
                    s1 ^= _s1;
                }
                nextUInt64();
            }
        }

        _s0 = s0;
        _s1 = s1;
    }

    public override bool Equals(object? obj)
        => obj is not null
        && obj is RNG them
        && them._s0 == this._s0
        && them._s1 == this._s1;

    public override int GetHashCode()
        => _s0.CombineHashWith(_s1);

    public override string ToString()
        => $"{GetType().FullName}({_s0:X},{_s1:X})";
}

/// <summary>
/// Represent a generic random number generator with seed is the type.
///
/// We strongly suggest use the type name as the seed as the hash code of
/// the type may change in different builds which lead to inconsistent behaviour.
/// </summary>
/// <typeparam name="T">The type of the object using RNG</typeparam>
public class RNG<T> : RNG
{
    public RNG(bool useTypeName = true)
        : base(useTypeName ? typeof(T).FullName!.GetHash()
                           : typeof(T).GetHash())
    {
    }

    public RNG(string worker, bool useTypeName = true)
        : base(useTypeName ? typeof(T).FullName!.GetHash()
                           : typeof(T).GetHash(), worker)
    {
    }
}
