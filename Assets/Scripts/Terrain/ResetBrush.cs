using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Brush to fix terrain to certain height.
/// </summary>
public class ResetBrush : TerrainBrush
{
    #region Variables

    [Header("Reset Brush Settings")]
    [Range(0f, 10f)] public float heigthTerrain = 1f;

    #endregion

    /// <summary>
    /// Override the method to set certain height.
    /// </summary>
    /// <param name="xLeft"></param>
    /// <param name="zLeft"></param>
    /// <param name="xRight"></param>
    /// <param name="zRight"></param>
    public override void DrawFootprint(int xLeft, int zLeft, int xRight, int zRight)
    {
        // Left Foot 
        for (int zi = -10; zi <= 10; zi++)
        {
            for (int xi = -10; xi <= 10; xi++)
            {
                Vector3 rayGridLeft = new Vector3(xLeft + xi, HeightIKRight, zLeft + zi);
                terrain.Set(rayGridLeft.x, rayGridLeft.z, 1.0f);
            }
        }

        // Right Foot 
        for (int zi = -10; zi <= 10; zi++)
        {
            for (int xi = -10; xi <= 10; xi++)
            {
                Vector3 rayGridRight = new Vector3(xRight + xi, HeightIKRight, zRight + zi);
                terrain.Set(rayGridRight.x, rayGridRight.z, 1.0f);
            }
        }
    }
}
