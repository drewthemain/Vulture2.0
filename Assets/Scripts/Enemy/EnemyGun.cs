using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : EnemyWeapon
{

    [Tooltip("The speed of the bullet")]
    [SerializeField] protected float _bulletSpeed = 20;

    [Header("Aiming Variables")]

    [Tooltip("The percentage of shots that should hit the player")]
    [Range(0, 1)]
    [SerializeField] protected float _aimPercentage = 0.5f;

    [Tooltip("The distance in which shots can miss in each axis")]
    [SerializeField] protected float _maxMissDistance = 2;

    protected override void Fire()
    {
        base.Fire();

        if (_colliderPrefab != null)
        {
            // Instaniate the bullet and set as child
            GameObject newBullet = Instantiate(_colliderPrefab, transform.position + transform.forward * _colliderOffset, Quaternion.identity);
            newBullet.GetComponent<Bullet>().SetDamage(_colliderDamage);

            // Determine chances of successful aiming
            float aimCheck = Random.Range(0f, 1f);
            Vector3 dir = transform.forward;

            // Is an unsuccessful aim chance
            if (aimCheck > _aimPercentage)
            {
                dir += new Vector3(
                    Random.Range(-_maxMissDistance, _maxMissDistance),
                    Random.Range(-_maxMissDistance, _maxMissDistance),
                    Random.Range(-_maxMissDistance, _maxMissDistance));

                dir.Normalize();
            }

            // Add velocity to bullet rigid body and fire!
            Rigidbody body = newBullet.GetComponent<Rigidbody>();
            body.velocity = dir * _bulletSpeed;
        }
    }
}
