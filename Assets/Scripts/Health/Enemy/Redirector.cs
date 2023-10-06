using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redirector : MonoBehaviour
{
    #region Variables

    // Reference to a Soldier's gun script
    private EnemyGun gun;

    #endregion

    #region Methods

    private void Awake()
    {
        gun = GetComponentInParent<EnemyGun>();
    }

    /// <summary>
    /// Helps spawn a Soldier bullet from an animation trigger
    /// </summary>
    public void Fire()
    {
        if (gun)
        {
            gun.SpawnBullet();
        }
    }

    #endregion
}
