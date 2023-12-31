using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerADS : MonoBehaviour
{
    [Header("ADS Values")]
    [Tooltip("The time it takes for the gun to snap to and from the ADS point (can split into two separate times if needed)")]
    [SerializeField] private float adsSnapTime;
    [Tooltip("Point that the gun will lerp to when aiming down sights")]
    [SerializeField] private Transform adsTransform;
    [Tooltip("Non-ADS (hipfire) point that the gun starts at")]
    [SerializeField] private Transform initialTransform;
    [Tooltip("Reference to the gun model")]
    [SerializeField] private Transform gunModel;

    // Booleans
    // Tracks the current ADS state
    [HideInInspector] public bool aiming;

    // References
    // Reference to the player's input actions
    private InputManager input;
    // Reference to the UI manager
    private UIManager ui;
    // Reference to the crosshair manager
    private CrosshairManager crosshair;
    // Reference to the player gun
    private PlayerGun gun;
    // Reference to the player controller
    private PlayerController controller;

    private void Awake()
    {
        gun = GetComponent<PlayerGun>();
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        if (controller == null) { Debug.LogWarning("Missing a player in the scene!"); }
    }

    private void Start()
    {
        input = InputManager.instance;
        ui = UIManager.instance;
        crosshair = CrosshairManager.instance;
    }

    private void Update()
    {
        if((input.PlayerStartedAiming() || input.PlayerIsAiming()) && !aiming && !gun.reloading)
        {
            StartADS();
        }
        if (!input.PlayerIsAiming() && aiming)
        {
            StopADS();
        }
    }

    /// <summary>
    /// Begin aiming down sights (start the coroutine)
    /// </summary>
    public void StartADS()
    {
        aiming = true;
        StartCoroutine(LerpPosition(initialTransform.localPosition, adsTransform.localPosition));
        //gunModel.localPosition = adsTransform.localPosition;
    }

    /// <summary>
    /// Stop aiming down sights and lower gun
    /// </summary>
    public void StopADS()
    {
        aiming = false;
        StopAllCoroutines();
        StartCoroutine(LerpPosition(adsTransform.localPosition, initialTransform.localPosition));
        //gunModel.localPosition = initialTransform.localPosition;
    }

    /// <summary>
    /// Lerp the gun to / from a target from a starting point.
    /// Used for both raising and lowering the gun, changing the crosshair based on the aiming status.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    IEnumerator LerpPosition(Vector3 start, Vector3 target)
    {
        float time = 0;
        if(!aiming)
        {
            crosshair.EnableHipfireScreenCamCrosshair();
            controller.DisableAimSensitivity();
        }
        while (time < adsSnapTime)
        {
            gunModel.localPosition = Vector3.Lerp(start, target, time / adsSnapTime);
            time += Time.deltaTime;
            yield return null;
        }
        gunModel.localPosition = target;

        // gun has fully scoped in
        if (aiming && gunModel.localPosition == adsTransform.localPosition)
        {
            crosshair.EnableADSWorldspaceCrosshair();
            // switch the sensitivity to aiming
            controller.EnableAimSensitivity();
        }
        yield return null;
    }
}
