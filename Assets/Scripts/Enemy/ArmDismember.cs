using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmDismember : MonoBehaviour
{
    [Header("References")]
    public GameObject fleshyBit;
    public GameObject dismemberedArm;
    public GameObject scaledArm;

    [Header("Options")]

    public float health = 20;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Dismember();
        }
    }

    public void Dismember()
    {
        if (!fleshyBit.activeSelf)
        {
            fleshyBit.SetActive(true);
            dismemberedArm.SetActive(true);
            dismemberedArm.transform.parent = null;
            scaledArm.transform.localScale = new Vector3(0, 0, 0);
        }
        else
        {
            Destroy(this);
        }
    }

}
