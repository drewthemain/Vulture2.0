using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmDismember : MonoBehaviour
{
    [Header("References")]

    [Tooltip("The fleshy bit appearing after dismemberment")]
    public GameObject fleshyBit;
    [Tooltip("The arm that was dismembered")]
    public GameObject dismemberedArm;
    [Tooltip("The normal arm to be turned off")]
    public GameObject scaledArm;

    [Header("Options")]

    [Tooltip("The health of the arm before dismemberment")]
    public float health = 20;

    /// <summary>
    /// Damage taken from LimbCollider
    /// </summary>
    /// <param name="damage">The amount of damage taken</param>
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Dismember();
        }
    }

    /// <summary>
    /// Turn on/off the correct gameobjects for the effect
    /// </summary>
    public void Dismember()
    {
        if (!fleshyBit.activeSelf)
        {
            scaledArm.transform.localScale = new Vector3(0, 0, 0);
            fleshyBit.SetActive(true);
            dismemberedArm.SetActive(true);
            dismemberedArm.transform.parent = null;
        }
        else
        {
            Destroy(this);
        }
    }

}
