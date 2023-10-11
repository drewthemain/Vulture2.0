using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public static class EventFactory
{
    public static void IncreaseEnemyMaxHealth(EnemyHealth health, bool up, Event.UDictionary parameters)
    {
        float increaseAmount = parameters["Enemy Health Boost"].number;
        if (up)
        {
            health.IncreaseMax(increaseAmount);
            return;
        }

        health.ResetMax();
    }

    public static void EnemySpeedBoost(NavMeshAgent agent, bool up, Event.UDictionary parameters)
    {
        float increaseAmount = parameters["Enemy Speed Boost"].number;
        if (up)
        {
            agent.speed += increaseAmount;
            return;
        }

        agent.speed -= increaseAmount;
    }

    public static void ToggleDoubleBullets(Soldier soldier, bool up, Event.UDictionary parameters)
    {
        if (up)
        {
            soldier.GetComponent<EnemyGun>().ToggleDoubleBullets(true);
            return;
        }

        soldier.GetComponent<EnemyGun>().ToggleDoubleBullets(false);
    }

    public static void ToggleGravity(bool up, Event.UDictionary parameters)
    {
        if (up)
        {
            GameManager.instance.ToggleGravity(true, null);
            return;
        }

        GameManager.instance.ResetGravity();
    }

    public static void BoostOnDamage(PlayerController controller, bool up, Event.UDictionary parameters)
    {
        float increaseAmount = parameters["Boost Amount"].number;
        if (up)
        {
            controller.GetComponent<PlayerHealth>().speedIncreaseOnDamage = increaseAmount;
            return;
        }

        controller.GetComponent<PlayerHealth>().speedIncreaseOnDamage = 0;
        controller.speedMultiplier = 1;
    }

    public static void PlayerSpeedBoost(PlayerController controller, bool up, Event.UDictionary parameters)
    {
        float multiplier = parameters["Speed Multiplier"].number;
        if (up)
        {
            controller.speedMultiplier = multiplier;
            return;
        }

        controller.speedMultiplier = 1;
    }
}
