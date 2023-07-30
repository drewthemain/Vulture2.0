using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GunRecoilShake : MonoBehaviour
{
    // Reference to the impulse source
    private CinemachineImpulseSource impulse;

    private void Start()
    {
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    /// <summary>
    /// Call the impulse source on the gun to create an impulse (shake) in the camera direction
    /// </summary>
    public void ScreenShake()
    {
        impulse.GenerateImpulse(Camera.main.transform.forward);
    }
}
