using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GunRecoilShake : MonoBehaviour
{
    // Reference to the impulse source
    private CinemachineImpulseSource impulse;

    //[Tooltip("")]
    //[SerializeField] private float hipfireAmplitude;
    //[Tooltip("")]
    //[SerializeField] private float adsAmplitude;

    private void Start()
    {
        impulse = GetComponent<CinemachineImpulseSource>();
    }

    /// <summary>
    /// Call the impulse source on the gun to create an impulse (shake) in the camera direction
    /// </summary>
    public void ScreenShake()
    {
        //if(ads)
        //{
        //    impulse.m_ImpulseDefinition.m_RawSignal
        //}
        impulse.GenerateImpulse(Camera.main.transform.forward);
    }
}
