using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Swarm : NavMeshEnemy
{
    #region Variables

    [Header("Layer Masks")]

    [Tooltip("The layermask for detecting walls to climb")]
    [SerializeField] private LayerMask wallMask;

    [Tooltip("The layermask for detecting floors")]
    [SerializeField] private LayerMask floorMask;

    // Is the enemy currently grounded?
    public bool grounded = true;

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

        // Check constantly whether grounded
        GroundedCheck();

        switch (state)
        {
            case EnemyStates.OutOfRange:

                if (agent)
                {
                    agent.SetDestination(target.position);

                    UpdateRotation();
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

        if (anim)
        {
            anim.SetBool("isWalking", true);
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
                    anim.SetLayerWeight(1, 0);
                }

                break;
            case EnemyStates.InRange:

                if (weapon)
                {
                    agent.ResetPath();
                    weapon.ToggleFiring(true);
                    anim.SetLayerWeight(1, 1);
                    anim.SetBool("isWalking", false);
                }

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Action:
                break;
        }
    }

    /// <summary>
    /// Uses a small raycast to look for floor layer
    /// </summary>
    private void GroundedCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, agent.height, floorMask))
        {
            grounded = true;

            anim.SetBool("isGrounded", grounded);

            return;
        }

        anim.SetBool("isGrounded", grounded);

        grounded = false;
    }

    /// <summary>
    /// Updates rotation based whether on ceiling or floor
    /// </summary>
    private void UpdateRotation()
    {
        NavMeshHit navMeshHit;
        agent.SamplePathPosition(NavMesh.AllAreas, 0f, out navMeshHit);

        // If the enemy is on the ceiling, set new rotation
        // 262144 is the bitmap location for the ceiling area in the NavMesh areas map
        if (navMeshHit.mask == 262144)
        {
            agent.updateRotation = false;

            if (agent.velocity != Vector3.zero)
            {
                Quaternion newRot = Quaternion.LookRotation(agent.velocity);
                newRot *= Quaternion.Euler(0, 0, 180);

                transform.rotation = newRot;
                return;
            }
        }

        agent.updateRotation = true;
    }

    #endregion
}
