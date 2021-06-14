using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Brush to create dynamic footprints on the heighmap terrain. 
/// First, it analyzes the cells to be affected based on the ray-cast and feet colliders.
/// Then, it calculates the displacement per cell based on contact area and character weight / force applied.
/// </summary>
public class PhysicalFootprint : TerrainBrushPhysicalFootprint
{
    #region Variables

    [Header("Physically-based Footprints Deformation")]
    public bool applyFootprints = false;

    [Header("Deformation - Debug")]
    public bool showGridDebugLeft = false;
    public bool showGridDebugRigth = false;
    public bool printTerrainInformation = false;
    public bool printDeformationInformation = false;

    [Header("Deformation - Grid Settings")]
    [Range(0, 20)] public int gridSize = 10;
    [Range(0f, 1f)] public float rayDistance = 0.1f;
    [Range(0f, 1f)] public float offsetRay = 0.04f;

    [Header("Deformation - Number of hits")]
    public int counterHitsLeft;
    public int counterHitsRight;

    [Header("Deformation - Contact Area Feet-Ground")]
    public float areaCell;
    public float areaTotal = 0f;
    public float areaTotalLeft = 0f;
    public float areaTotalRight = 0f;
    private float oldAreaTotalLeft = 0f;
    private float oldAreaTotalRight = 0f;
    private float lenghtCellX;
    private float lenghtCellZ;

    [Header("Deformation - Pressure by feet")]
    public float pressure;
    public float pressureLeft;
    public float pressureRight;

    [Header("Terrain Deformation - Settings")]
    [Range(100000, 10000000)] public double youngModulusPa = 1000000;
    public float originaLength = 1f;

    [Header("Terrain Deformation - Info")]
    public double heightCellDisplacementYoungLeft = 0f;
    public double heightCellDisplacementYoungRight = 0f;
    public float displacementLeft;
    public float displacementRight;
    private double oldHeightCellDisplacementYoungLeft = 0f;
    private double oldHeightCellDisplacementYoungRight = 0f;

    [Header("Filter - STILL TO DO")]
    public bool applyPostFilter = false;
    public bool applyPreFilterLeft = false;
    public bool applyPreFilterRight = false;
    [Range(0, 5)] public int gridSizeKernel = 1;

    // Others - Not used
    private float[,] heightMapLeft;
    private float[,] heightMapRight;
    private float[,] heightMapLeftFiltered;
    private float[,] heightMapRightFiltered;
    private int[,] heightMapLeftBool;
    private int[,] heightMapRightBool;

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

        //       Initial Declarations      //
        // =============================== //

        // Reset counter hits
        counterHitsLeft = 0;
        counterHitsRight = 0;

        // Heightmaps for each foot
        float[,] heightMapLeft = new float[2 * gridSize + 1, 2 * gridSize + 1];
        float[,] heightMapRight = new float[2 * gridSize + 1, 2 * gridSize + 1];
        int[,] heightMapLeftBool = new int[2 * gridSize + 1, 2 * gridSize + 1];
        int[,] heightMapRightBool = new int[2 * gridSize + 1, 2 * gridSize + 1];

        // Warning: Supossing that terrain is squared!
        if (printTerrainInformation)
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

        //    Contact Area Calculation     //
        // =============================== //

        // 2D iteration for both feet
        // It counts the number of hits, save the classified cell in a list and debug ray-casting
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // Calculate each cell position wrt World and Heightmap - Left Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi) - offsetRay, zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // Calculate each cell position wrt World and Heightmap - Right Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi) - offsetRay, zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                //------//

                // Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                //------//

