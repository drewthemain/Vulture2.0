using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tracer : Enemy
{
    #region Variables

    public enum WallActions
    {
        Charge,
        Traverse,
        Climb,
        Drop,
    }

    public enum GroundActions
    {
        Charge,
        Zig,
        Zag,
    }

    [Header("General Tracer Options")]

    //[Tooltip("The damage done by one strike")]
    //[SerializeField] private float _strikeDamage = 5f;

    [Tooltip("The time constraints of which a charge can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _chargeTimeConstraints;

    [Tooltip("Initial chances of moving from floor to wall or vice versa (should be small, grows over time")]
    [Range(0, 1)]
    [SerializeField] private float _initialSwapOdds = 0.05f;

    [Tooltip("Growth to chances of swapping from floor to wall or vice versa")]
    [Range(0, 0.1f)]
    [SerializeField] private float _swapOddsGrowth = 0.01f;

    [Header("Grounded Behavior")]

    [Tooltip("The current grounded action of the tracer")]
    [SerializeField] private GroundActions _groundAction = GroundActions.Charge;

    [Tooltip("The max distance of a diagonal pattern")]
    [SerializeField] private float _maxZigZagDistance = 10;

    [Tooltip("The time constraints of which a zigzag can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _zigZagTimeConstraints;

    [Header("Wall Behavior")]

    [Tooltip("The current on wall action of the tracer")]
    [SerializeField] private WallActions _wallAction = WallActions.Charge;

    [Tooltip("The time constraints of which a climb can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _climbTimeConstraints;

    [Tooltip("The time constraints of which a drop can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _dropTimeConstraints;

    [Tooltip("The time constraints of which a traverse can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _traverseTimeConstraints;

    [Header("Layer Masks")]

    [Tooltip("The layermask for detecting walls to climb")]
    [SerializeField] private LayerMask _wallMask;

    [Tooltip("The layermask for detecting floors")]
    [SerializeField] private LayerMask _floorMask;

    // The target of this enemies movement
    private Transform _target;

    // The target position reference for pathing (strafing)
    private Vector3 _targetPosition;

    // The timer for each action
    private float _actionTimer = 0;

    // The point in which a cover will be stopped
    private float _actionLimit = 0;

    // Is the enemy currently grounded?
    public bool _grounded = true;

    //// Was the enemy previously grounded when switching orientation?
    //private bool _previousGrounded = true;

    // The current odds of choosing a cover
    private float _swapOdds = 0.05f;

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

        GroundedCheck();

        switch (_state)
        {
            case EnemyStates.OutOfRange:

                if (_grounded)
                {
                    ThinkGrounded();
                    return;
                }

                ThinkWall();

                break;
            case EnemyStates.InRange:

                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:

                if (Vector3.Distance(transform.position, _targetPosition) < 2f)
                {
                    ChangeState(EnemyStates.OutOfRange);
                }

                break;
        }
    }

    public override void ChangeState(EnemyStates newState)
    {
        base.ChangeState(newState);

        // Resets (clears out) the path of the navmesh for every change
        if (_agent != null)
        {
            _agent.ResetPath();
            _target = null;
            _targetPosition = Vector3.zero;

            // Resets all timers
            _actionTimer = 0;
            _actionLimit = 0;
        }

        switch (_state)
        {
            case EnemyStates.OutOfRange:

                if (_playerRef)
                {
                    _target = _playerRef;
                }

                break;
            case EnemyStates.InRange:
                break;
            case EnemyStates.NoGrav:
                break;
            case EnemyStates.Covering:

                TriggerSwap();

                break;
        }
    }

    /// <summary>
    /// Changes the ground action based on parameter
    /// </summary>
    /// <param name="newAction">The new action to perform</param>
    private void ChangeGroundAction(GroundActions newAction)
    {
        // Resets all action values
        _groundAction = newAction;
        _agent.ResetPath();
        _agent.speed = _baseSpeed;
        _target = null;
        _targetPosition = Vector3.zero;

        if (_agent)
        {
            _agent.updateRotation = true;
        }

        float swapRoll = Random.Range(0f, 1f);

        if (swapRoll < _swapOdds)
        {
            // _previousGrounded = true;
            ChangeState(EnemyStates.Covering);
            return;
        }

        _swapOdds += _swapOddsGrowth;

        switch (newAction)
        {
            case GroundActions.Charge:

                // Target the player
                _actionLimit = Random.Range(_chargeTimeConstraints.x, _chargeTimeConstraints.y);
                _target = _playerRef;

                break;

            case GroundActions.Zig:

                if (_agent)
                {
                    _agent.updateRotation = false;
                }

                _actionLimit = Random.Range(_zigZagTimeConstraints.x, _zigZagTimeConstraints.y);

                _targetPosition = GetDiagonalPosition(true);
                _agent.SetDestination(_targetPosition);

                break;

            case GroundActions.Zag:

                if (_agent)
                {
                    _agent.updateRotation = false;
                }

                _actionLimit = Random.Range(_zigZagTimeConstraints.x, _zigZagTimeConstraints.y);

                _targetPosition = GetDiagonalPosition(false);
                _agent.SetDestination(_targetPosition);

                break;
        }
    }

    /// <summary>
    /// Changes the wall action based on parameter
    /// </summary>
    /// <param name="newAction">The new action to perform</param>
    private void ChangeWallAction(WallActions newAction)
    {
        // Resets all action values
        _wallAction = newAction;
        _agent.ResetPath();
        _agent.speed = _baseSpeed;
        _target = null;
        _targetPosition = Vector3.zero;

        float swapRoll = Random.Range(0f, 1f);

        if (swapRoll < _swapOdds)
        {
            // _previousGrounded = false;
            ChangeState(EnemyStates.Covering);
            return;
        }

        _swapOdds += _swapOddsGrowth;

        switch (newAction)
        {
            case WallActions.Charge:

                // Target the player
                _actionLimit = Random.Range(_chargeTimeConstraints.x, _chargeTimeConstraints.y);
                _target = _playerRef;

                break;

            case WallActions.Climb:

                _actionLimit = Random.Range(_climbTimeConstraints.x, _climbTimeConstraints.y);

                _targetPosition = transform.position + (transform.forward * (Random.Range(5, _maxZigZagDistance)));
                _agent.SetDestination(_targetPosition);

                break;

            case WallActions.Drop:

                _actionLimit = Random.Range(_dropTimeConstraints.x, _dropTimeConstraints.y);

                _targetPosition = transform.position + (-transform.forward * (Random.Range(5, _maxZigZagDistance)));
                _agent.SetDestination(_targetPosition);

                break;

            case WallActions.Traverse:

                _actionLimit = Random.Range(_traverseTimeConstraints.x, _traverseTimeConstraints.y);

                _targetPosition = transform.position + ((Random.Range(0, 2) * 2 - 1) * transform.right * (Random.Range(5, _maxZigZagDistance)));
                _agent.SetDestination(_targetPosition);

                break;
        }
    }

    /// <summary>
    /// Handles constant logic for the in-range status
    /// </summary>
    private void ThinkGrounded()
    {
        // Checking if the current action is finished
        // If so, start new action
        if (_actionTimer > _actionLimit)
        {
            _actionTimer = 0;

            ChangeGroundAction((GroundActions)Random.Range(0, 2));
        }

        // Constant behavior for every action
        switch (_groundAction)
        {
            case GroundActions.Zig:

                _targetPosition = GetDiagonalPosition(true);
                _agent.SetDestination(_targetPosition);

                transform.LookAt(_playerRef);

                break;
            case GroundActions.Zag:

                _targetPosition = GetDiagonalPosition(false);
                _agent.SetDestination(_targetPosition);

                transform.LookAt(_playerRef);

                break;

            case GroundActions.Charge:

                if (_agent && _target != null)
                {
                    _agent.SetDestination(_target.position);
                }

                break;
        }

        // Update the timer to increment between actions
        _actionTimer += Time.deltaTime;
    }

    /// <summary>
    /// Handles constant logic for the in-range status
    /// </summary>
    private void ThinkWall()
    {
        // Checking if the current action is finished
        // If so, start new action
        if (_actionTimer > _actionLimit)
        {
            _actionTimer = 0;

            ChangeWallAction((WallActions)Random.Range(0, 4));
        }

        // Constant behavior for every action
        switch (_wallAction)
        {
            case WallActions.Charge:
                break;
            case WallActions.Climb:
                break;
            case WallActions.Drop:
                break;
            case WallActions.Traverse:
                break;
        }

        // Update the timer to increment between actions
        _actionTimer += Time.deltaTime;
    }

    /// <summary>
    /// Gets a diagonal position based on zig zag patterns
    /// </summary>
    /// <param name="isLeft">Is the enemy tying to diagonal left?</param>
    /// <returns>A position of the target diagonal location</returns>
    private Vector3 GetDiagonalPosition(bool isLeft)
    {

        Vector3 half = (_playerRef.position + transform.position) / 2;
        Vector3 directDir = _playerRef.position - transform.position;
        Vector3 dir = Vector3.Cross(directDir, Vector3.up).normalized;

        if (!isLeft)
        {
            dir *= -1;
        }

        Vector3 offset = half + (dir * Random.Range(5, _maxZigZagDistance));

        return offset;
    }

    /// <summary>
    /// Triggers a swap from floor to wall (or vice versa)
    /// </summary>
    private void TriggerSwap()
    {
        _swapOdds = _initialSwapOdds;

        if (_grounded)
        {
            _targetPosition = GetWallPosition();

            if (_targetPosition == Vector3.zero)
            {
                ChangeState(EnemyStates.OutOfRange);
                return;
            }

            _agent.SetDestination(_targetPosition);
        }
    }

    /// <summary>
    /// Gets a wall position for the enemy to initially target
    /// </summary>
    /// <returns>Position on a wall of a viable space</returns>
    private Vector3 GetWallPosition()
    {
        Vector3 upVec = new Vector3(0, Random.Range(_agent.height, _agent.height * 2), 0);

        RaycastHit hit;

        Vector3 directDir = (_playerRef.position + upVec) - (transform.position + upVec);
        Vector3 dir = Vector3.Cross(directDir, Vector3.up).normalized;

        // Left or right direction
        dir *= (Random.Range(0, 2) * 2 - 1);

        Debug.DrawRay((transform.position + upVec), dir * 100, Color.green);

        if (Physics.Raycast((transform.position + upVec), dir, out hit, 100, _wallMask, QueryTriggerInteraction.Ignore))
        {
            NavMeshPath path = new NavMeshPath();

            GameObject test = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            test.transform.position = hit.point;

            if (_agent.CalculatePath(hit.point, path))
            {
                return hit.point;
            }
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Checks whether the enemy is currently grounded
    /// </summary>
    private void GroundedCheck()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, _agent.height, _floorMask))
        {
            _grounded = true;
            return;
        }

        _grounded = false;
    }

    /// <summary>
    /// CheckProximity override that has different logic for covering
    /// </summary>
    protected override void CheckProximity()
    {
        switch (_state)
        {
            case EnemyStates.OutOfRange:

                if (_distanceFromPlayer <= _inRangeProximity && (_playerInSight || _distanceFromPlayer < 2f))
                {
                    ChangeState(EnemyStates.InRange);
                }
                return;

            case EnemyStates.InRange:

                if (_distanceFromPlayer > _inRangeProximity)
                {
                    ChangeState(EnemyStates.OutOfRange);
                }
                return;

            case EnemyStates.Covering:

                return;
        }
    }

    #endregion
}
