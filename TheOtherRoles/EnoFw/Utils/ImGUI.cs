using System;
using UnityEngine;

namespace TheOtherRoles.EnoFw.Utils;

public static class ImGUI
{
    public static readonly Color PrimaryColor = Colors.FromHex("#2f3136");
    public static readonly Color SecondaryColor = Colors.FromHex("#b9bbbe");
    public static readonly Color DarkColor = Colors.FromHex("#252529");
    public static readonly Color GrayColor = Colors.FromHex("#40444b");

    public static readonly Texture2D PrimaryTexture = Styles.Colored(PrimaryColor);
    public static readonly Texture2D SecondaryTexture = Styles.Colored(SecondaryColor);
    public static readonly Texture2D DarkTexture = Styles.Colored(DarkColor);
    public static readonly Texture2D GrayTexture = Styles.Colored(GrayColor);

    public class Styles
    {
        public static readonly Styles Instance = new();
        
        public readonly GUIStyle TitleLabel = new(GUI.skin.label)
        {
            fontSize = 20,
            normal =
            {
                textColor = SecondaryColor
            },
            alignment = TextAnchor.UpperLeft,
        };

        public readonly GUIStyle HeadingLabel = new(GUI.skin.label)
        {
            fontSize = 22,
            normal =
            {
                textColor = SecondaryColor
            },
            padding =
            {
                left = 20,
            },
            alignment = TextAnchor.MiddleLeft,
            fontStyle = FontStyle.Bold,
        };

        public readonly GUIStyle InvisibleButton = new(GUI.skin.button)
        {
            padding =
            {
                top = 0,
                right = 0,
                bottom = 0,
                left = 0,
            },
            fontSize = 40,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = SecondaryColor,
            },
            hover =
            {
                textColor = SecondaryColor,
            },
            focused =
            {
                textColor = SecondaryColor,
            },
            active =
            {
                textColor = SecondaryColor,
            },
        };

        public readonly GUIStyle OptionLabel = new(GUI.skin.label)
        {
            fontSize = 18,
            normal =
            {
                textColor = SecondaryColor
            }
        };

        public static Texture2D Colored(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply(true, false);
            return texture;
        }
    }
    
    public static class MainStyles
    {
        private static bool _initialized;
        
        private static GUIStyleState _windowStyleState;

        private static void CreateStyles()
        {
            if (_initialized) return;
            _windowStyleState = new GUIStyleState();
            _windowStyleState.background = new Texture2D(0, 0);
            _windowStyleState.background.SetPixel(0, 0, DarkColor);
            _windowStyleState.background.Apply();
        }

        public static GUIStyleState WindowStyleState
        {
            get
            {
                CreateStyles();
                return _windowStyleState;
            }
        }
    }
}