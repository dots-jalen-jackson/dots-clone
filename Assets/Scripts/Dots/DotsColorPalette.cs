using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Stores the colors that will be used for the Dots in a color palette
/// </summary>
[System.Serializable]
public struct DotsColorPalette
{
    /// <summary>
    /// List of colors that can be edited in the DotsBoard's inspector
    /// </summary>
    public List<Color> colors;
    
    /// <summary>
    /// Calculates random index between 0 and the number of colors in the color palette
    /// </summary>
    /// <returns>Random color in the color palette</returns>
    public Color GetRandomColor()
    {
        int colorIndex = Random.Range(0, colors.Count);
        Color color = colors[colorIndex];
        return color;
    }
}