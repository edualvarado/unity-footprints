/****************************************************
 * File: BrushPhysicalFootprint.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Physically-driven Footprints Generation for Real-Time Interactions between a Character and Deformable Terrains
*****************************************************/

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// General parent class to create brushses that affect the ground (e.g. footprint brush).
/// </summary>
abstract public class BrushPhysicalFootprintSphere : MonoBehaviour
{
    #region Variables

    private bool active = false;

    // Terrain Data
    protected DeformTerrainMasterSphere terrain;
    private Vector3 heightmapSize;
    private Vector3 terrainSize;
    private TerrainData terrainData;
    private float[,] heightmap_data_filtered;

    // Body Properties
    private float mass;
    private bool isLeftFootGrounded;
    private bool isRightFootGrounded;
    private Collider leftFootCollider;
    private Collider rightFootCollider;
    private float weightInLeftFoot;
    private float weightInRightFoot;

    // Physics
    private float contactTime;
    private float feetSpeedLeftY;
    private float feetSpeedRightY;
    private Vector3 feetSpeedLeft;
    private Vector3 feetSpeedRight;
    private float totalForceY;
    private float totalForceLeftY;
    private float totalForceRightY;
    private Vector3 totalForce;
    private Vector3 totalForceLeft;
    private Vector3 totalForceRight;
    private float minTotalForceLeftFootZNorm;
    private float maxTotalForceLeftFootZNorm;
    private float minTotalForceRightFootZNorm;
    private float maxTotalForceRightFootZNorm;

    // Material/Ground
    private bool useTerrainPrefabs;
    private double youngM;
    private int filterIte;
    private float poissonRatio;
    private bool activateBump;

    // UI
    private bool useUI;
    private Slider youngSlider;
    private Slider poissonSlider;
    private Slider iterationsSlider;
    private Toggle activateToggleDef;
    private Toggle activateToggleBump;
    private Toggle activateToggleGauss;
    private Toggle activateToggleShowGrid;
    private Toggle activateToggleShowBump;

    #endregion

    #region Body Properties

    public float Mass
    {
        get { return mass; }
        set { mass = value; }
    }

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

    public float WeightInLeftFoot
    {
        get { return weightInLeftFoot; }
        set { weightInLeftFoot = value; }
    }
    public float WeightInRightFoot
    {
        get { return weightInRightFoot; }
        set { weightInRightFoot = value; }
    }

    #endregion

    #region Material Properties

    public bool ActivateBump
    {
        get { return activateBump; }
        set { activateBump = value; }
    }

    public float PoissonRatio
    {
        get { return poissonRatio; }
        set { poissonRatio = value; }
    }

    public double YoungM
    {
        get { return youngM; }
        set { youngM = value; }
    }

    public int FilterIte
    {
        get { return filterIte; }
        set { filterIte = value; }
    }

    #endregion

    #region Terrain Properties

    public TerrainData TerrainData
    {
        get { return terrainData; }
        set { terrainData = value; }
    }
    public Vector3 TerrainSize
    {
        get { return terrainSize; }
        set { terrainSize = value; }
    }

    public Vector3 HeightmapSize
    {
        get { return heightmapSize; }
        set { heightmapSize = value; }
    }

    public float[,] HeightMapFiltered
    {
        get { return heightmap_data_filtered; }
        set { heightmap_data_filtered = value; }

    }

    #endregion

    #region Force Properties

    public float MinTotalForceLeftFootZNorm
    {
        get { return minTotalForceLeftFootZNorm; }
        set { minTotalForceLeftFootZNorm = value; }
    }

    public float MaxTotalForceLeftFootZNorm
    {
        get { return maxTotalForceLeftFootZNorm; }
        set { maxTotalForceLeftFootZNorm = value; }
    }

    public float MinTotalForceRightFootZNorm
    {
        get { return minTotalForceRightFootZNorm; }
        set { minTotalForceRightFootZNorm = value; }
    }

    public float MaxTotalForceRightFootZNorm
    {
        get { return maxTotalForceRightFootZNorm; }
        set { maxTotalForceRightFootZNorm = value; }
    }

    public float ContactTime
    {
        get { return contactTime; }
        set { contactTime = value; }
    }

    public float FeetSpeedLeftY
    {
        get { return feetSpeedLeftY; }
        set { feetSpeedLeftY = value; }
    }
    public float FeetSpeedRightY
    {
        get { return feetSpeedRightY; }
        set { feetSpeedRightY = value; }
    }

    public Vector3 FeetSpeedLeft
    {
        get { return feetSpeedLeft; }
        set { feetSpeedLeft = value; }
    }
    public Vector3 FeetSpeedRight
    {
        get { return feetSpeedRight; }
        set { feetSpeedRight = value; }
    }

    public float TotalForceSphereY
    {
        get { return totalForceLeftY; }
        set { totalForceLeftY = value; }
    }
    public float TotalForceRightY
    {
        get { return totalForceRightY; }
        set { totalForceRightY = value; }
    }

