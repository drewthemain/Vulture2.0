using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : NavMeshEnemy
{
    #region Variables

    [Header("Patrol References")]

    [Tooltip("The patrol mover this guard will follow")]
    [SerializeField] private Transform patrolMover;

    [Header("Patrol Options")]

    [Tooltip("Should the enemy chase the player when close enough?")]
    [SerializeField] private bool chaseInProximity = false;

    [Tooltip("Should the enemy face the player?")]
    [SerializeField] private bool facePlayer = false;

    [Tooltip("Should the enemy shoot at the player?")]
    [SerializeField] private bool shootPlayer = false;

    [Tooltip("The angle of the patrol search sweeping movement")]
    [SerializeField] private float sweepAngle = 90f;

    [Tooltip("The speed of the patrol search sweeping movement")]
    [SerializeField] private float sweepSpeed = 0.5f;

    // Is the enemy currently moving?
    private bool isMoving = false;

    // The upper rotation during the sweeping phase
    private float higherRotation = 0;

    // The initial direction the enemy is facing upon a sweep
    private float initDir = 0;

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (patrolMover == null)
        {
            Debug.LogWarning($"Guard '{transform.name}' is missing a Patrol Mover!");
            return;
        }

        if (facePlayer)
        {
            agent.updateRotation = false;
        }

        weapon.ToggleFiring(shootPlayer);

        // Start patrol sequence
        target = patrolMover;
        ChangeState(EnemyStates.OutOfRange);
    }

    protected override void Update()
    {
        base.Update();

        switch(state)
        {
            case EnemyStates.OutOfRange:

                CheckPath();

                if (facePlayer && playerRef)
                {
                    transform.LookAt(playerRef);
                }

                break;

            case EnemyStates.InRange:

                CheckPath();

                if (facePlayer && playerRef)
                {
                    transform.LookAt(playerRef);
                }

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Action:
                break;
        }
    }

    private void CheckPath()
    {
        if (agent && target)
        {
            if (Vector3.Distance(transform.position, target.position) < 2f)
            {
                if (isMoving)
                {
                    agent.ResetPath();

                    isMoving = false;

                    // Sets the angle spread for guard sweeping based on initial direction

                    initDir = transform.rotation.eulerAngles.y;

                    higherRotation = initDir + sweepAngle;
                }

                // Sweeps enemy rotation back and forth
                float rY = Mathf.SmoothStep(initDir, higherRotation, Mathf.PingPong(Time.time * sweepSpeed, 1));
                transform.rotation = Quaternion.Euler(0, rY, 0);

                return;
            }
            else
            {
                isMoving = true;
            }

            agent.SetDestination(target.position);
        }
    }

    public override void ChangeState(EnemyStates newState)
    {
        base.ChangeState(newState);

        switch (newState)
        {
            case EnemyStates.OutOfRange:

                if (agent && target)
                {
                    agent.SetDestination(target.position);
                }

                break;

            case EnemyStates.InRange:

                if (chaseInProximity)
                {
                    target = playerRef;
                }

                break;

            case EnemyStates.NoGrav:

                break;

            case EnemyStates.Action:

                break;
        }
    }

    #endregion region

}
