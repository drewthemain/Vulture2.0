using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EventFactory
{
    public static void AlterMaxHealth(Health health, bool up)
    {
        if (up)
        {
            health.IncreaseMax(20);
            return;
        }

        health.ResetMax();
    }
}
