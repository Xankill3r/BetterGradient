using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace BetterGradient
{
    public static class BetterGradient
    {
        public enum GradientMode
        {
            RGB,
            LChuv
        }

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
        public static bool GradientToColors(Color[] colors, float[] colorKeyLocations, float[] alphas, float[] alphaKeyLocations, ref Color[] gradientColors, GradientMode mode)
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
            for (int i = 0; i < textureWidth; i++)
            {
                float f = i / (float)textureWidth;
                if (f >= nextAlphaKey)
                {
                    if (nextAlphaKey == alphaKeyLocations[alphaKeyLocations.Length - 1])
                    {
                        // Last key is before 1f
                        prevAlphaKey = alphaKeyLocations[alphaKeyLocations.Length - 1];
                        nextAlphaKey = 1f;
                        prevAlpha = nextAlpha = alphas[alphas.Length - 1];
                    }
                    else
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
                    if (nextColorKey == colorKeyLocations[colorKeyLocations.Length - 1])
                    {
                        // Last key is before 1f
                        prevColorKey = colorKeyLocations[colorKeyLocations.Length - 1];
                        nextColorKey = 1f;
                        prevColor = nextColor = colors[colors.Length - 1];
                    }
                    else
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
    }
}
