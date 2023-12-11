using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasCloud : MonoBehaviour
{
    #region Variables

    [Tooltip("The time in between each hit of damage")]
    [SerializeField] private float damageBuffer = 2;

    [Tooltip("The amount of damage this does per second")]
    [SerializeField] private float damagePerBuffer = 5;

    private PlayerHealth playerHealth;
    private float damageTimer = 0;
    #endregion

    #region Methods

    private void OnTriggerStay(Collider other)
    {
        if (playerHealth)
        {
            if (damageTimer > damageBuffer)
            {
                playerHealth.TakeDamage(damagePerBuffer, 1);
                damageTimer = 0;
            }

            damageTimer += Time.deltaTime;
        } else
        {
            playerHealth = other.GetComponent<PlayerHealth>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (playerHealth)
        {
            playerHealth = null;
        }
    }

    #endregion
}
