/****************************************************
 * File: TerrainBrushPhysicalFootprintSphere.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

/* Code can be not updated */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class to call brush for footprint drawing
/// </summary>
public abstract class TerrainBrushPhysicalFootprintSphere : BrushPhysicalFootprintSphere
{
    public override void CallFootprint(float x, float z)
    {
        // 1. 2. Pass the positions though here to convert them wrt Heightmap before calling final brush
        Vector3 gridSphere = terrain.World2Grid(x, z);

        // 3. Call Footprint method and filter it 
        DrawFootprint((int)gridSphere.x, (int)gridSphere.z);

        // 4. Save the terrain
        terrain.Save();
    }

    public override void DrawFootprint(float x, float z)
    {
        DrawFootprint((int)x, (int)z);
    }
}
