using TMPro;
using UnityEngine;

public class PlayerWallRun : MonoBehaviour
{
    [Header("Wall Run Values")]
    [Tooltip("The amount of force that's pushing the player when wall running")]
    [SerializeField] private float wallRunForce;
    [Tooltip("Amount of vertical force when jumping off of a wall run")]
    [SerializeField] private float wallJumpUpForce;
    [Tooltip("Amount of horizontal force when jumping off of a wall run")]
    [SerializeField] private float wallJumpSideForce;
    [Tooltip("The maximum duration that the player can continue to wall run")]
    [SerializeField] private float maxWallRunTime;
    [Tooltip("Time the player must wait between jumping from a wall and starting another wall run")]
    [SerializeField] private float wallRunCooldown;
    // Active timer for the wall run's duration
    private float wallRunTimer;
    // Active timer for the cooldown after the player jumps from a wall
    private float exitWallTimer;

    [Header("Detection Values")]
    [Tooltip("Distance that a player will register a wall for wall running")]
    [SerializeField] private float wallCheckDistance;
    [Tooltip("The angle that the player is allowed to look away from the wall that they are running on before falling (total distance between blue / red debug raycasts in scene)")]
    [SerializeField] private float wallRunLookAngle;
    [Tooltip("Minimum distance a player needs to jump to wall run")]
    [SerializeField] private float minJumpHeight;

    [Header("Gravity")]
    [Tooltip("Whether or not the wall run will be affected by gravity / slowly drop while running")]
    [SerializeField] private bool useGravity;
    [Tooltip("Amount of force applied to counteract the effects of gravity - higher = player drops less (or flies away)")]
    [SerializeField] private float gravityCounterForce;

    [Header("Debug Values")]
    [Tooltip("Enable to show raycasts from the player used for calculating when the wall run should end - blue = left, red = right")]
    [SerializeField] private bool showRaycastsInScene;

    // Bools
    // Checks if there's a runnable wall to the right of the player
    private bool wallRight;
    // Checks if there's a runnable wall to the left of the player
    private bool wallLeft;
    // Checks if the wall exiting timer has run out
    private bool exitingWall;

    // Raycasts
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

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

    // Start is called before the first frame update
    void Start()
    {
        input = InputManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(controller.wallrunning)
        {
            WallRunMovement();
        }
    }

    /// <summary>
    /// Uses raycasts to check whether or not the player is next to a wall, and specifies which side they're next to.
    /// While not wall running, the raycasts are a straight line out to the right and left of the player.
    /// While wall running, the raycasts have increased range to help keep the player stuck to the wall.
    /// </summary>
    private void CheckForWall()
    {
        float adjustedAngle = wallRunLookAngle / 2;
        DebugWallRunRaycasts(adjustedAngle);

        wallLeft = Physics.Raycast(transform.position, -cameraTransform.right, out leftWallHit, wallCheckDistance, controller.wallLayer);

        if (controller.wallrunning)
        {
            bool wallLeftForward = Physics.Raycast(transform.position, Quaternion.AngleAxis(adjustedAngle, Vector3.up) * -cameraTransform.right, wallCheckDistance, controller.wallLayer);
            bool wallLeftBackward = Physics.Raycast(transform.position, Quaternion.AngleAxis(-adjustedAngle, Vector3.up) * -cameraTransform.right, wallCheckDistance, controller.wallLayer);
            wallLeft = wallLeft || wallLeftForward || wallLeftBackward;
        }

        wallRight = Physics.Raycast(transform.position, cameraTransform.right, out rightWallHit, wallCheckDistance, controller.wallLayer);

        if (controller.wallrunning)
        {
            bool wallRightForward = Physics.Raycast(transform.position, Quaternion.AngleAxis(-adjustedAngle, Vector3.up) * cameraTransform.right, wallCheckDistance, controller.wallLayer);
            bool wallRightBackward = Physics.Raycast(transform.position, Quaternion.AngleAxis(adjustedAngle, Vector3.up) * cameraTransform.right, wallCheckDistance, controller.wallLayer);
            wallRight = wallRight || wallRightForward || wallRightBackward;
        }
    }

    /// <summary>
    /// When active, shows blue / red raycasts in the scene view for the angles used to calculate wall run walls.
    /// Blue = left, red = right
    /// </summary>
    /// <param name="ang">The total angle that the raycasts cover</param>
    private void DebugWallRunRaycasts(float ang)
    {
        if(showRaycastsInScene)
        {
            // left straight out
            Debug.DrawRay(transform.position, -cameraTransform.right, Color.blue);
            // left and forwards
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(ang, Vector3.up) * -cameraTransform.right, Color.blue);
            // left and backwards
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-ang, Vector3.up) * -cameraTransform.right, Color.blue);

            // right straight out
            Debug.DrawRay(transform.position, cameraTransform.right, Color.red);
            // right and forwards
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-ang, Vector3.up) * cameraTransform.right, Color.red);
            // right and backwards
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(ang, Vector3.up) * cameraTransform.right, Color.red);
        }
    }

    /// <summary>
    /// Layer check for the player.
    /// </summary>
    /// <returns>Returns if the player is grounded or not</returns>
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, controller.groundLayer);
    }   
    
    /// <summary>
    /// Checks the player state and adjusts accordingly based on position to a wall, keyboard inputs, and other factors. 
    /// </summary>
    private void StateMachine()
    {
        // wall running
        if ((wallLeft || wallRight) && input.PlayerIsHoldingForward() && AboveGround() && !exitingWall)
        {
            if(!controller.wallrunning)
            {
                StartWallRun();
            }

            // wall run timer
            if(wallRunTimer > 0)
            {
                wallRunTimer -= Time.deltaTime;
            }

            if(wallRunTimer <= 0 && controller.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = wallRunCooldown;
            }


            // wall jump
            if(input.PlayerStartedJumping())
            {
                WallJump();
            }
        }

        else if(exitingWall)
        {
            if(controller.wallrunning)
            {
                StopWallRun();
            }
            if(exitWallTimer > 0)
            {
                exitWallTimer -= Time.deltaTime;
            }
            if(exitWallTimer <= 0)
            {
                exitingWall = false;
            }
        }

        // not wall running
        else
        {
            if(controller.wallrunning)
            {
                StopWallRun();
            }
        }
        
    }

    /// <summary>
    /// Begin the wall run, removing the players vertical velocity.
    /// </summary>
    private void StartWallRun()
    {
        controller.wallrunning = true;
        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
    }

    /// <summary>
    /// Calculates the amount of force being added and the direction to add it for the player while wall running.
    /// Calculates forward push, push to the wall, and gravity.
    /// </summary>
    private void WallRunMovement()
    {
        rb.useGravity = useGravity;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if((cameraTransform.forward - wallForward).magnitude > (cameraTransform.forward - -wallForward).magnitude)
        {
            wallForward = -wallForward;
        }

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        //push to wall force
        if(!(wallLeft && input.PlayerIsHoldingLeft()) && !(wallRight && input.PlayerIsHoldingRight()))
        {
            rb.AddForce(-wallNormal * 100, ForceMode.Force);
        }

        // weaken gravity
        if (useGravity)
        {
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
        }
    }

    /// <summary>
    /// Reset wallrunning boolean state.
    /// </summary>
    private void StopWallRun()
    {
        controller.wallrunning = false;
    }

    /// <summary>
    /// Jump off the wall according to the given values for the vertical and horizontal planes of the jump, along with reseting the wall timer.
    /// </summary>
    private void WallJump()
    {
        // enter exiting wall state
        exitingWall = true;
        exitWallTimer = wallRunCooldown;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }
}
