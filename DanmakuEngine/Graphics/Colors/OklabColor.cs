//
// Copyright (c) 2020 Björn Ottosson
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
// of the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// c.f. https://bottosson.github.io/posts/oklab/
//

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DanmakuEngine.Graphics.Colors;

[StructLayout(LayoutKind.Sequential)]
public struct OklabColor
{
    /// <summary>
    /// Perceived lightness
    /// </summary>
    public float L;

    /// <summary>
    /// How green/red the color is
    /// </summary>
    public float A;

    /// <summary>
    /// How blue/yellow the color is
    /// </summary>
    public float B;

    public float Opacity;

    public OklabColor(float L, float a, float b, float opacity = 1.0f)
    {
        this.L = L;
        this.A = a;
        this.B = b;
        this.Opacity = opacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static OklabColor FromSRGB(SRGBColor sRGBColor)
    {
        // The simd implementation is not faster than the scalar implementation.
        // So we use the scalar implementation for now.x

        var linear = sRGBColor.ToLinearRGB();

        float l = (0.4122214708f * linear.X) + (0.5363325363f * linear.Y) + (0.0514459929f * linear.Z);
        float m = (0.2119034982f * linear.X) + (0.6806995451f * linear.Y) + (0.1073969566f * linear.Z);
        float s = (0.0883024619f * linear.X) + (0.2817188376f * linear.Y) + (0.6299787005f * linear.Z);

        float l_ = MathF.Cbrt(l);
        float m_ = MathF.Cbrt(m);
        float s_ = MathF.Cbrt(s);

        return new OklabColor
        {
            L = (0.2104542553f * l_) + (0.7936177850f * m_) - (0.0040720468f * s_),
            A = (1.9779984951f * l_) - (2.4285922050f * m_) + (0.4505937099f * s_),
            B = (0.0259040371f * l_) + (0.7827717662f * m_) - (0.8086757660f * s_),
            Opacity = sRGBColor.A / 255.0f,
        };
    }

    public SRGBColor ToSRGB()
        => ToSRGB(this);

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static unsafe SRGBColor ToSRGB(OklabColor oklab)
    {
        float l_ = oklab.L + (0.3963377774f * oklab.A) + (0.2158037573f * oklab.B);
        float m_ = oklab.L - (0.1055613458f * oklab.A) - (0.0638541728f * oklab.B);
        float s_ = oklab.L - (0.0894841775f * oklab.A) - (1.2914855480f * oklab.B);

        float m = m_ * m_ * m_;
        float l = l_ * l_ * l_;
        float s = s_ * s_ * s_;

        return SRGBColor.FromLinear(new Vector4
        {
            X = (+4.0767416621f * l) - (3.3077115913f * m) + (0.2309699292f * s),
            Y = (-1.2684380046f * l) + (2.6097574011f * m) - (0.3413193965f * s),
            Z = (-0.0041960863f * l) - (0.7034186147f * m) + (1.7076147010f * s),
            W = oklab.Opacity * 255f,
        });
    }
}
