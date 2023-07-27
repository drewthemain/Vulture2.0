using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbCollider : MonoBehaviour
{
    #region Variables

    [Tooltip("The multiplier of this specific limb")]
    [SerializeField] private float _damageMultiplier = 1;

    // Reference to the overall enemy health
    EnemyHealth health;

    #endregion

    #region Methods

    private void Awake()
    {
        health = GetComponentInParent<EnemyHealth>();
    }

    /// <summary>
    /// Triggers damage in this enemy with correct multiplier
    /// </summary>
    /// <param name="damage">The damage from the gun</param>
    public void SignalDamage(int damage)
    {
        if (health != null)
        {
            health.TakeDamage(damage, _damageMultiplier);
        }
    }

    #endregion
}
