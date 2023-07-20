using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierHealth : EnemyHealth
{
    // Reference to Soldier script
    private Soldier _soldier;

    private void Awake()
    {
        _soldier = GetComponent<Soldier>();
    }

    /// <summary>
    /// Override for soldiers taking cover when hurt
    /// </summary>
    /// <param name="dmg">damage taken</param>
    public override void TakeDamage(float dmg, float multiplier=1)
    {
        base.TakeDamage(dmg, multiplier);

        if (_currentHealth <= (_maxHealth / 2) && _currentHealth > 0 && _soldier.GetState() != Enemy.EnemyStates.Covering)
        {
            _soldier.ChangeState(Enemy.EnemyStates.Covering);
        }
    }
}
