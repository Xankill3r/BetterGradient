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
    public class ImportTextureToGradient : EditorWindow
    {
        [MenuItem("BetterGradient/Texture To Gradient")]
        public static void Display()
        {
            GetWindow<ImportTextureToGradient>();
        }

        Texture2D inputTexture;

        Gradient approxOutput;
        GradientField outputGradientField;
        List<GradientColorKey> approxColorKeys = new List<GradientColorKey>(8);
        List<GradientAlphaKey> approxAlphaKeys = new List<GradientAlphaKey>(8);

        public void OnEnable()
        {
            if (approxOutput == null)
            {
                approxOutput = new Gradient();
            }
            if (inputTexture == null)
            {
                inputTexture = Texture2D.whiteTexture;
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
            // Display Texture selector
            root.Add(new IMGUIContainer(OnTextureSelectorGUI)
            {
                style =
                {
                    width = 200,
                    height = 50,
                    marginLeft = 10,
                    marginRight = 10,
                    marginTop = 10
                }
            });
            root.Add(inputContainer);

            var button = new Button(ApproximateGradient)
            {
                text = "Approximate",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                }
            };
            root.Add(button);

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
                text = "Output Gradient",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    width = 140
                }
            });
            // GradientField
            outputGradientField = new GradientField()
            {
                value = approxOutput,
                style =
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    flexGrow = 1
                }
            };
            outputContainer.Add(outputGradientField);
            root.Add(outputContainer);
        }
        void OnTextureSelectorGUI()
        {
            inputTexture = EditorGUILayout.ObjectField("Input Texture", inputTexture, typeof(Texture2D), false) as Texture2D;
        }

        private void ApproximateGradient()
        {
            var gradientColors = inputTexture.GetPixels(0, 0, inputTexture.width, 1);

            var error = BetterGradient.ApproximateColorArrayAsGradient(gradientColors, approxOutput, approxColorKeys, approxAlphaKeys);
            Debug.Log(error);
            outputGradientField.value = approxOutput;
        }
    }
}
