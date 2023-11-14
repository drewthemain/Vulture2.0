using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsManager
{
    public enum Stats
    {
        totalEnemiesKilled,
        totalRoundsPlayed,
        totalGamesPlayed,
        gameEnemiesKilled,
        gameRoundsPlayed
    }

    public static void IncrementStat(Stats stat)
    {
        string name = stat.ToString();

        if (!PlayerPrefs.HasKey(name))
        {
            SetStat(stat, 0);
        }

        PlayerPrefs.SetInt(name, PlayerPrefs.GetInt(name) + 1);
    }

    public static void SetStat(Stats stat, int value)
    {
        string name = stat.ToString();
        PlayerPrefs.SetInt(name, value);
    }

    public static void ClearStat(Stats stat)
    {
        string name = stat.ToString();
        PlayerPrefs.DeleteKey(name);
    }


}
