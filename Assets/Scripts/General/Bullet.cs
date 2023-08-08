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

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<Health>())
        {
            other.transform.GetComponent<Health>().TakeDamage(_damage, 1);
        }
        else if (other.transform.GetComponentInParent<Health>())
        {
            other.transform.GetComponentInParent<Health>().TakeDamage(_damage, 1);
        }

        Destroy(this.gameObject);
    }

    #endregion
}
