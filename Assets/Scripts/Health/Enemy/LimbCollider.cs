using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbCollider : MonoBehaviour
{
    #region Variables

    [Tooltip("The multiplier of this specific limb")]
    public float damageMultiplier = 1;

    // Reference to the overall enemy health
    EnemyHealth health;

    private ArmDismember dismember;

    #endregion

    #region Methods

    private void Awake()
    {
        health = GetComponentInParent<EnemyHealth>();

        if (GetComponent<ArmDismember>())
        {
            dismember = GetComponent<ArmDismember>();
        }
    }

    /// <summary>
    /// Triggers damage in this enemy with correct multiplier
    /// </summary>
    /// <param name="damage">The damage from the gun</param>
    public void SignalDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage, damageMultiplier);

            if (dismember)
            {
                dismember.TakeDamage(damage);
            }
        }
    }

    #endregion
}
