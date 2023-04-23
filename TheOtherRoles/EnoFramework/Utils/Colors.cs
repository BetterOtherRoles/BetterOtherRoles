using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace TheOtherRoles.EnoFramework.Utils;

public static class Colors
{
    public static readonly Color Crewmate = FromHex("#5cebed");
    public static readonly Color Neutral = FromHex("#384747");
    public static readonly Color Impostor = FromHex("#cf0000");
    public static readonly Color Modifier = FromHex("#cad40b");
    
    public static string Cs(string hexColor, string text)
    {
        return Cs(FromHex(hexColor), text);
    }

    public static string Cs(Color c, string s)
    {
        return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
    }

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte) (f * 255);
    }

    public static Color Blend(List<Color> clrArr)
    {
        var r = 0f;
        var g = 0f;
        var b = 0f;
        foreach (var color in clrArr)
        {
            r += color.r;
            g += color.g;
            b += color.b;
        }

        r /= clrArr.Count;
        g /= clrArr.Count;
        b /= clrArr.Count;
        return new Color(r, g, b);
    }

    public static Color FromHex(string hexColor, int alpha = 255)
    {
        if (hexColor.IndexOf('#') != -1)
            hexColor = hexColor.Replace("#", string.Empty);

        var red = 0;
        var green = 0;
        var blue = 0;

        switch (hexColor.Length)
        {
            case 6:
                red = int.Parse(hexColor.AsSpan(0, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                green = int.Parse(hexColor.AsSpan(2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                blue = int.Parse(hexColor.AsSpan(4, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                break;
            case 3:
                red = int.Parse(
                    hexColor[0] + hexColor[0].ToString(),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                green = int.Parse(
                    hexColor[1] + hexColor[1].ToString(),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                blue = int.Parse(
                    hexColor[2] + hexColor[2].ToString(),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture);
                break;
        }

        return new Color(red / 255f, green / 255f, blue / 255f, alpha / 255f);
    }
}