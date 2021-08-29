/****************************************************
 * File: PhysicalFootprint.cs
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

    [Header("Physically-based Footprints Deformation")]
    public bool applyFootprints = false;
    public bool applyBumps = false;

    [Header("Deformation - Debug")]
    public bool showGridDebugSphere = false;
    //public bool showGridDebugRight = false;
    public bool showGridBumpDebug = false;
    public bool showGridBumpFrontBack = false;
    public bool printTerrainInformation = false;
    public bool printDeformationInformation = false;

    [Header("Deformation - Grid Settings")]
    [Range(0, 20)] public int gridSize = 10;
    [Range(0f, 1f)] public float rayDistanceSphere = 0.25f;
    [Range(0f, 1f)] public float offsetRay = 0.04f;

    [Header("Terrain Deformation - Settings - (CONFIG)")]
    [Space(20)]
    [Range(100000, 1000000)] public double youngModulus = 1000000;
    public float originalLength = 1f;

    [Header("Terrain Deformation - Info")]
    public double heightCellDisplacementYoungSphere = 0f;
    public float displacementSphere;
    private double oldHeightCellDisplacementYoungSphere = 0f;

    [Header("Terrain Deformation - Number of hits")]
    public int counterHitsSphere;

    [Header("Terrain Deformation - Contact Area Feet-Ground")]
    public float areaCell;
    public float areaTotal = 0f;
    public float areaTotalSphere = 0f;
    private float oldAreaTotalSphere = 0f;
    private float lenghtCellX;
    private float lenghtCellZ;

    [Header("Terrain Deformation - Volume Rod Approximation")]
    //public double volumeOriginalSphere = 0f; // Original volume under left foot
    //public double volumeTotalSphere = 0f; // Volume left after deformation
    //public double volumeVariationPoissonSphere; // Volume change due to compressibility of the material
    //public double volumeDifferenceSphere; // Volume difference pre/post deformation without taking into account compressibility
    //public double volumeNetDifferenceSphere; // Volume difference pre/post deformation taking into account compressibility
    //public double volumeCellSphere; // Volume/cell distributed over countour

    [Header("Terrain Deformation - Pressure (Stress) by feet")]
    public float pressureStress;
    public float pressureStressSphere;

    [Header("Bump Deformation - Settings - (CONFIG)")]
    [Space(20)]
    public bool useManualBumpDeformation = false;
    [Range(0, 0.05f)] public double bumpHeightDeformation = 0.03f; // In case one wants to do it manually
    public int offsetBumpGrid = 2;
    public int neighboursSearchArea = 2;
    [Range(0, 0.5f)] public float poissonR = 0.4f;

    [Header("Bump Deformation - Info")] // TODO - Still need to be checked
    public double newBumpHeightDeformationSphere = 0f;
    public float bumpDisplacementLeftBack;
    public float bumpDisplacementRightBack;
    private float bumpDisplacementLeftFront;
    private float bumpDisplacementRightFront;
    private float oldNeighbourAreaTotalSphere;

    [Header("Bump Deformation - Neighbour Area Feet-Ground")]
    public int neighbourCellsSphere;
    public float neighbourAreaTotalSphere;

    [Header("Bump Deformation - Bump Vector3 Coordinates")]
    public List<Vector3> neighboursPositionsRightFront = new List<Vector3>();
    public List<Vector3> neighboursPositionsLeftFront = new List<Vector3>();
    public List<Vector3> neighboursPositionsRightBack = new List<Vector3>();
    public List<Vector3> neighboursPositionsLeftBack = new List<Vector3>();

    [Header("Filtering - Settings - (CONFIG)")]
    [Space(20)]
    public bool applyFilterLeft = false;
    public bool applyFilterRight = false;
    public int filterIterationsLeftFoot = 2;
    public int filterIterationsRightFoot = 2;
    public int marginAroundGrid = 3;
    private int filterIterationsLeftCounter = 0;
    private int filterIterationsRightCounter = 0;
    private bool isFilteredLeft = false;
    private bool isFilteredRight = false;
    private bool applyFilterLeft2 = false;
    private bool applyFilterRight2 = false;

    // Others for filtering
    [Range(0, 5)] private int gridSizeKernel = 1;

    // Others
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
    public override void DrawFootprint(int x, int z)
    {

        //       Initial Declarations      //
        // =============================== //

        // 3. Reset counter hits
        counterHitsSphere = 0;
        neighbourCellsSphere = 0;

        // 3. Reset lists
        neighboursPositionsRightFront.Clear();
        neighboursPositionsLeftFront.Clear();
        neighboursPositionsRightBack.Clear();
        neighboursPositionsLeftBack.Clear();

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
                if (LeftFootCollider.Raycast(upRaySphere, out sphereHit, rayDistanceSphere))
                {
                    // Cell contacting directly
                    heightMapSphereBool[zi + gridSize, xi + gridSize] = 2;
                    counterHitsSphere++;

                    if (showGridDebugSphere)
                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * rayDistanceSphere, Color.blue);
                }
                else
                {
                    // No contact
                    heightMapSphereBool[zi + gridSize, xi + gridSize] = 0;

                    if (showGridDebugSphere)
                        Debug.DrawRay(rayGridWorldSphere, Vector3.up * rayDistanceSphere, Color.red);
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
            //volumeOriginalSphere = areaTotalSphere * (originalLength);
        }

        // Total Area and Volume for both feet
        areaTotal = areaTotalSphere;

        //        Detecting Contour        //
        // =============================== //

        /*
      
        if (IsRightFootGrounded)
        {
            // 1. We don't need to check the whole grid - just in the 5x5 inner grid is enough
            for (int zi = -gridSize + offsetBumpGrid; zi <= gridSize - offsetBumpGrid; zi++)
            {
                for (int xi = -gridSize + offsetBumpGrid; xi <= gridSize - offsetBumpGrid; xi++)
                {
                    // A. If the cell was not in contact, it's a potential neighbour (countour) cell
                    if (heightMapRightBool[zi + gridSize, xi + gridSize] == 0)
                    {
                        // B. Only checking adjacent cells - increasing this would allow increasing the area of the bump
                        for (int zi_sub = -neighboursSearchArea; zi_sub <= neighboursSearchArea; zi_sub++)
                        {
                            for (int xi_sub = -neighboursSearchArea; xi_sub <= neighboursSearchArea; xi_sub++)
                            {
                                // C. If there is a contact point around the cell
                                if (heightMapRightBool[zi + zi_sub + gridSize, xi + xi_sub + gridSize] == 2)
                                {
                                    Vector3 rayGridRight = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                                    Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                                    if (showGridBumpDebug)
                                        Debug.DrawRay(rayGridWorldRight, Vector3.up * 0.2f, Color.yellow);

                                    // D. Mark that cell as a countour point
                                    heightMapRightBool[zi + gridSize, xi + gridSize] = 1;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (IsLeftFootGrounded)
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
                                    Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                                    Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                                    if(showGridBumpDebug)
                                        Debug.DrawRay(rayGridWorldLeft, Vector3.up * 0.2f, Color.yellow);

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
                    Vector3 rayGridLeft = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                    Vector3 rayGridWorldLeft = terrain.Grid2World(rayGridLeft);

                    // Position of the neighbour relative to the foot
                    Vector3 relativePos = rayGridWorldLeft - LeftFootCollider.transform.position;

                    // C. Check if is in front/back of the foot
                    if(Vector3.Dot(LeftFootCollider.transform.forward, relativePos) > 0.0f)
                    {
                        // Store the Vector3 positions in a dynamic array
                        neighboursPositionsLeftFront.Add(rayGridWorldLeft);

                        // TODO - 3 is contour in FRONT
                        heightMapSphereBool[zi + gridSize, xi + gridSize] = 3;

                        if (showGridBumpFrontBack)
                            Debug.DrawRay(LeftFootCollider.transform.position, relativePos, Color.red);
                    }
                    else
                    {
                        // Store the Vector3 positions in a dynamic array
                        neighboursPositionsLeftBack.Add(rayGridWorldLeft);

                        // TODO - 4 is contour in BACK
                        heightMapSphereBool[zi + gridSize, xi + gridSize] = 4;

                        if (showGridBumpFrontBack)
                            Debug.DrawRay(LeftFootCollider.transform.position, relativePos, Color.blue);
                    }

                    // D. Counter neightbours
                    neighbourCellsLeft++;
                }

                // A. If cell is neightbour
                if (heightMapRightBool[zi + gridSize, xi + gridSize] == 1)
                {
                    // B. Each neightbour cell in world space
                    Vector3 rayGridRight = new Vector3(x + xi, terrain.Get(x + xi, z + zi) - offsetRay, z + zi);
                    Vector3 rayGridWorldRight = terrain.Grid2World(rayGridRight);

                    // Position of the neighbour relative to the foot
                    Vector3 relativePos = rayGridWorldRight - RightFootCollider.transform.position;

                    // C. Check if is in front/back of the foot
                    if (Vector3.Dot(RightFootCollider.transform.forward, relativePos) > 0.0f)
                    {
                        // Store the Vector3 positions in a dynamic array
                        neighboursPositionsRightFront.Add(rayGridWorldRight);

                        // TODO - 3 is contour in FRONT
                        heightMapRightBool[zi + gridSize, xi + gridSize] = 3;

                        if (showGridBumpFrontBack)
                            Debug.DrawRay(RightFootCollider.transform.position, relativePos, Color.red);
                    }
                    else
                    {
                        // Store the Vector3 positions in a dynamic array
                        neighboursPositionsRightBack.Add(rayGridWorldRight);

                        // TODO - 4 is contour in BACK
                        heightMapRightBool[zi + gridSize, xi + gridSize] = 4;

                        if (showGridBumpFrontBack)
                            Debug.DrawRay(RightFootCollider.transform.position, relativePos, Color.blue);
                    }

                    // D. Counter neightbours
                    neighbourCellsRight++;
                }
            }
        }
        

        // 3. Calculate the neightbour area for each foot
        oldNeighbourAreaTotalLeft = ((neighbourCellsLeft) * areaCell);
        if (oldNeighbourAreaTotalLeft >= neighbourAreaTotalLeft)
        {
            // Area of bump - Not used yet - TODO
            neighbourAreaTotalLeft = ((neighbourCellsLeft) * areaCell);
        }

        oldNeighbourAreaTotalRight = ((neighbourCellsRight) * areaCell);
        if (oldNeighbourAreaTotalRight >= neighbourAreaTotalRight)
        {
            // Area of bump - Not used yet - TODO
            neighbourAreaTotalRight = ((neighbourCellsRight) * areaCell);
        }
        */

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
            //volumeTotalSphere = areaTotalSphere * (originalLength + (-heightCellDisplacementYoungSphere));

            // TODO - Calculate the difference in volume, takes into account the compressibility and estimate volume up per neighbour cell
            //volumeDifferenceSphere =  volumeTotalSphere - volumeOriginalSphere; // NEGATIVE CHANGE
            //volumeNetDifferenceSphere = -volumeDifferenceSphere + volumeVariationPoissonSphere; // Calculate directly the volume in the bump upwards (positive)
            //volumeCellSphere = volumeNetDifferenceSphere / neighbourCellsSphere;

            // 1. Calculate positive deformation for the contour based on the downward deformation and Poisson
            //newBumpHeightDeformationLeft = ((volumeTotalLeft - volumeVariationPoissonLeft) / areaTotalLeft) - originalLength;

            // 2. In this case, we do it with volume. Remember: must be negative for later.
            //newBumpHeightDeformationSphere = volumeCellSphere / areaCell;
        }

        // 3. Given the entire deformation in Y, we calculate the corresponding frame-based deformation based on the frame-time.
        displacementSphere = (Time.deltaTime * (float)heightCellDisplacementYoungSphere) / ContactTime;

        //if(useManualBumpDeformation)
        //{
        //    newBumpHeightDeformationSphere = -bumpHeightDeformation;
        //    newBumpHeightDeformationRight = -bumpHeightDeformation;
        //}

        // Given the  deformation in Y for the bump, we calculate the corresponding frame-based deformation based on the frame-time.
        //bumpDisplacementLeftBack = (Time.deltaTime * (float)newBumpHeightDeformationSphere) / ContactTime;
        //bumpDisplacementRightBack = (Time.deltaTime * (float)newBumpHeightDeformationRight) / ContactTime;
        //bumpDisplacementLeftFront = (Time.deltaTime * (float)newBumpHeightDeformationSphere) / ContactTime;
        //bumpDisplacementRightFront = (Time.deltaTime * (float)newBumpHeightDeformationRight) / ContactTime;

        //     Physics+ Calculation     //
        // =============================== //

        // Strains (compression) - Info
        //strainLong = -(heightCellDisplacementYoungRight) / originalLength;
        //strainTrans = poissonR * strainLong;

        // 1. If Poisson is 0.5 : ideal imcompressible material (no change in volume) - Compression : -/delta_L
        //volumeVariationPoissonSphere = (1 - 2 * poissonR) * (-heightCellDisplacementYoungSphere / originalLength) * volumeOriginalSphere; // NEGATIVE CHANGE

        //        Apply Deformation        //
        // =============================== //

        // 2D iteration Deformation
        // Once we have the displacement, we saved the actual result of applying it to the terrain (only when the foot is grounded)
        if (IsLeftFootGrounded)
        {
            Debug.Log("START COROUTINE");
            StartCoroutine(DecreaseTerrainSphere(heightMapSphere, heightMapSphereBool, x, z));
        }
        else if(!IsLeftFootGrounded)
        {
            // Every time we lift the foot, we reset the variables and stop the coroutines.
            heightCellDisplacementYoungSphere = 0;
            StopAllCoroutines();
        }

        //         Apply Smoothing         //
        // =============================== //

        // 1. First smoothing version
        if (applyFilterLeft)
        {
            // 2. Provisional: When do we smooth?
            if (IsLeftFootGrounded && !IsRightFootGrounded)
            {
                if (!isFilteredLeft)
                {
                    NewFilterHeightMap(x, z, heightMapSphere);
                    filterIterationsLeftCounter++;
                }

                if (filterIterationsLeftCounter >= filterIterationsLeftFoot)
                {
                    isFilteredLeft = true;
                }
            }
            else
            {
                isFilteredLeft = false;
                filterIterationsLeftCounter = 0;
            }
        }

        // 1. First smoothing version
        if (applyFilterRight)
        {
            // 2. Provisional: When do we smooth?
            if (IsRightFootGrounded && !IsLeftFootGrounded)
            {
                if (!isFilteredRight)
                {
                    NewFilterHeightMap(x, z, heightMapRight);
                    filterIterationsRightCounter++;
                }

                if (filterIterationsRightCounter >= filterIterationsRightFoot)
                {
                    isFilteredRight = true;
                }
            }
            else
            {
                isFilteredRight = false;
                filterIterationsRightCounter = 0;
            }
        }
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
                if (LeftFootCollider.Raycast(upRaySphere, out sphereHit, rayDistanceSphere) && (heightMapSphereBool[zi + gridSize, xi + gridSize] == 2))
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
                } // 3: Front, 4: Back
                else if (!LeftFootCollider.Raycast(upRaySphere, out sphereHit, rayDistanceSphere) && (heightMapSphereBool[zi + gridSize, xi + gridSize] == 4) && applyBumps)
                {
                    // G. If ray does not hit and is classified as BACK neightbour, we create a bump.
                    if (terrain.Get(rayGridSphere.x, rayGridSphere.z) <= terrain.GetConstant(rayGridSphere.x, rayGridSphere.z) + newBumpHeightDeformationSphere)
                    {
                        //heightMapLeft[zi + gridSize, xi + gridSize] = terrain.Get(rayGridLeft.x, rayGridLeft.z) - (bumpDisplacementLeftBack * MaxTotalForceLeftFootZNorm);

                        // H. Substract
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z) + (bumpDisplacementLeftBack);
                    }
                    else
                    {
                        // I. Keep same
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z);
                    }
                }
                else if (!LeftFootCollider.Raycast(upRaySphere, out sphereHit, rayDistanceSphere) && (heightMapSphereBool[zi + gridSize, xi + gridSize] == 3) && applyBumps)
                {
                    if (terrain.Get(rayGridSphere.x, rayGridSphere.z) <= terrain.GetConstant(rayGridSphere.x, rayGridSphere.z) + newBumpHeightDeformationSphere)
                    {
                        heightMapSphere[zi + gridSize, xi + gridSize] = terrain.Get(rayGridSphere.x, rayGridSphere.z) + (bumpDisplacementLeftBack);
                    }
                    else
                    {
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

        // 2. Save terrain
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
