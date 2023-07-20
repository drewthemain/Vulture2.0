using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanisterHealth : Health
{
    #region Variables

    [Header("Canister Options")]

    [Tooltip("A reference to the window this is tied to")]
    [SerializeField] private Window _window;

    #endregion

    #region Methods

    public override void TakeDamage(float dmg, float multiplier = 1)
    {
        base.TakeDamage(dmg, multiplier);
    }

    protected override void Die()
    {
        base.Die();

        // If canister is broken, break the window
        _window.Depressurize();

        Heal(100);
        gameObject.SetActive(false);
    }

    #endregion
}
