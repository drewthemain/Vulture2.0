using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Soldier : Enemy
{
    #region Variables

    public enum InRangeActions
    {
        StrafeLeft,
        StrafeRight,
        Charge,
        Stand,
        Back
    }

    public enum CoverActions
    {
        Shoot,
        Wait
    }

    [Header("In Range Behavior")]

    [Tooltip("The current in range action of the soldier")]
    [SerializeField] private InRangeActions _inRangeAction = InRangeActions.StrafeLeft;

    [Tooltip("The range in which the player is too close to the enemy")]
    [SerializeField] private float _closestDesiredRange = 10f;

    [Tooltip("The max distance a strafe can be performed")]
    [SerializeField] private float maxStrafeDistance = 10;

    [Tooltip("The time constraints of which a strafe can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _strafeTimeConstraints;

    [Tooltip("The time constraints of which a charge can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _chargeTimeConstraints;

    [Tooltip("The time constraints of which a stand can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _standTimeConstraints;

    [Tooltip("Should this enemy shoot while moving?")]
    [SerializeField] private bool _shootWhileMoving = true;

    [Header("Cover Behavior")]

    [Tooltip("The current in range action of the soldier")]
    [SerializeField] private CoverActions _coverAction = CoverActions.Shoot;

    [Tooltip("Initial chances of recieveing a cover action (should be small, grows over time")]
    [Range(0, 1)]
    [SerializeField] private float _initialCoverOdds = 0.05f;

    [Tooltip("Growth to chances of recieveing a cover after every action")]
    [Range(0, 0.1f)]
    [SerializeField] private float _coverOddsGrowth = 0.01f;

    [Tooltip("The time constraints of which a cover can be performed. X is bottom constraint, Y is top")]
    [SerializeField] private Vector2 _coverTimeConstraints;

    // The target transform reference for pathing
    private Transform _target;

    // The target position reference for pathing (strafing)
    private Vector3 _targetPosition;

    // The timer for each action
    private float _actionTimer = 0;

    // The point in which a cover will be stopped
    private float _actionLimit = 0;

    // An additional timer for more complex actions
    private float _additionalTimer = 0;

    // An additional limit for more complex actions
    private float _additionalLimit = 0;

    // Have the cover actions been started?
    private bool _coverActionsStarted = false;

    // The current odds of choosing a cover
    private float _coverOdds = 0.05f;

    #endregion

    #region Override Methods

    /// <summary>
    /// Changes and properly transitions all soldier states
    /// </summary>
    /// <param name="newState">The new state of this enemy</param>
    public override void ChangeState(EnemyStates newState)
    {
        // Resets (clears out) the path of the navmesh for every change
        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
            _target = null;
            _targetPosition = Vector3.zero;

            // Resets all timers
            _actionTimer = 0;
            _actionLimit = 0;
        }

        base.ChangeState(newState);

        switch (newState)
        {
            case EnemyStates.OutOfRange:

                // Initial following of the player
                if (_agent)
                {
                    _target = _playerRef;
                    _agent.SetDestination(_target.position);
                }

                break;

            case EnemyStates.InRange:

                // Turn firing back on if we weren't shooting while moving
                if (_weapon && !_shootWhileMoving && _playerInSight)
                {
                    _weapon.ToggleFiring(true);
                }

                _actionTimer = 0;

                break;

            case EnemyStates.NoGrav:

                if (_weapon)
                {
                    _weapon.ToggleFiring(false);
                }

                break;
            case EnemyStates.Covering:

                // Query cover from room and move
                if (_agent)
                {
                    if (_currentRoom == null)
                    {
                        ChangeState(EnemyStates.InRange);
                        return;
                    }

                    _target = _currentRoom.QueryCover(_playerRef, this.transform, _sightMask);

                    if (_target == null)
                    {
                        ChangeState(EnemyStates.InRange);
                        return;
                    }

                    if (_weapon != null)
                    {
                        _weapon.ToggleFiring(false);
                    }

                    _agent.speed = _baseSpeed * 1.5f;
                    _actionLimit = Random.Range(_coverTimeConstraints.x, _coverTimeConstraints.y);

                    // Cover action resetting
                    _coverOdds = _initialCoverOdds;
                    _coverActionsStarted = false;
                    _additionalLimit = 0;
                    _additionalTimer = 0;

                    if (_weapon)
                    {
                        _weapon.ToggleFiring(true);
                    }

                    _agent.SetDestination(_target.position);
                }

                break;
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // Start by targeting the player
        if (_playerRef)
        {
            _target = _playerRef;
        }

        if (_agent)
        {
            _agent.updateRotation = false;
        }

        _coverOdds = _initialCoverOdds;
    }

    protected override void Update()
    {
        base.Update();

        if (_playerRef)
        {
            switch (_state)
            {
                case EnemyStates.OutOfRange:

                    // Soldiers chase the player when out of attack proximity
                    if (_agent != null && _target != null)
                    {
                        _agent.SetDestination(_target.position);
                        transform.LookAt(_target);
                    }
                    
                    break;
                case EnemyStates.InRange:

                    // Handle the logic for in-range thinking
                    ThinkInRange();

                    break;
                case EnemyStates.NoGrav:

                    break;
                case EnemyStates.Covering:

                    if (!_coverActionsStarted)
                    {
                        // If done moving to cover, start doing actions!
                        if (_agent.remainingDistance < 2f)
                        {
                            _coverActionsStarted = true;

                            _additionalTimer = 0;
                            ChangeCoverAction((CoverActions)Random.Range(0, 2));
                        }
                        else
                        {
                            transform.LookAt(_playerRef);
                        }
                    }
                    else
                    {
                        ThinkCover();
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Changes the current action within a cover status
    /// </summary>
    /// <param name="newAction">The new cover action to perform</param>
    private void ChangeCoverAction(CoverActions newAction)
    {
        _coverAction = newAction;
        _agent.ResetPath();
        _additionalLimit = 0;
        _additionalTimer = 0;

        switch (newAction)
        {
            case CoverActions.Shoot:

                _additionalLimit = Random.Range(_coverTimeConstraints.x / 2, _coverTimeConstraints.y / 2);

                break;
            case CoverActions.Wait:

                // Send the enemy back to cover
                if (_agent && _target)
                {
                    _agent.SetDestination(_target.position);
                }

                // Stop firing!
                if (_weapon && !_playerInSight)
                {
                    _weapon.ToggleFiring(false);
                }

                // Wait currently has less credence than shoot
                _additionalLimit = Random.Range(_coverTimeConstraints.x / 4, _coverTimeConstraints.y / 4);

                break;
        }
    }

    /// <summary>
    /// Handles constant logic for the cover status
    /// </summary>
    private void ThinkCover()
    {
        // Timer for the OVERALL cover status
        if (_actionTimer >= _actionLimit)
        {
            _currentRoom.ReturnCover(_target);
            ChangeState(EnemyStates.InRange);
        }

        // If player rushes enemy in cover, break out of cover
        if (_distanceFromPlayer < _closestDesiredRange / 2)
        {
            _currentRoom.ReturnCover(_target);
            ChangeState(EnemyStates.InRange);
            return;
        }

        // Timer for the actions within the cover
        if (_additionalTimer >= _additionalLimit)
        {
            _additionalTimer = 0;
            ChangeCoverAction((CoverActions)Random.Range(0, 2));
        }

        switch (_coverAction)
        {
            case CoverActions.Shoot:

                if (!_playerInSight)
                {
                    if (_agent)
                    {
                        _agent.SetDestination(_playerRef.position);
                    }
                }
                else
                {
                    // If player can be seen, stop and start shooting!
                    _agent.ResetPath();

                    if (_weapon)
                    {
                        _weapon.ToggleFiring(true);
                    }
                }

                transform.LookAt(_playerRef);

                break;
            case CoverActions.Wait:

                transform.LookAt(_playerRef);

                if (_target)
                {
                    if (Vector3.Distance(_target.position, transform.position) < 0.5f)
                    {
                        if (_playerInSight)
                        {
                            ChangeState(EnemyStates.InRange);
                        }
                        else
                        {
                            if (_weapon)
                            {
                                _weapon.ToggleFiring(false);
                            }
                        }
                    }
                }

                break;
        }

        _actionTimer += Time.deltaTime;
        _additionalTimer += Time.deltaTime;
    }

    /// <summary>
    /// Changes the in-range action based on parameter
    /// </summary>
    /// <param name="newAction">The new action to perform</param>
    private void ChangeAction(InRangeActions newAction)
    {
        // Resets all action values
        _inRangeAction = newAction;
        _agent.ResetPath();
        _agent.speed = _baseSpeed;
        _target = null;
        _targetPosition = Vector3.zero;

        if (_weapon)
        {
            _weapon.ToggleFiring(true);
        }

        // Only add to cover odds under normal circumstances
        if (_inRangeAction != InRangeActions.Back && _currentRoom != null)
        {
            float coverRoll = Random.Range(0f, 1f);

            if (coverRoll < _coverOdds)
            {
                ChangeState(EnemyStates.Covering);
                return;
            }

            _coverOdds += _coverOddsGrowth;
        }

        switch (newAction)
        {
            case InRangeActions.StrafeLeft:

                // Set new target position
                _actionLimit = Random.Range(_strafeTimeConstraints.x, _strafeTimeConstraints.y);
                _targetPosition = transform.position + (-transform.right * (Random.Range(5, maxStrafeDistance)));
                _agent.SetDestination(_targetPosition);

                break;

            case InRangeActions.StrafeRight:

                // Set new target position
                _actionLimit = Random.Range(_strafeTimeConstraints.x, _strafeTimeConstraints.y);
                _targetPosition = transform.position + (transform.right * (Random.Range(5, maxStrafeDistance)));
                _agent.SetDestination(_targetPosition);

                break;

            case InRangeActions.Charge:

                // Target the player
                _actionLimit = Random.Range(_chargeTimeConstraints.x, _chargeTimeConstraints.y);
                _target = _playerRef;

                break;

            case InRangeActions.Stand:

                _actionLimit = Random.Range(_standTimeConstraints.x, _standTimeConstraints.y);

                break;

            case InRangeActions.Back:

                _actionLimit = Random.Range(_strafeTimeConstraints.x, _strafeTimeConstraints.y);

                // Try to flee backwards, else take cover
                _targetPosition = GetFleePosition();
                if (_targetPosition == Vector3.zero)
                {
                    if (_currentRoom != null)
                    {
                        ChangeState(EnemyStates.Covering);
                        return;
                    }
                    else
                    {
                        // If no cover, strafe instead!
                        _agent.speed = _baseSpeed * 1.5f;
                        _targetPosition = transform.position + ((Random.Range(0, 2) * 2 - 1) * transform.right * (Random.Range(5, maxStrafeDistance)));
                    }
                }

                _agent.SetDestination(_targetPosition);

                break;
        }
    }

    /// <summary>
    /// Handles constant logic for the in-range status
    /// </summary>
    private void ThinkInRange()
    {
        // Checking if the current action is finished
        // If so, start new action
        if (_actionTimer > _actionLimit)
        {
            _actionTimer = 0;

            ChangeAction((InRangeActions)Random.Range(0, 4));
        }

        // If the player is too close to enemy, try to back up or find cover
        if (_distanceFromPlayer < _closestDesiredRange)
        {
            if (_inRangeAction != InRangeActions.Back && _state != EnemyStates.Covering)
            {
                _actionTimer = 0;

                ChangeAction(InRangeActions.Back);
            }
        }

        // Constant behavior for every action
        switch (_inRangeAction)
        {
            case InRangeActions.StrafeLeft:

                transform.LookAt(_playerRef);

                break;
            case InRangeActions.StrafeRight:

                transform.LookAt(_playerRef);

                break;

            case InRangeActions.Charge:

                transform.LookAt(_playerRef);

                if (_agent && _target != null)
                {
                    _agent.SetDestination(_target.position);
                }

                break;
            case InRangeActions.Stand:

                transform.LookAt(_playerRef);

                break;
            case InRangeActions.Back:

                transform.LookAt(_playerRef);

                break;
        }

        // Update the timer to increment between actions
        _actionTimer += Time.deltaTime;
    }

    /// <summary>
    /// Sets firing on and off based on if player is in raycast
    /// </summary>
    /// <param name="playerSpotted">Is the player spotted?</param>
    protected override void TogglePlayerSightline(bool playerSpotted)
    {
        base.TogglePlayerSightline(playerSpotted);

        // If moving and we don't want to shoot, turn firing off!
        if (_state == EnemyStates.OutOfRange && !_shootWhileMoving)
        {
            _weapon.ToggleFiring(false);
            return;
        }

        if (!_mutable)
        {
            return;
        }

        _weapon.ToggleFiring(playerSpotted);
    }

    protected override void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            if (_state == EnemyStates.Covering)
            {
                _currentRoom.ReturnCover(_target);
            }
        }

        base.OnTriggerExit(other);
    }

    /// <summary>
    /// If player is too close to enemy, try to find a way out
    /// </summary>
    /// <returns></returns>
    public Vector3 GetFleePosition()
    {
        Vector3 testPos = _playerRef.position - transform.forward * _inRangeProximity * 2;

        NavMeshPath path = new NavMeshPath();
        if (!_agent.CalculatePath(testPos, path))
        {
            return Vector3.zero;
        }

        return testPos;
    }

    #endregion
}
