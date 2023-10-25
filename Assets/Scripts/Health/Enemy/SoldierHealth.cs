using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierHealth : EnemyHealth
{
    // Reference to Soldier script
    private Soldier soldier;

    private void Awake()
    {
        soldier = GetComponent<Soldier>();
    }

    /// <summary>
    /// Override for soldiers taking cover when hurt
    /// </summary>
    /// <param name="dmg">damage taken</param>
    public override void TakeDamage(float dmg, float multiplier=1)
    {
        base.TakeDamage(dmg, multiplier);

        if (currentHealth <= (currentMaxHealth / 2) && currentHealth > 0 && soldier.GetState() != Enemy.EnemyStates.Action)
        {
            soldier.ChangeState(Enemy.EnemyStates.Action);
        }
    }
}
