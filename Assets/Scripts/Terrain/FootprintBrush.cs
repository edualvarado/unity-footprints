using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Brush to create dynamic footprints on the heighmap terrain. 
/// First, it analyzes the cells to be affected based on the ray-cast and feet colliders.
/// Then, it calculates the displacement per cell based on contact area and character weight / force applied.
/// </summary>
public class FootprintBrush : TerrainBrush
{
    #region Variables

    [Header("Footprint Grid Settings")]
    [Range(0, 10)] public int gridSize = 10;
    [Range(0f, 2f)] public float rayDistance = 0.03f;
    //[Range(0f, 10f)] public float heightFootprintChange = 0.1f;

    [Header("Options")]
    public bool applyNewTerrainModification = false;
    public bool showGridDebug = false;
    public bool printFootprintsInformation = false;
    public bool printDeformationInformation = false;
    //public bool applyTerrainModification = false;

    [Header("Hits")]
    public int counterHitsLeft;
    public int counterHitsRight;

    [Header("Contact Area")]
    public float lenghtCellX;
    public float lenghtCellZ;
    public float areaCell;
    public float areaTotal;
    public float testDistance = 1f;

    [Header("Pressure")]
    public float pressure;
    public float heightCellDisplacement = 0f;
    //public bool useFixedPressureRange;
    //public float force = 50f;

    [Header("Filter")]
    public bool applyPostFilter = false;
    public bool applySmoothLeft = false;
    public bool applySmoothRight = false;
    [Range(0, 10)] public int gridSizeSmooth = 10;

    private float maxPressure;
    private float minPressure;

    [Header("Cells to be affected")]
    private List<Vector3> cellsAffected = new List<Vector3>();
    private List<Vector3> cellsNotAffected = new List<Vector3>();
    private List<Vector3> heightMapLeftList = new List<Vector3>();
    private List<Vector3> heightMapLeftFilteredList = new List<Vector3>();

    public float[,] heightMapLeft;
    public float[,] heightMapRight;
    public float[,] heightMapLeftFiltered;
    public float[,] heightMapRightFiltered;
    public int[,] heightMapLeftBool;
    public int[,] heightMapRightBool;

    #endregion

