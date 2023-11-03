using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Round : ScriptableObject
{
    #region Variables 

    [Header("Segment Values")]

    [Tooltip("The segments that make up this round")]
    public List<Segment> segments;

    [Header("Options")]

    [Tooltip("The max amount of enemies that can be spawned at one time")]
    public int maxEnemiesSpawned = 10;

    [Tooltip("The id of an event that will occur with this round")]
    public int connectedEvent = -1;

    #endregion

    #region Methods

    /// <summary>
    /// Getter for the total number of enemies across all segments
    /// </summary>
    /// <returns>Total number of enemies in a round</returns>
    public int GetTotalEnemies()
    {
        int total = 0;

        foreach (Segment segment in segments)
        {
            total += segment.GetTotalEnemies();
        }

        return total;
    }

    /// <summary>
    /// Strips the name and removes the ID
    /// </summary>
    /// <returns>The string portion of the name</returns>
    public string GetDisplayName()
    {
        int indexOf = name.IndexOf('_');
        if (indexOf >= 0)
        {
            return name.Substring(indexOf + 1);
        }

        return "";
    }

    /// <summary>
    /// Gets the id section of the name as an integer
    /// </summary>
    /// <returns>The ID of the round as an int</returns>
    public int GetId()
    {
        int indexOf = name.IndexOf('_');
        if (indexOf >= 0)
        {

            return int.Parse(name.Substring(0, indexOf));
        }

        return -1;
    }

    #endregion
}
