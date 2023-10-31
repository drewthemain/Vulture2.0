using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Order
{
    #region Variables
    public enum EnemyTypes
    {
        Soldier,
        Swarm,
        Splicer
    }

    [Tooltip("The type of enemy of this order")]
    public EnemyTypes enemy;

    [Tooltip("The amount of this enemy to spawn ")]
    public int enemyAmount = 1;

    #endregion

}
