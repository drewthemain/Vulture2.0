using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyClaw : EnemyWeapon
{
    protected override void Fire()
    {
        base.Fire();

        if (_colliderPrefab != null)
        {
            // Instaniate the bullet and set as child
            GameObject clawCollider = Instantiate(_colliderPrefab, transform.position + transform.forward * _colliderOffset, Quaternion.identity);
            clawCollider.GetComponent<Bullet>().SetDamage(_colliderDamage);
        }
    }
}