    public float TotalForceY
    {
        get { return totalForceY; }
        set { totalForceY = value; }
    }

    public Vector3 TotalForceSphere
    {
        get { return totalForceLeft; }
        set { totalForceLeft = value; }
    }
    public Vector3 TotalForceRight
    {
        get { return totalForceRight; }
        set { totalForceRight = value; }
    }

    public Vector3 TotalForce
    {
        get { return totalForce; }
        set { totalForce = value; }
    }

    #endregion

    #region Other Properties

    public bool UseTerrainPrefabs
    {
        get { return useTerrainPrefabs; }
        set { useTerrainPrefabs = value; }
    }

    public bool UseUI
    {
        get { return useUI; }
        set { useUI = value; }
    }

    public Slider YoungSlider
    {
        get { return youngSlider; }
        set { youngSlider = value; }
    }

    public Slider PoissonSlider
    {
        get { return poissonSlider; }
        set { poissonSlider = value; }
    }

    public Slider IterationsSlider
    {
        get { return iterationsSlider; }
        set { iterationsSlider = value; }
    }

    public Toggle ActivateToggleDef
    {
        get { return activateToggleDef; }
        set { activateToggleDef = value; }
    }

    public Toggle ActivateToggleBump
    {
        get { return activateToggleBump; }
        set { activateToggleBump = value; }
    }

    public Toggle ActivateToggleGauss
    {
        get { return activateToggleGauss; }
        set { activateToggleGauss = value; }
    }

    public Toggle ActivateToggleShowGrid
    {
        get { return activateToggleShowGrid; }
        set { activateToggleShowGrid = value; }
    }

    public Toggle ActivateToggleShowBump
    {
        get { return activateToggleShowBump; }
        set { activateToggleShowBump = value; }
    }

    #endregion

    void Start()
    {
        // Get the terrain
        terrain = GetComponent<DeformTerrainMasterSphere>();

        // Retrieve once public variables from DeformTerrainMaster.cs
        LeftFootCollider = terrain.mySphereCollider;
        Mass = terrain.mass;
        ContactTime = terrain.contactTime;

        // Retrieve once though methods of DeformTerrainMaster.cs
        HeightmapSize = terrain.GridSize();
        TerrainSize = terrain.TerrainSize();
    }

    void Update()
    {
        // 1. Retrieve each frame public variables from DeformTerrainMaster.cs

        // A. Velocity of feet - Calculated in DeformTerrainMaster.cs
        FeetSpeedLeftY = terrain.sphereSpeed.y; 
        //FeetSpeedRightY = terrain.feetSpeedRight.y;
        FeetSpeedLeft = terrain.sphereSpeed;
        //FeetSpeedRight = terrain.feetSpeedRight;

        // Gravity + Reaction Force - Calculated in DeformTerrainMaster.cs
        // B. Only need magnitude in this case - that is why we take the GRF (is fine!)
        TotalForceSphereY = terrain.totalGRForceSphere.y; 
        TotalForceSphere = terrain.totalGRForceSphere;

        // C. Keep track of min and max forces reached during the gait
        //MinTotalForceLeftFootZNorm = terrain.minTotalForceLeftFootZNorm;
        //MaxTotalForceLeftFootZNorm = terrain.maxTotalForceLeftFootZNorm;
        //MinTotalForceRightFootZNorm = terrain.minTotalForceRightFootZNorm;
        //MaxTotalForceRightFootZNorm = terrain.maxTotalForceRightFootZNorm;

        // D. Are the feet grounded?
        IsLeftFootGrounded = terrain.isSphereGrounded;
        //IsRightFootGrounded = terrain.isRightFootGrounded;

        // E. Get if we are using prefabs option
        //UseTerrainPrefabs = terrain.useTerrainPrefabs;

        // Get if we are using the UI
        //UseUI = terrain.useUI;
        //YoungSlider = terrain.youngSlider;
        //PoissonSlider = terrain.poissonSlider;
        //IterationsSlider = terrain.iterationsSlider;
        //ActivateToggleDef = terrain.activateToggleDef;
        //ActivateToggleBump = terrain.activateToggleBump;
        //ActivateToggleGauss = terrain.activateToggleGauss;
        //ActivateToggleShowGrid = terrain.activateToggleShowGrid;
        //ActivateToggleShowBump = terrain.activateToggleShowBump;
    }

    public void Deactivate()
    {
        if (active)
            terrain.SetFootprintBrush(null);
        active = false;
    }

    public void Activate()
    {
        BrushPhysicalFootprintSphere active_brush = terrain.GetFootprintBrush();
        if (active_brush)
            active_brush.Deactivate();
        terrain.SetFootprintBrush(this);
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

    // 2. Virtual method that is used to pass the feet positions and create the physically-based footprint
    public virtual void CallFootprint(float x, float z)
    {
        DrawFootprint(x, z);
    }

    // abstract = incomplete implementation that will be fullfiled in the child class (TerrainBrush)
    public abstract void DrawFootprint(float x, float z);
    public abstract void DrawFootprint(int x, int z);
}
