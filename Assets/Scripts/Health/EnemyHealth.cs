using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : Health
{
    #region Variables

    public Order.EnemyTypes _enemyType;

    [Tooltip("Prefab reference for damage numbers")]
    [SerializeField] private GameObject _damageNumber;

    #endregion

    #region Methods

    /// <summary>
    /// Spawns enemy damage numbers
    /// </summary>
    /// <param name="dmg">The amount of damage taken</param>
    public override void TakeDamage(float dmg, float multiplier=1)
    {
        if (_damageNumber)
        {
            GameObject dmgNumber = Instantiate(_damageNumber);
            dmgNumber.transform.SetParent(UIManager.instance._worldSpaceCanvas.transform, true);
            dmgNumber.transform.position = transform.position + new Vector3(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f));
            dmgNumber.GetComponent<DamageNumber>().Initialize(dmg, multiplier);
        }

        if (multiplier == -1) multiplier *= -1;

        base.TakeDamage(dmg, multiplier);
    }

    /// <summary>
    /// Upon death, delete GameObject
    /// </summary>
    protected override void Die()
    {
        base.Die();

        RoundManager._instance.RecordEnemyKill(_enemyType);

        if (GetComponent<Enemy>()._ragdoll != null)
        {
            GetComponent<Enemy>().Ragdollize();
        }

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Gets a multipler based on what part of the enemy was shot
    /// </summary>
    /// <param name="hitPoint">The position in world space of bullet impact</param>
    /// <returns>A multiplier based on specific position</returns>
    public virtual float HitLocation(Vector3 hitPoint)
    {
        float colliderHeightStep = GetComponent<CapsuleCollider>().height / 3;
        float midPoint = transform.TransformPoint(GetComponent<CapsuleCollider>().center).y;

        if (hitPoint.y > midPoint + colliderHeightStep / 2)
        {
            return 1.5f;
        }
        else if (hitPoint.y > midPoint - colliderHeightStep / 2)
        {
            return 1f;
        }
        else
        {
            return 0.5f;
        }
    }

    #endregion
}
