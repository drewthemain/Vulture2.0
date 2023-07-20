using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    #region Variables

    public enum EnemyStates
    {
        OutOfRange,
        InRange,
        NoGrav,
        Covering
    }

    [Header("References")]

    [Tooltip("Reference to this enemy's normal mesh")]
    public GameObject _mesh;

    [Tooltip("Reference to this enemy's ragdoll GO")]
    public GameObject _ragdoll;

    [Header("Detection Debug")]

    [Tooltip("The current state of the enemy")]
    [SerializeField] protected EnemyStates _state;

    [Tooltip("Is the player in sight?")]
    [SerializeField] protected bool _playerInSight = false;

    [Header("Detection")]

    [Tooltip("The distance from the player to trigger an attack state")]
    [SerializeField] protected float _inRangeProximity = 8f;

    [Tooltip("The distance in which the enemy can 'see' the player")]
    [SerializeField] protected float _sightDistance = 30f;

    [Tooltip("The layers that will be allowed in the player detection raycast")]
    [SerializeField] protected LayerMask _sightMask;

    [Header("Movement")]

    [Tooltip("Should the enemy slow down once the player is spotted?")]
    [SerializeField] protected bool _slowWhenPlayerSpotted = true;

    [Tooltip("If slowed, this value will be the speed reduction percentage")]
    [Range(0, 1)]
    [SerializeField] protected float _speedReductionPercentage = 0.5f;

    [Tooltip("Should the enemy speed up the further they are from the player?")]
    [SerializeField] protected bool _proximitySpeedBoost = true;

    [Tooltip("If out of range, this value will be the speed boost percentage")]
    [Range(0, 1)]
    [SerializeField] protected float _speedBoostPercentage = 0.5f;

    [Header("Room")]

    [Tooltip("The layermask for room detection")]
    [SerializeField] protected LayerMask _roomMask;

    [Tooltip("The speed in which an enemy will be pulled out of the room")]
    [SerializeField] protected float _pullSpeed = 300f;

    [Tooltip("The slowdown multiplier after an enemy is pulled out")]
    [Range(0, 1)]
    [SerializeField] protected float _pullSlowdownMultiplier = 0.5f;

    [Tooltip("The time after sucked until destroy")]
    [SerializeField] protected float _timeTilDeathAfterSuck = 5f;

    // Reference to the player GameObject
    protected Transform _playerRef;

    // Reference to the NavMesh Agent
    protected NavMeshAgent _agent;

    // Reference to this enemies weapon script
    protected EnemyWeapon _weapon;

    protected Rigidbody _body;

    protected Animator _anim;

    // Reference to the room this enemy is within
    public Room _currentRoom;

    // Reference to the base speed of the enemy
    protected float _baseSpeed = 0;

    // Reference to the current speed of the enemy
    protected float _currentSpeed = 0;

    // Is this enemy currently mutable in terms of speed/actions?
    protected bool _mutable = true;

    // Updated distance from player
    protected float _distanceFromPlayer = 0;

    // An integer check for aiding the suck process
    private int _pullChecks = 0;

    private float _pullTimer = 0f;

    #endregion

    #region Methods

    /// <summary>
    /// Changes and properly transitions all enemy states
    /// </summary>
    /// <param name="newState">The new state of this enemy</param>
    public virtual void ChangeState(EnemyStates newState)
    {
        _state = newState;

        if (_agent && _agent.enabled == false)
        {
            _agent.enabled = true;
            _body.isKinematic = true;
        }

        switch (newState)
        {
            case EnemyStates.OutOfRange:

                if (_agent && _proximitySpeedBoost)
                {
                    _agent.speed = _baseSpeed + (_baseSpeed * _speedBoostPercentage);
                }

                _mutable = true;
                break;
            case EnemyStates.InRange:

                if (_agent)
                {
                    _agent.speed = _baseSpeed;
                }

                _mutable = true;
                break;
            case EnemyStates.NoGrav:

                // Turn off navmesh when floating
                _agent.enabled = false;

                // Turn on rigidbody
                _body.isKinematic = false;

                gameObject.layer = LayerMask.NameToLayer("Sucked");

                // Set velocity toward the window
                if (_currentRoom.GetWindow() != null)
                {
                    Vector3 targetDir = (_currentRoom.GetWindow().transform.position - transform.position).normalized;
                    _body.velocity = targetDir * _pullSpeed * 10;

                    Ragdollize("Sucked");
                }

                _mutable = false;
                break;
            case EnemyStates.Covering:
                _mutable = false;
                break;
        }
    }

    protected virtual void Awake()
    {
        // Initial reference grabbing
        _state = EnemyStates.OutOfRange;
        _agent = GetComponent<NavMeshAgent>();
        _weapon = GetComponent<EnemyWeapon>();
        _body = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();

        _baseSpeed = _agent.speed;

        if (!_agent)
        {
            Debug.LogWarning($"Enemy {this.transform.name} is missing a NavMeshAgent component!");
        }

        if (!_weapon)
        {
            Debug.LogWarning($"Enemy {this.transform.name} is missing a weapon component!");
        }
    }

    protected virtual void Start()
    {
        // Grabs player reference AFTER it's set in GameManager
        if (GameManager.instance)
        {
            _playerRef = GameManager.instance.GetPlayerReference();
        }
    }

    protected virtual void Update()
    {
        if (_playerRef)
        {
            _distanceFromPlayer = Vector3.Distance(transform.position, _playerRef.position);

            switch (_state)
            {
                case EnemyStates.OutOfRange:
                    CheckProximity();
                    break;
                case EnemyStates.InRange:
                    CheckProximity();

                    if (!_playerInSight)
                    {
                        ChangeState(EnemyStates.OutOfRange);
                    }

                    break;
                case EnemyStates.NoGrav:

                    if (_pullTimer > _timeTilDeathAfterSuck)
                    {
                        _pullTimer = -1000f;
                        GetComponent<EnemyHealth>().TakeDamage(1000, -1);
                    }

                    _pullTimer += Time.deltaTime;

                    break;
                case EnemyStates.Covering:
                    CheckProximity();
                    break;
            }

            // Checks for player raycast
            if (!_playerInSight)
            {
                if (CheckSightline())
                {
                    TogglePlayerSightline(true);
                }
            }
            else
            {
                if (!CheckSightline())
                {
                    TogglePlayerSightline(false);
                }
            }
        }
    }

    /// <summary>
    /// Toggles on and off whether a player can be seen
    /// </summary>
    /// <param name="playerSpotted">Is the player currently spotted?</param>
    protected virtual void TogglePlayerSightline(bool playerSpotted)
    {
        _playerInSight = playerSpotted;
        
        if (_slowWhenPlayerSpotted && _mutable)
        {
            _agent.speed = !playerSpotted ? _baseSpeed : (_baseSpeed - (_baseSpeed * _speedReductionPercentage));

            if (_proximitySpeedBoost && _state == EnemyStates.OutOfRange)
            {
                _agent.speed += _baseSpeed * _speedBoostPercentage;
            }
        }
    }

    /// <summary>
    /// Checks proximity to player, changes state if outside of parameters
    /// </summary>
    protected virtual void CheckProximity()
    {
        // Depressurize check
        if (_currentRoom != null)
        {
            if (_currentRoom.GetPressureStatus())
            {
                ChangeState(EnemyStates.NoGrav);
                return;
            }
        }

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

                if (_distanceFromPlayer > (_inRangeProximity * 1.5f))
                {
                    ChangeState(EnemyStates.OutOfRange);
                }
                return;
        }
    }

    /// <summary>
    /// Checks raycast visiblity to player and returns results
    /// </summary>
    /// <returns>True if player is visible, else false</returns>
    protected virtual bool CheckSightline()
    {
        RaycastHit hit;
        Vector3 dir = _playerRef.position - this.transform.position;

        // Simple raycast
        if (Physics.Raycast(transform.position, dir, out hit, _sightDistance, _sightMask))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            // If the enemy doesn't have a current room OR the found room is different, set as new room
            if (_currentRoom == null || other.GetComponent<Room>().GetRoomID() != _currentRoom.GetRoomID())
            {
                _currentRoom = other.GetComponent<Room>();
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            _currentRoom = null;

            if (_state == EnemyStates.NoGrav)
            {
                _pullChecks++;

                if (_pullChecks > 1)
                {
                    _body.velocity *= _pullSlowdownMultiplier;
                    _pullTimer = 0;
                }
            }
        }
    }

    /// <summary>
    /// Getter for the current enemy state
    /// </summary>
    /// <returns>The current enemy state</returns>
    public EnemyStates GetState()
    {
        return _state;
    }

    public void Ragdollize(string layer = "")
    {
        _mesh.SetActive(false);
        _ragdoll.SetActive(true);

        _ragdoll.transform.parent = this.transform.parent;

        _ragdoll.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = true;

        if (GameManager.instance._isLowGrav)
        {
            foreach (Rigidbody body in _ragdoll.GetComponentsInChildren<Rigidbody>())
            {
                body.useGravity = false;

                if (layer != "")
                {
                    body.gameObject.layer = LayerMask.NameToLayer(layer);
                }
            }
        }
    }

    #endregion
}