/****************************************************
 * File: BalanceTorque.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

/* In progress to improve - Version belongs to short paper "SoftWalks" */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalanceTorque : MonoBehaviour
{
    #region Instance Fields

    [Header("Torque")] 
    public bool addTorque = true;
    public bool showTerrainNormalAngleDebug;
    [SerializeField] private float slopeAngle = 0.0f;
    [SerializeField] private float slopeAngleOld = 0.0f;
    [SerializeField] private float alpha = 30.0f;
    [SerializeField] private float beta = 6.0f;

    [Header("Terrain Deformation")] 
    public bool modifyTerrainOLD = false;
    public bool showFootprintDebug = false;
    public float rayDistance = 1f;
    [Range(1, 10)] public int filter_coefficient = 1; // Wet Sand: 2; Dry Sand: 10 
    [Range(0, 1.0f)] public float compression_coefficient = 0.5f; // Wet Sand: 0.9f; Dry Sand: 0.3f 
    [Range(0, 5.0f)] public float depth_coefficient = 0.5f; // Wet Sand: 0.5f; Dry Sand: 0.3f

    public Collider leftFoot;
    public Collider rightFoot;

    #endregion

    #region Read-only & Static Fields

    private IKFeetPlacement _feetIK;
    private Rigidbody _rb;
    private Animator _anim;

    private Vector3 rightFootPosition;
    private Vector3 leftFootPosition;
    private Vector3 rightFootIKPosition;
    private Vector3 leftFootIKPosition;

    private float targetRbPosition;

    private Terrain terrain;
    private TerrainData terrain_data;
    private int heightmap_width;
    private int heightmap_height;
    private float[,] heightmap_data;
    private float[,] heightmap_buffer;
    private float[,] heightmap_filtered;
    private int[,] contourmap_data;
    private int[,] contourmap_buffer;

    private int count;

    #endregion

    #region Unity Methods

    void Start()
    {
        GetTerrain();

        _feetIK = GetComponent<IKFeetPlacement>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
    }

    void Update()
    {
        GetTerrainSlope();

        rightFootPosition = _feetIK.rightFootPosition;
        leftFootPosition = _feetIK.leftFootPosition;
        rightFootIKPosition = _feetIK.rightFootIKPosition;
        leftFootIKPosition = _feetIK.leftFootIKPosition;
    }

    private void FixedUpdate()
    {
        if (modifyTerrainOLD)
            ModifyTerrain();
    }

    #endregion

    #region Instance Methods

    private void OnAnimatorIK(int layerIndex)
    {
        BalanceCharacter();
    }

    private void BalanceCharacter()
    {
        float dotRightFoot = Vector3.Dot(rightFootPosition, transform.forward);
        float dotLeftFoot = Vector3.Dot(leftFootPosition, transform.forward);

        if ((dotRightFoot <= dotLeftFoot)) //Left foot in front of right foot
        {
            targetRbPosition = Vector3.Dot(_feetIK.RightFootPosition, transform.forward) + ((_feetIK.LeftFootIKPosition - _feetIK.RightFootIKPosition).magnitude / 2.0f) * Mathf.Cos(slopeAngleOld * 3.1415f / 180.0f);
        }
        else
        {
            targetRbPosition = Vector3.Dot(_feetIK.LeftFootPosition, transform.forward) + ((_feetIK.RightFootIKPosition - _feetIK.LeftFootIKPosition).magnitude / 2.0f) * Mathf.Cos(slopeAngleOld * 3.1415f / 180.0f);
        }

        float currentPos = Vector3.Dot(_anim.bodyPosition, transform.forward);
        float currentAngVel = Vector3.Dot(_rb.angularVelocity, transform.right);

        float tau = alpha * (currentPos - targetRbPosition) + beta * (currentAngVel - 0);

        if (addTorque)
        {
            Debug.Log("[INFO] Adding torque (tau:  " + tau + ")  to character!");
            _rb.AddTorque(-transform.right * tau);
        }
    }

    private void GetTerrainSlope()
    {
        // Get terrain slope
        float pos_x = _rb.position.x / terrain_data.size.x;
        float pos_z = _rb.position.z / terrain_data.size.z;
        Vector3 normal = terrain_data.GetInterpolatedNormal(pos_x, pos_z);
        float gradient = terrain_data.GetSteepness(pos_x, pos_z);

        // To define if character is climbing up or down with respect to its direction
        Vector3 local_normal = this.transform.InverseTransformDirection(normal);
        slopeAngle = local_normal.z < 0 ? gradient : -gradient;

        // Old method
        if (normal.z < 0)
        {
            slopeAngleOld = gradient;
        }
        else
        {
            slopeAngleOld = -gradient;
        }

        // To visualize the ray
        if (showTerrainNormalAngleDebug)
        {
            Debug.DrawLine(_rb.position, _rb.position + normal, Color.cyan);
            Debug.Log("[INFO] slopeAngle: " + slopeAngle);
            Debug.Log("[INFO] slopeAngleOld: " + slopeAngleOld);
        }
    }

    private void GetTerrain()
    {
        // Get terrain information
        if (!terrain)
            terrain = Terrain.activeTerrain;
        terrain_data = terrain.terrainData;
        heightmap_width = terrain_data.heightmapResolution;
        heightmap_height = terrain_data.heightmapResolution;
        heightmap_data = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_buffer = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        heightmap_filtered = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);
        contourmap_data = new int[heightmap_height, heightmap_width];
        contourmap_buffer = new int[heightmap_height, heightmap_width];
    }

    private void ModifyTerrain()
    {
        count = 0;

        /*
         * If we modify the terrain at each moment, some noise appears. We need to only do it when each individual foot is grounded.
         * Now, only terrain gets modified when the foot is grounded.We can change the distance of the checkSphere to alter this.
         * Also is faster, since updates only happen when the foot are grounded.
         */

        if (_feetIK.isLeftFootGrounded)
        {
            // Modify contourmap with raycasting
            SetContourMap(_feetIK.LeftFootIKPosition);

            // Set contours of the foot prints
            SetOnes(_feetIK.LeftFootIKPosition);

            // Holes under the feet
            SetHeightMap(_feetIK.LeftFootIKPosition);

            // Blur the contours and iterate
            FilterHeightMap(heightmap_buffer);
            for (int i = 1; i < filter_coefficient; i++)
            {
                FilterHeightMap(heightmap_filtered);
            }

            // Save changes
            terrain_data.SetHeights(0, 0, heightmap_filtered);
        }

        if (_feetIK.isRightFootGrounded)
        {
            // Modify contourmap with raycasting
            SetContourMap(_feetIK.RightFootIKPosition);

            // Set contours of the foot prints
            SetOnes(_feetIK.RightFootIKPosition);

            // Holes under the feet
            SetHeightMap(_feetIK.RightFootIKPosition);

            // Blur the contours and iterate
            FilterHeightMap(heightmap_buffer);
            for (int i = 1; i < filter_coefficient; i++)
            {
                FilterHeightMap(heightmap_filtered);
            }

            // Save changes
            terrain_data.SetHeights(0, 0, heightmap_filtered);
        }
    }

    private void SetContourMap(Vector3 targetFoot)
    {
        // Feet location in the terrain with respect to the heighmap resolution (e.g. [0, 512])
        int indX = (int)((targetFoot.x / terrain_data.size.x) * heightmap_width);
        int indZ = (int)((targetFoot.z / terrain_data.size.z) * heightmap_height);

        Debug.Log("[INFO] indX: " + indX + " | indZ: " + indZ);

        /* Why?
        // Out of bounds: Close to the limits of the terrain in heightmap scale
        if ((indX <= 20) || (indZ <= 20) || (indX > heightmap_width - 20) || (indZ > heightmap_height - 20))
        {
            Debug.Log("OUT OF BOUNDS");
            return;
        }
        */

        // 20 since is around the foot
        for (int i = -10; i < 10; i++)
        {
            for (int j = -10; j < 10; j++)
            {
                //Debug.Log("[INFO] Targetfoot lower than floor");
                // World Space Coordinates of each cell or column on the grid
                Vector3 columnPos = new Vector3((indX + i) * terrain_data.size.x / heightmap_width, targetFoot.y, (indZ + j) * terrain_data.size.z / heightmap_height);

                //Debug.DrawRay(columnPos, Vector3.up, Color.white);

                Ray ray = new Ray();
                ray.origin = columnPos;
                ray.direction = Vector3.up * rayDistance;
                RaycastHit outHit;

                if (targetFoot == _feetIK.RightFootIKPosition && rightFoot.Raycast(ray, out outHit, 2.0f))
                {
                    if (showFootprintDebug)
                    {
                        //Debug.Log("[INFO] Hit right sole");
                        Debug.DrawRay(columnPos, Vector3.up, Color.blue);
                    }

                    contourmap_data[indZ + j, indX + i] = 2;
                    contourmap_buffer[indZ + j, indX + i] = 2;
                    count += 1;
                }
                else if (targetFoot == _feetIK.LeftFootIKPosition && leftFoot.Raycast(ray, out outHit, 2.0f))
                {
                    if (showFootprintDebug)
                    {
                        //Debug.Log("[INFO] Hit left sole");
                        Debug.DrawRay(columnPos, Vector3.up, Color.red);
                    }

                    contourmap_data[indZ + j, indX + i] = 2;
                    contourmap_buffer[indZ + j, indX + i] = 2;
                    count += 1;
                }

                // Need to see why this if! Works better without that constraint

                /*
                // If the foot are below the terrain.
                if (targetFoot.y < heightmap_data[indZ + j, indX + i] * terrain_data.heightmapScale.y)
                {
                    //Debug.Log("Targetfoot lower than floor");
                    // World Space Coordinates of each cell or column on the grid.
                    Vector3 columnPos = new Vector3((indX + i) * terrain_data.size.x / heightmap_width, targetFoot.y, (indZ + j) * terrain_data.size.z / heightmap_height);

                    //Debug.DrawRay(columnPos, Vector3.up, Color.white);

                    Ray ray = new Ray();
                    ray.origin = columnPos;
                    ray.direction = Vector3.up;
                    RaycastHit outHit;

                    if (targetFoot == rightFootIKPosition && soleColliderRight.Raycast(ray, out outHit, 2.0f))
                    {
                        if(showFootprintDebug)
                        {
                            //Debug.Log("[INFO] Hit right sole");
                            Debug.DrawRay(columnPos, Vector3.up, Color.blue);
                        }

                        contourmap_data[indZ + j, indX + i] = 2;
                        contourmap_buffer[indZ + j, indX + i] = 2;
                        count += 1;
                    }
                    else if (targetFoot == leftFootIKPosition && soleColliderLeft.Raycast(ray, out outHit, 2.0f))
                    {
                        if (showFootprintDebug)
                        {
                            //Debug.Log("[INFO] Hit left sole");
                            Debug.DrawRay(columnPos, Vector3.up, Color.red);
                        }

                        contourmap_data[indZ + j, indX + i] = 2;
                        contourmap_buffer[indZ + j, indX + i] = 2;
                        count += 1;
                    }
                }
                */
            }
        }
    }

    private void SetOnes(Vector3 targetFoot)
    {
        //Indices of foot transform in grid
        int indX = (int)((targetFoot.x / terrain_data.size.x) * heightmap_width);
        int indZ = (int)((targetFoot.z / terrain_data.size.z) * heightmap_height);

        //Out of bounds
        if ((indX <= 15) || (indZ <= 15) || (indX > heightmap_width - 15) || (indZ > heightmap_height - 15))
        {
            return;
        }

        for (int i = -10; i < 10; i++)
        {
            for (int j = -10; j < 10; j++)
            {
                if (contourmap_data[indZ + j, indX + i] == 0)
                {
                    if ((contourmap_data[indZ + j - 1, indX + i + 1] == 2) || (contourmap_data[indZ + j - 1, indX + i - 1] == 2)
                        || (contourmap_data[indZ + j - 1, indX + i] == 2) || (contourmap_data[indZ + j + 1, indX + i + 1] == 2)
                        || (contourmap_data[indZ + j + 1, indX + i - 1] == 2) || (contourmap_data[indZ + j + 1, indX + i] == 2)
                        || (contourmap_data[indZ + j, indX + i - 1] == 2) || (contourmap_data[indZ + j, indX + i + 1] == 2))
                    {
                        contourmap_data[indZ + j, indX + i] = 1;
                        contourmap_buffer[indZ + j, indX + i] = 1;
                    }
                }
            }
        }
    }

    private void SetHeightMap(Vector3 targetFoot)
    {
        int indX = (int)((targetFoot.x / terrain_data.size.x) * heightmap_width);
        int indZ = (int)((targetFoot.z / terrain_data.size.z) * heightmap_height);

        if ((indX <= 10) || (indZ <= 10) || (indX > heightmap_width - 10) || (indZ > heightmap_height - 10))
        {
            return;
        }

        // Decrease for foot and increase surrounding cells
        for (int i = -10; i < 10; i++)
        {
            for (int j = -10; j < 10; j++)
            {
                if (contourmap_buffer[indZ + j, indX + i] == 2)
                {
                    heightmap_buffer[indZ + j, indX + i] = heightmap_data[indZ + j, indX + i] - ((0.1f * depth_coefficient) / terrain_data.heightmapScale.y);
                    contourmap_buffer[indZ + j, indX + i] = 0;
                }
                else if (contourmap_buffer[indZ + j, indX + i] == 1)
                {
                    heightmap_buffer[indZ + j, indX + i] = heightmap_data[indZ + j, indX + i] + (0.003f * (1.0f - compression_coefficient) * count / terrain_data.heightmapScale.y);
                    contourmap_buffer[indZ + j, indX + i] = 0;
                }
            }
        }

        // Without filtering
        //terrain_data.SetHeights(0, 0, heightmap_buffer);
    }

    private void FilterHeightMap(float[,] heightmap)
    {
        float[,] result = terrain_data.GetHeights(0, 0, heightmap_width, heightmap_height);

        int indX = (int)((transform.position.x / terrain_data.size.x) * heightmap_width);
        int indZ = (int)((transform.position.z / terrain_data.size.z) * heightmap_height);

        if ((indX <= 10) || (indZ <= 10) || (indX > heightmap_width - 10) || (indZ > heightmap_height - 10))
        {
            return;
        }

        //Decrease for foot and increase surrounding cells
        for (int i = -10; i < 10; i++)
        {
            for (int j = -10; j < 10; j++)
            {
                //Gaussian filter 3x3
                /*                result[indZ + j, indX + i] = heightmap[indZ + j - 1, indX + i - 1]
                                    + heightmap[indZ + j - 1, indX + i + 1]
                                    + heightmap[indZ + j + 1, indX + i - 1]
                                   + heightmap[indZ + j + 1, indX + i + 1]
                                   + 2 * (heightmap[indZ + j - 1, indX + i]
                                   + heightmap[indZ + j + 1, indX + i]
                                   + heightmap[indZ + j, indX + i - 1]
                                   + heightmap[indZ + j, indX + i + 1])
                                   + 4 * heightmap[indZ + j, indX + i];

                                heightmap[indZ + j, indX + i] *= 1.0f / 16.0f;*/

                //Gaussian filter 5x5
                result[indZ + j, indX + i] =
                    heightmap[indZ + j - 2, indX + i - 2]
                    + 4 * heightmap[indZ + j - 2, indX + i - 1]
                    + 6 * heightmap[indZ + j - 2, indX + i]
                    + heightmap[indZ + j - 2, indX + i + 2]
                    + 4 * heightmap[indZ + j - 2, indX + i + 1]
                    + 4 * heightmap[indZ + j - 1, indX + i + 2]
                    + 16 * heightmap[indZ + j - 1, indX + i + 1]
                    + 4 * heightmap[indZ + j - 1, indX + i - 2]
                    + 16 * heightmap[indZ + j - 1, indX + i - 1]
                    + 24 * heightmap[indZ + j - 1, indX + i]
                    + 6 * heightmap[indZ + j, indX + i - 2]
                    + 24 * heightmap[indZ + j, indX + i - 1]
                    + 6 * heightmap[indZ + j, indX + i + 2]
                    + 24 * heightmap[indZ + j, indX + i + 1]
                    + 36 * heightmap[indZ + j, indX + i]
                    + heightmap[indZ + j + 2, indX + i - 2]
                    + 4 * heightmap[indZ + j + 2, indX + i - 1]
                    + 6 * heightmap[indZ + j + 2, indX + i]
                    + heightmap[indZ + j + 2, indX + i + 2]
                    + 4 * heightmap[indZ + j + 2, indX + i + 1]
                    + 4 * heightmap[indZ + j + 1, indX + i + 2]
                    + 16 * heightmap[indZ + j + 1, indX + i + 1]
                    + 4 * heightmap[indZ + j + 1, indX + i - 2]
                    + 16 * heightmap[indZ + j + 1, indX + i - 1]
                    + 24 * heightmap[indZ + j + 1, indX + i];

                result[indZ + j, indX + i] *= 1.0f / 256.0f;
            }
        }

        heightmap_filtered = result;
    }

    #endregion
}
