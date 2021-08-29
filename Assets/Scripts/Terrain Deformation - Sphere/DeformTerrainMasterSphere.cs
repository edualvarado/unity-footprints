/****************************************************
 * File: DeformTerrainMasterSphere.cs
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
/// Master class where we calculate all forces taking place during gait and call footprint child class.
/// </summary>
public class DeformTerrainMasterSphere : MonoBehaviour
{
    #region Variables

    [Header("Sphere - (CONFIG)")]
    [Tooltip("Your character - make sure is the parent GameObject")]
    public GameObject mySphere;
    [Tooltip("Collider attached to Left Foot")]
    public Collider mySphereCollider;

    [Header("Sphere - System Info")]
    public float mass;

    [Header("Sphere - Feet Info")]
    public bool printSpherePositions = false;
    public bool isSphereGrounded = false;

    public Vector3 centerGridSphereHeight;
    private Vector3 centerGridSphere;

    private Vector3 newIKSpherePosition;
    private Vector3 oldIKSpherePosition;

    [Header("Sphere - Physics - Debug")]
    [Space(20)]
    public bool printSphereForces = false;
    public bool drawWeightForces = false;
    public bool drawMomentumForces = false;
    public bool drawGRForces = false;
    public bool drawFeetForces = false;
    public bool drawVelocities = false;

    [Header("Sphere - Physics - Weight Forces Info")]
    public Vector3 weightForce;

    [Header("Sphere - Physics - Velocities Info")]
    public Vector3 sphereSpeed = Vector3.zero;

    [Header("Sphere - Physics - Impulse and Momentum Forces Info")]
    public Vector3 impulseSphere = Vector3.zero;
    public Vector3 momentumForceSphere = Vector3.zero;

    [Header("Sphere - Physics - GRF and Total Forces Info")]
    public Vector3 totalGRForceSphere;
    public Vector3 totalForceSphere;

    [Header("Terrain Deformation - Contact Time Settings - (CONFIG)")]
    [Space(20)]
    public float timePassed = 0f;
    [Tooltip("Time that the terrain requires to absorve the force from the hitting foot. More time results in a smaller require force. On the other hand, for less time, the terrain requires a larger force to stop the foot.")]
    public float contactTime = 0.1f;
    [Tooltip("Small delay, sometimes needed, to give the system enough time to perform the deformation.")]
    public float offset = 0.5f;

    // Types of brushes
    private BrushPhysicalFootprintSphere brushPhysicalFootprint;

    // Terrain Properties
    private Terrain terrain;
    private Collider terrain_collider;
    private TerrainData terrain_data;
    private Vector3 terrain_size;
    private int heightmap_width;
    private int heightmap_height;
    private float[,] heightmap_data;
    private float[,] heightmap_data_constant;
    private float[,] heightmap_data_filtered;

    #endregion

    #region Plotting

    //               Extra for plotting              //
    // ============================================= //
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float weightForceLeftYFloat = 0f;
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float weightForceRightYFloat = 0f;
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float weightForceYFloat = 0f;

    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float momentumForceLeftYFloat = 0f;
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float momentumForceRightYFloat = 0f;
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float momentumForceYFloat = 0f;

    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float totalGRForceLeftYFloat;
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float totalGRForceRightYFloat;
    ////[UPyPlot.UPyPlotController.UPyProbe]
    //private float totalGRForceYFloat;
    // ============================================= //

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        // 1. Extract terrain information
        if (!terrain)
        {
            terrain = Terrain.activeTerrain;
            Debug.Log("[INFO] Main terrain: " + terrain.name);
        }

        terrain_collider = terrain.GetComponent<Collider>();
        terrain_data = terrain.terrainData;
        terrain_size = terrain_data.size;
        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;
        heightmap_data = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_data_constant = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_data_filtered = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        brushPhysicalFootprint = null;

        // 2. Retrieve components and attributes from character
        mass = mySphere.GetComponent<Rigidbody>().mass;

        // Old Feet Y-component position
        oldIKSpherePosition = mySphere.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //       Initial Information       //
        // =============================== //

        centerGridSphere = World2Grid(mySphere.transform.position.x, mySphere.transform.position.z);
        centerGridSphereHeight = new Vector3(mySphere.transform.position.x, Get(centerGridSphere.x, centerGridSphere.z), mySphere.transform.position.z);

        //       Sphere Information       //
        // =============================== //

        //  1. Calculate Forces  //
        // ===================== //

        if (brushPhysicalFootprint)
        {
            // A. Weight Forces - Negative Y-component
            weightForce = mass * (Physics.gravity);

            //               Extra for plotting              //
            // ============================================= //
            //weightForceLeftYFloat = weightForceLeft.y;
            //weightForceRightYFloat = weightForceRight.y;
            //weightForceYFloat = weightForce.y;
            // ============================================= //

            // Weight Force is already zero if the foot is not grounded - however, we draw only when foot is grounded
            //if (drawWeightForces && !isMoving)
            //{
            //    DrawForce.ForDebug3D(centerGridLeftFootHeight, weightForceLeft, Color.blue, 0.0025f);
            //    DrawForce.ForDebug3D(centerGridRightFootHeight, weightForceRight, Color.blue, 0.0025f);
            //}
            //else
            //{
            //    if (drawWeightForces && isSphereGrounded)
            //    {
            //        DrawForce.ForDebug3D(centerGridLeftFootHeight, weightForceLeft, Color.blue, 0.0025f);
            //    }

            //    if (drawWeightForces && isRightFootGrounded)
            //    {
            //        DrawForce.ForDebug3D(centerGridRightFootHeight, weightForceRight, Color.blue, 0.0025f);
            //    }
            //}

            //--------------

            // ake only velocities going downward //
            // ================================== //

            // B. Impulse per foot - Linear Momentum change (final velocity for the feet is 0)
            //feetImpulseLeft = mass * weightInLeftFoot * (Vector3.zero - feetSpeedLeft);
            //feetImpulseRight = mass * weightInRightFoot * (Vector3.zero - feetSpeedRight);

            if (sphereSpeed.y <= 0f)
                impulseSphere = mass * 1f * (Vector3.zero - sphereSpeed);
            else
                impulseSphere = Vector3.zero;

            // ================================== //

            //--------------

            // C. Momentum force exerted by ground to each foot - Calculated using Impulse and Contact Time
            // Positive (upward) if foot lands (negative velocity)
            // Negative (downward) if foot rises (positive velocity)
            momentumForceSphere = impulseSphere / contactTime;

            //  Extra for plotting (only positive values - when feet hit the ground) //
            // ===================================================================== //
            //if (momentumForceLeft.y > 0f)
            //    momentumForceLeftYFloat = momentumForceLeft.y;

            //if (momentumForceRight.y > 0f)
            //    momentumForceRightYFloat = momentumForceRight.y;

            //if (momentumForce.y > 0f)
            //    momentumForceYFloat = momentumForce.y;
            // ===================================================================== //

            // Momentum Forces are created when we hit the ground (that is, when such forces are positive in y, and the feet are grounded)
            //if (drawMomentumForces && isMoving && momentumForceLeft.y > 0f && isLeftFootGrounded)
            //{
            //    DrawForce.ForDebug3D(centerGridLeftFootHeight, momentumForceLeft, Color.red, 0.0025f);
            //}

            //if (drawMomentumForces && isMoving && momentumForceRight.y > 0f && isRightFootGrounded)
            //{
            //    DrawForce.ForDebug3D(centerGridRightFootHeight, momentumForceRight, Color.red, 0.0025f);
            //}

            // Momentum Forces are created when we hit the ground
            //if (drawMomentumForces && isMoving && isSphereGrounded)
            //{
            //    DrawForce.ForDebug3D(centerGridLeftFootHeight, momentumForceLeft, Color.red, 0.0025f);
            //}

            //if (drawMomentumForces && isMoving && isRightFootGrounded)
            //{
            //    DrawForce.ForDebug3D(centerGridRightFootHeight, momentumForceRight, Color.red, 0.0025f);
            //}

            //--------------

            // D. GRF (Ground Reaction Force) that the ground exerts to each foot
            totalGRForceSphere = momentumForceSphere - (weightForce);

            //               Extra for plotting              //
            // ============================================= //
            //totalGRForceLeftYFloat = totalGRForceLeft.y;
            //totalGRForceRightYFloat = totalGRForceRight.y;
            //totalGRForceYFloat = totalGRForce.y;
            // ============================================= //

            // Color for GR Forces
            Color darkGreen = new Color(0.074f, 0.635f, 0.062f, 1f);

            // GRF is already zero if the foot is not grounded - however, we draw only when foot is grounded
            //if (drawGRForces && !isMoving)
            //{
            //    DrawForce.ForDebug3D(centerGridLeftFootHeight, totalGRForceLeft, darkGreen, 0.0025f);
            //    DrawForce.ForDebug3D(centerGridRightFootHeight, totalGRForceRight, darkGreen, 0.0025f);
            //}
            //else
            //{
            //    if (drawGRForces && isSphereGrounded)
            //    {
            //        DrawForce.ForDebug3D(centerGridLeftFootHeight, totalGRForceLeft, darkGreen, 0.0025f);
            //    }

            //    if (drawGRForces && isRightFootGrounded)
            //    {
            //        DrawForce.ForDebug3D(centerGridRightFootHeight, totalGRForceRight, darkGreen, 0.0025f);
            //    }
            //}

            //--------------

            // E. Reaction Force for the feet (3rd Newton Law)
            totalForceSphere = -totalGRForceSphere;

            //--------------

            // Save max/min values reached for the feet forces in Z
            //maxTotalForceLeftFootZOld = totalForceLeftFoot.z;
            //if (maxTotalForceLeftFootZOld > maxTotalForceLeftFootZ)
            //{
            //    maxTotalForceLeftFootZ = maxTotalForceLeftFootZOld;
            //    maxTotalForceLeftFootZNorm = totalForceLeftFoot.normalized.z;
            //}
            //minTotalForceLeftFootZOld = totalForceLeftFoot.z;
            //if (minTotalForceLeftFootZOld < minTotalForceLeftFootZ)
            //{
            //    minTotalForceLeftFootZ = minTotalForceLeftFootZOld;
            //    minTotalForceLeftFootZNorm = totalForceLeftFoot.normalized.z;
            //}

            // Reset Values
            //if (!isSphereGrounded)
            //{
            //    maxTotalForceLeftFootZ = 0f;
            //    minTotalForceLeftFootZ = 0f;
            //    maxTotalForceLeftFootZNorm = 0f;
            //    minTotalForceLeftFootZNorm = 0f;
            //}

            // Save max/min values reached for the feet forces in Z
            //maxTotalForceRightFootZOld = totalForceRightFoot.z;
            //if (maxTotalForceRightFootZOld > maxTotalForceRightFootZ)
            //{
            //    maxTotalForceRightFootZ = maxTotalForceRightFootZOld;
            //    maxTotalForceRightFootZNorm = totalForceRightFoot.normalized.z;

            //}
            //minTotalForceRightFootZOld = totalForceRightFoot.z;
            //if (minTotalForceRightFootZOld < minTotalForceRightFootZ)
            //{
            //    minTotalForceRightFootZ = minTotalForceRightFootZOld;
            //    minTotalForceRightFootZNorm = totalForceRightFoot.normalized.z;
            //}

            // Reset Values
            //if (!isRightFootGrounded)
            //{
            //    maxTotalForceRightFootZ = 0f;
            //    minTotalForceRightFootZ = 0f;
            //    maxTotalForceRightFootZNorm = 0f;
            //    minTotalForceRightFootZNorm = 0f;
            //}

            //--------------

            // Feet Forces are created when we hit the ground (that is, when the Y-component of the Momentum Force is positive)
            // Only when the feet rise up, Feet Forces do not exist. The muscle is the responsable to lift the foot up
            // Also, the foot must be grounded to have a Feet Force actuating onto the ground
            //if (drawFeetForces && !isMoving)
            //{
            //    DrawForce.ForDebug3D(centerGridLeftFootHeight, totalForceLeftFoot, Color.black, 0.0025f);
            //    DrawForce.ForDebug3D(centerGridRightFootHeight, totalForceRightFoot, Color.black, 0.0025f);
            //}
            //else
            //{
            //    if (drawFeetForces && momentumForceLeft.y > 0f && isSphereGrounded)
            //    {
            //        DrawForce.ForDebug3D(centerGridLeftFootHeight, totalForceLeftFoot, Color.black, 0.0025f);
            //    }

            //    if (drawFeetForces && momentumForceRight.y > 0f && isRightFootGrounded)
            //    {
            //        DrawForce.ForDebug3D(centerGridRightFootHeight, totalForceRightFoot, Color.black, 0.0025f);
            //    }

            //    //if (drawFeetForces && isLeftFootGrounded)
            //    //{
            //    //    DrawForce.ForDebug3D(centerGridLeftFootHeight, totalForceLeftFoot, Color.black, 0.0025f);
            //    //}

            //    //if (drawFeetForces && isRightFootGrounded)
            //    //{
            //    //    DrawForce.ForDebug3D(centerGridRightFootHeight, totalForceRightFoot, Color.black, 0.0025f);
            //    //}

            //}
        }

        // =============================== //

        // F. Print the position of the feet in both systems (world and grid)
        if (printSpherePositions)
        {
            Debug.Log("[INFO] Sphere Coords (World): " + mySphere.transform.position.ToString());
            Debug.Log("[INFO] Sphere Coords (Grid): " + centerGridSphere.ToString());
            Debug.Log("-----------------------------------------");
        }

        // Print the forces
        if (printSphereForces)
        {
            Debug.Log("[INFO] Weight Force: " + weightForce);

            Debug.Log("[INFO] Sphere Speed: " + sphereSpeed);

            Debug.Log("[INFO] Sphere Impulse: " + impulseSphere);

            Debug.Log("[INFO] GRF Sphere: " + totalGRForceSphere);

            Debug.Log("[INFO] Total Force Sphere: " + totalForceSphere);
            Debug.Log("-----------------------------------------");
        }

        // =============================== //

        // 2. Apply brush to feet
        if (brushPhysicalFootprint)
        {
            // Brush is only called if we are within the contactTime.
            // Due to the small values, the provisional solution requires to add an offset to give the system enough time to create the footprint.
            
            if (isSphereGrounded)
                timePassed += Time.deltaTime;

            if ((timePassed <= contactTime + offset) && isSphereGrounded)
            {
                // Brush that takes limbs positions and creates physically-based footprints
                brushPhysicalFootprint.CallFootprint(mySphere.transform.position.x, mySphere.transform.position.z);
                //brushPhysicalFootprint.CallFootprint(_feetPlacement.LeftFootIKPosition.x, _feetPlacement.LeftFootIKPosition.z,
                //    _feetPlacement.RightFootIKPosition.x, _feetPlacement.RightFootIKPosition.z);
            }
        }

        if (isSphereGrounded == false)
            timePassed = 0f;
    }

    public void FixedUpdate()
    {
        // Calculate Velocity for the feet //
        // =============================== //

        newIKSpherePosition = mySphere.transform.position; // Before: LeftFoot
        var mediaLeft = (newIKSpherePosition - oldIKSpherePosition);

        sphereSpeed = new Vector3((mediaLeft.x / Time.fixedDeltaTime), (mediaLeft.y / Time.fixedDeltaTime), (mediaLeft.z / Time.fixedDeltaTime));

        oldIKSpherePosition = newIKSpherePosition;

        newIKSpherePosition = mySphere.transform.position;

        //if (drawVelocities)
        //{
        //    DrawForce.ForDebug3D(newIKLeftPosition, -feetSpeedLeft, Color.cyan, 0.0025f);
        //}
    }

    //      Getters       //
    // ================== //

    public Vector3 Get3(int x, int z)
    {
        return new Vector3(x, Get(x, z), z);
    }
    public Vector3 Get3(float x, float z)
    {
        return new Vector3(x, Get(x, z), z);
    }
    public Vector3 GetInterp3(float x, float z)
    {
        return new Vector3(x, GetInterp(x, z), z);
    }

    // Given one node of the heightmap, get the height
    public float Get(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data[z, x] * terrain_data.heightmapScale.y;
    }
    public float Get(float x, float z)
    {
        return Get((int)x, (int)z);
    }

    // Get entire array with heightmap without being scaled
    public float[,] GetHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return heightmap_data;
    }

    // Given one node of the heightmap (constant at start), get the height
    public float GetConstant(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data_constant[z, x] * terrain_data.heightmapScale.y;
    }
    public float GetConstant(float x, float z)
    {
        return GetConstant((int)x, (int)z);
    }

    // Get entire array with initial constant heightmap without being scaled
    public float[,] GetConstantHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return heightmap_data_constant;
    }
    // Given one node of the heightmap, get the height (post-filter version)
    public float GetFiltered(int x, int z)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        return heightmap_data_filtered[z, x] * terrain_data.heightmapScale.y;
    }
    public float GetFiltered(float x, float z)
    {
        return GetFiltered((int)x, (int)z);
    }

    // Get entire array with post-filtered heightmap
    public float[,] GetFilteredHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return heightmap_data_filtered;
    }

    public float GetInterp(float x, float z)
    {
        return terrain_data.GetInterpolatedHeight(x / heightmap_width,
                                                  z / heightmap_height);
    }
    public float GetSteepness(float x, float z)
    {
        return terrain_data.GetSteepness(x / heightmap_width,
                                         z / heightmap_height);
    }
    public Vector3 GetNormal(float x, float z)
    {
        return terrain_data.GetInterpolatedNormal(x / heightmap_width,
                                                  z / heightmap_height);
    }

    //      Setters       //
    // ================== //

    // Given one node of the heightmap, set the height
    public void Set(int x, int z, float val)
    {
        x = (x + heightmap_width) % heightmap_width;
        z = (z + heightmap_height) % heightmap_height;
        heightmap_data[z, x] = val / terrain_data.heightmapScale.y;
    }

    public void Set(float x, float z, float val)
    {
        Set((int)x, (int)z, val);
    }

    //      Terrain Methods       //
    // ========================== //

    // Get dimensions of the heightmap grid
    public Vector3 GridSize()
    {
        return new Vector3(heightmap_width, 0.0f, heightmap_height);
    }

    // Get real dimensions of the terrain (World Space)
    public Vector3 TerrainSize()
    {
        return terrain_size;
    }

    // Get terrain data
    public TerrainData GetTerrainData()
    {
        return terrain_data;
    }

    // Convert from Grid Space to World Space
    public Vector3 Grid2World(Vector3 grid)
    {
        return new Vector3(grid.x * terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z * terrain_data.heightmapScale.z);
    }

    public Vector3 Grid2World(float x, float y, float z)
    {
        return Grid2World(new Vector3(x, y, z));
    }

    public Vector3 Grid2World(float x, float z)
    {
        return Grid2World(x, 0.0f, z);
    }

    // Convert from World Space to Grid Space
    public Vector3 World2Grid(Vector3 grid)
    {
        return new Vector3(grid.x / terrain_data.heightmapScale.x,
                           grid.y,
                           grid.z / terrain_data.heightmapScale.z);
    }

    public Vector3 World2Grid(float x, float y, float z)
    {
        return World2Grid(new Vector3(x, y, z));
    }

    public Vector3 World2Grid(float x, float z)
    {
        return World2Grid(x, 0.0f, z);
    }

    // Reset to flat terrain
    public void Reset()
    {
        for (int z = 0; z < heightmap_height; z++)
        {
            for (int x = 0; x < heightmap_width; x++)
            {
                heightmap_data[z, x] = 0;
            }
        }

        Save();
    }

    // Smooth terrain
    public void AverageSmooth()
    {
        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {
                float n = 2.0f * 2 + 1.0f;
                float sum = 0;
                for (int szi = -2; szi <= 2; szi++)
                {
                    for (int sxi = -2; sxi <= 2; sxi++)
                    {
                        sum += heightmap_data[z + szi, x + sxi];
                    }
                }

                heightmap_data[z, x] = sum / (n * n);
            }
        }

        Save();
    }

    // Calculate Kernel
    public static float[,] CalculateKernel(int length, float sigma)
    {
        float[,] Kernel = new float[length, length];
        float sumTotal = 0f;

        int kernelRadius = length / 2;
        double distance = 0f;

        float calculatedEuler = 1.0f / (2.0f * (float)Math.PI * sigma * sigma);

        for (int idY = -kernelRadius; idY <= kernelRadius; idY++)
        {
            for (int idX = -kernelRadius; idX <= kernelRadius; idX++)
            {
                distance = ((idX * idX) + (idY * idY)) / (2 * (sigma * sigma));

                Kernel[idY + kernelRadius, idX + kernelRadius] = calculatedEuler * (float)Math.Exp(-distance);

                sumTotal += Kernel[idY + kernelRadius, idX + kernelRadius];
            }
        }

        for (int y = 0; y < length; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Kernel[y, x] = Kernel[y, x] * (1.0f / sumTotal);
            }
        }

        return Kernel;
    }

    // Gaussian Filter (Custom Kernel)
    public void GaussianBlurCustom()
    {
        float[,] kernel = CalculateKernel(3, 1f);

        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {

                heightmap_data[z, x] =
                    kernel[0, 0] * heightmap_data[z - 1, x - 1]
                    + kernel[0, 1] * heightmap_data[z - 1, x]
                    + kernel[0, 2] * heightmap_data[z - 1, x + 1]
                    + kernel[1, 0] * heightmap_data[z, x - 1]
                    + kernel[1, 1] * heightmap_data[z, x]
                    + kernel[1, 2] * heightmap_data[z, x + 1]
                    + kernel[2, 0] * heightmap_data[z + 1, x - 1]
                    + kernel[2, 1] * heightmap_data[z + 1, x]
                    + kernel[2, 2] * heightmap_data[z + 1, x + 1];
            }
        }

        Save();
    }

    // Gaussian Blur 3x3
    public void GaussianBlur3()
    {
        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {

                heightmap_data[z, x] =
                    heightmap_data[z - 1, x - 1]
                    + 2 * heightmap_data[z - 1, x]
                    + 1 * heightmap_data[z - 1, x + 1]
                    + 2 * heightmap_data[z, x - 1]
                    + 4 * heightmap_data[z, x]
                    + 2 * heightmap_data[z, x + 1]
                    + 1 * heightmap_data[z + 1, x - 1]
                    + 2 * heightmap_data[z + 1, x]
                    + 1 * heightmap_data[z + 1, x + 1];

                heightmap_data[z, x] *= 1.0f / 16.0f;

            }
        }

        Save();

    }

    // Gaussian Blur 5x5
    public void GaussianBlur5()
    {
        for (int z = 10; z < heightmap_height - 10; z++)
        {
            for (int x = 10; x < heightmap_width - 10; x++)
            {

                heightmap_data[z, x] =
                    heightmap_data[z - 2, x - 2]
                    + 4 * heightmap_data[z - 2, x - 1]
                    + 6 * heightmap_data[z - 2, x]
                    + heightmap_data[z - 2, x + 2]
                    + 4 * heightmap_data[z - 2, x + 1]
                    + 4 * heightmap_data[z - 1, x + 2]
                    + 16 * heightmap_data[z - 1, x + 1]
                    + 4 * heightmap_data[z - 1, x - 2]
                    + 16 * heightmap_data[z - 1, x - 1]
                    + 24 * heightmap_data[z - 1, x]
                    + 6 * heightmap_data[z, x - 2]
                    + 24 * heightmap_data[z, x - 1]
                    + 6 * heightmap_data[z, x + 2]
                    + 24 * heightmap_data[z, x + 1]
                    + 36 * heightmap_data[z, x]
                    + heightmap_data[z + 2, x - 2]
                    + 4 * heightmap_data[z + 2, x - 1]
                    + 6 * heightmap_data[z + 2, x]
                    + heightmap_data[z + 2, x + 2]
                    + 4 * heightmap_data[z + 2, x + 1]
                    + 4 * heightmap_data[z + 1, x + 2]
                    + 16 * heightmap_data[z + 1, x + 1]
                    + 4 * heightmap_data[z + 1, x - 2]
                    + 16 * heightmap_data[z + 1, x - 1]
                    + 24 * heightmap_data[z + 1, x];

                heightmap_data[z, x] *= 1.0f / 256.0f;

            }
        }

        Save();

    }

    // Register changes made to the terrain
    public void Save()
    {
        terrain_data.SetHeights(0, 0, heightmap_data);
    }

    // Get and set active brushes
    public void SetFootprintBrush(BrushPhysicalFootprintSphere brush)
    {
        Debug.Log("[INFO] Setting brush to " + brush);
        brushPhysicalFootprint = brush;
    }
    public BrushPhysicalFootprintSphere GetFootprintBrush()
    {
        return brushPhysicalFootprint;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Sphere")
        {
            isSphereGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Sphere")
        {
            isSphereGrounded = false;
        }
    }
}
