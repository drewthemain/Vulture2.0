using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    #region Variables

    [Header("Bullet Options")]

    [Tooltip("The time until the bullet will despawn naturally")]
    [SerializeField] private float timeTilDestroy = 3f;

    [Header("Collision Values")]

    [Tooltip("The layer for enemy bullets")]
    [SerializeField] private string _enemyBulletLayer;

    // The damage done by this bullet
    private float _damage;

    #endregion

    #region Methods

    private void Start()
    {
        // Starts countdown for destruction
        Destroy(this.gameObject, timeTilDestroy);
    }

    public void SetDamage(float dmg)
    {
        _damage = dmg;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.GetComponent<Health>())
        {
            collision.transform.GetComponent<Health>().TakeDamage(_damage, 1);
        }
        else if (collision.transform.GetComponentInParent<Health>())
        {
            collision.transform.GetComponentInParent<Health>().TakeDamage(_damage, 1);
        }

        Destroy(this.gameObject);
    }

    #endregion
}
