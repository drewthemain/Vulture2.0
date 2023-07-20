using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPlayerWallRun : MonoBehaviour
{
    [Header("Wall Run Values")]
    [Tooltip("Layermask for specifying what should be wall-runnable")]
    [SerializeField] private LayerMask wallRunLayer;
    [Tooltip("The amount of force used when wall running")]
    [SerializeField] private float wallRunForce;
    [Tooltip("The maximum duration that the player can continue to wall run")]
    [SerializeField] private float maxWallRunTime;
    [Tooltip("The maximum speed that the player can reach while wall running")]
    [SerializeField] private float maxWallRunSpeed;
    [Tooltip("The maximum amount of tilt the camera can reach while wall running")]
    [SerializeField] private float maxWallRunCameraTilt;
    [Tooltip("Current amount of camera tilt from wall running")]
    [SerializeField] private float wallRunCameraTilt;
    
    // Bools
    // Checks if there's a runnable wall to the right of the player
    public bool isWallRight;
    // Checks if there's a runnable wall to the left of the player
    public bool isWallLeft;
    // Checks if the player is wall running
    public bool isWallRunning;

    // Reference to the player rigidbody
    private Rigidbody rb;
    // Reference to the player's input actions
    private InputManager input;
    // Reference for the camera transform
    private Transform cameraTransform;
    // Reference to the player controller
    private PlayerController controller;
    // Orientation of the player without vertical input
    [SerializeField] private Transform orientation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<PlayerController>();
        cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        input = InputManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        CheckForWall();
        WallRunInput();
    }

    private void WallRunInput()
    {
        // Start Wall Run
        if(input.PlayerIsHoldingRight() && isWallRight)
        {
            StartWallRun();
        }
        if (input.PlayerIsHoldingLeft() && isWallLeft)
        {
            StartWallRun();
        }
    }

    private void StartWallRun()
    {
        Debug.Log("test");
        rb.useGravity = false;
        isWallRunning = true;
        controller.wallrunning = true;

        if (rb.velocity.magnitude <= maxWallRunSpeed)
        {
            rb.AddForce(orientation.forward * wallRunForce * Time.deltaTime);

            // Stick to wall
            if (isWallRight)
            {
                rb.AddForce(cameraTransform.right * wallRunForce / 5f * Time.deltaTime);
            }
            else
            {
                rb.AddForce(-cameraTransform.right * wallRunForce / 5f * Time.deltaTime);

            }
        }
    }

    private void StopWallRun()
    {
        Debug.Log("stop");

        rb.useGravity = true;
        isWallRunning = false;
        controller.wallrunning = false;
    }

    private void CheckForWall()
    {
        Debug.DrawRay(transform.position, cameraTransform.forward, Color.green, 10f);
        Debug.DrawRay(transform.position, cameraTransform.right, Color.red, 10f);
        Debug.DrawRay(transform.position, -cameraTransform.right, Color.blue, 10f);

        isWallRight = Physics.Raycast(transform.position, cameraTransform.right, 1f, wallRunLayer);
        isWallLeft = Physics.Raycast(transform.position, -cameraTransform.right, 1f, wallRunLayer);

        // Exit wall run
        if (!isWallLeft && !isWallRight)
        {
            StopWallRun();
        }
    }
}
