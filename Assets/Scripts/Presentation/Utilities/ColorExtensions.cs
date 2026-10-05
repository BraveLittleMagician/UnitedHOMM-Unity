#nullable enable

using System;
using UnityEngine;

public static class ColorExtensions
{
    public static Color HslToColor(double h, double s, double l)
    {
        h = ((h % 360) + 360) % 360;
        s = Math.Clamp(s, 0.0, 1.0);
        l = Math.Clamp(l, 0.0, 1.0);

        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double hp = h / 60.0;
        double m = l - c / 2;

        double r1 = Math.Clamp(Math.Abs(hp - 3) - 1, 0, 1);
        double g1 = Math.Clamp(2 - Math.Abs(hp - 2), 0, 1);
        double b1 = Math.Clamp(2 - Math.Abs(hp - 4), 0, 1);

        byte r = (byte)Math.Round((r1 * c + m) * 255);
        byte g = (byte)Math.Round((g1 * c + m) * 255);
        byte b = (byte)Math.Round((b1 * c + m) * 255);

        return new Color(r / 255f, g / 255f, b / 255f);
    }
}
