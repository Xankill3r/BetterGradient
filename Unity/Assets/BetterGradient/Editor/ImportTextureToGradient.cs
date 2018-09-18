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

        Gradient output;
        GradientField outputField;

        public void OnEnable()
        {
            if (output == null)
            {
                output = new Gradient();
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
                text = "Input Texture",
                style =
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    width = 140
                }
            });
            // Todo : Display Texture selector
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
            outputField = new GradientField()
            {
                value = output,
                style =
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    flexGrow = 1
                }
            };
            outputContainer.Add(outputField);
            root.Add(outputContainer);
        }

        private void ApproximateGradient()
        {
        }
    }
}
