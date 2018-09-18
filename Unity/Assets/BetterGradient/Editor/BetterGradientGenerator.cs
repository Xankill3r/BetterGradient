using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;

namespace BetterGradient
{
    public class BetterGradientGenerator : EditorWindow
    {
        [MenuItem("BetterGradient/Generator")]
        public static void ShowExample()
        {
            GetWindow<BetterGradientGenerator>();
        }

        Gradient input;
        GradientField inputField;
        GradientColorKey[] inputColorKeys = new GradientColorKey[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.blue, 1) };
        GradientAlphaKey[] inputAlphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1, 1), new GradientAlphaKey(1, 1) };
        VisualElement outputGradientTexture;
        Texture2D gradientTexture;
        Color[] gradientColors;

        public void OnEnable()
        {
            if (input == null)
            {
                input = new Gradient();
                input.SetKeys(inputColorKeys, inputAlphaKeys);
            }
            if (gradientTexture == null)
            {
                gradientTexture = new Texture2D(256, 1);
                gradientColors = gradientTexture.GetPixels();
            }

            var root = this.GetRootVisualContainer();

            var inputContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 6,
                    marginBottom = 6,
                    flexDirection = FlexDirection.Row,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f),
                }
            };
            // Label
            inputContainer.Add(new Label()
            {
                text = "Input Gradient",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    width = 140
                }
            });
            // GradientField
            inputField = new GradientField()
            {
                value = input,
                style =
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    flexGrow = 1
                }
            };
            inputContainer.Add(inputField);
            root.Add(inputContainer);

            root.Add(new Button(() => { ConvertToTexture(BetterGradient.GradientMode.RGB); })
            {
                text = "Convert (RGB)",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            });
            root.Add(new Button(() => { ConvertToTexture(BetterGradient.GradientMode.LChuv); })
            {
                text = "Convert (LChuv)",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            });

            var outputContainer = new VisualElement()
            {
                style =
                {
                    marginTop = 6,
                    marginBottom = 6,
                    flexDirection = FlexDirection.Row,
                    backgroundColor = new Color(0.3f, 0.3f, 0.3f),
                }
            };
            // Label
            outputContainer.Add(new Label()
            {
                text = "Output Texture",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    width = 140
                }
            });
            outputGradientTexture = new VisualElement()
            {
                style =
                {
                    marginLeft = 6,
                    marginRight = 6,
                    marginTop = 2,
                    marginBottom = 2,
                    width = 256,
                    height = 30
                }
            };
            outputContainer.Add(outputGradientTexture);
            outputContainer.Add(new Button(() => { SaveTexture(outputGradientTexture.style.backgroundImage.value); })
            {
                text = "Save",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            });
            root.Add(outputContainer);
        }

        private void ConvertToTexture(BetterGradient.GradientMode mode)
        {
            var colorKeys = inputField.value.colorKeys;
            var alphaKeys = inputField.value.alphaKeys;

            var colors = new Color[colorKeys.Length];
            var colorKeyLocations = new float[colorKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
            {
                colors[i] = colorKeys[i].color;
                colorKeyLocations[i] = colorKeys[i].time;
            }

            var alphas = new float[alphaKeys.Length];
            var alphaKeyLocations = new float[alphaKeys.Length];
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                alphas[i] = alphaKeys[i].alpha;
                alphaKeyLocations[i] = alphaKeys[i].time;
            }

            BetterGradient.GradientToColors(colors, colorKeyLocations, alphas, alphaKeyLocations, ref gradientColors, mode);
            gradientTexture.SetPixels(gradientColors);
            gradientTexture.Apply();
            outputGradientTexture.style.backgroundImage = gradientTexture;
        }

        private void SaveTexture(Texture2D texture)
        {
            if (texture != null)
            {
                var path = EditorUtility.SaveFilePanel("Save Texture", "", "", "png");
                if (!string.IsNullOrEmpty(path))
                {
                    byte[] textureData = texture.EncodeToPNG();
                    System.IO.File.WriteAllBytes(path, textureData);
                }
            }
        }
    }
}
