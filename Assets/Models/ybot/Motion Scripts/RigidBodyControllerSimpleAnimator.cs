using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyControllerSimpleAnimator : MonoBehaviour
{
    #region Variables

    [Header("Motion Options")]
    public Rigidbody _body;
    public Transform rootKinematicSkeleton;
    public Vector3 moveDirection;
    public Vector3 _inputs = Vector3.zero;
    public bool shooterCameraMode = false;

    [Header("Move using transform.Translate")]
    public bool blockCamera = false;
    public bool moveForwardOnly = false;
    public bool applyTransformPosition;
    public float moveSpeed = 1.0f;
    public float rotationSpeed = 280f;

    //public float JumpHeight = 2f;
    //public float DashDistance = 5f;

    [Header("Ground")]
    public Transform _groundChecker;
    public bool _isGrounded = true;
    public float GroundDistance = 0.2f;
    public LayerMask Ground;
    [Tooltip("Important: You need to put the terrain initially in the inspector")]
    public Terrain currentTerrain;

    [Header("Animation")]
    public Animator _anim;
    public float inputMagnitude;

    [Header("Based on Terrain")]
    //public float slopeAngle;
    public bool applyChangeSpeedTerrain;
    private TerrainMaster _terrain;
    //private HipsPIDController _hipsPIDController; <-- MODIFIED

    [Header("Experimental")]
    public bool applyMove = true;
    public bool applyRotation = false;
    public Vector3 m_EulerAngleVelocity = new Vector3(0, 100, 0);
    public bool applyTorque = false;
    public float torque;
    public float timer;
    public float signRotation = 1f;

    #endregion

    void Start()
    {
        // From same GameObject
        _body = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _terrain = GetComponent<TerrainMaster>();
        //_hipsPIDController = GetComponent<HipsPIDController>(); <-- MODIFIED

        // It takes it from Inspector
        //_groundChecker = transform.GetChild(0);
        //_groundChecker = transform.GetChild(transform.childCount - 1); // Since we disable gravity in our RB, we use to check ground

        // Set CoM for lower-body into the hips
        _body.centerOfMass = rootKinematicSkeleton.localPosition;

    }

    private void OnCollisionEnter(Collision collision)
    {

        Debug.Log("Collision: " + collision.gameObject.name);
        // Get the current terrain where the character is
        currentTerrain = collision.gameObject.GetComponent<Terrain>();
    }

    //***
    // ALL MOVEMENT SHOULD GO TO FIXED UPDATE!!! 
    //***

    // THIS WAS CHANGED FROM UPDATE TO FIXED UPDATE!
    void FixedUpdate()
    {
        if (applyChangeSpeedTerrain)
        {
            // Change speed based on slope - 30f in InverseLerp could be externalized.
            float t = Mathf.InverseLerp(0f, 30f, _terrain.SlopeAngle);
            
            //moveSpeed = Mathf.Lerp(1.5f, 1.0f, t);
            _anim.SetFloat("speedAnimation", Mathf.Lerp(1f, 0.75f, t), 0.0f, Time.deltaTime);

        }

        // Is grounded?
        _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);

        // User input
        _inputs = Vector3.zero;
        _inputs.x = Input.GetAxis("Horizontal");
        _inputs.z = Input.GetAxis("Vertical");

        // Direction of the character with respect to the input (e.g. W = (0,0,1))
        moveDirection = Vector3.forward * _inputs.z + Vector3.right * _inputs.x;
        //Debug.Log("moveDirection 1: " + moveDirection);

        // 1) Rotate with respect to the camera: Calculate camera projection on ground -> Change direction to be with respect to camera.
        Vector3 projectedCameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        Quaternion rotationToCamera = Quaternion.LookRotation(projectedCameraForward, Vector3.up);
        moveDirection = rotationToCamera * moveDirection;
        //Debug.Log("moveDirection 2: " + moveDirection);

        // 2) How to rotate the character: In shooter mode, the character rotates such that always points to the forward of the camera.
        if (shooterCameraMode)
        {
            if (_inputs != Vector3.zero)
            {
                if(!blockCamera)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToCamera, rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            if (_inputs != Vector3.zero && !moveForwardOnly)
            {
                Quaternion rotationToMoveDirection = Quaternion.LookRotation(moveDirection, Vector3.up);
                if (!blockCamera)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationToMoveDirection, rotationSpeed * Time.deltaTime);
            }
        }

        // We don't need to manipulate the transform - root motion moves the character though animations.
        // Onle in shooter mode, if you don't have the animations for lateral movement, you will need to move it this way.
        //if(applyTransformPosition)
        //{
        //    transform.position += moveDirection * moveSpeed * Time.deltaTime;
        //}

        // Animation Part
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

        //_inputs.Normalize();
        inputMagnitude = _inputs.sqrMagnitude;

        _anim.SetFloat("InputMagnitude", inputMagnitude, 0.0f, Time.deltaTime);

        _anim.SetFloat("speedAnimation", (moveSpeed * 0.66f), 0.0f, Time.deltaTime);

        /*
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _body.AddForce(Vector3.up * Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
        if (Input.GetButtonDown("Dash"))
        {
            Vector3 dashVelocity = Vector3.Scale(transform.forward, DashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * _body.drag + 1)) / -Time.deltaTime), 0, (Mathf.Log(1f / (Time.deltaTime * _body.drag + 1)) / -Time.deltaTime)));
            _body.AddForce(dashVelocity, ForceMode.VelocityChange);
        }
        */
    }

    //***
    // ALL MOVEMENT SHOULD GO TO FIXED UPDATE!!! 
    //***

    //void FixedUpdate()
    //{
    //    // Need to apply movement in FixedUpdate
    //    if (applyTransformPosition)
    //    {
    //        transform.position += moveDirection * moveSpeed * Time.fixedDeltaTime;
    //    }

    //    if (moveForwardOnly)
    //    {
    //        Camera.main.transform.rotation = Quaternion.identity;
    //        transform.position += transform.forward * _inputs.z * moveSpeed * Time.fixedDeltaTime;
    //    }

    //    // We don't need to push the RB - root motion moves the character though animations.
    //    if (applyMove)
    //        _body.MovePosition(_body.position + _inputs * moveSpeed * Time.fixedDeltaTime);

    //    /*
    //     * Use Rigidbody.MoveRotation to rotate a Rigidbody, complying with the Rigidbody's interpolation setting.
    //     * If Rigidbody interpolation is enabled on the Rigidbody, calling Rigidbody.MoveRotation will resulting in a smooth transition between the two rotations in any intermediate frames rendered.
    //     * This should be used if you want to continuously rotate a rigidbody in each FixedUpdate.
    //     */
    //    if (applyRotation)
    //    {
    //        timer += Time.deltaTime;

    //        Quaternion deltaRotation = Quaternion.Euler(signRotation * m_EulerAngleVelocity * Time.deltaTime);
    //        _body.MoveRotation(_body.rotation * deltaRotation);

    //        if (timer > 1)
    //        {
    //            timer = 0f;
    //            signRotation = -signRotation;
    //        }
    //    }

    //    /*
    //     * Adds a torque to the rigidbody.
    //     * Force can be applied only to an active rigidbody. If a GameObject is inactive, AddTorque has no effect.
    //     */
    //    if (applyTorque)
    //    {
    //        _body.AddTorque(torque * transform.up);
    //    }
    //}

}
