using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

//[RequireComponent(typeof(CharacterController))]
public class IKFeetPlacement : MonoBehaviour
{
    #region Variables

    public Animator anim;
    //public CharacterController controller;

    [Header("Feet Positions")]
    public Vector3 rightFootPosition;
    public Vector3 leftFootPosition;
    public Vector3 rightFootIKPosition;
    public Vector3 leftFootIKPosition;

    public Vector3 RightFootPosition { 
        get { return rightFootPosition; }
        set { rightFootPosition = value; }
    }

    public Vector3 LeftFootPosition
    {
        get { return leftFootPosition; }
        set { leftFootPosition = value; }
    }

    public Vector3 RightFootIKPosition
    {
        get { return rightFootIKPosition; }
        set { rightFootIKPosition = value; }
    }
    public Vector3 LeftFootIKPosition
    {
        get { return leftFootIKPosition; }
        set { leftFootIKPosition = value; }
    }

    private Quaternion leftFootIKRotation, rightFootIKRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    [Header("Feet Grounder")]
    public bool enableFeetIK = true;
    [Range(0, 20f)] [SerializeField] private float heightFromGroundRaycast = 0.2f;
    [Range(0, 20f)] [SerializeField] private float raycastDownDistance = 1.0f;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float pelvisOffset = 0f;
    [Range(0, 1f)] [SerializeField] private float pelvisUpAndDownSpeed = 0.3f;
    [Range(0, 1f)] [SerializeField] private float feetToIKPositionSpeed = 0.2f;

    [Header("Individual Feet Grounder")]
    public bool drawSensorRayGrounder = false;
    public bool isLeftFootGrounded = false;
    public bool isRightFootGrounded = false;
    public Transform groundCheckerLeftFootBack;
    public Transform groundCheckerLeftFoot;
    public Transform groundCheckerRightFoot;
    public Transform groundCheckerRightFootBack;
    public float feetToGroundDistance = 0.1f;

    [Header("Other Settings")]
    public string leftFootAnimVariableName = "LeftFootCurve";
    public string rightFootAnimVariableName = "RightFootCurve";
    public bool useProIKFeature = true;
    public bool showSolverDebug = true;

    #endregion

    // Start is called before the first frame update.
    void Start()
    {
        // Getting components from Inspector.
        anim = GetComponent<Animator>();
        //controller = GetComponent<CharacterController>();

        // Set Environment Layer
        environmentLayer = LayerMask.GetMask("Ground");
    }

    #region FeetGrounding

