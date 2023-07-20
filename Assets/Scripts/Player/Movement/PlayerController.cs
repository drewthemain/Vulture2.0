using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    // Speed that the player movement uses to move the player
    private float moveSpeed;
    [Tooltip("Walking speed for the player")]
    [SerializeField] private float walkSpeed;
    [Tooltip("Sprinting speed for the player")]
    [SerializeField] private float sprintSpeed;
    [Tooltip("Sliding speed for the player")]
    [SerializeField] private float slideSpeed;
    [Tooltip("Wall run speed for the player")]
    [SerializeField] private float wallRunSpeed;
    [Tooltip("Climbing speed for the player")]
    [SerializeField] private float climbSpeed;

    [Tooltip("Intended speed for the player to be moving at")]
    private float desiredMoveSpeed;
    [Tooltip("Most recent speed that the player should have been at")]
    private float lastDesiredMoveSpeed;

    [Tooltip("The amount of drag that the player will feel when grounded, slowing them down")]
    [SerializeField] private float groundDrag;

    [Tooltip("*Don't change this if you're not confident* The amount of speed needed for a speed change to instantly change the player's speed or gradually lower it")]
    [SerializeField] private float instantSpeedChangeThreshold = 4f;

    [Tooltip("The players current state")]
    public MovementState state;
    [Tooltip("Possible states for the player")]
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        air,
        wallrunning,
        climbing
    }

    public bool sliding;
    public bool wallrunning;
    public bool climbing;

    [Header("Jump Values")]
    [Tooltip("Determines the jump height of the player, along with impacts from physics (gravity, etc.)")]
    [SerializeField] private float jumpForce;
    [Tooltip("Cooldown before the player can jump again")]
    [SerializeField] private float jumpCooldown;
    [Tooltip("Multiplier for speed while flying through the air, also takes into account moveSpeed")]
    [SerializeField] private float airMultiplier;
    // Boolean for keeping track of when the player can jump based on the jump cooldown
    private bool readyToJump;

    [Header("Crouch Values")]
    [Tooltip("Crouching speed for the player")]
    [SerializeField] private float crouchSpeed;
    [Tooltip("How tall the player will be while crouching")]
    [SerializeField] private float crouchYScale;
    // The initial height of the player
    private float startYScale;

    [Header("Ground Check")]
    [Tooltip("The height of the player")]
    [SerializeField] private float playerHeight;
    [Tooltip("(Do not touch this) General boolean for checking if the player is on the ground")]
    public bool grounded;

    [Header("Slope Handling")]
    [Tooltip("Maximum angle for slopes before player can no longer walk up them")]
    [SerializeField] private float maxSlopeAngle;
    // Raycast checking for a slope under the player
    private RaycastHit slopeHit;
    // Handling for when the player jumps when on a slope
    private bool exitingSlope;

    [Header("Layer References")]
    [Tooltip("Layermask for checking for the ground, which is used for deciding when certain player actions can begin")]
    public LayerMask groundLayer;
    [Tooltip("Layermask for walls, used in wall run and climbing")]
    public LayerMask wallLayer;

    [Header("Debug")]
    [Tooltip("Drag in a TMPro textbox that will be set to the current state (one can be found in the Canvas in the GamePackage prefab labeled 'PlayerStateText')")]
    [SerializeField] private TextMeshProUGUI debugText;
    [Tooltip("Enable the debug textbox displaying the player state")]
    [SerializeField] private bool enableDebugText;

    // Global variables for storing player information
    // Global variable assigned by players input for calculating movement direction
    [HideInInspector] public float horizontalInput;
    // Global variable assigned by players input for calculating movement direction
    [HideInInspector] public float verticalInput;
    // Direction the player should move after the inputs have been multiplied in
    private Vector3 moveDirection;

    // References
    // Reference to the player rigidbody
    private Rigidbody rb;
    // Reference to the player's input actions
    private InputManager input;
    // Reference for the camera transform (used in calculating player direction)
    private Transform cameraTransform;
    // Reference to this players virtual camera
    private CinemachineVirtualCamera vCam;
    // Reference to the playerClimb script
    private PlayerClimb climb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        vCam = GetComponentInChildren<CinemachineVirtualCamera>();
        cameraTransform = Camera.main.transform;
        climb = GetComponent<PlayerClimb>();
    }

    private void Start()
    {
        input = InputManager.instance;
        rb.freezeRotation = true;
        readyToJump = true;

        startYScale = transform.localScale.y;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Raycast for checking if the player is on the ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, groundLayer);

        // Include being above a wall in the raycast
        bool groundedOnWall = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, wallLayer);
        if (groundedOnWall)
        {
            grounded = true;
        }

        UpdateInput();
        SpeedControl();
        StateHandler();

        // Drag handling
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Pulls player movement from the input manager and checks for jump input + conditions
    /// </summary>
    private void UpdateInput()
    {
        Vector2 movement = input.GetPlayerMovement();
        horizontalInput = movement.x;
        verticalInput = movement.y;

        // jump
        if (input.PlayerIsJumping() && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouching
        if(state != MovementState.crouching && input.PlayerIsCrouching() && !sliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouching
        if(!input.PlayerIsCrouching() && !sliding)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    /// <summary>
    /// Manages the different states for the player (sprint, walk, air)
    /// </summary>
    private void StateHandler()
    {
        // State - Climbing
        if(climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        // State - Wall Running
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallRunSpeed;
        }
        // State - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            if(OnSlope() && rb.velocity.y < 0.1f)
            {
                desiredMoveSpeed = slideSpeed;
            }
            else
            {
                desiredMoveSpeed = sprintSpeed;
            }
        }
        // State - Crouching
        else if (input.PlayerIsCrouching())
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        // State - Sprinting
        else if (grounded && input.PlayerIsSprinting())
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        // State - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        // State - Air
        else
        {
            state = MovementState.air;
        }

        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > instantSpeedChangeThreshold && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(LerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

        if (debugText && enableDebugText)
        {
            debugText.text = state.ToString();
        }
    }

    /// <summary>
    /// Physically moves the player, uses camera forward to determine look direction, multiplied by the player's inputs
    /// Add force to rigidbody + normalize
    /// </summary>
    private void MovePlayer()
    {
        if (climb.exitingWall) return;

        // calculate movement direction
        moveDirection = cameraTransform.forward * verticalInput + cameraTransform.right * horizontalInput;
        moveDirection.y = 0f;

        if(OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // gravity off while on slope
        if(!wallrunning) rb.useGravity = !OnSlope();
    }

    /// <summary>
    /// Caps the rigidbody velocity / overall player speed to avoid the player going faster than intended
    /// </summary>
    private void SpeedControl()
    {
        if(OnSlope() && !exitingSlope)
        {
            if(rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    /// <summary>
    /// Makes the player jump, adds force to the rigidbody
    /// </summary>
    private void Jump()
    {
        exitingSlope = true;
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Resets the jump ability to "ready" when cooldown runs out
    /// </summary>
    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    /// <summary>
    /// Checks whether or not the player is on a slope using a raycast and 
    /// comparing to the slope angle to the max slope angle variable.
    /// </summary>
    /// <returns>A boolean for if the player is on a slope</returns>
    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    /// <summary>
    /// Calculates the move direction by projecting a direction onto a slope.
    /// Primarily created for calculating the direction of the players inputs relative to the slope.
    /// </summary>
    /// <param name="dir">Direction being projected onto the plane</param>
    /// <returns>Returns the adjusted direction relative to the plane</returns>
    public Vector3 GetSlopeMoveDirection(Vector3 dir)
    {
        return Vector3.ProjectOnPlane(dir, slopeHit.normal).normalized;
    }

    /// <summary>
    /// Smoother speed change used to maintain some momentum + adjust the speed over time, rather than instantaneously.
    /// </summary>
    /// <returns></returns>
    private IEnumerator LerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            time += Time.deltaTime;
            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    /// <summary>
    /// Toggles the pausing of the game
    /// </summary>
    public void PauseGame()
    {
        if (GameManager.instance.TogglePause())
        {
            Cursor.lockState = CursorLockMode.None;
            vCam.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            vCam.enabled = true;
        }
    }

    public void ChangeSensitivity(float settingsValue)
    {

    }

    public void ChangeAimSensitivity(float settingsValue)
    {

    }
}
