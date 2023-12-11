using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggHealth : EnemyHealth
{
    #region Variables

    private EnemyEgg enemyEgg;

    #endregion

    #region Methods

    private void Awake()
    {
        enemyEgg = GetComponent<EnemyEgg>();
    }

    protected override void Die()
    {
        if (enemyEgg)
        {
            // Trigger the start of the explode sequence
            enemyEgg.ChangeState(Enemy.EnemyStates.Action);
        }
    }

    #endregion
}
