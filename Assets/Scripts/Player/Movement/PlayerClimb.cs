using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerClimb : MonoBehaviour
{
    [Header("Climb Values")]
    [Tooltip("The speed that the player will climb at")]
    [SerializeField] private float climbSpeed;
    [Tooltip("The maximum duration that the player can climb before being kicked off the wall")]
    [SerializeField] private float maxClimbTime;

    [Header("Climb Jump Values")]
    [Tooltip("Amount of vertical force when jumping off the wall after climbing")]
    [SerializeField] private float climbJumpUpForce;
    [Tooltip("Amount of backwards force when jumping off the wall after climbing")]
    [SerializeField] private float climbJumpBackForce;
    [Tooltip("Amount of jumps the player is allowed to have while climbing")]
    [SerializeField] private float climbJumps;

    [Header("Detection Values")]
    [Tooltip("Distance that a player will register a wall (raycast length)")]
    [SerializeField] private float detectionLength;
    [Tooltip("The angle that the player is allowed to look away from the wall that they are climbing on before falling")]
    [SerializeField] private float maxWallLookAngle;
    [Tooltip("Raycast length for detecting the wall")]
    [SerializeField] private float sphereCastRadius;
    [Tooltip("The minimum amount that a wall normal needs to change")]
    [SerializeField] private float minWallNormalAngleChange;
    [Tooltip("Delay for the duration that the player must have exited the wall for in order to jump again")]
    [SerializeField] private float exitWallTime;

    // Hidden Values
    // Current angle of the player
    private float wallLookAngle;
    // Storage for the spherecast to return into
    private RaycastHit frontWallHit;
    // The last wall climbed by the player
    private Transform lastWall;
    // The normalized Vector3 of the last wall's transform
    private Vector3 lastWallNormal;
    // Counter for remaining jumps
    private float climbJumpsLeft;

    // Bools
    [Tooltip("Boolean to check whether or not the player is exiting the wall")]
    [HideInInspector] public bool exitingWall;
    // Boolean checking if a wall is directly in front of the player
    private bool wallFront;

    // Timers
    // Timer to track how long the player has been climbing
    private float climbTimer;
    // Timer for keeping track of exiting the wall
    private float exitWallTimer;

    // References
    // Reference to the player rigidbody
    private Rigidbody rb;
    // Reference to the player's input actions
    private InputManager input;
    // Reference for the camera transform
    private Transform cameraTransform;
    // Reference to the player controller
    private PlayerController controller;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        input = InputManager.instance;
    }

    private void Update()
    {
        WallCheck();
        StateMachine();
        if(controller.climbing && !exitingWall)
        {
            ClimbingMovement();
        }
    }

    /// <summary>
    /// Checks the player state and adjusts accordingly based on position to a wall, keyboard inputs, and other factors 
    /// </summary>
    private void StateMachine()
    {
        if (wallFront && input.PlayerIsHoldingForward() && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            if (!controller.climbing && climbTimer > 0)
            {
                StartClimbing();
            }
            if(climbTimer > 0) 
            { 
                climbTimer -= Time.deltaTime;
            }
            if (climbTimer < 0) 
            {
                StopClimbing();
            }
        }

        else if(exitingWall)
        {
            if (controller.climbing)
            {
                StopClimbing();
            }
            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer < 0)
            {
                exitingWall = false;
            }
        }

        else
        {
            if (controller.climbing)
            { 
                StopClimbing(); 
            }
        }

        if (wallFront && input.PlayerStartedJumping() && climbJumpsLeft > 0)
        {
            ClimbJump();
        }
    }

    /// <summary>
    /// Uses a spherecast to detect whether or not a wall is within a given angle from the player
    /// This information is used to detect when the player should be able to climb
    /// </summary>
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, cameraTransform.forward, out frontWallHit, detectionLength, controller.wallLayer);
        wallLookAngle = Vector3.Angle(cameraTransform.forward, -frontWallHit.normal);

        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if((wallFront && newWall) || controller.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climbJumps;
        }
    }

    // Debug utility, uncomment to see the raycast sphere in game
    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(transform.position, sphereCastRadius);
    //}

    /// <summary>
    /// Begins the climbing movement and changes the input state
    /// </summary>
    private void StartClimbing()
    {
        controller.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;
    }

    /// <summary>
    /// Sets the velocity to match the climbing speed
    /// </summary>
    private void ClimbingMovement()
    {
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
    }

    /// <summary>
    /// Stops the climbing state
    /// </summary>
    private void StopClimbing()
    {
        controller.climbing = false;
    }

    /// <summary>
    /// Adds force to the player for jumping off of the wall while climbing
    /// </summary>
    private void ClimbJump()
    {
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 forceToApply = transform.up * climbJumpUpForce + frontWallHit.normal * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }
}
