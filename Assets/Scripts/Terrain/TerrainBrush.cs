using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainBrush : Brush
{
    public override void CallFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        // Pass the positions though here to convert them wrt Heightmap before calling final brush
        Vector3 gridLeft = terrain.World2Grid(xLeft, zLeft);
        Vector3 gridRight = terrain.World2Grid(xRight, zRight);

        // Call Footprint method and filter it 
        DrawFootprint((int)gridLeft.x, (int)gridLeft.z, (int)gridRight.x, (int)gridRight.z);

        // Save the terrain
        terrain.Save();
    }

    public override void DrawFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawFootprint((int)xLeft, (int)zLeft, (int)xRight, (int)zRight);
    }

}
