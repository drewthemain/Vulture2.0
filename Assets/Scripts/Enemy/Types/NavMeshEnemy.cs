using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshEnemy : Enemy
{
    [Header("Movement")]

    [Tooltip("Should the enemy slow down once the player is spotted?")]
    [SerializeField] protected bool slowWhenPlayerSpotted = true;

    [Tooltip("If slowed, this value will be the speed reduction percentage")]
    [Range(0, 1)]
    [SerializeField] protected float speedReductionPercentage = 0.5f;

    [Tooltip("Should the enemy speed up the further they are from the player?")]
    [SerializeField] protected bool proximitySpeedBoost = true;

    [Tooltip("If out of range, this value will be the speed boost percentage")]
    [Range(0, 1)]
    [SerializeField] protected float speedBoostPercentage = 0.5f;

    // Reference to the NavMesh Agent
    protected NavMeshAgent agent;

    /// <summary>
    /// Changes and properly transitions all enemy states
    /// </summary>
    /// <param name="newState">The new state of this enemy</param>
    public override void ChangeState(EnemyStates newState)
    {
        base.ChangeState(newState);

        state = newState;

        if (agent && agent.enabled == false)
        {
            agent.enabled = true;
            body.isKinematic = true;
        }

        switch (newState)
        {
            case EnemyStates.OutOfRange:

                if (agent && proximitySpeedBoost)
                {
                    agent.speed = baseSpeed + (baseSpeed * speedBoostPercentage);
                }

                mutable = true;
                break;
            case EnemyStates.InRange:

                if (agent)
                {
                    agent.speed = baseSpeed;
                }

                mutable = true;
                break;
            case EnemyStates.NoGrav:

                // Turn off navmesh when floating
                agent.enabled = false;

                // Turn on rigidbody
                body.isKinematic = false;

                gameObject.layer = LayerMask.NameToLayer("Sucked");

                // Set velocity toward the window
                if (currentRoom.GetBrokenWindow() != null)
                {
                    Vector3 targetDir = (currentRoom.GetBrokenWindow().pullTarget.transform.position - transform.position).normalized;
                    body.velocity = targetDir * pullSpeed * 10;

                    Ragdollize("Sucked");
                }

                mutable = false;
                break;
            case EnemyStates.Action:
                mutable = false;
                break;
        }
    }

    protected override void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeed = agent.speed;

        if (!agent)
        {
            Debug.LogWarning($"NavMeshEnemy {this.transform.name} is missing a NavMeshAgent component!");
        }

        base.Awake();
    }

    /// <summary>
    /// Toggles on and off whether a player can be seen
    /// </summary>
    /// <param name="playerSpotted">Is the player currently spotted?</param>
    protected override void TogglePlayerSightline(bool playerSpotted)
    {
        base.TogglePlayerSightline(playerSpotted);

        if (slowWhenPlayerSpotted && mutable)
        {
            agent.speed = !playerSpotted ? baseSpeed : (baseSpeed - (baseSpeed * speedReductionPercentage));

            if (proximitySpeedBoost && state == EnemyStates.OutOfRange)
            {
                agent.speed += baseSpeed * speedBoostPercentage;
            }
        }
    }
}
