using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyClaw : EnemyWeapon
{
    /// <summary>
    /// Override for firing, damages the player directly (no colliders)
    /// </summary>
    protected override void Fire()
    {
        base.Fire();

        // Damage the player directly
        playerHealth.TakeDamage(colliderDamage, 1);
    }
}
