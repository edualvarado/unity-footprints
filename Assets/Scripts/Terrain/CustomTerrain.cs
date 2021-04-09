using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using CustomRagdoll; <-- MODIFIED

public class CustomTerrain : MonoBehaviour
{
    #region Variables

    [Header("Feet Properties")]
    public bool printFeetPositions = false;
    public Collider leftFootCollider;
    public Collider rightFootCollider;

    [Header("Feet Information ")]
    public bool isLeftFootGrounded;
    public bool isRightFootGrounded;
    public float heightIKLeft;
    public float heightIKRight;
    public Vector3 centerGridLeftFoot;
    public Vector3 centerGridRightFoot;

    [Header("Body Information ")]
    //public bool useCustomMass;
    //public float mass;
    public float customMass = 75f;
    public float force;

    private Brush current_brush;
    private Terrain terrain;
    private Collider terrain_collider;
    private TerrainData terrain_data;
    private Vector3 terrain_size;
    private int heightmap_width;
    private int heightmap_height;
    private float[,] heightmap_data;
    private float[,] heightmap_data_constant;
    private float[,] heightmap_data_filtered;

    private IKFeetPlacement _feetPlacement = null;
    //private Ragdoll _ragdoll = null; <-- MODIFIED

    #endregion

    // Initialization
    void Start()
    {
        // Extract terrain information
        if (!terrain)
            terrain = Terrain.activeTerrain;
        terrain_collider = terrain.GetComponent<Collider>();
        terrain_data = terrain.terrainData;
        terrain_size = terrain_data.size;
        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;
        heightmap_data = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_data_constant = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_data_filtered = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        current_brush = null;

        // Get classes
        _feetPlacement = FindObjectOfType<IKFeetPlacement>();
        //_ragdoll = FindObjectOfType<Ragdoll>(); <-- MODIFIED

        // Retrieve mass from Ragdoll
        //mass = _ragdoll.CalculateMass(); <-- MODIFIED

    }

    // Called once per frame
    void Update()
    {

        // Calculate mass and force
        //if (!useCustomMass)
        //    force = mass * Mathf.Abs(Physics.gravity.y);
        //else
        //    force = customMass * Mathf.Abs(Physics.gravity.y);

        force = customMass * Mathf.Abs(Physics.gravity.y);

        // Brush.cs needs them
        heightIKLeft = _feetPlacement.LeftFootIKPosition.y;
        heightIKRight = _feetPlacement.RightFootIKPosition.y;

        // For debug
        centerGridLeftFoot = World2Grid(_feetPlacement.LeftFootIKPosition.x, _feetPlacement.LeftFootIKPosition.z);
        centerGridRightFoot = World2Grid(_feetPlacement.RightFootIKPosition.x, _feetPlacement.RightFootIKPosition.z);

        // Just informative
        isLeftFootGrounded = _feetPlacement.isLeftFootGrounded;
        isRightFootGrounded = _feetPlacement.isRightFootGrounded;

        // Plot the position of the feet in both systems
        if (printFeetPositions)
        {
            Debug.Log("[INFO] Left Foot Coords (World): " + _feetPlacement.LeftFootIKPosition.ToString());
            Debug.Log("[INFO] Left Foot Coords (Grid): " + centerGridLeftFoot.ToString());
            Debug.Log("[INFO] Right Foot Coords (World): " + _feetPlacement.RightFootIKPosition.ToString());
            Debug.Log("[INFO] Right Foot Coords (Grid): " + centerGridRightFoot.ToString());
        }

        // Apply brush to feet
        if (current_brush)
        {
            current_brush.CallFootprint(_feetPlacement.LeftFootIKPosition.x, _feetPlacement.LeftFootIKPosition.z,
                _feetPlacement.RightFootIKPosition.x, _feetPlacement.RightFootIKPosition.z);
        }

    }


    /*
     * =======
     * Getters
     * =======
     */

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

    // Given one node of the heightmap, get the height.
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

    public float[,] GetHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return heightmap_data;
    }

    // Given one node of the heightmap (constant at start), get the height.
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

    public float[,] GetConstantHeightmap()
    {
        // IMPORTANT: When getting a value, must be multiplied by terrain_data.heightmapScale.y!
        return heightmap_data_constant;
    }
    // Given one node of the heightmap (constant at start), get the height.
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

    /*
     * =======
     * Setters
     * =======
     */

    // Given one node of the heightmap, set the height.
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

    /*
     * ====
     * Terrain Methods
     * ====
     */

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

    // Register changes made to the terrain
    public void Save()
    {
        terrain_data.SetHeights(0, 0, heightmap_data);
    }

    // Get and set active brushes
    public void SetBrush(Brush brush)
    {
        Debug.Log("[INFO] Setting brush to " + brush);
        current_brush = brush;
    }
    public Brush GetBrush()
    {
        return current_brush;
    }
}
