using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyClaw : EnemyWeapon
{
    protected override void Fire()
    {
        base.Fire();

        if (colliderPrefab != null)
        {
            // Damage the player directly
            playerHealth.TakeDamage(colliderDamage, 1);
        }
    }
}
