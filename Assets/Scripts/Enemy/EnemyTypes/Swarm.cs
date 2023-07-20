using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Swarm : Enemy
{
    #region Variables

    [Header("Layer Masks")]

    [Tooltip("The layermask for detecting walls to climb")]
    [SerializeField] private LayerMask _wallMask;

    [Tooltip("The layermask for detecting floors")]
    [SerializeField] private LayerMask _floorMask;

    // The target of this enemies movement
    private Transform _target;

    // Is the enemy currently grounded?
    public bool _grounded = true;

    #endregion

    #region Methods

    protected override void Start()
    {
        base.Start();

        if (_agent)
        {
            _agent.updateRotation = true;
        }

        // Start by targeting the player
        if (_playerRef)
        {
            _target = _playerRef;
        }

        ChangeState(EnemyStates.OutOfRange);
    }

    protected override void Update()
    {
        base.Update();

        // Check constantly whether grounded
        GroundedCheck();

        switch (_state)
        {
            case EnemyStates.OutOfRange:

                if (_agent)
                {
                    _agent.SetDestination(_target.position);

                    UpdateRotation();
                }

                break;
            case EnemyStates.InRange:

                transform.LookAt(_playerRef);

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:
                break;
        }
    }

    public override void ChangeState(EnemyStates newState)
    {
        // Resets (clears out) the path of the navmesh for every change
        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
            _target = null;
        }

        base.ChangeState(newState);

        switch (_state)
        {
            case EnemyStates.OutOfRange:

                if (_playerRef)
                {
                    _target = _playerRef;
                }

                if (_weapon)
                {
                    _weapon.ToggleFiring(false);
                }

                break;
            case EnemyStates.InRange:

                if (_weapon)
                {
                    _weapon.ToggleFiring(true);
                }

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:
                break;
        }
    }

    /// <summary>
    /// Uses a small raycast to look for floor layer
    /// </summary>
    private void GroundedCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, _agent.height, _floorMask))
        {
            _grounded = true;

            _anim.SetBool("isGrounded", _grounded);

            return;
        }

        _anim.SetBool("isGrounded", _grounded);

        _grounded = false;
    }

    /// <summary>
    /// Updates rotation based whether on ceiling or floor
    /// </summary>
    private void UpdateRotation()
    {
        NavMeshHit navMeshHit;
        _agent.SamplePathPosition(NavMesh.AllAreas, 0f, out navMeshHit);

        // If the enemy is on the ceiling, set new rotation
        // 262144 is the bitmap location for the ceiling area in the NavMesh areas map
        if (navMeshHit.mask == 262144)
        {
            _agent.updateRotation = false;

            if (_agent.velocity != Vector3.zero)
            {
                Quaternion newRot = Quaternion.LookRotation(_agent.velocity);
                newRot *= Quaternion.Euler(0, 0, 180);

                transform.rotation = newRot;
                return;
            }
        }

        _agent.updateRotation = true;
    }

    #endregion
}
