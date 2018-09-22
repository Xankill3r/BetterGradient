using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace BetterGradient
{
    public struct ColorLChuv
    {
        private const float Gamma = 3f;
        private const float AlphaScale = 1 / 100f;

        // Corresponds roughly to RGB brighter/darker
        private const float Kn = 18f;

        // D65 standard referent
        private const float Xn = 0.950470f;
        private const float Yn = 1f;
        private const float Zn = 1.088830f;

        private const float t0 = 0.137931034f;  // 4 / 29
        private const float t1 = 0.206896552f;  // 6 / 29
        private const float t2 = 3 * t1 * t1;
        private const float t3 = t1 * t1 * t1;

        /// <summary>
        /// Luminance
        /// </summary>
        public readonly float L;
        /// <summary>
        /// Chroma
        /// </summary>
        public readonly float C;
        /// <summary>
        /// Hue (in degrees)
        /// </summary>
        public readonly float H;
        /// <summary>
        /// Alpha (0 - 1)
        /// </summary>
        public readonly float A;

        #region Constructors

        /// <summary>
        /// Construct from given L, C and H values
        /// </summary>
        /// <param name="l"></param>
        /// <param name="c"></param>
        /// <param name="h"></param>
        /// <param name="a"></param>
        public ColorLChuv(float l, float c, float h, float a)
        {
            L = l;
            C = c;
            H = h;
            A = a;
        }

        /// <summary>
        /// Construct from a given Color32 color
        /// </summary>
        /// <param name="color"></param>
        public ColorLChuv(Color32 color) : this(color.r, color.g, color.b, color.a) { }

        /// <summary>
        /// Construct from given R, G, B and A values
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public ColorLChuv(byte r, byte g, byte b, byte a)
        {
            var rLin = LinearColor(r);
            var gLin = LinearColor(g);
            var bLin = LinearColor(b);
            var x = XYZToLab((0.4124564f * rLin + 0.3575761f * gLin + 0.1804375f * bLin) / Xn);
            var y = XYZToLab((0.2126729f * rLin + 0.7151522f * gLin + 0.0721750f * bLin) / Yn);
            var z = XYZToLab((0.0193339f * rLin + 0.1191920f * gLin + 0.9503041f * bLin) / Zn);

            var l = 116 * y - 16;
            var astar = 500 * (x - y);
            var bstar = 200 * (y - z);

            L = l;
            C = Mathf.Sqrt(astar * astar + bstar * bstar);
            H = Mathf.Atan2(bstar, astar);
            if (!float.IsNaN(H))
            {
                H = (H * Mathf.Rad2Deg + 360) % 360;
            }

            A = a;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float LinearColor(float r)
        {
            r /= 255f;
            if (r <= 0.04045f)
            {
                return r / 12.92f;
            }
            else
            {
                return Mathf.Pow((r + 0.055f) / 1.055f, 2.4f);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float XYZToLab(float t)
        {
            if (t > t3)
            {
                return Mathf.Pow(t, 1 / 3f);
            }
            else
            {
                return t / t2 + t0;
            }
        }

        #endregion

        #region Converters

        public Color GetColor()
        {
            var astar = Mathf.Cos(H * Mathf.Deg2Rad) * C;
            var bstar = Mathf.Sin(H * Mathf.Deg2Rad) * C;

            var y = (L + 16f) / 116f;
            var x = float.IsNaN(astar) ? y : y + astar / 500f;
            var z = float.IsNaN(bstar) ? y : y - bstar / 200f;

            y = Yn * LabToXYZ(y);
            x = Xn * LabToXYZ(x);
            z = Zn * LabToXYZ(z);

            var r = XYZToRGB(3.2404542f * x - 1.5371385f * y - 0.4985314f * z);
            var g = XYZToRGB(-0.9692660f * x + 1.8760108f * y + 0.0415560f * z);
            var b = XYZToRGB(0.0556434f * x - 0.2040259f * y + 1.0572252f * z);

            return new Color(r, g, b, A / 255f);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float LabToXYZ(float t)
        {
            return t > t1 ? t * t * t : t2 * (t - t0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float XYZToRGB(float r)
        {
            return r <= 0.00304f ? 12.92f * r : 1.055f * Mathf.Pow(r, 1 / 2.4f) - 0.055f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ColorLChuv(Color32 c)
        {
            return new ColorLChuv(c);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ColorLChuv(Color c)
        {
            return new ColorLChuv(c);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Color(ColorLChuv c)
        {
            return c.GetColor();
        }

        #endregion

        public static ColorLChuv Lerp(ColorLChuv c0, ColorLChuv c1, float f)
        {
            f = Mathf.Clamp01(f);

            float l, h;
            float c = -1;

            // Hue
            if (!float.IsNaN(c0.H) && !float.IsNaN(c1.H))
            {
                h = Mathf.LerpAngle(c0.H, c1.H, f);
            }
            else if (!float.IsNaN(c0.H))
            {
                h = c0.H;
                if (c0.L == 0 || c1.L == 1) c = c0.C;
            }
            else if (!float.IsNaN(c1.H))
            {
                h = c1.H;
                if (c0.L == 1 || c1.L == 0) c = c1.C;
            }
            else
            {
                h = float.NaN;
            }

            c = c == -1 ? c0.C + f * (c1.C - c0.C) : c;
            l = c0.L + f * (c1.L - c0.L);

            return new ColorLChuv(l, c, h, Mathf.Lerp(c0.A, c1.A, f));
        }

        public override string ToString()
        {
            return $"LCHuv({L:0.##}, {C:0.##}, {H:0.#}, {A:0.#})";
        }
    }
}
