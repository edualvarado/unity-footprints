/****************************************************
 * File: RigidBodyControllerSimpleAnimator.cs
   * Author: Eduardo Alvarado
   * Email: eduardo.alvarado-pinero@polytechnique.edu
   * Date: Created by LIX on 01/08/2021
   * Project: Real-Time Locomotion on Soft Grounds with Dynamic Footprints
   * Last update: 07/02/2022
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyControllerSimpleAnimator : MonoBehaviour
{
    #region Instance Fields

    [Header("Motion Options")]
    public Transform rootKinematicSkeleton;
    public Vector3 moveDirection;
    public Vector3 _inputs = Vector3.zero;
    public bool shooterCameraMode = false;
    public bool blockCamera = false;
    public bool moveForwardOnly = false;
    public float speedRotation = 280f;

    public enum motionMode
    {
        applyRootMotion,
        applyTransformPosition,
        applyRigidBodyVelocity,
        applyExperimental
    }

    [Header("Motion Modes")]
    public motionMode motion;
    public float speedAnimation = 1.0f;
    public float speedTransform = 1.0f;
    public float speedRigidBody = 1.0f;
    public float animationMultiplier = 0.66f;

    [Header("Ground")]
    public Transform _groundChecker;
    public bool _isGrounded = true;
    public float GroundDistance = 0.2f;
    public LayerMask Ground;
    [Tooltip("Important: You need to put the terrain initially in the inspector")]
    public Terrain currentTerrain;

    [Header("Animation")]
    public float inputMagnitude;

    [Header("Based on Terrain")]
    public float slopeAngle;
    public bool applyChangeSpeedTerrain;

    [Header("Experimental")]
    public bool applyMovePositionRB = true;
    public bool applyRotationRB = false;
    public Vector3 m_EulerAngleVelocity = new Vector3(0, 100, 0);
    public bool applyTorqueRB = false;
    public float torque;
    public float timer;
    public float signRotation = 1f;
    public Transform leftFoot;
    public Transform rightFoot;

    #endregion

    #region Read-only & Static Fields

    private Rigidbody _rb;
    private Animator _anim;
    private TerrainMaster _terrain;

    private float timeElapsedRun = 0f;
    private float timeElapsedWalk = 0f;

    #endregion

    #region Unity Methods

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        //_terrain = GetComponent<TerrainMaster>();

        // Set COM for lower-body into the hips
        _rb.centerOfMass = rootKinematicSkeleton.localPosition;

        // For running with keyboard
        timeElapsedWalk = 1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Get the current terrain where the character is
        currentTerrain = collision.gameObject.GetComponent<Terrain>();
        Debug.Log("[INFO] Collision: " + collision.gameObject.name);
    }

    private void FixedUpdate()
    {
        #region Terrain

        // Is it grounded?
        _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);
        
        // Terrain Angle
        //slopeAngle = _terrain.SlopeAngle;

        // TEST 
        if (applyChangeSpeedTerrain)
        {
            // Change speed based on slope - 30f in InverseLerp could be externalized
            float t = Mathf.InverseLerp(0f, 30f, slopeAngle);
            _anim.SetFloat("speedAnimation", Mathf.Lerp(1f, 0.75f, t), 0.0f, Time.deltaTime);
        }

        #endregion

        #region Motion

        // User input
        _inputs = Vector3.zero;
        _inputs.x = Input.GetAxis("Horizontal");
        _inputs.z = Input.GetAxis("Vertical");

        // We normalize the input in case we walk in diagonal
        if (_inputs.sqrMagnitude > 1f) _inputs.Normalize();

        // Direction of the character with respect to the input (e.g. W = (0,0,1))
        moveDirection = Vector3.forward * _inputs.z + Vector3.right * _inputs.x;
        //Debug.Log("[INFO] moveDirection 1: " + moveDirection);

        // 1) Rotate with respect to the camera: Calculate camera projection on ground -> Change direction to be with respect to camera
        Vector3 projectedCameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        Quaternion rotationToCamera = Quaternion.LookRotation(projectedCameraForward, Vector3.up);
        moveDirection = rotationToCamera * moveDirection;
        //Debug.Log("[INFO] moveDirection 2: " + moveDirection);

        // 2) How to rotate the character: In shooter mode, the character rotates such that always points to the forward of the camera
        if (shooterCameraMode)
        {
            if (_inputs != Vector3.zero)
            {
                if (!blockCamera)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToCamera, speedRotation * Time.deltaTime);
            }
        }
        else
        {
            if (_inputs != Vector3.zero && !moveForwardOnly)
            {
                Quaternion rotationToMoveDirection = Quaternion.LookRotation(moveDirection, Vector3.up);
                if (!blockCamera)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToMoveDirection, speedRotation * Time.deltaTime);
            }
        }

        // TEST
        // We don't need to manipulate the transform - if root motion is active, it moves the character though animations
        // Only in shooter mode, if you don't have the animations for lateral movement, you will need to move it this way
        if (motion == motionMode.applyTransformPosition)
        {
            _anim.applyRootMotion = false;
            
            if(moveForwardOnly)
                moveDirection = Vector3.forward * _inputs.z + Vector3.right * _inputs.x;
            
            transform.position += moveDirection * speedTransform * Time.deltaTime;
        }
        else if (motion == motionMode.applyRigidBodyVelocity)
        {
            _anim.applyRootMotion = false;

            if (moveForwardOnly)
                moveDirection = Vector3.forward * _inputs.z + Vector3.right * _inputs.x;
            
            _rb.velocity = moveDirection * speedRigidBody;
        }
        else if (motion == motionMode.applyExperimental)
        {
            _anim.applyRootMotion = false;
        }
        else if (motion == motionMode.applyRootMotion)
        {
            _anim.applyRootMotion = true;
        }

        #endregion

        #region Animation Controller

        if (_inputs != Vector3.zero)
        {
            _anim.SetBool("isWalking", true);
        }
        else
        {
            _anim.SetBool("isWalking", false);
        }

        _anim.SetFloat("InputX", _inputs.x, 0.0f, Time.deltaTime);
        _anim.SetFloat("InputZ", _inputs.z, 0.0f, Time.deltaTime);

        inputMagnitude = _inputs.sqrMagnitude;

        // For running with keyboard
        if ((!Input.GetKey(KeyCode.Space)) && inputMagnitude > 0.5f)
        {
            inputMagnitude = 0.5f;

            if (timeElapsedWalk < 0.8f)
            {
                inputMagnitude = Mathf.Lerp(1f, 0.5f, timeElapsedWalk / 0.8f);
                timeElapsedWalk += Time.deltaTime;
            }

            timeElapsedRun = 0f;
        }
        else if ((Input.GetKey(KeyCode.Space)) && inputMagnitude > 0.5f)
        {
            if (timeElapsedRun < 0.8f)
            {
                inputMagnitude = Mathf.Lerp(0.5f, 1f, timeElapsedRun / 0.8f);
                timeElapsedRun += Time.deltaTime;
            }

            timeElapsedWalk = 0f;
        }

        _anim.SetFloat("InputMagnitude", inputMagnitude, 0.0f, Time.deltaTime);

        // By changing speedAnimation, makes the corresponding change in the animation
        if (motion == motionMode.applyTransformPosition)
        {
            _anim.SetFloat("speedAnimation", (speedTransform * animationMultiplier), 0.0f, Time.deltaTime);
        }
        else if ((motion == motionMode.applyExperimental))
        {
            _anim.SetFloat("speedAnimation", (speedRigidBody * animationMultiplier), 0.0f, Time.deltaTime);
        }
        else if (motion == motionMode.applyRootMotion)
        {
            _anim.SetFloat("speedAnimation", speedAnimation, 0.0f, Time.deltaTime);
        }

        // Jump
        if (Input.GetKey(KeyCode.Space))
        {
            _anim.SetBool("isJumping", true);
        }
        else
        {
            _anim.SetBool("isJumping", false);
        }

        /*
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _rb.AddForce(Vector3.up * Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
        if (Input.GetButtonDown("Dash"))
        {
            Vector3 dashVelocity = Vector3.Scale(transform.forward, DashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * _rb.drag + 1)) / -Time.deltaTime), 0, (Mathf.Log(1f / (Time.deltaTime * _rb.drag + 1)) / -Time.deltaTime)));
            _rb.AddForce(dashVelocity, ForceMode.VelocityChange);
        }
        */

        #endregion

        #region Experimental

        if (motion == motionMode.applyExperimental)
        {
            // TEST
            if (applyMovePositionRB)
                _rb.MovePosition(_rb.position + transform.TransformVector(_inputs) * speedRigidBody * Time.deltaTime);

            /*
             * Use Rigidbody.MoveRotation to rotate a Rigidbody, complying with the Rigidbody's interpolation setting.
             * If Rigidbody interpolation is enabled on the Rigidbody, calling Rigidbody.MoveRotation will resulting in a smooth transition between the two rotations in any intermediate frames rendered.
             * This should be used if you want to continuously rotate a rigidbody in each FixedUpdate.
             */

            if (applyRotationRB)
            {
                timer += Time.deltaTime;

                Quaternion deltaRotation = Quaternion.Euler(signRotation * m_EulerAngleVelocity * Time.deltaTime);
                _rb.MoveRotation(_rb.rotation * deltaRotation);

                if (timer > 1)
                {
                    timer = 0f;
                    signRotation = -signRotation;
                }
            }

            /*
             * Adds a torque to the rigidbody.
             * Force can be applied only to an active rigidbody. If a GameObject is inactive, AddTorque has no effect.
             */

            if (applyTorqueRB)
            {
                _rb.AddTorque(torque * transform.up);
            }
        }

        #endregion
    }

    private void LateUpdate()
    {
        // TEST
        // In case there is friction that slows down the rigid body, we change the animation by the actual velocity
        if(motion == motionMode.applyRigidBodyVelocity)
        {
            _anim.SetFloat("speedAnimation", (_rb.velocity.sqrMagnitude * animationMultiplier), 0.0f, Time.deltaTime);
        }
    }

    #endregion
}
