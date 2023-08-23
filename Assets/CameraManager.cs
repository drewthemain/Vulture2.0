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
    }

    public void CameraShake(float intensity, float duration)
    {
        noise.m_AmplitudeGain = intensity;
        StartCoroutine(ShakeTimer(duration));
    }

    IEnumerator ShakeTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetIntensity();
    }

    private void ResetIntensity()
    {
        noise.m_AmplitudeGain = 0f;
    }

    public void CameraTilt(float tiltAmount, float tiltDuration, bool left)
    {
        initialRotation = Quaternion.Euler(camParent.transform.rotation.x, camParent.transform.rotation.y, camParent.transform.rotation.z);
        Quaternion rot = camParent.transform.localRotation;

        tiltAmount = left ? tiltAmount : -tiltAmount;

        //target = Quaternion.Euler(rot.x, rot.y, rot.z + tiltAmount);
        //target = Quaternion.Euler(camParent.transform.localRotation.x, camParent.transform.localRotation.y, camParent.transform.localRotation.z + tiltAmount);
        Quaternion target = Quaternion.Euler(camParent.transform.localRotation.x, camParent.transform.localRotation.y, tiltAmount);

        targetRotation = target;
        StartCoroutine(CameraTiltOutwards(target, tiltDuration));
    }

    /// <summary>
    /// The outwards camera tilt
    /// </summary>
    /// <param name="target">Target position for the gun to rotate into</param>
    /// <param name="duration">The length that the recoil lasts</param>
    /// <returns></returns>
    IEnumerator CameraTiltOutwards(Quaternion target, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            camParent.transform.transform.rotation = Quaternion.Lerp(initialRotation, target, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
    }
    
    public void ResetCameraTilt(float duration)
    {
        StartCoroutine(CameraTiltInwards(targetRotation, duration));
    }

    /// <summary>
    /// The downwards portion of the recoil
    /// Recoil can be interrupted by another shot, causing it to restart the upward portion and cancel the current status
    /// </summary>
    /// <param name="target">Target position for the gun to rotate into</param>
    /// <param name="duration">The length that the recoil lasts</param>
    /// <returns></returns>
    IEnumerator CameraTiltInwards(Quaternion target, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            camParent.transform.localRotation = Quaternion.Lerp(target, initialRotation, time / duration);

            time += Time.deltaTime;
            yield return null;
        }
    }
}
