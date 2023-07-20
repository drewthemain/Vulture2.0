using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    #region Variables
    #endregion

    #region Methods
    protected override void Die()
    {
        base.Die();

        if (GameManager.instance)
        {
            GameManager.instance.LoseGame();
        }
    }

    public override void TakeDamage(float dmg, float multiplier)
    {
        base.TakeDamage(dmg, multiplier);

        UIManager.instance.UpdateHealthText(_currentHealth);
    }

    protected override void Heal(float heal)
    {
        base.Heal(heal);
    }

    #endregion
}
