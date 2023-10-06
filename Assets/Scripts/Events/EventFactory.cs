using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class EventFactory
{
    public static void IncreaseEnemyMaxHealth(EnemyHealth health, bool up)
    {
        if (up)
        {
            health.IncreaseMax(20);
            return;
        }

        health.ResetMax();
    }

    public static void EnemySpeedBoost(NavMeshAgent agent, bool up)
    {
        if (up)
        {
            agent.speed += 3;
            return;
        }

        agent.speed -= 3;
    }

    public static void ToggleDoubleBullets(Soldier soldier, bool up)
    {
        if (up)
        {
            soldier.GetComponent<EnemyGun>().ToggleDoubleBullets(true);
            return;
        }

        soldier.GetComponent<EnemyGun>().ToggleDoubleBullets(false);
    }

    public static void ToggleGravity(bool up)
    {
        if (up)
        {
            GameManager.instance.ToggleGravity(true, null);
            return;
        }

        GameManager.instance.ResetGravity();
    }
}