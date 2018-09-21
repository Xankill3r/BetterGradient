using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BetterGradient
{
    public enum GradientMode
    {
        RGB,
        LChuv
    }

    public struct ErrorData
    {
        public readonly double AverageDifference;
        public readonly double StdDeviation;
        public readonly double MaxDeviation;

        public ErrorData(double averageDifference, double stdDeviation, double maxDeviation)
        {
            AverageDifference = averageDifference;
            StdDeviation = stdDeviation;
            MaxDeviation = maxDeviation;
        }

        public override string ToString()
        {
            return $"ErrorData (Avg:{AverageDifference}, StdDev:{StdDeviation}, MaxDev:{MaxDeviation})";
        }
    }

    public static class BetterGradient
    {
        /// <summary>
        /// Generates a texture that encodes the Gradient specified by colors, colorKeyLocations, alpha and alphaKeyLocations.
        /// The generated texture is textureWidth x 1 pixels in size.
        /// Supports more than 8 each of Alpha Keys and Color Keys
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="colorKeyLocations"></param>
        /// <param name="alphas"></param>
        /// <param name="alphaKeyLocations"></param>
        /// <param name="textureWidth"></param>
        /// <returns></returns>
        public static bool GradientToColors(Color[] colors, float[] colorKeyLocations, float[] alphas, float[] alphaKeyLocations, Color[] gradientColors, GradientMode mode)
        {
            Assert.AreEqual(colors.Length, colorKeyLocations.Length);
            Assert.AreEqual(alphas.Length, alphaKeyLocations.Length);
            // Invalid array lengths
            if (colors.Length != colorKeyLocations.Length || alphas.Length != alphaKeyLocations.Length) return false;
            // Too few keys
            if (colors.Length < 2 && alphas.Length < 2) return false;

            float prevColorKey = 0f, nextColorKey = colorKeyLocations[0];
            Color prevColor = colors[0], nextColor = colors[0];
            float prevAlphaKey = 0f, nextAlphaKey = alphaKeyLocations[0];
            float prevAlpha = alphas[0], nextAlpha = alphas[0];
            var textureWidth = gradientColors.Length;
            var invTextureWidth = 1f / (textureWidth - 1);
            for (int i = 0; i < textureWidth; i++)
            {
                float f = i * invTextureWidth;
                if (f >= nextAlphaKey)
                {
                    if (nextAlphaKey < 1f)
                    {
                        prevAlphaKey = nextAlphaKey;
                        prevAlpha = nextAlpha;
                        int k = 0;
                        while (k < alphaKeyLocations.Length && f >= alphaKeyLocations[k])
                        {
                            k++;
                            nextAlphaKey = k < alphaKeyLocations.Length ? alphaKeyLocations[k] : 1f;
                            nextAlpha = alphas[k < alphas.Length ? k : k - 1];
                        }
                    }
                }
                if (f >= nextColorKey)
                {
                    if (nextColorKey < 1f)
                    {
                        prevColorKey = nextColorKey;
                        prevColor = nextColor;
                        int k = 0;
                        while (k < colorKeyLocations.Length && f >= colorKeyLocations[k])
                        {
                            k++;
                            nextColorKey = k < colorKeyLocations.Length ? colorKeyLocations[k] : 1f;
                            nextColor = colors[k < colors.Length ? k : k - 1];
                        }
                    }
                }
                var colorFraction = (f - prevColorKey) / (nextColorKey - prevColorKey);
                var alphaFraction = (f - prevAlphaKey) / (nextAlphaKey - prevAlphaKey);
                var lerpedAlpha = Mathf.Lerp(prevAlpha, nextAlpha, alphaFraction);
                Color lerpedColor;
                switch (mode)
                {
                    case GradientMode.LChuv:
                        lerpedColor = ColorLChuv.Lerp(prevColor, nextColor, colorFraction);
                        break;
                    default:
                        lerpedColor = Color.Lerp(prevColor, nextColor, colorFraction);
                        break;
                }
                gradientColors[i] = new Color(lerpedColor.r, lerpedColor.g, lerpedColor.b, lerpedAlpha);
            }
            return true;
        }

        /// <summary>
        /// Converts an array of colors into an approximate Gradient using a max of 8 color keys and 8 alpha keys
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="gradient"></param>
        /// <param name="approxColors"></param>
        /// <param name="approxAlpha"></param>
        /// <returns>Error data specifying deviation from ideal case</returns>
        public static ErrorData ApproximateColorArrayAsGradient(Color[] colors, Gradient gradient, List<GradientColorKey> approxColors, List<GradientAlphaKey> approxAlpha)
        {
            approxAlpha.Clear();
            approxColors.Clear();

            float invLength = 1f / (colors.Length - 1);
            // Init the lists
            for (int i = 0; i < colors.Length - 1; i++)
            {
                // Add first alpha value that deviates from front
                if (colors[i].a != colors[i + 1].a)
                {
                    var t = i * invLength;
                    approxAlpha.Add(new GradientAlphaKey(colors[i].a, t));
                    break;
                }
            }
            for (int i = 0; i < colors.Length - 1; i++)
            {
                // Add first color value that deviates from front
                if (colors[i].r != colors[i + 1].r || colors[i].g != colors[i + 1].g || colors[i].b != colors[i + 1].b)
                {
                    var t = i * invLength;
                    approxColors.Add(new GradientColorKey(colors[i], t));
                    break;
                }
            }
            for (int i = colors.Length - 1; i > 0; i--)
            {
                // Add first alpha value that deviates from back
                if (colors[i].a != colors[i - 1].a)
                {
                    var t = i * invLength;
                    approxAlpha.Add(new GradientAlphaKey(colors[i].a, t));
                    break;
                }
            }
            for (int i = colors.Length - 1; i > 0; i--)
            {
                // Add first color value that deviates from back
                if (colors[i].r != colors[i - 1].r || colors[i].g != colors[i - 1].g || colors[i].b != colors[i - 1].b)
                {
                    var t = i * invLength;
                    approxColors.Add(new GradientColorKey(colors[i], t));
                    break;
                }
            }
            gradient.SetKeys(approxColors.ToArray(), approxAlpha.ToArray());

            int iters = 8;
            while (iters-- > 0)
            {
                float maxColorDistance = 0f, maxAlphaDistance = 0f;
                int maxColorDistanceIndex = -1, maxAlphaDistanceIndex = -1;
                for (int i = 0; i < colors.Length; i++)
                {
                    var gradientColor = gradient.Evaluate(i * invLength);
                    var alphaDistance = Mathf.Abs(colors[i].a - gradientColor.a);
                    var colorDistance = ColorRGBSqrDistance(colors[i], gradientColor);
                    if (alphaDistance > maxAlphaDistance && alphaDistance > 1e-3f)
                    {
                        maxAlphaDistance = alphaDistance;
                        maxAlphaDistanceIndex = i;
                    }
                    if (colorDistance > maxColorDistance && colorDistance > 1e-3f)
                    {
                        maxColorDistance = colorDistance;
                        maxColorDistanceIndex = i;
                    }
                }
                // Break if no deviations found
                if (maxColorDistanceIndex == -1 && maxAlphaDistanceIndex == -1)
                {
                    break;
                }
                // Insert new keys
                if (maxAlphaDistanceIndex != -1)
                {
                    var newKeyLocation = maxAlphaDistanceIndex * invLength;
                    var newAlpha = colors[maxAlphaDistanceIndex].a;
                    for (int i = 0; i < approxAlpha.Count; i++)
                    {
                        if (newKeyLocation > approxAlpha[i].time)
                        {
                            approxAlpha.Insert(i + 1, new GradientAlphaKey(newAlpha, newKeyLocation));
                            break;
                        }
                    }
                }
                if (maxColorDistanceIndex != -1)
                {
                    var newKeyLocation = maxColorDistanceIndex * invLength;
                    var newColor = colors[maxColorDistanceIndex];
                    for (int i = 0; i < approxColors.Count; i++)
                    {
                        if (newKeyLocation > approxColors[i].time)
                        {
                            approxColors.Insert(i + 1, new GradientColorKey(newColor, newKeyLocation));
                            break;
                        }
                    }
                }
                // Update gradient
                gradient.SetKeys(approxColors.ToArray(), approxAlpha.ToArray());
                // Break if we have used maximum allowed Color and Alpha keys
                if (approxAlpha.Count == 8 || approxColors.Count == 8)
                {
                    break;
                }
            }

            // Calculate error values
            var totalErr = 0f;
            var maxErr = 0f;
            for (int i = 0; i < colors.Length; i++)
            {
                var approx = gradient.Evaluate(i / (float)(colors.Length - 1));
                var error = Mathf.Sqrt(ColorRGBASqrDistance(approx, colors[i]));
                totalErr += error;
                maxErr = Mathf.Max(maxErr, error);
            }
            var avg = totalErr / colors.Length;
            var totalDeviation = 0f;
            for (int i = 0; i < colors.Length; i++)
            {
                var approx = gradient.Evaluate(i / (float)(colors.Length - 1));
                var error = Mathf.Sqrt(ColorRGBASqrDistance(approx, colors[i]));
                var deviation = (error - avg) * (error - avg);
                totalDeviation += deviation;
            }
            var stdDev = Mathf.Sqrt(totalDeviation / colors.Length);

            return new ErrorData(avg, stdDev, maxErr);
        }

        private static float ColorRGBSqrDistance(Color a, Color b)
        {
            return (a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b);
        }
        private static float ColorRGBASqrDistance(Color a, Color b)
        {
            return (a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b) + (a.a - b.a) * (a.a - b.a);
        }
    }
}
