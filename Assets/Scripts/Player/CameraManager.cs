using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    // Reference to the singleton instance
    public static CameraManager instance;
    // Reference to the player transform
    private Transform player;
    // Reference to the virtual camera
    private CinemachineVirtualCamera cam;
    // Reference to the noise component of the camera
    private CinemachineBasicMultiChannelPerlin noise;
    // Reference to the virtual camera parent transform
    private Transform camParent;
    // Storage for the player rotation when they start wall running
    private Quaternion initialRotation;
    // Storage for the target player rotation
    private Quaternion targetRotation;
    // Reference to the main camera
    private Transform mainCam;
    // Boolean to determine wall run status
    private bool isWallRunning;
    // Storage for the amount of tilt on the camera
    private float tiltAmount;
    // Storage for the tilt duration
    private float tiltDuration;
    // Reference to the timer tilt
    private float tiltTimer = 0;
    // Rotation storage
    private Quaternion intermediateRotation;

    // Toggle for the camera shake
    [HideInInspector] public bool cameraShakeEnabled = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        cam = player.GetComponentInChildren<CinemachineVirtualCamera>();
        camParent = cam.transform.parent;
        noise = cam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        mainCam = Camera.main.transform;
        ResetIntensity();
    }

    private void Start()
    {
        initialRotation = Quaternion.Euler(camParent.transform.rotation.x, camParent.transform.rotation.y, camParent.transform.rotation.z);
        // load in camera setting from playerprefs
    }

    private void Update()
    {
        if (isWallRunning)
        {
            targetRotation = Quaternion.AngleAxis(tiltAmount, Camera.main.transform.forward);

            if (tiltTimer < tiltDuration)
            {
                camParent.transform.localRotation = Quaternion.Lerp(intermediateRotation, targetRotation, tiltTimer / tiltDuration);
            }
            else
            {
                if (targetRotation != camParent.transform.localRotation)
                {
                    intermediateRotation = camParent.transform.rotation;
                    tiltTimer = 0;
                }
            }

            tiltTimer += Time.deltaTime;
        }
        else
        {
            if (tiltTimer < tiltDuration)
            {
                camParent.transform.localRotation = Quaternion.Lerp(intermediateRotation, initialRotation, tiltTimer / tiltDuration);
            }
            else
            {
                camParent.transform.localRotation = initialRotation;
            }

            tiltTimer += Time.deltaTime;
        }
    }
    
    /// <summary>
    /// General camera shake function
    /// </summary>
    /// <param name="intensity">The strength of the camera shake</param>
    /// <param name="duration">How long the shake will last</param>
    public void CameraShake(float intensity, float duration)
    {
        if(cameraShakeEnabled)
        {
            noise.m_AmplitudeGain = intensity;
            StartCoroutine(ShakeTimer(duration));
        }
    }

    public void ToggleCameraShake(float val)
    {
        cameraShakeEnabled = (val == 1);
        //int toggle = cameraShakeEnabled ? 1 : 0;
        PlayerPrefs.SetInt("cameraShake", (int)val);
    }
    
    IEnumerator ShakeTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetIntensity();
    }

    /// <summary>
    /// Reset the camera shake intensity
    /// </summary>
    private void ResetIntensity()
    {
        noise.m_AmplitudeGain = 0f;
    }

    /// <summary>
    /// Tilts the camera towards the specified direction
    /// </summary>
    /// <param name="tiltA">The amount in degrees that the angle will change</param>
    /// <param name="tiltD">The duration of the tilt</param>
    /// <param name="left">Whether or not the tilt will lean left or right, passed in from the wall run</param>
    public void CameraTilt(float tiltA, float tiltD, bool left)
    {
        initialRotation = Quaternion.Euler(camParent.transform.rotation.x, camParent.transform.rotation.y, camParent.transform.rotation.z);
        intermediateRotation = initialRotation;
        Quaternion rot = camParent.transform.localRotation;

        tiltAmount = left ? tiltA : -tiltA;
        tiltDuration = tiltD;

        isWallRunning = true;
    }

    ///// <summary>
    ///// The outwards camera tilt
    ///// </summary>
    ///// <param name="target">Target position for the gun to rotate into</param>
    ///// <param name="duration">The length that the recoil lasts</param>
    ///// <returns></returns>
    //IEnumerator CameraTiltOutwards(Quaternion target, float duration)
    //{
    //    float time = 0;
    //    while (time < duration)
    //    {
    //        camParent.transform.transform.rotation = Quaternion.Lerp(initialRotation, target, time / duration);

    //        time += Time.deltaTime;
    //        yield return null;
    //    }
    //}
    
    /// <summary>
    /// Return the camera 
    /// </summary>
    /// <param name="duration"></param>
    public void ResetCameraTilt(float duration)
    {
        isWallRunning = false;

        tiltTimer = 0;
        intermediateRotation = camParent.transform.rotation;

    }

    ///// <summary>
    ///// The downwards portion of the recoil
    ///// Recoil can be interrupted by another shot, causing it to restart the upward portion and cancel the current status
    ///// </summary>
    ///// <param name="target">Target position for the gun to rotate into</param>
    ///// <param name="duration">The length that the recoil lasts</param>
    ///// <returns></returns>
    //IEnumerator CameraTiltInwards(Quaternion target, float duration)
    //{
    //    float time = 0;
    //    while (time < duration)
    //    {
    //        camParent.transform.localRotation = Quaternion.Lerp(target, initialRotation, time / duration);

    //        time += Time.deltaTime;
    //        yield return null;
    //    }
    //}
}