                // If hits the Left Foot, increase counter and add cell to be affected
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, rayDistance))
                {
                    counterHitsLeft++;
                    if (showGridDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * rayDistance, Color.blue);
                }
                else
                {
                    if (showGridDebugLeft)
                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * rayDistance, Color.red);
                }

                // If hits the Right Foot, increase counter and add cell to be affected
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, rayDistance))
                {
                    counterHitsRight++;

                    if (showGridDebugRigth)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * rayDistance, Color.blue);
                }
                else
                {
                    if (showGridDebugRigth)
                        Debug.DrawRay(rayGridWorldRight, Vector3.up * rayDistance, Color.red);
                }
            }
        }

        // Terrain Deformation is affected by an increasing value of the contact area, therefore the deformation
        // will be defined by the maximum contact area in each frame
        oldAreaTotalLeft = ((counterHitsLeft) * areaCell);
        if (oldAreaTotalLeft >= areaTotalLeft)
        {
            areaTotalLeft = ((counterHitsLeft) * areaCell);
        }

        oldAreaTotalRight = ((counterHitsRight) * areaCell);
        if (oldAreaTotalRight >= areaTotalRight)
        {
            areaTotalRight = ((counterHitsRight) * areaCell);
        }

        // Total Area for both feet
        areaTotal = areaTotalLeft + areaTotalRight;

        //       Physics Calculation       //
        // =============================== //

        // Calculate Pressure applicable per frame - if no contact, there is no pressure
        // The three values should be similar, since pressure is based on the contact area

        // Pressure by left feet
        if (counterHitsLeft == 0)
            pressureLeft = 0f;
        else
            pressureLeft = (TotalForceLeftY) / areaTotalLeft;

        // Pressure by right feet
        if (counterHitsRight == 0)
            pressureRight = 0f;
        else
            pressureRight = (TotalForceRightY) / areaTotalRight;

        // Total pressure
        if (counterHitsLeft == 0 || counterHitsRight == 0)
            pressure = 0f;
        else
            pressure = (TotalForceY) / areaTotal;

        //     Deformation Calculation     //
        // =============================== //

        // Given area, pressure and terrain parameters, we calculate the displacement on the terrain
        // The decrement will depend also on the ContactTime used to calculate the corresponding force
        // As for the area, we keep the maximum value

        oldHeightCellDisplacementYoungLeft = pressureLeft * (originaLength / (youngModulusPa));
        if (oldHeightCellDisplacementYoungLeft >= heightCellDisplacementYoungLeft)
        {
            heightCellDisplacementYoungLeft = pressureLeft * (originaLength / youngModulusPa);

        }

        oldHeightCellDisplacementYoungRight = pressureRight * (originaLength / (youngModulusPa));
        if (oldHeightCellDisplacementYoungRight >= heightCellDisplacementYoungRight)
        {
            heightCellDisplacementYoungRight = pressureRight * (originaLength / youngModulusPa);
        }

        // Given the entire deformation in Y, we calculate the corresponding frame-based deformation based on the frame-time.
        displacementLeft = (Time.deltaTime * (float)heightCellDisplacementYoungLeft) / ContactTime;
        displacementRight = (Time.deltaTime * (float)heightCellDisplacementYoungRight) / ContactTime;

        //        Apply Deformation        //
        // =============================== //

        // 2D iteration Deformation
        // Once we have the displacement, we saved the actual result of applying it to the terrain (only when the foot is grounded)
        if (IsLeftFootGrounded)
        {
            StartCoroutine(DecreaseTerrainLeft(heightMapLeft, heightMapLeftBool, xLeft, zLeft));
        }
        else if(!IsLeftFootGrounded)
        {
            // Every time we lift the foot, we reset the variables and stop the coroutines.
            heightCellDisplacementYoungLeft = 0;
            StopAllCoroutines();
        }

        if (IsRightFootGrounded)
        {
            StartCoroutine(DecreaseTerrainRight(heightMapRight, heightMapRightBool, xRight, zRight));
        }
        else if(!IsRightFootGrounded)
        {
            heightCellDisplacementYoungRight = 0;
            StopAllCoroutines();
        }
    }

    IEnumerator DecreaseTerrainLeft(float[,] heightMapLeft, int[,] heightMapLeftBool, int xLeft, int zLeft)
    {
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // Calculate each cell position wrt World and Heightmap - Left Foot
                Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                // Create each ray for the grid (wrt World) - Left
                RaycastHit leftFootHit;
                Ray upRayLeftFoot = new Ray(rayGridWorldLeft, Vector3.up);

                // If hits the Left Foot, increase counter and add cell to be affected
                if (LeftFootCollider.Raycast(upRayLeftFoot, out leftFootHit, rayDistance))
                {
                    // Cell contacting directly
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 2;

                    if (terrain.Get(rayGridLeft.x, rayGridLeft.z) >= terrain.GetConstant(rayGridLeft.x, rayGridLeft.z) - heightCellDisplacementYoungLeft)
                    {
                        heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z) - (displacementLeft);
                    }
                    else
                    {
                        heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                    }

                }
                else
                {
                    // No contact
                    heightMapLeftBool[zi + gridSize, xi + gridSize] = 0;
                    heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z);
                }
            }
        }

        // 2. Gaussian pre-filter for Left Foot (To improve)
        if (applyPreFilterLeft)
        {
            heightMapLeft = FilterBufferLeft(heightMapLeft, heightMapLeftBool);
        }

        // 3. Save terrain
        if (applyFootprints)
        {
            for (int zi = -gridSize; zi <= gridSize; zi++)
            {
                for (int xi = -gridSize; xi <= gridSize; xi++)
                {
                    Vector3 rayGridLeft = new Vector3(xLeft + xi, terrain.Get(xLeft + xi, zLeft + zi), zLeft + zi);
                    terrain.Set(rayGridLeft.x, rayGridLeft.z, heightMapLeft[zi + gridSize, xi + gridSize]);
                }
            }
        }

        yield return null;
    }

    IEnumerator DecreaseTerrainRight(float[,] heightMapRight, int[,] heightMapRightBool, int xRight, int zRight)
    {
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // Calculate each cell position wrt World and Heightmap - Right Foot
                Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                // Create each ray for the grid (wrt World) - Right
                RaycastHit rightFootHit;
                Ray upRayRightFoot = new Ray(rayGridWorldRight, Vector3.up);

                // If hits the Right Foot, increase counter and add cell to be affected
                if (RightFootCollider.Raycast(upRayRightFoot, out rightFootHit, rayDistance))
                {
                    // Cell contacting directly
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 2;

                    if (terrain.Get(rayGridRight.x, rayGridRight.z) >= terrain.GetConstant(rayGridRight.x, rayGridRight.z) - heightCellDisplacementYoungRight)
                    {
                        heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z) - (displacementRight);
                    }
                    else
                    {
                        heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                    }
                }
                else
                {
                    // No contact
                    heightMapRightBool[zi + gridSize, xi + gridSize] = 0;
                    heightMapRight[zi + gridSize, xi + gridSize] = terrain.Get(rayGridRight.x, rayGridRight.z);
                }
            }
        }

        // 2. Gaussian pre-filter for Right Foot (To improve)
        if (applyPreFilterRight)
        {
            heightMapRight = FilterBufferRight(heightMapRight, heightMapRightBool);
        }

        // 3. Save terrain
        if (applyFootprints)
        {

            for (int zi = -gridSize; zi <= gridSize; zi++)
            {
                for (int xi = -gridSize; xi <= gridSize; xi++)
                {
                    Vector3 rayGridRight = new Vector3(xRight + xi, terrain.Get(xRight + xi, zRight + zi), zRight + zi);
                    terrain.Set(rayGridRight.x, rayGridRight.z, heightMapRight[zi + gridSize, xi + gridSize]);
                }
            }
        }

        yield return null;
    }

    // Pre-filter Methods

    private float[,] FilterBufferLeft(float[,] heightMapLeft, int[,] heightMapLeftBool)
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
                    float n = 2.0f * gridSizeKernel + 1.0f;
                    float sum = 0;

                    for (int szi = -gridSizeKernel; szi <= gridSizeKernel; szi++)
                    {
                        for (int sxi = -gridSizeKernel; sxi <= gridSizeKernel; sxi++)
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

    private float[,] FilterBufferRight(float[,] heightMapRight, int[,] heightMapRightBool)
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
                    float n = 2.0f * gridSizeKernel + 1.0f;
                    float sum = 0;

                    for (int szi = -gridSizeKernel; szi <= gridSizeKernel; szi++)
                    {
                        for (int sxi = -gridSizeKernel; sxi <= gridSizeKernel; sxi++)
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

    // Post-filter Methods

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
}
