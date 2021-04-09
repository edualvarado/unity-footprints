using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Brush : MonoBehaviour
{

    #region Variables

    private bool active = false;

    protected CustomTerrain terrain;

    public bool isLeftFootGrounded;
    public bool isRightFootGrounded;
    private float heightIKLeft;
    private float heightIKRight;
    private Collider leftFootCollider;
    private Collider rightFootCollider;
    private Vector3 heightmapSize;
    private Vector3 terrainSize;
    private TerrainData terrainData;

    private float[,] heightmap_data_filtered;

    private float force;
    //private float mass;

    #endregion

    #region Properties

    public bool IsLeftFootGrounded
    {
        get { return isLeftFootGrounded; }
        set { isLeftFootGrounded = value; }

    }
    public bool IsRightFootGrounded
    {
        get { return isRightFootGrounded; }
        set { isRightFootGrounded = value; }
    }

    public float HeightIKLeft
    {
        get { return heightIKLeft; }
        set { heightIKLeft = value; }
    }
    public float HeightIKRight
    {
        get { return heightIKRight; }
        set { heightIKRight = value; }
    }

    public Collider LeftFootCollider
    {
        get { return leftFootCollider; }
        set { leftFootCollider = value; }
    }

    public Collider RightFootCollider
    {
        get { return rightFootCollider; }
        set { rightFootCollider = value; }
    }

    public Vector3 HeightmapSize
    {
        get { return heightmapSize; }
        set { heightmapSize = value; }
    }

    public Vector3 TerrainSize
    {
        get { return terrainSize; }
        set { terrainSize = value; }
    }

    //public float Mass
    //{
    //    get { return mass; }
    //    set { mass = value; }
    //}

    public float Force
    {
        get { return force; }
        set { force = value; }
    }

    public TerrainData TerrainData
    {
        get { return terrainData; }
        set { terrainData = value; }
    }

    public float[,] HeightMapFiltered
    {
        get { return heightmap_data_filtered; }
        set { heightmap_data_filtered = value; }

    }

    #endregion

    void Start()
    {
        terrain = GetComponent<CustomTerrain>();

        // Retrieve once public variables from CustomTerrain.cs
        LeftFootCollider = terrain.leftFootCollider;
        RightFootCollider = terrain.rightFootCollider;

        // Retrieve once though methods of CustomTerrain.cs
        HeightmapSize = terrain.GridSize();
        TerrainSize = terrain.TerrainSize();

        //mass = terrain.mass;
        //force = mass * 10f;

    }

    void Update()
    {
        // Retrieve each frame public variables from CustomTerrain.cs
        HeightIKLeft = terrain.heightIKLeft;
        HeightIKRight = terrain.heightIKRight;
        IsLeftFootGrounded = terrain.isLeftFootGrounded;
        IsRightFootGrounded = terrain.isRightFootGrounded;
        Force = terrain.force;

        // Retrieve each frame though methods of CustomTerrain.cs
        TerrainData = terrain.GetTerrainData();
    }

    public void Deactivate()
    {
        if (active)
            terrain.SetBrush(null);
        active = false;
    }

    public void Activate()
    {
        Brush active_brush = terrain.GetBrush();
        if (active_brush)
            active_brush.Deactivate();
        terrain.SetBrush(this);
        active = true;
    }

    public void Toggle()
    {
        if (IsActive())
            Deactivate();
        else
            Activate();
    }

    public bool IsActive()
    {
        return active;
    }

    public virtual void CallFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawFootprint(xLeft, zLeft, xRight, zRight);
    }

    // abstract = incomplete implementation that will be fullfiled in the child class (TerrainBrush)
    public abstract void DrawFootprint(float xLeft, float zLeft, float xRight, float zRight);
    public abstract void DrawFootprint(int xLeft, int zLeft, int xRight, int zRight);
}