    /// <summary>
    /// Method that takes the IK positions for each feet and apply displacement to ground.
    /// </summary>
    /// <param name="xLeft"></param>
    /// <param name="zLeft"></param>
    /// <param name="xRight"></param>
    /// <param name="zRight"></param>
    public override void DrawFootprint(int xLeft, int zLeft, int xRight, int zRight)
    {
        // Reset counter hits
        counterHitsLeft = 0;
        counterHitsRight = 0;

        // Clear cells to be affected
        cellsAffected.Clear();

        // New
        float[,] heightMapLeft = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] heightMapRight = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] heightMapLeftFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] heightMapRightFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];
        int[,] heightMapLeftBool = new int[2 * gridSize + 1, 2 * gridSize + 1];
        int[,] heightMapRightBool = new int[2 * gridSize + 1, 2 * gridSize + 1];

        // Warning: Supossing that terrain is squared!
        if (printFootprintsInformation)
        {
            Debug.Log("[INFO] Length Terrain - X: " + terrain.TerrainSize().x);
            Debug.Log("[INFO] Length Terrain - Z: " + terrain.TerrainSize().z);
            Debug.Log("[INFO] Number of heightmap cells: " + (terrain.GridSize().x - 1));
            Debug.Log("[INFO] Lenght of one cell - X: " + (terrain.TerrainSize().x / (terrain.GridSize().x - 1)));
            Debug.Log("[INFO] Lenght of one cell - Z: " + (terrain.TerrainSize().z / (terrain.GridSize().z - 1)));
            Debug.Log("[INFO] Area of one cell: " + (terrain.TerrainSize().x / (terrain.GridSize().x - 1)) * (terrain.TerrainSize().z / (terrain.GridSize().z - 1)));
        }

        // Calculate area per cell outside the loop
        lenghtCellX = terrain.TerrainSize().x / (terrain.GridSize().x - 1);
        lenghtCellZ = terrain.TerrainSize().z / (terrain.GridSize().z - 1);
        areaCell = lenghtCellX * lenghtCellZ;

        // Max. applicable pressure
        maxPressure = Force / areaCell;

        // TODO: Calculate automatically
        // Min. applicable pressure (IDLE position, gets 40 hits)
        minPressure = Force / (areaCell * 40);

        // 2D iteration for Both Foot
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // Calculate each cell position wrt World and Heightmap - Left Foot
                Vector3 rayGridLeft = new Vector3(xLeft + xi, HeightIKLeft, zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // Calculate each cell position wrt World and Heightmap - Right Foot
                Vector3 rayGridRight = new Vector3(xRight + xi, HeightIKRight, zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                // Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                // If hits the Left Foot, increase counter and add cell to be affected
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, rayDistance))
                {
                    counterHitsLeft++;
                    cellsAffected.Add(rayGridLeft);

                    if (showGridDebug)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * rayDistance, Color.blue);
                }
                else
                {
                    cellsNotAffected.Add(rayGridLeft);

                    if (showGridDebug)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * rayDistance, Color.red);

                }

                // If hits the Right Foot, increase counter and add cell to be affected
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, rayDistance))
                {
                    counterHitsRight++;
                    cellsAffected.Add(rayGridRight);

                    if (showGridDebug)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * rayDistance, Color.blue);
                }
                else
                {
                    cellsNotAffected.Add(rayGridRight);

                    if (showGridDebug)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * rayDistance, Color.red);
                }
            }
        }

        // Calculate the pressure per cell that we need to apply given the force and contact area
        areaTotal = ((counterHitsLeft + counterHitsRight) * areaCell);
        pressure = Force / areaTotal;

        // TODO: See how pressure should affect realistically based on material
        // PROVISIONAL -> NEED TO TACKLE MATERIAL: COMPRESIBILITY AND OTHERS
        float normalizedValuePressure = Mathf.InverseLerp(0, 75000, pressure);
        heightCellDisplacement = Mathf.Lerp(0f, 0.15f, normalizedValuePressure);

        if (printFootprintsInformation)
        {
            Debug.Log("[INFO] Counter Hits Left: " + counterHitsLeft);
            Debug.Log("[INFO] Counter Hits Right: " + counterHitsRight);
            Debug.Log("[INFO] Total Contact Area: " + areaTotal);
            Debug.Log("[INFO] Current Force: " + Force);
            Debug.Log("[INFO] Pressure/Cell NOW: " + pressure);
            Debug.Log("[INFO] Min Pressure: " + minPressure);
            Debug.Log("[INFO] Max Pressure: " + maxPressure);
        }

        if (printDeformationInformation)
        {
            Debug.Log("normalizedValuePressure: " + normalizedValuePressure);
            Debug.Log("heightCellDisplacement: " + heightCellDisplacement);
        }

        // 2D iteration Deformation
        // Once we have the displacement based on the weight, we saved the actual result of applying it to the terrain
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // Calculate each cell position wrt World and Heightmap - Left Foot
                Vector3 rayGridLeft = new Vector3(xLeft + xi, HeightIKLeft, zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // Calculate each cell position wrt World and Heightmap - Right Foot
                Vector3 rayGridRight = new Vector3(xRight + xi, HeightIKRight, zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                // Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                // If hits the Left Foot, increase counter and add cell to be affected
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, rayDistance))
                {
                    // Cell contacting directly
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 2;

                    heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z) - heightCellDisplacement;
                }
                else
                {
                    // No contact
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 0;

                    heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                }

                // If hits the Right Foot, increase counter and add cell to be affected
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, rayDistance))
                {
                    heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z) - heightCellDisplacement;
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 2;
                }
                else
                {
                    heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 0;
                }
            }
        }

        // Pre-filter - TEST //
        ///////////////////////

        // Gaussian filter
        if (applySmoothLeft)
        {
            heightMapLeft = FilterBuffetLeft(heightMapLeft, heightMapLeftBool);
        }

        // Gaussian filter
        if (applySmoothRight)
        {
            heightMapRight = FilterBuffetRight(heightMapRight, heightMapRightBool);
        }

        // Deformation - Method using array //
        //////////////////////////////////////

        if (applyNewTerrainModification)
        {
            for (int zi = -gridSize; zi <= gridSize; zi++)
            {
                for (int xi = -gridSize; xi <= gridSize; xi++)
                {
                    Vector3 rayGridLeft = new Vector3(xLeft + xi, HeightIKLeft, zLeft + zi);
                    Vector3 rayGridRight = new Vector3(xRight + xi, HeightIKRight, zRight + zi);

                    if (terrain.Get(rayGridLeft.x, rayGridLeft.z) >= terrain.GetConstant(rayGridLeft.x, rayGridLeft.z) - heightCellDisplacement)
                    {
                        terrain.Set(rayGridLeft.x, rayGridLeft.z, heightMapLeft[zi + gridSize, xi + gridSize]);
                    }

                    if (terrain.Get(rayGridRight.x, rayGridRight.z) >= terrain.GetConstant(rayGridRight.x, rayGridRight.z) - heightCellDisplacement)
                    {
                        terrain.Set(rayGridRight.x, rayGridRight.z, heightMapRight[zi + gridSize, xi + gridSize]);
                    }
                }
            }
        }

        // Post-filter - TEST //
        ////////////////////////

        if (applyPostFilter)
        {
            // Current height-map, including the footprints deformations
            // If refer to a cell, remember to multiply by * TerrainData.heightmapScale.y
            float[,] currentHeightMap = TerrainData.GetHeights(0, 0, (int)terrain.GridSize().x, (int)terrain.GridSize().z);

            // I provide the initial//current height-map to be filtered (for each foot? maybe more efficient way doing only once?)
            FilterHeightmap(zLeft, xLeft, currentHeightMap);
            FilterHeightmap(zRight, xRight, currentHeightMap);

            // Recursive filtering n times
            for (int i = 0; i < 5; i++)
            {
                FilterHeightmap(zLeft, xLeft, HeightMapFiltered);
                FilterHeightmap(zRight, xRight, HeightMapFiltered);
            }

            // Saves the height-map
            Debug.Log("Applying filter");
            TerrainData.SetHeights(0, 0, HeightMapFiltered);

        }
    }

    // Method used for post-filtering
    private void FilterHeightmap(int zLeft, int xLeft, float[,] heightmap)
    {
        float[,] result = TerrainData.GetHeights(0, 0, (int)terrain.GridSize().x, (int)terrain.GridSize().z);

        for (int zi = -gridSize; zi < gridSize; zi++)
        {
            for (int xi = -gridSize; xi < gridSize; xi++)
            {
                result[zLeft + zi, xLeft + xi] = 
                    heightmap[zLeft + zi - 2, xLeft + xi - 2]
                    + 4 * heightmap[zLeft + zi - 2, xLeft + xi - 1]
                    + 6 * heightmap[zLeft + zi - 2, xLeft + xi]
                    + heightmap[zLeft + zi - 2, xLeft + xi + 2]
                    + 4 * heightmap[zLeft + zi - 2, xLeft + xi + 1]
                    + 4 * heightmap[zLeft + zi - 1, xLeft + xi + 2]
                    + 16 * heightmap[zLeft + zi - 1, xLeft + xi + 1]
                    + 4 * heightmap[zLeft + zi - 1, xLeft + xi - 2]
                    + 16 * heightmap[zLeft + zi - 1, xLeft + xi - 1]
                    + 24 * heightmap[zLeft + zi - 1, xLeft + xi]
                    + 6 * heightmap[zLeft + zi, xLeft + xi - 2]
                    + 24 * heightmap[zLeft + zi, xLeft + xi - 1]
                    + 6 * heightmap[zLeft + zi, xLeft + xi + 2]
                    + 24 * heightmap[zLeft + zi, xLeft + xi + 1]
                    + 36 * heightmap[zLeft + zi, xLeft + xi]
                    + heightmap[zLeft + zi + 2, xLeft + xi - 2]
                    + 4 * heightmap[zLeft + zi + 2, xLeft + xi - 1]
                    + 6 * heightmap[zLeft + zi + 2, xLeft + xi]
                    + heightmap[zLeft + zi + 2, xLeft + xi + 2]
                    + 4 * heightmap[zLeft + zi + 2, xLeft + xi + 1]
                    + 4 * heightmap[zLeft + zi + 1, xLeft + xi + 2]
                    + 16 * heightmap[zLeft + zi + 1, xLeft + xi + 1]
                    + 4 * heightmap[zLeft + zi + 1, xLeft + xi - 2]
                    + 16 * heightmap[zLeft + zi + 1, xLeft + xi - 1]
                    + 24 * heightmap[zLeft + zi + 1, xLeft + xi];

                result[zLeft + zi, xLeft + xi] *= 1.0f / 256.0f;
            }
        }

        HeightMapFiltered = result;
    }

    // Both methods used for pre-filtering
    private float[,] FilterBuffetLeft(float[,] heightMapLeft, int[,] heightMapLeftBool)
    {
        float[,] heightMapLeftFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                if (heightMapLeftBool[zi + gridSize, xi + gridSize] == 0)
                {
                    heightMapLeftFiltered[zi + gridSize, xi + gridSize] = heightMapLeft[zi + gridSize, xi + gridSize];
                }
                else
                {
                    float n = 2.0f * gridSizeSmooth + 1.0f;
                    float sum = 0;

                    for (int szi = -gridSizeSmooth; szi <= gridSizeSmooth; szi++)
                    {
                        for (int sxi = -gridSizeSmooth; sxi <= gridSizeSmooth; sxi++)
                        {
                            sum += heightMapLeft[gridSize + szi, gridSize + sxi];
                        }
                    }

                    heightMapLeftFiltered[zi + gridSize, xi + gridSize] = sum / (n * n);
                }
            }
        }

        return heightMapLeftFiltered;
    }

    private float[,] FilterBuffetRight(float[,] heightMapRight, int[,] heightMapRightBool)
    {
        float[,] heightMapRightFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                if (heightMapRightBool[zi + gridSize, xi + gridSize] == 0)
                {
                    heightMapRightFiltered[zi + gridSize, xi + gridSize] = heightMapRight[zi + gridSize, xi + gridSize];
                }
                else
                {
                    float n = 2.0f * gridSizeSmooth + 1.0f;
                    float sum = 0;

                    for (int szi = -gridSizeSmooth; szi <= gridSizeSmooth; szi++)
                    {
                        for (int sxi = -gridSizeSmooth; sxi <= gridSizeSmooth; sxi++)
                        {
                            sum += heightMapRight[gridSize + szi, gridSize + sxi];
                        }
                    }

                    heightMapRightFiltered[zi + gridSize, xi + gridSize] = sum / (n * n);
                }
            }
        }

        return heightMapRightFiltered;
    }

}
