#nullable enable

using UnityEngine;

public static class PieceColorPalette
{
    public static Color GetColor(IdOfPlayer player, int teamCount, int subteamCount)
    {
        if (teamCount == 1 && subteamCount == 1) return Color.white;
        var (hue, lightness) = GetHueAndLightness(player, teamCount, subteamCount);
        return ColorExtensions.HslToColor(hue, 1, lightness);
    }

    private static (double Hue, double Lightness) GetHueAndLightness(
        IdOfPlayer index, int teamCount, int subteamCount)
    {
        int half = teamCount / 2;
        double hueOffset = 60.0;
        double step = 360.0 / Mathf.Max(teamCount, 1);
        double v = subteamCount <= 1 ? 0.0 : index.IndexOfPlayerOnSide / (double)(subteamCount - 1);
        double hue;
        double u;
        double lightness;
        if (index.IndexOfSide < half)
        {
            u = half <= 1 ? 0.0 : index.IndexOfSide / (double)(half - 1);
            hue = hueOffset - index.IndexOfSide * step;
            lightness = 1.0 - (u + v) * 0.25;
        }
        else
        {
            int localIndex = index.IndexOfSide - half;
            int localCount = teamCount - half;
            int reversedLocalIndex = localCount - 1 - localIndex;
            u = localCount <= 1 ? 1.0 : reversedLocalIndex / (double)(localCount - 1);
            double vRev = 1.0 - v;
            hue = 240.0 - localIndex * step;
            lightness = 0.5 - (u + vRev) * 0.25;
        }
        return (hue, lightness);
    }
}