    /// <summary>
    /// Update the AdjustFeetTarget method and also find the position of each foot inside our Solver Position.
    /// </summary>
    private void FixedUpdate()
    {
        if(enableFeetIK == false) { return; }
        if(anim == null) { return; }

        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

        // Find a raycast to the ground to find positions.
        FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation); // Handle the solver for right foot
        FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation); // Handle the solver for left foot

        // Check if each foot is grounded.
        CheckFeetAreGrounded();

        // IMPORTANT!
        // The pivot is the most stable point between the left and right foot of the avatar. For a value of 0, the left foot is the most stable point. For a value of 1, the right foot is the most stable point.
        //Debug.Log("Pivot Weight: " + anim.pivotWeight);
        //Debug.Log("Pivot Position: " + this.transform.InverseTransformPoint(anim.pivotPosition));

    }

    /*
    private void CheckFeetAreGrounded()
    {
        if (!isRightFootGrounded)
        {
            if (Physics.CheckSphere(groundCheckerLeftFoot.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(groundCheckerLeftFootBack.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            {
                isLeftFootGrounded = true;
            }
            else
            {
                isLeftFootGrounded = false;
            }
        }

        if (!isLeftFootGrounded)
        {
            if (Physics.CheckSphere(groundCheckerRightFoot.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(groundCheckerRightFootBack.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
            {
                isRightFootGrounded = true;
            }
            else
            {
                isRightFootGrounded = false;
            }
        }
    }
    */

    private void CheckFeetAreGrounded()
    {
  
        if (Physics.CheckSphere(groundCheckerLeftFoot.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(groundCheckerLeftFootBack.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
        {
            isLeftFootGrounded = true;
        }
        else
        {
            isLeftFootGrounded = false;
        }
        
        if (Physics.CheckSphere(groundCheckerRightFoot.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(groundCheckerRightFootBack.position, feetToGroundDistance, environmentLayer, QueryTriggerInteraction.Ignore))
        {
            isRightFootGrounded = true;
        }
        else
        {
            isRightFootGrounded = false;
        }

        if(drawSensorRayGrounder)
        {
            Debug.DrawRay(groundCheckerLeftFoot.position, -Vector3.up * feetToGroundDistance, Color.white);
            Debug.DrawRay(groundCheckerLeftFootBack.position, -Vector3.up * feetToGroundDistance, Color.white);
            Debug.DrawRay(groundCheckerRightFoot.position, -Vector3.up * feetToGroundDistance, Color.white);
            Debug.DrawRay(groundCheckerRightFootBack.position, -Vector3.up * feetToGroundDistance, Color.white);
        }
    }

    /// <summary>
    /// Called when IK Pass is activated.
    /// </summary>
    /// <param name="layerIndex"></param>
    private void OnAnimatorIK(int layerIndex)
    {
        if (anim == null) { return; }

        MovePelvisHeight();

        // RightFoot IK Position  - Max. IK position.
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        // RightFoot IK Rotation  - for PRO feature.
        if (useProIKFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
        }

        // Move RightFoot to the target IK position and rotation.
        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);

        // LeftFoot IK Position  - Max. IK position.
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

        // LeftFoot IK Rotation  - for PRO feature.
        if (useProIKFeature)
        {
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
        }

        // Move LeftFoot to the target IK position and rotation.
        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
    }

    #endregion

    #region FeetGroundingMethods

    /// <summary>
    /// Move feet to the IK target.
    /// </summary>
    /// <param name="foot"></param>
    /// <param name="positionIKHolder"></param>
    /// <param name="rotationIKHolder"></param>
    /// <param name="lastFootPositionY"></param>
    void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY)
    {
        //  Get the current position of the foot, which we are going to move.
        Vector3 targetIKPosition = anim.GetIKPosition(foot);

        // If there is a IK target in a different position (not 0 locally) than the position where we have our foot currently.
        if (positionIKHolder != Vector3.zero)
        {
            // Convert the world coordinates for current/target foot positions to local coordinates with respect to the character.
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder);

            // Calculate the translation in Y necessary to move the last foot position to the target position, by a particular speed.
            float yVariable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, feetToIKPositionSpeed);

            // Add this desired translation in Y to our current feet position.
            targetIKPosition.y += yVariable;

            // We update the last foot position in Y.
            lastFootPositionY = yVariable;

            // Convert the current foot position to world coordinates.
            targetIKPosition = transform.TransformPoint(targetIKPosition);

            // Set the new goal rotation (world coordinates) for the foot.
            anim.SetIKRotation(foot, rotationIKHolder);
        }

        // Set the new goal position (world coordinates) for the foot.
        anim.SetIKPosition(foot, targetIKPosition);
    }

    /// <summary>
    /// Adapt height of pelvis - TODO: REVIEW
    /// </summary>
    private void MovePelvisHeight()
    {
        if(rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = anim.bodyPosition.y;
            return;
        }

        float leftOffsetPosition = leftFootIKPosition.y - transform.position.y;
        float rightOffsetPosition = rightFootIKPosition.y - transform.position.y;

        float totalOffset = (leftOffsetPosition < rightOffsetPosition) ? leftOffsetPosition: rightOffsetPosition;

        // Hold new pelvis position where we want to move to.
        // Move from last to new position with certain speed.
        Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;
        newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);

        // Update current body position.
        anim.bodyPosition = newPelvisPosition;

        // Now the last known pelvis position in Y is the current body position in Y.
        lastPelvisPositionY = anim.bodyPosition.y;
    }

    /// <summary>
    /// Locate the feet position via a raycast and then solving.
    /// </summary>
    /// <param name="fromSkyPosition"></param>
    /// <param name="feetIKPositions"></param>
    /// <param name="feetIKRotations"></param>
    private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations)
    {
        // To store all the info regarding the hit of the ray
        RaycastHit feetoutHit;

        // To visualize the ray
        if (showSolverDebug)
        {
            Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDownDistance + heightFromGroundRaycast), Color.yellow);
        }

        // If the ray, starting at the sky position, goes down certain distance and hits an environment layer.
        if (Physics.Raycast(fromSkyPosition, Vector3.down, out feetoutHit, raycastDownDistance + heightFromGroundRaycast, environmentLayer))
        {
            // Position the new IK feet positions parallel to the sky position, and put them where the ray intersects with the environment layer.
            feetIKPositions = fromSkyPosition;
            feetIKPositions.y = feetoutHit.point.y + pelvisOffset;
            // Creates a rotation from the (0,1,0) to the normal of where the feet is placed it in the terrain.
            feetIKRotations = Quaternion.FromToRotation(Vector3.up, feetoutHit.normal) * transform.rotation;

            return;
        }

        feetIKPositions = Vector3.zero; // If we reach this, it didn't work.
    }

    /// <summary>
    /// Adjust the IK target for the feet.
    /// </summary>
    /// <param name="feetPositions"></param>
    /// <param name="foot"></param>
    private void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        // Takes the Vector3 transform of that human bone id.
        feetPositions = anim.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + heightFromGroundRaycast;
    }

    #endregion

}
