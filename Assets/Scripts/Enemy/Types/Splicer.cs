using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Splicer : NavMeshEnemy
{
    #region Variables

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();

        if (agent)
        {
            agent.updateRotation = true;
        }

        // Start by targeting the player
        if (playerRef)
        {
            target = playerRef;
        }

        ChangeState(EnemyStates.OutOfRange);
    }

    protected override void Update()
    {
        base.Update();

        switch (state)
        {
            case EnemyStates.OutOfRange:

                if (agent)
                {
                    agent.SetDestination(target.position);
                }

                break;
            case EnemyStates.InRange:

                LockedLookAt(playerRef);

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Action:
                break;
        }
    }

    public override void ChangeState(EnemyStates newState)
    {
        // Resets (clears out) the path of the navmesh for every change
        if (agent != null && agent.enabled)
        {
            agent.ResetPath();
            target = null;
        }

        base.ChangeState(newState);

        switch (state)
        {
            case EnemyStates.OutOfRange:

                if (playerRef)
                {
                    target = playerRef;
                }

                if (weapon)
                {
                    weapon.ToggleFiring(false);
                }

                break;
            case EnemyStates.InRange:

                if (weapon)
                {
                    agent.ResetPath();
                    weapon.ToggleFiring(true);
                }

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Action:
                break;
        }
    }

    #endregion
}
