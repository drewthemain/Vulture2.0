using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redirector : MonoBehaviour
{
    #region Variables

    // Reference to a Soldier's gun script
    private EnemyGun _gun;

    #endregion

    #region Methods

    private void Awake()
    {
        _gun = GetComponentInParent<EnemyGun>();
    }

    /// <summary>
    /// Helps spawn a Soldier bullet from an animation trigger
    /// </summary>
    public void Fire()
    {
        if (_gun)
        {
            _gun.SpawnBullet();
        }
    }

    #endregion
}
