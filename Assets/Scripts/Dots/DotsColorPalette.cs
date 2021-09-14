using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct DotsColorPalette
{
    public Color PurpleColor;
    public Color BlueColor;
    public Color RedColor;
    public Color GreenColor;
    public Color YellowColor;
    
    public Color GetRandomColor()
    {
        List<Color> dotsColorPaletteList = ToList();

        int colorIndex = Random.Range(0, dotsColorPaletteList.Count);
        Color color = dotsColorPaletteList[colorIndex];
        return color;
    }

    private List<Color> ToList()
    {
        List<Color> colors = new List<Color>();
        colors.Add(PurpleColor);
        colors.Add(BlueColor);
        colors.Add(RedColor);
        colors.Add(GreenColor);
        colors.Add(YellowColor);

        return colors;
    }
}