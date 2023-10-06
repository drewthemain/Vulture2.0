using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Segment
{
    #region Variables

    [Tooltip("The orders in this round segment")]
    public List<Order> orders;

    [Tooltip("If the prior segment is about to end, begin spawning from new segment instead of waiting")]
    public bool allowEarlySpawning = true;

    #endregion

    #region Methods

    /// <summary>
    /// Getter for total number of enemies across all orders
    /// </summary>
    /// <returns>Total number of enemies</returns>
    public int GetTotalEnemies()
    {
        int total = 0;

        foreach (Order order in orders)
        {
            total += order.enemyAmount;
        }

        return total;
    }

    #endregion
}
