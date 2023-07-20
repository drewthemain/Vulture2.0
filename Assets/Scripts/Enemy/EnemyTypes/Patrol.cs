using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : Enemy
{
    #region Variables

    [Header("Patrol References")]

    [Tooltip("The patrol mover this guard will follow")]
    [SerializeField] private Transform _patrolMover;

    [Header("Patrol Options")]

    [Tooltip("Should the enemy chase the player when close enough?")]
    [SerializeField] private bool _chaseInProximity = false;

    [Tooltip("Should the enemy face the player?")]
    [SerializeField] private bool _facePlayer = false;

    [Tooltip("Should the enemy shoot at the player?")]
    [SerializeField] private bool _shootPlayer = false;

    [Tooltip("The angle of the patrol search sweeping movement")]
    [SerializeField] private float _sweepAngle = 90f;

    [Tooltip("The speed of the patrol search sweeping movement")]
    [SerializeField] private float _sweepSpeed = 0.5f;

    // The target of this enemies target
    private Transform _target;

    // Is the enemy currently moving?
    private bool _isMoving = false;

    // The upper rotation during the sweeping phase
    private float _higherRotation = 0;

    // The initial direction the enemy is facing upon a sweep
    private float _initDir = 0;

    #endregion

    #region Methods

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        if (_patrolMover == null)
        {
            Debug.LogWarning($"Guard '{transform.name}' is missing a Patrol Mover!");
            return;
        }

        if (_facePlayer)
        {
            _agent.updateRotation = false;
        }

        _weapon.ToggleFiring(_shootPlayer);

        // Start patrol sequence
        _target = _patrolMover;
        ChangeState(EnemyStates.OutOfRange);
    }

    protected override void Update()
    {
        base.Update();

        switch(_state)
        {
            case EnemyStates.OutOfRange:

                CheckPath();

                if (_facePlayer && _playerRef)
                {
                    transform.LookAt(_playerRef);
                }

                break;

            case EnemyStates.InRange:

                CheckPath();

                if (_facePlayer && _playerRef)
                {
                    transform.LookAt(_playerRef);
                }

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:
                break;
        }
    }

    private void CheckPath()
    {
        if (_agent && _target)
        {
            if (Vector3.Distance(transform.position, _target.position) < 2f)
            {
                if (_isMoving)
                {
                    _agent.ResetPath();

                    _isMoving = false;

                    // Sets the angle spread for guard sweeping based on initial direction

                    _initDir = transform.rotation.eulerAngles.y;

                    _higherRotation = _initDir + _sweepAngle;
                }

                // Sweeps enemy rotation back and forth
                float rY = Mathf.SmoothStep(_initDir, _higherRotation, Mathf.PingPong(Time.time * _sweepSpeed, 1));
                transform.rotation = Quaternion.Euler(0, rY, 0);

                return;
            }
            else
            {
                _isMoving = true;
            }

            _agent.SetDestination(_target.position);
        }
    }

    public override void ChangeState(EnemyStates newState)
    {
        base.ChangeState(newState);

        switch (newState)
        {
            case EnemyStates.OutOfRange:

                if (_agent && _target)
                {
                    _agent.SetDestination(_target.position);
                }

                break;

            case EnemyStates.InRange:

                if (_chaseInProximity)
                {
                    _target = _playerRef;
                }

                break;

            case EnemyStates.NoGrav:

                break;

            case EnemyStates.Covering:

                break;
        }
    }

    #endregion region

}
