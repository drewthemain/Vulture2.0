using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// TO DO LIST
// set the recoil super low (switch out recoil object)
// -- physical recoil to half + spread lower
// swap out crosshair canvas overlay mode (worldspace vs camera)
// in UI manager add crosshair reference / canvas


public class PlayerADS : MonoBehaviour
{
    [Header("ADS Values")]
    [Tooltip("The time it takes for the gun to snap to and from the ADS point (can split into two separate times if needed)")]
    [SerializeField] private float adsSnapTime;
    [Tooltip("Point that the gun will lerp to when aiming down sights")]
    [SerializeField] private Transform adsTransform;
    [Tooltip("Non-ADS point that the gun starts at")]
    [SerializeField] private Transform initialTransform;
    [Tooltip("Reference to the gun model")]
    [SerializeField] private Transform gunModel;

    // Booleans
    // Tracks the current ADS state
    bool aiming;

    // References
    // Reference to the player's input actions
    private InputManager input;
    // Reference for the camera transform (used in calculating player direction)
    private Transform cameraTransform;
    // Reference to this players virtual camera
    private CinemachineVirtualCamera vCam;

    private void Awake()
    {
        vCam = GetComponentInChildren<CinemachineVirtualCamera>();
        cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        input = InputManager.instance;
    }

    private void Update()
    {
        if(input.PlayerStartedAiming() && !aiming)
        {
            StartADS();
        }
        if (!input.PlayerIsAiming() && aiming)
        {
            StopADS();
        }
    }

    private void StartADS()
    {
        aiming = true;
        StartCoroutine(LerpPosition(initialTransform.localPosition, adsTransform.localPosition));
        //gunModel.localPosition = adsTransform.localPosition;
    }

    private void StopADS()
    {
        aiming = false;
        StopAllCoroutines();
        StartCoroutine(LerpPosition(adsTransform.localPosition, initialTransform.localPosition));
        //gunModel.localPosition = initialTransform.localPosition;
    }


    /// <summary>
    /// The upwards portion of the recoil
    /// Recoil can be interrupted by another shot, causing it to restart the upward portion and cancel the current status
    /// </summary>
    /// <param name="target"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    IEnumerator LerpPosition(Vector3 start, Vector3 target)
    {
        float time = 0;
        while (time < adsSnapTime)
        {
            gunModel.localPosition = Vector3.Lerp(start, target, time / adsSnapTime);
            time += Time.deltaTime;
            yield return null;
        }

        gunModel.localPosition = target;
        yield return null;
    }
}
