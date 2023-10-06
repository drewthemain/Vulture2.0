using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : Health
{
    #region Variables

    public Order.EnemyTypes enemyType;

    [Tooltip("Prefab reference for damage numbers")]
    [SerializeField] private GameObject damageNumber;

    #endregion

    #region Methods

    /// <summary>
    /// Spawns enemy damage numbers
    /// </summary>
    /// <param name="dmg">The amount of damage taken</param>
    public override void TakeDamage(float dmg, float multiplier=1)
    {
        if (damageNumber)
        {
            GameObject dmgNumber = Instantiate(damageNumber);
            dmgNumber.transform.SetParent(UIManager.instance.worldSpaceCanvas.transform, true);
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

        RoundManager.instance.RecordEnemyKill(enemyType);

        if (GetComponent<Enemy>().ragdoll != null)
        {
            GetComponent<Enemy>().Ragdollize();
        }

        Destroy(this.gameObject);
    }

    #endregion
}
