using UnityEngine;
using TMPro;

public class PlayerSlide : MonoBehaviour
{
    // References
    // Reference to the player rigidbody
    private Rigidbody rb;
    // Reference to the player's input actions
    private InputManager input;
    // Reference for the camera transform (used in calculating player direction)
    private Transform cameraTransform;
    // Reference to the player controller
    private PlayerController controller;

    [Header("Slide Values")]
    [Tooltip("The maximum time in seconds that the slide should last before becoming a crouch")]
    [SerializeField] private float maxSlideDuration;
    [Tooltip("Multiplier for how powerful the slide will be")]
    [SerializeField] private float slideForce;
    [Tooltip("The height the player should become when sliding")]
    [SerializeField] private float slideYScale;
    [Tooltip("The speed the player must be going to slide rather than crouching")]
    [SerializeField] private float slideSpeedThreshold;

    // The initial height of the player before sliding
    private float startYScale;
    // Elapsed time of the slide
    private float slideTimer;

    // Not currently assigned
    private float horizontalInput;
    // Not currently assigned
    private float verticalInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        input = InputManager.instance;
        startYScale = transform.localScale.y;
    }

    void Update()
    {
        // begin sliding if player is...
        // holding crouch
        // moving forward
        // not already sliding
        // moving fast enough to begin sliding
        if (input.PlayerStartedCrouching() && input.GetPlayerMovement().y > 0 && !controller.sliding && SlideCheck())
        {
            StartSlide();
        }
        // cancel slide if player presses sprint, 
        if (input.PlayerStartedSprinting() && controller.sliding)
        {
            StopSlide();
        }
        //if (input.PlayerStartedCrouching() && controller.sliding)
        //{
        //    Debug.Log("Crouch one");
        //    StopSlide();
        //}
        // cancel slide if player jumps
        if(input.PlayerStartedJumping() && controller.sliding)
        {
            StopSlide();
        }
    }

    private void FixedUpdate()
    {
        if(controller.sliding)
        {
            SlidingMovement();
        }
    }
    
    /// <summary>
    /// Start sliding the player, scale down to slide Y scale size and add force to lower them to that height. 
    /// Also resets the slide timer back to full.
    /// </summary>
    public void StartSlide()
    {
        controller.sliding = true;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideDuration;
    }

    /// <summary>
    /// Calculation for the force being added to the player to move them while sliding.
    /// Also increments the slide timer while the player is sliding on a non-sloped surface.
    /// </summary>
    private void SlidingMovement()
    {
        // currently doesn't account for input, needs tweaking
        Vector3 inputDir = cameraTransform.forward * verticalInput + cameraTransform.right * horizontalInput;

        if (!controller.OnSlope())
        {
            rb.AddForce(inputDir.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }

        else
        {
            // adds downward force to keep player on slope
            rb.AddForce(Vector3.down * 100f, ForceMode.Force);
            rb.AddForce(controller.GetSlopeMoveDirection(inputDir) * slideForce, ForceMode.Force);
        }


        if (slideTimer <= 0)
        {
            StopSlideCrouch();
        }
    }

    /// <summary>
    /// Stopping functionality for when the timer runs out for the slide duration but the player has not cancelled input.
    /// Function will keep the player crouching (not change transform back) and state handler will take over for crouch speed change.
    /// </summary>
    public void StopSlideCrouch()
    {
        controller.sliding = false;
    }

    /// <summary>
    /// Standard slide stop in which the player's sliding value will be set back and the player will stand back up to normal height. 
    /// </summary>
    public void StopSlide()
    {
        controller.sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

    /// <summary>
    /// Checks whether or not the player's rigidbody is moving fast enough for a crouch to be considered a slide / vice versa
    /// </summary>
    /// <returns>Returns TRUE if the player is fast enough to SLIDE, returns FALSE if slow enough to CROUCH </returns>
    public bool SlideCheck()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (flatVel.magnitude > slideSpeedThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
