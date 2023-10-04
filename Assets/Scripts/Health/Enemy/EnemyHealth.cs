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
            dmgNumber.transform.position = transform.position + 
                new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.7f, 0.7f), Random.Range(-0.7f, 0.7f));
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

    #endregion
}
