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
            // Instaniate the bullet and set as child
            GameObject clawCollider = Instantiate(colliderPrefab, transform.position + (transform.forward * colliderOffset.x + transform.up * colliderOffset.y), Quaternion.identity);
            clawCollider.GetComponent<Bullet>().SetDamage(colliderDamage);
        }
    }
}
