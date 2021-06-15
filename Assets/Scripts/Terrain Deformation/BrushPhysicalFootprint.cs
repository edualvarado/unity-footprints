using UnityEngine;

abstract public class BrushPhysicalFootprint : MonoBehaviour
{
    #region Variables

    private bool active = false;

    // Terrain Data
    protected DeformTerrainMaster terrain;
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

    // Materials
    public double youngM;

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

    public double YoungM
    {
        get { return youngM; }
        set { youngM = value; }
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

    public float TotalForceLeftY
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

    public Vector3 TotalForceLeft
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

    void Start()
    {
        // Get the terrain
        terrain = GetComponent<DeformTerrainMaster>();

        // Retrieve once public variables from DeformTerrainMaster.cs
        LeftFootCollider = terrain.leftFootCollider;
        RightFootCollider = terrain.rightFootCollider;
        Mass = terrain.mass;
        ContactTime = terrain.contactTime;

        // Retrieve once though methods of DeformTerrainMaster.cs
        HeightmapSize = terrain.GridSize();
        TerrainSize = terrain.TerrainSize();
    }

    void Update()
    {
        // Retrieve each frame public variables from DeformTerrainMaster.cs

        // Velocity of feet - Calculated in DeformTerrainMaster.cs
        FeetSpeedLeftY = terrain.feetSpeedLeft.y; 
        FeetSpeedRightY = terrain.feetSpeedRight.y;
        FeetSpeedLeft = terrain.feetSpeedLeft;
        FeetSpeedRight = terrain.feetSpeedRight;

        // Gravity + Reaction Force - Calculated in DeformTerrainMaster.cs
        // Only need magnitude in this case - that is why we take the GRF (is fine!)
        TotalForceY = terrain.totalGRForce.y;
        TotalForceLeftY = terrain.totalGRForceLeft.y; 
        TotalForceRightY = terrain.totalGRForceRight.y;
        TotalForce = terrain.totalGRForce;
        TotalForceLeft = terrain.totalGRForceLeft;
        TotalForceRight = terrain.totalGRForceRight;

        // Are the feet grounded?
        IsLeftFootGrounded = terrain.isLeftFootGrounded;
        IsRightFootGrounded = terrain.isRightFootGrounded;
    }

    public void Deactivate()
    {
        if (active)
            terrain.SetFootprintBrush(null);
        active = false;
    }

    public void Activate()
    {
        BrushPhysicalFootprint active_brush = terrain.GetFootprintBrush();
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

    // Virtual method that is used to pass the feet positions and create the physically-based footprint
    public virtual void CallFootprint(float xLeft, float zLeft, float xRight, float zRight)
    {
        DrawFootprint(xLeft, zLeft, xRight, zRight);
    }

    // abstract = incomplete implementation that will be fullfiled in the child class (TerrainBrush)
    public abstract void DrawFootprint(float xLeft, float zLeft, float xRight, float zRight);
    public abstract void DrawFootprint(int xLeft, int zLeft, int xRight, int zRight);
}
