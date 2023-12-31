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
        Action,
        Stop
    }

    [Header("References")]

    [Tooltip("Reference to this enemy's normal mesh")]
    public GameObject mesh;

    [Tooltip("Reference to this enemy's ragdoll GO")]
    public GameObject ragdoll;

    [Header("Detection Debug")]

    [Tooltip("The current state of the enemy")]
    [SerializeField] protected EnemyStates state;

    [Tooltip("Is the player in sight?")]
    [SerializeField] protected bool playerInSight = false;

    [Header("Detection")]

    [Tooltip("The distance from the player to trigger an attack state")]
    [SerializeField] protected float inRangeProximity = 8f;

    [Tooltip("The distance in which the enemy can 'see' the player")]
    [SerializeField] protected float sightDistance = 30f;

    [Tooltip("The layers that will be allowed in the player detection raycast")]
    [SerializeField] protected LayerMask sightMask;

    [Tooltip("The range of time in between the enemy determining if it should respawn")]
    [SerializeField] protected float respawnCheckBuffer = 5f;

    [Header("Room")]

    [Tooltip("The layermask for room detection")]
    [SerializeField] protected LayerMask roomMask;

    [Tooltip("The speed in which an enemy will be pulled out of the room")]
    [SerializeField] protected float pullSpeed = 300f;

    [Tooltip("The slowdown multiplier after an enemy is pulled out")]
    [Range(0, 1)]
    [SerializeField] protected float pullSlowdownMultiplier = 0.5f;

    [Tooltip("The time after sucked until destroy")]
    [SerializeField] protected float timeTilDeathAfterSuck = 5f;

    // Reference to the player GameObject
    protected Transform playerRef;

    // The target transform reference for pathing
    protected Transform target;

    // Reference to this enemies weapon script
    protected EnemyWeapon weapon;

    protected Rigidbody body;

    protected Animator anim;

    // Reference to the room this enemy is within
    public Room currentRoom;

    // Reference to the base speed of the enemy
    protected float baseSpeed = 0;

    // Reference to the current speed of the enemy
    protected float currentSpeed = 0;

    // Is this enemy currently mutable in terms of speed/actions?
    protected bool mutable = true;

    // Updated distance from player
    protected float distanceFromPlayer = 0;

    // Integer checks for aiding the suck process
    private int pullChecks = 0;
    private float pullTimer = 0f;
    private float respawnTimer = 0f;

    #endregion

    #region Methods

    private void OnEnable()
    {
        GameManager.OnClear += Clear;
    }

    private void OnDisable()
    {
        GameManager.OnClear -= Clear;
    }

    /// <summary>
    /// Changes and properly transitions all enemy states
    /// </summary>
    /// <param name="newState">The new state of this enemy</param>
    public virtual void ChangeState(EnemyStates newState)
    {
    }

    protected virtual void Awake()
    {
        // Initial reference grabbing
        state = EnemyStates.OutOfRange;
        weapon = GetComponent<EnemyWeapon>();
        body = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        if (!weapon)
        {
            Debug.LogWarning($"Enemy {this.transform.name} is missing a weapon component!");
        }
    }

    protected virtual void Start()
    {
        // Grabs player reference AFTER it's set in GameManager
        if (GameManager.instance)
        {
            playerRef = GameManager.instance.GetPlayerReference();
        }
    }

    protected virtual void Update()
    {
        if (playerRef)
        {
            distanceFromPlayer = Vector3.Distance(transform.position, playerRef.position);

            switch (state)
            {
                case EnemyStates.OutOfRange:
                    CheckProximity();
                    break;
                case EnemyStates.InRange:
                    CheckProximity();

                    if (!playerInSight)
                    {
                        ChangeState(EnemyStates.OutOfRange);
                    }

                    break;
                case EnemyStates.NoGrav:

                    if (pullTimer > timeTilDeathAfterSuck)
                    {
                        pullTimer = -1000f;
                        GetComponent<EnemyHealth>().TakeDamage(1000, -1);
                    }

                    pullTimer += Time.deltaTime;

                    break;
                case EnemyStates.Action:
                    CheckProximity();
                    break;
            }

            // Checks for player raycast
            if (!playerInSight)
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
        playerInSight = playerSpotted;
    }

    /// <summary>
    /// Checks proximity to player, changes state if outside of parameters
    /// </summary>
    protected virtual void CheckProximity()
    {
        // Depressurize check
        if (currentRoom != null)
        {
            if (currentRoom.GetPressureStatus())
            {
                ChangeState(EnemyStates.NoGrav);
                return;
            }
        }

        switch (state)
        {
            case EnemyStates.OutOfRange:

                CheckIfStuck();

                if (distanceFromPlayer <= inRangeProximity && (playerInSight || distanceFromPlayer < 2f))
                {
                    ChangeState(EnemyStates.InRange);
                }
                return;

            case EnemyStates.InRange:

                if (distanceFromPlayer > inRangeProximity)
                {
                    ChangeState(EnemyStates.OutOfRange);
                }
                return;

            case EnemyStates.Action:

                if (distanceFromPlayer > (inRangeProximity * 1.5f))
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
        Vector3 dir = playerRef.position - this.transform.position;

        // Simple raycast
        if (Physics.Raycast(transform.position, dir, out hit, sightDistance, sightMask))
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
            if (currentRoom == null || other.GetComponent<Room>().GetRoomID() != currentRoom.GetRoomID())
            {
                currentRoom = other.GetComponent<Room>();
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Room"))
        {
            currentRoom = null;

            if (state == EnemyStates.NoGrav)
            {
                pullChecks++;

                if (pullChecks > 1)
                {
                    body.velocity *= pullSlowdownMultiplier;
                    pullTimer = 0;
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
        return state;
    }

    /// <summary>
    /// Turns the enemy into a ragdoll state after death
    /// Handles additional logic for window pulling
    /// </summary>
    /// <param name="layer"></param>
    public void Ragdollize(string layer = "")
    {
        mesh.SetActive(false);
        ragdoll.SetActive(true);

        ragdoll.transform.parent = this.transform.parent;

        ragdoll.GetComponentInChildren<SkinnedMeshRenderer>().updateWhenOffscreen = true;

        if (GameManager.instance.isLowGrav)
        {
            foreach (Rigidbody body in ragdoll.GetComponentsInChildren<Rigidbody>())
            {
                body.useGravity = false;

                if (layer != "")
                {
                    body.gameObject.layer = LayerMask.NameToLayer(layer);
                }
            }
        }
    }

    /// <summary>
    /// Checks whether an enemy is stuck
    /// </summary>
    void CheckIfStuck()
    {
        if ((currentRoom && !currentRoom.isActiveRoom()) && !playerInSight)
        {
            respawnTimer += Time.deltaTime;

            if (respawnTimer >= respawnCheckBuffer)
            {
                respawnTimer = 0;
                Respawn();
            }
        }
        else
        {
            respawnTimer = 0;
        }
    }

    /// <summary>
    /// Respawns an enemy if they're stuck too long
    /// </summary>
    public void Respawn()
    {
        ChangeState(EnemyStates.Stop);
        SmartMap.instance.RespawnEnemy(GetComponent<EnemyHealth>().enemyType);

        Debug.Log($"Respawning, stuck in room {(currentRoom ? currentRoom.GetRoomID() : "hallway")}".Color("yellow").Italic());

        Destroy(this.gameObject);
    }

    /// <summary>
    /// Keeps enemy look ats locked on one axis, stops weird rotation
    /// </summary>
    /// <param name="target">The target to look at</param>
    protected void LockedLookAt(Transform target)
    {
        Vector3 targetPostition = new Vector3(target.position.x, this.transform.position.y, target.position.z);
        this.transform.LookAt(targetPostition);
    }

    /// <summary>
    /// Wiped out of existence...
    /// </summary>
    private void Clear()
    {
        Destroy(this.gameObject);
    }

    #endregion
}