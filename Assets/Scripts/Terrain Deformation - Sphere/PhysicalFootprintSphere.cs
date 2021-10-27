/****************************************************
 * File: PhysicalFootprintSphere.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Physically-driven Footprints Generation for Real-Time Interactions between a Character and Deformable Terrains
*****************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Brush to create dynamic footprints on the heighmap terrain. 
/// First, it analyzes the cells to be affected based on the ray-cast and feet colliders.
/// Then, it calculates the displacement per cell based on contact area and character weight / force applied.
/// </summary>
public class PhysicalFootprintSphere : TerrainBrushPhysicalFootprintSphere
{
    #region Variables

    [Header("Physically-based Footprints Deformation - (SET UP)")]
    public bool applyFootprints = false;
    public bool applyBumps = false;

    [Header("Terrain Deformation - (SET UP)")]
    [Range(100000, 1000000)] public double youngModulus = 1000000;
    [Range(0, 0.5f)] public float poissonR = 0.4f;
    public float originalLength = 30f;

    [Header("Grids - (SET UP)")]
    [Range(0, 20)] public int gridSize = 10;
    [Range(0f, 1f)] public float rayDistance = 0.1f;
    [Range(0f, 1f)] public float offsetRay = 0.04f;

    [Header("Bump Deformation - (SET UP)")]
    public int offsetBumpGrid = 2;
    public int neighboursSearchArea = 2;

    [Header("Gaussian Filtering - (SET UP)")]
    public bool applyFilterSphere = false;
    public int filterIterationsGaussSphere = 15;
    public int marginAroundGrid = 3;
    private int filterIterationsSphereCounter = 0;
    private bool isFilteredSphere = false;

    [Header("Grids - Debug")]
    [Space(20)]
    public bool showGridDebugSphere = false;
    public bool showGridBumpDebug = false;
    //public bool showGridBumpFrontBack = false;
    public bool printTerrainInformation = false;
    public bool printDeformationInformation = false;

    [Header("Grids - Number of hits")]
    public int counterHitsSphere;
    public int neighbourCellsSphere;
    public List<Vector3> neighboursPositionsSphere = new List<Vector3>();
    //public List<Vector3> neighboursPositionsSphereFront = new List<Vector3>();
    //public List<Vector3> neighboursPositionsSphereBack = new List<Vector3>();

    [Header("Grids - Contact Area Feet-Ground")]
    public float areaCell;
    public float areaTotal = 0f;
    public float areaTotalSphere = 0f;
    public float neighbourAreaTotalSphere;
    private float oldAreaTotalSphere = 0f;
    private float lenghtCellX;
    private float lenghtCellZ;

    [Header("Terrain Deformation - Pressure")]
    [Space(20)]
    public float pressureStressSphere;

    [Header("Terrain Deformation - Displacement")]
    public double heightCellDisplacementYoungSphere = 0f;
    public float displacementSphere;
    private double oldHeightCellDisplacementYoungSphere = 0f;

    [Header("Bump Deformation - Displacement")]
    public double newBumpHeightDeformationSphere = 0f;
    public float bumpDisplacementSphere;
    private float bumpDisplacementLeftFront;
    private float oldNeighbourAreaTotalSphere;

    [Header("Terrain Deformation - Volume Rod Approximation")]
    public double volumeOriginalSphere = 0f; // Original volume under left foot
    public double volumeTotalSphere = 0f; // Volume left after deformation
    public double volumeVariationPoissonSphere; // Volume change due to compressibility of the material
    public double volumeDifferenceSphere; // Volume difference pre/post deformation without taking into account compressibility
    public double volumeNetDifferenceSphere; // Volume difference pre/post deformation taking into account compressibility
    public double volumeCellSphere; // Volume/cell distributed over countour

    [Header("Bump Deformation - Settings - (CONFIG)")]
    [Space(20)]
    public bool useManualBumpDeformation = false;
    [Range(0, 0.05f)] public double bumpHeightDeformation = 0.03f; // In case one wants to do it manually

    // Others
    [Range(0, 5)] private int gridSizeKernel = 1;
    private float[,] heightMapLeft;
    private float[,] heightMapRight;
    private float[,] heightMapLeftFiltered;
    private float[,] heightMapRightFiltered;
    private int[,] heightMapLeftBool;
    private int[,] heightMapRightBool;

    // AUX
    [Header("AUX!")]
    public bool startCoroutineNow = false;

    #endregion

    /// <summary>
    /// Method that takes the IK positions for each feet and apply displacement to ground.
    /// </summary>
    /// <param name="xLeft"></param>
    /// <param name="zLeft"></param>
    /// <param name="xRight"></param>
    /// <param name="zRight"></param>
    public override void DrawFootprint(int x, int z)
    {

        //       Initial Declarations      //
        // =============================== //

        // 3. Reset counter hits
        counterHitsSphere = 0;
        neighbourCellsSphere = 0;

        // 3. Reset lists
        neighboursPositionsSphere.Clear();

        // 4. Heightmaps for each foot
        float[,] heightMapSphere = new float[2 * gridSize + 1, 2 * gridSize + 1];
        int[,] heightMapSphereBool = new int[2 * gridSize + 1, 2 * gridSize + 1];

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

        // 4. Calculate area per cell outside the loop
        lenghtCellX = terrain.TerrainSize().x / (terrain.GridSize().x - 1);
        lenghtCellZ = terrain.TerrainSize().z / (terrain.GridSize().z - 1);
        areaCell = lenghtCellX * lenghtCellZ;

        //    Contact Area Calculation     //
        // =============================== //

        // 2D iteration for both feet
        // 1. It counts the number of hits, save the classified cell in a list and debug ray-casting
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Left Foot
                // The sensors that counts the number of hits always remain on the surface
                Vector3 rayGridSphere = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                Vector3 rayGridWorldSphere = terrain.Grid2World(rayGridSphere);

                //------//

                // B. Create each ray for the grid (wrt World) - Left
                RaycastHit sphereHit;
                Ray upRaySphere = new Ray(rayGridWorldSphere, Vector3.up);

                //------//

                // C. If hits the Left Foot, increase counter and add cell to be affected
                if (MySphereCollider.Raycast(upRaySphere, out sphereHit, rayDistance))
                {
                    // AUX
                    startCoroutineNow = true;

                    // Cell contacting directly
                    heightMapSphereBool[zi + gridSize, xi + gridSize] = 2;
                    counterHitsSphere++;

                    if (showGridDebugSphere)
                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * rayDistance, Color.blue);
                }
                else
                {
                    // No contact
                    heightMapSphereBool[zi + gridSize, xi + gridSize] = 0;

                    if (showGridDebugSphere)
                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * rayDistance, Color.red);
                }
            }
        }

        // 2. Terrain Deformation is affected by an increasing value of the contact area, therefore the deformation
        // will be defined by the maximum contact area in each frame
        oldAreaTotalSphere = ((counterHitsSphere) * areaCell);
        if (oldAreaTotalSphere >= areaTotalSphere)
        {
            // Area of contact
            areaTotalSphere = ((counterHitsSphere) * areaCell);

            // Volume under the foot for that recent calculated area
            volumeOriginalSphere = areaTotalSphere * (originalLength);
        }

        // Total Area and Volume for both feet
        areaTotal = areaTotalSphere;

        //        Detecting Contour        //
        // =============================== //

        if (IsSphereGrounded)
        {
            // 1. We don't need to check the whole grid - just in the 5x5 inner grid is enough
            for (int zi = -gridSize + offsetBumpGrid; zi <= gridSize - offsetBumpGrid; zi++)
            {
                for (int xi = -gridSize + offsetBumpGrid; xi <= gridSize - offsetBumpGrid; xi++)
                {
                    // If the cell was not in contact, it's a potential neighbour (countour) cell
                    if (heightMapSphereBool[zi + gridSize, xi + gridSize] == 0)
                    {
                        // Only checking adjacent cells - increasing this would allow increasing the area of the bump
                        for (int zi_sub = -neighboursSearchArea; zi_sub <= neighboursSearchArea; zi_sub++)
                        {
                            for (int xi_sub = -neighboursSearchArea; xi_sub <= neighboursSearchArea; xi_sub++)
                            {
                                if (heightMapSphereBool[zi + zi_sub + gridSize, xi + xi_sub + gridSize] == 2)
                                {
                                    Vector3 rayGridSphere = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                                    Vector3 rayGridWorldSphere = terrain.Grid2World(rayGridSphere);

                                    if (showGridBumpDebug)
                                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * 0.2f, Color.yellow);

                                    // Mark that cell as a countour point
                                    heightMapSphereBool[zi + gridSize, xi + gridSize] = 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        // 2. Calculating number of neightbouring hits to later get the area
        for (int zi = -gridSize + offsetBumpGrid; zi <= gridSize - offsetBumpGrid; zi++)
        {
            for (int xi = -gridSize + offsetBumpGrid; xi <= gridSize - offsetBumpGrid; xi++)
            {
                // A. If cell is neightbour
                if (heightMapSphereBool[zi + gridSize, xi + gridSize] == 1)
                {
                    // B. Each neightbour cell in world space
                    Vector3 rayGridSphere = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                    Vector3 rayGridWorldSphere = terrain.Grid2World(rayGridSphere);

                    // Store the Vector3 positions in a dynamic array
                    neighboursPositionsSphere.Add(rayGridWorldSphere);

                    // D. Counter neightbours
                    neighbourCellsSphere++;

                    if (showGridBumpDebug)
                        Debug.DrawRay(MySphereCollider.transform.position, Vector3.up * rayDistance, Color.red);
                }
            }
        }
         
        // 3. Calculate the neightbour area for each foot
        oldNeighbourAreaTotalSphere = ((neighbourCellsSphere) * areaCell);
        if (oldNeighbourAreaTotalSphere >= neighbourAreaTotalSphere)
        {
            // Area of bump - Not used yet - TODO
            neighbourAreaTotalSphere = ((neighbourCellsSphere) * areaCell);
        }


        //       Physics Calculation       //
        // =============================== //

        // 1. Calculate Pressure applicable per frame - if no contact, there is no pressure
        // The three values should be similar, since pressure is based on the contact area

        // Pressure by left feet
        if (counterHitsSphere == 0)
            pressureStressSphere = 0f;
        else
            pressureStressSphere = (TotalForceSphereY) / areaTotalSphere;

        //     Deformation Calculation     //
        // =============================== //

        // 1. Given area, pressure and terrain parameters, we calculate the displacement on the terrain
        // The decrement will depend also on the ContactTime used to calculate the corresponding force

        // 2. As for the area, we keep the maximum value

        oldHeightCellDisplacementYoungSphere = pressureStressSphere * (originalLength / (youngModulus));
        if (oldHeightCellDisplacementYoungSphere >= heightCellDisplacementYoungSphere)
        {
            // We use abs. value but for compression, the change in length is negative
            heightCellDisplacementYoungSphere = pressureStressSphere * (originalLength / youngModulus);

            // Resulting volume under the left foot after displacement - CHANGED
            volumeTotalSphere = areaTotalSphere * (originalLength + (-heightCellDisplacementYoungSphere));

            // TODO - Calculate the difference in volume, takes into account the compressibility and estimate volume up per neighbour cell
            volumeDifferenceSphere =  volumeTotalSphere - volumeOriginalSphere; // NEGATIVE CHANGE
            volumeNetDifferenceSphere = -volumeDifferenceSphere + volumeVariationPoissonSphere; // Calculate directly the volume in the bump upwards (positive)
            volumeCellSphere = volumeNetDifferenceSphere / neighbourCellsSphere;

            // 1. Calculate positive deformation for the contour based on the downward deformation and Poisson
            //newBumpHeightDeformationSphere = ((volumeTotalSphere - volumeVariationPoissonSphere) / areaTotalSphere) - originalLengthNew;

            // 2. In this case, we do it with volume. Remember: must be negative for later.
            newBumpHeightDeformationSphere = volumeCellSphere / areaCell;
        }

        // 3. Given the entire deformation in Y, we calculate the corresponding frame-based deformation based on the frame-time.
        displacementSphere = (Time.deltaTime * (float)heightCellDisplacementYoungSphere) / ContactTime;

        // Given the  deformation in Y for the bump, we calculate the corresponding frame-based deformation based on the frame-time.
        bumpDisplacementSphere = (Time.deltaTime * (float)newBumpHeightDeformationSphere) / ContactTime;

        //     Physics+ Calculation     //
        // =============================== //

        // Strains (compression) - Info
        //strainLong = -(heightCellDisplacementYoungRight) / originalLength;
        //strainTrans = poissonR * strainLong;

        // 1. If Poisson is 0.5 : ideal imcompressible material (no change in volume) - Compression : -/delta_L
        volumeVariationPoissonSphere = (1 - 2 * poissonR) * (-heightCellDisplacementYoungSphere / originalLength) * volumeOriginalSphere; // NEGATIVE CHANGE

        //        Apply Deformation        //
        // =============================== //

        // ==== Solution for sphere ==== //

        // 2D iteration Deformation
        // Once we have the displacement, we saved the actual result of applying it to the terrain (only when the foot is grounded)
        if (IsSphereGrounded)
        {
            Debug.Log("START COROUTINE");
            StartCoroutine(DecreaseTerrainSphere(heightMapSphere, heightMapSphereBool, x, z));
        }
        else if (!IsSphereGrounded)
        {
            // Every time we lift the foot, we reset the variables and stop the coroutines.
            heightCellDisplacementYoungSphere = 0;
            StopAllCoroutines();
        }

        //if (startCoroutineNow)
        //{
        //    Debug.Log("START COROUTINE");
        //    StartCoroutine(DecreaseTerrainSphere(heightMapSphere, heightMapSphereBool, x, z));
        //}
        //else if (!startCoroutineNow)
        //{
        //    // Every time we lift the foot, we reset the variables and stop the coroutines.
        //    heightCellDisplacementYoungSphere = 0;
        //    StopAllCoroutines();
        //}

        //         Apply Smoothing         //
        // =============================== //

        // 1. First smoothing version
        //if (applyFilterSphere)
        //{
        //    // 2. Provisional: When do we smooth?
        //    if (IsSphereGrounded)
        //    {
        //        if (!isFilteredSphere)
        //        {
        //            NewFilterHeightMap(x, z, heightMapSphere);
        //            filterIterationsSphereCounter++;
        //        }

        //        if (filterIterationsSphereCounter >= filterIterationsGaussSphere)
        //        {
        //            isFilteredSphere = true;
        //        }
        //    }
        //    else
        //    {
        //        isFilteredSphere = false;
        //        filterIterationsSphereCounter = 0;
        //    }
        //}
    }

    IEnumerator DecreaseTerrainSphere(float[,] heightMapSphere, int[,] heightMapSphereBool, int x, int z)
    {
        // 1. Apply frame-per-frame deformation ("displacement")
        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // A. Calculate each cell position wrt World and Heightmap - Left Foot
                Vector3 rayGridSphere = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);
                Vector3 rayGridWorldSphere = terrain.Grid2World(rayGridSphere);

                // B. Create each ray for the grid (wrt World) - Left
                RaycastHit sphereHit;
                Ray upRaySphere = new Ray(rayGridWorldSphere, Vector3.up);

                // C. If hits the Left Foot and the cell was classified with 2 (direct contact):
                if (MySphereCollider.Raycast(upRaySphere, out sphereHit, rayDistance) && (heightMapSphereBool[zi + gridSize, xi + gridSize] == 2))
                {
                    // D. Cell contacting directly - Decrease until limit reached
                    if (terrain.Get(rayGridSphere.x, rayGridSphere.z) >= terrain.GetConstant(rayGridSphere.x, rayGridSphere.z) - heightCellDisplacementYoungSphere)
                    {
                        // E. Substract
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z) - (displacementSphere);
                    }
                    else
                    {
                        // F. Keep same
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z);
                    }
                } // 1: Neighbour
                else if (!MySphereCollider.Raycast(upRaySphere, out sphereHit, rayDistance) && (heightMapSphereBool[zi + gridSize, xi + gridSize] == 1) && applyBumps)
                {
                    // G. If ray does not hit and is classified as BACK neightbour, we create a bump.
                    if (terrain.Get(rayGridSphere.x, rayGridSphere.z) <= terrain.GetConstant(rayGridSphere.x, rayGridSphere.z) + newBumpHeightDeformationSphere)
                    {
                        // H. Add
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z) + (bumpDisplacementSphere);
                    }
                    else
                    {
                        // I. Keep same
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z);
                    }
                }
                else
                {
                    // J. If is out of reach
                    heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z);
                }
            }
        }

        // 2. Applying filtering in frame-basis
        if (applyFilterSphere)
        {
            // 2. Provisional: When do we smooth?
            if (IsSphereGrounded)
            {
                if (!isFilteredSphere)
                {
                    heightMapSphere = NewFilterHeightMapReturn(x, z, heightMapSphere);
                    filterIterationsSphereCounter++;
                }

                if (filterIterationsSphereCounter >= filterIterationsGaussSphere)
                {
                    isFilteredSphere = true;
                }
            }
            else
            {
                isFilteredSphere = false;
                filterIterationsSphereCounter = 0;
            }
        }

        // 3. Save terrain
        if (applyFootprints)
        {
            for (int zi = -gridSize; zi <= gridSize; zi++)
            {
                for (int xi = -gridSize; xi <= gridSize; xi++)
                {
                    Vector3 rayGridSphere = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);
                    terrain.Set(rayGridSphere.x, rayGridSphere.z, heightMapSphere[zi + gridSize, xi + gridSize]);
                }
            }
        }

        yield return null;
    }

    // Pre-filter Methods - Not used //
    // ============================= //

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

    // Post-filter Methods - Not used //
    // ============================== //

    // Old-version Gaussian Blur
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

    // New-version Gaussian Blur (3x3) 
    public void NewFilterHeightMap(int x, int z, float[,] heightMap)
    {
        float[,] heightMapFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        for (int zi = -gridSize + marginAroundGrid; zi <= gridSize - marginAroundGrid; zi++)
        {
            for (int xi = -gridSize + marginAroundGrid; xi <= gridSize - marginAroundGrid; xi++)
            {
                Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);

                heightMapFiltered[zi + gridSize, xi + gridSize] =
                    heightMap[zi + gridSize - 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize - 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize - 1, xi + gridSize + 1]
                    + 2 * heightMap[zi + gridSize, xi + gridSize - 1]
                    + 4 * heightMap[zi + gridSize, xi + gridSize]
                    + 2 * heightMap[zi + gridSize, xi + gridSize + 1]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize + 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize + 1];

                heightMapFiltered[zi + gridSize, xi + gridSize] *= 1.0f / 16.0f;

                terrain.Set(rayGridLeft.x, rayGridLeft.z, (heightMapFiltered[zi + gridSize, xi + gridSize]));
            }
        }
    }

    // New-version Gaussian Blur (3x3) with return 
    public float[,] NewFilterHeightMapReturn(int x, int z, float[,] heightMap)
    {
        float[,] heightMapFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        // Places outside filtering will remain the same
        heightMapFiltered = heightMap;

        for (int zi = -gridSize + marginAroundGrid; zi <= gridSize - marginAroundGrid; zi++)
        {
            for (int xi = -gridSize + marginAroundGrid; xi <= gridSize - marginAroundGrid; xi++)
            {
                Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);

                heightMapFiltered[zi + gridSize, xi + gridSize] =
                    heightMap[zi + gridSize - 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize - 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize - 1, xi + gridSize + 1]
                    + 2 * heightMap[zi + gridSize, xi + gridSize - 1]
                    + 4 * heightMap[zi + gridSize, xi + gridSize]
                    + 2 * heightMap[zi + gridSize, xi + gridSize + 1]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize - 1]
                    + 2 * heightMap[zi + gridSize + 1, xi + gridSize]
                    + 1 * heightMap[zi + gridSize + 1, xi + gridSize + 1];

                heightMapFiltered[zi + gridSize, xi + gridSize] *= 1.0f / 16.0f;
            }
        }

        return heightMapFiltered;
    }

    // TEST - New Gaussian Blur (3x3) - Return (TODO STILL NOT WORK - Version 1 works fine, so skipping)
    /*
    public float[,] NewFilterHeightMapReturn(int x, int z, float[,] heightMap)
    {
        float[,] heightMapFiltered = new float[2 * gridSize + 1, 2 * gridSize + 1];

        for (int zi = -gridSize; zi <= gridSize; zi++)
        {
            for (int xi = -gridSize; xi <= gridSize; xi++)
            {
                // It avoids the offset or border
                if((zi > -gridSize + marginAroundGrid) && (zi < gridSize - marginAroundGrid) &&
                    (xi > -gridSize + marginAroundGrid) && (xi < gridSize - marginAroundGrid))
                {
                    Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi), z + zi);

                    heightMapFiltered[zi + gridSize, xi + gridSize] =
                        heightMap[zi + gridSize - 1, xi + gridSize - 1]
                        + 2 * heightMap[zi + gridSize - 1, xi + gridSize]
                        + 1 * heightMap[zi + gridSize - 1, xi + gridSize + 1]
                        + 2 * heightMap[zi + gridSize, xi + gridSize - 1]
                        + 4 * heightMap[zi + gridSize, xi + gridSize]
                        + 2 * heightMap[zi + gridSize, xi + gridSize + 1]
                        + 1 * heightMap[zi + gridSize + 1, xi + gridSize - 1]
                        + 2 * heightMap[zi + gridSize + 1, xi + gridSize]
                        + 1 * heightMap[zi + gridSize + 1, xi + gridSize + 1];

                    heightMapFiltered[zi + gridSize, xi + gridSize] *= 1.0f / 16.0f;

                }
                else
                {
                    heightMapFiltered[zi + gridSize, xi + gridSize] = heightMap[zi + gridSize, xi + gridSize];
                }

            }
        }

        return heightMapFiltered;
    }
    */

    // TEST - Calculate Barycentric Coordinates Right
    /*
    private void computeBarycentricCoordinatesRight(Vector3 center, List<Vector3> neighboursPositionsRight)
    {
        float weightSumRight = 0;

        for (int i = 0; i < neighboursPositionsRight.Count; i++)
        {
            int prev = (i + neighboursPositionsRight.Count - 1) % neighboursPositionsRight.Count;
            int next = (i + 1) % neighboursPositionsRight.Count;

            allSumCotRight.Add(contangAnglePreviousRight(center, neighboursPositionsRight[i], neighboursPositionsRight[prev], i, prev) + contangAngleNextRight(center, neighboursPositionsRight[i], neighboursPositionsRight[next], i, next));
            //allSumCotRight[i] = contangAnglePreviousRight(center, neighboursPositionsRight[i], neighboursPositionsRight[prev], i, prev) + contangAngleNextRight(center, neighboursPositionsRight[i], neighboursPositionsRight[next], i, next);

            neighboursWeightsRight.Add(allSumCotRight[i] / Vector3.Distance(center, neighboursPositionsRight[i]));
            weightSumRight += neighboursWeightsRight[i];
        }

        for (int i = 0; i < neighboursWeightsRight.Count; i++)
        {
            neighboursWeightsRight[i] /= weightSumRight;
        }
    }

    private float contangAnglePreviousRight(Vector3 p, Vector3 j, Vector3 neighbour, int vertex, int vertex_neightbour)
    {
        var pj = p - j;
        var bc = neighbour - j;

        float angle = Mathf.Atan2(Vector3.Cross(pj, bc).magnitude, Vector3.Dot(pj, bc));
        float angleCot = 1f / Mathf.Tan(angle);

        //allAnglesPrevRight[vertex] = angle * Mathf.Rad2Deg;

        return angleCot;
    }

    private float contangAngleNextRight(Vector3 p, Vector3 j, Vector3 neighbour, int vertex, int vertex_neightbour)
    {
        var pj = p - j;
        var bc = neighbour - j;

        float angle = Mathf.Atan2(Vector3.Cross(pj, bc).magnitude, Vector3.Dot(pj, bc));
        float angleCot = 1f / Mathf.Tan(angle);

        //allAnglesNextRight[vertex] = angle * Mathf.Rad2Deg;

        return angleCot;
    }
    */
}
