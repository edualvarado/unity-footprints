/****************************************************
 * File: TerrainBrushPhysicalFootprint.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Physically-driven Footprints Generation for Real-Time Interactions between a Character and Deformable Terrains
*****************************************************/

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
