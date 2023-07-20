using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Round : ScriptableObject
{
    #region Variables 

    [Header("Segment Values")]

    [Tooltip("The segments that make up this round")]
    public List<Segment> _segments;

    [Header("Options")]

    [Tooltip("The max amount of enemies that can be spawned at one time")]
    public int _maxEnemiesSpawned = 10;

    #endregion

    #region Methods

    /// <summary>
    /// Getter for the total number of enemies across all segments
    /// </summary>
    /// <returns>Total number of enemies in a round</returns>
    public int GetTotalEnemies()
    {
        int total = 0;

        foreach (Segment segment in _segments)
        {
            total += segment.GetTotalEnemies();
        }

        return total;
    }

    #endregion
}
