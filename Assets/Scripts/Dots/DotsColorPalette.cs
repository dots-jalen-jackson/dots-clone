using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public struct DotsColorPalette
{
    public List<Color> Colors;
    
    public Color GetRandomColor()
    {
        int colorIndex = Random.Range(0, Colors.Count);
        Color color = Colors[colorIndex];
        return color;
    }
}