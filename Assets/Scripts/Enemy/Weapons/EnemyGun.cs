using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : EnemyWeapon
{

    [Tooltip("The speed of the bullet")]
    [SerializeField] protected float bulletSpeed = 20;

    [Header("Aiming Variables")]

    [Tooltip("The percentage of shots that should hit the player")]
    [Range(0, 1)]
    [SerializeField] protected float aimPercentage = 0.5f;

    [Tooltip("The distance in which shots can miss in each axis")]
    [SerializeField] protected float maxMissDistance = 2;

    private Animator anim;

    private bool fireDoubleBullets = false;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    protected override void Fire()
    {
        base.Fire();

        // Animation now triggers the bullet instantiation
        anim.SetTrigger("Shoot");
    }

    public void SpawnBullet()
    {
        if (colliderPrefab != null)
        {
            StopAllCoroutines();
            StartCoroutine("BulletInstantiate");
        }
    }

    public void ToggleDoubleBullets(bool doDouble)
    {
        fireDoubleBullets = doDouble;
    }

    IEnumerator BulletInstantiate()
    {
        int amount = fireDoubleBullets ? 2 : 1;

        for (int i = 0; i < amount; i++)
        {
            // Instaniate the bullet and set as child
            GameObject newBullet = Instantiate(colliderPrefab, transform.position + (transform.forward * colliderOffset.x + transform.up * colliderOffset.y), Quaternion.identity);
            newBullet.GetComponent<Bullet>().SetDamage(colliderDamage);

            // Determine chances of successful aiming
            float aimCheck = Random.Range(0f, 1f);
            Vector3 dir = playerReference.position - newBullet.transform.position;
            dir.Normalize();

            // Is an unsuccessful aim chance
            if (aimCheck > aimPercentage)
            {
                dir += new Vector3(
                    Random.Range(-maxMissDistance, maxMissDistance),
                    Random.Range(-maxMissDistance, maxMissDistance),
                    Random.Range(-maxMissDistance, maxMissDistance));

                dir.Normalize();
            }

            // Add velocity to bullet rigid body and fire!
            Rigidbody body = newBullet.GetComponent<Rigidbody>();
            body.velocity = dir * bulletSpeed;

            yield return new WaitForSeconds(0.1f);
        }
    }
}
