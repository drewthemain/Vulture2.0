using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    #region Variables 

    [Header("Identification")]

    [Tooltip("The unique ID of this room")]
    [SerializeField] private string _roomID = "";

    [Header("Spawning")]

    [Tooltip("Rooms that should be considered 'connected' to this room")]
    [SerializeField] private List<Room> _neighboringRooms;

    [Tooltip("The odds that an enemy will be spawned in a neighboring room")]
    [Range(0, 1)]
    [SerializeField] private float _neighboringSpawnOdds = 0.4f;

    [Header("Cover")]

    [Tooltip("List of cover transforms")]
    [SerializeField] private Transform _coverParent;

    // A record of which covers are taken
    private Dictionary<Transform, Transform> _coverRecords = new Dictionary<Transform, Transform>();

    // The transforms of every cover position
    private List<Transform> _coverTransforms = new List<Transform>();

    // The list of all possible spawners
    private List<SmartSpawner> _soldierSpawners = new List<SmartSpawner>();

    [Header("Environment")]

    [Tooltip("The amount of time the room will remain depressurized")]
    [SerializeField] private float _depressurizeTime = 15f;

    // Reference to the breakable window
    private Window _window;

    // Is the room currently depressurized?
    private bool _depressurized = false;

    // The list of all possible spawners
    private List<SmartSpawner> _swarmSpawners = new List<SmartSpawner>();

    // Is the player currently inside this room?
    private bool _playerInside = false;

    // The timer for the depressurize countdown
    private float _depressureTimer = 0;

    #endregion

    #region Methods

    void Start()
    {
        if (_roomID.Length == 0)
        {
            Debug.LogWarning("Each room should have a unique identifier. Try naming it after a function or landmark of the room.");
        }

        // Populate records with length of covers
        foreach (Transform cover in _coverParent)
        {
            _coverTransforms.Add(cover);
            _coverRecords[cover] = null;
        }

        foreach (SmartSpawner spawner in GetComponentsInChildren<SmartSpawner>())
        {
            if (spawner._enemyType == Order.EnemyTypes.Soldier)
            {
                _soldierSpawners.Add(spawner);
            }
            else
            {
                _swarmSpawners.Add(spawner);
            }
        }

        if (GetComponentInChildren<Window>())
        {
            _window = GetComponentInChildren<Window>();
        }
        else
        {
            Debug.LogWarning("This room is missing a depressurization window!");
        }
    }

    private void Update()
    {
        if (_depressurized && _window != null)
        {
            // Timer for the initial depressurization suck
            _depressureTimer += Time.deltaTime;

            if (_depressureTimer > _depressurizeTime)
            {
                _depressurized = false;
                _depressureTimer = 0;

                _window.Pressurize();
            }
        }
    }

    /// <summary>
    /// Getter for a room's ID
    /// </summary>
    /// <returns>The room ID string</returns>
    public string GetRoomID()
    {
        return _roomID;
    }


    /// <summary>
    /// Given a player, returns a cover point that the player cannot currently see
    /// </summary>
    /// <param name="player">Reference to the player transform</param>
    /// <param name="playerMask">A layermask for cover raycast detection</param>
    /// <returns>A transform of the appropriate cover</returns>
    public Transform QueryCover(Transform player, Transform enemy, LayerMask playerMask)
    {
        // Sort transforms by distance to enemy
        List<Transform> temp = new List<Transform>(_coverTransforms);
        temp.Sort((p1, p2) => Vector3.Distance(p1.position, enemy.position).CompareTo(Vector3.Distance(p2.position, enemy.position)));

        for (int i = 0; i < temp.Count; i++)
        {
            // If the cover is already being used, continue
            if (_coverRecords[temp[i]] != null)
            {
                continue;
            }

            RaycastHit hit;
            Vector3 dir = player.position - temp[i].position;

            Debug.DrawRay(temp[i].position, dir * 100, Color.green);

            if (Physics.Raycast(temp[i].position, dir, out hit, 100, playerMask))
            {
                // If the cover can "see" the player, don't use it!
                if (hit.transform.CompareTag("Player"))
                {
                    continue;
                }
            }

            // Player can't see the cover!
            _coverRecords[temp[i]] = enemy;
            return temp[i];
        }

        // Will only return null if there are no valid covers
        return null;
    }
    
    /// <summary>
    /// Returns the cover by reseting the record
    /// </summary>
    /// <param name="returnedCover">Reference to the returned cover</param>
    public void ReturnCover(Transform returnedCover)
    {
        if (_coverRecords[returnedCover] != null)
        {
            _coverRecords[returnedCover] = null;
        }
        else
        {
            Debug.LogWarning("Returning an unused cover!");
        }
    }

    /// <summary>
    /// Toggles whether the player is inside the room or not
    /// </summary>
    /// <param name="isInside">Is the player inside the room?</param>
    public void PlayerToggle(bool isInside)
    {
        _playerInside = isInside;

        // Let the SmartMap know which room is being prioritized
        if (isInside)
        {
            if (SmartMap.instance)
            {
                SmartMap.instance.SetActiveRoom(this);
            }
        }
    }

    /// <summary>
    /// Picks the correct spawners to be activated based on the order
    /// </summary>
    /// <param name="order">The order to be spawned</param>
    /// <returns>True if it was successful, else false</returns>
    public bool SmartSpawn(Order order, int multiplier)
    {
        for (int i = 0; i < order._enemyAmount * multiplier; i++)
        {
            // Start by getting a neighboring room
            Room neighborRoom = CheckNeighbors(order._enemy);

            if (neighborRoom)
            {
                // RNG to see if enemy will spawn in neighboring room instead of player room
                if (Random.Range(0f, 1f) <= _neighboringSpawnOdds)
                {
                    neighborRoom.CommandSpawn(order._enemy);
                    continue;
                }
            }

            // Try to spawn in player room. If failure, try spawning in neighbor
            if (!CommandSpawn(order._enemy))
            {
                if (neighborRoom)
                {
                    neighborRoom.CommandSpawn(order._enemy);
                }
                else
                {
                    // Nowhere for the enemy to spawn, return false
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Finds a possible neighboring room that could spawn an enemy
    /// </summary>
    /// <param name="_type">The type of enemy to be spawned</param>
    /// <returns>A room if one fits, else null</returns>
    public Room CheckNeighbors(Order.EnemyTypes _type)
    {
        List<Room> possibleRooms = new List<Room>();
        foreach (Room room in _neighboringRooms)
        {
            if (room.CheckSpawnerCapacity(_type))
            {
                possibleRooms.Add(room);
            }
        }

        if (possibleRooms.Count == 0)
        {
            return null;
        }

        // return a random room from the possible options
        return possibleRooms[Random.Range(0, possibleRooms.Count)];
    }

    /// <summary>
    /// Spawn one enemy based on the type from the available spawners
    /// </summary>
    /// <param name="_type">The type of enemy to be spawned</param>
    /// <returns>True if spawned, else false</returns>
    public bool CommandSpawn(Order.EnemyTypes _type)
    {
        List<SmartSpawner> spawners = _type == Order.EnemyTypes.Soldier ? _soldierSpawners : _swarmSpawners;

        if (spawners.Count == 0)
        {
            return false;
        }

        spawners[Random.Range(0, spawners.Count)].AcceptOrder(1);

        return true;
    }

    /// <summary>
    /// Checker to see if a room has that type of spawner available
    /// </summary>
    /// <param name="_type">The type of enemy that the spawner can handle</param>
    /// <returns>True if the room has spawners, else false</returns>
    public bool CheckSpawnerCapacity(Order.EnemyTypes _type)
    {
        switch (_type)
        {
            case Order.EnemyTypes.Soldier:
                return _soldierSpawners.Count > 0;
            case Order.EnemyTypes.Swarm:
                return _swarmSpawners.Count > 0;
        }

        return false;
    }

    /// <summary>
    /// Tells the spawners in this room to shut down
    /// </summary>
    /// <returns>The remaining enemies to be spawned in this room</returns>
    public Vector2 Shutdown()
    {
        Vector2 carryOn = Vector2.zero;

        foreach (SmartSpawner spawner in _soldierSpawners)
        {
            carryOn.x += spawner.ClearOrderRemaining();
        }

        foreach (SmartSpawner spawner in _swarmSpawners)
        {
            carryOn.y += spawner.ClearOrderRemaining();
        }

        return carryOn;
    }

    /// <summary>
    /// Getter for the window
    /// </summary>
    /// <returns>Window reference</returns>
    public Window GetWindow()
    {
        return _window;
    }

    /// <summary>
    /// Toggles the depressurization state
    /// </summary>
    public void Depressurize()
    {
        _depressurized = true;

        GameManager.instance.ToggleGravity(true);
    }

    /// <summary>
    /// Getter for the depressurization status
    /// </summary>
    /// <returns>Is the room depressurized?</returns>
    public bool GetPressureStatus()
    {
        return _depressurized;
    }

    #endregion
}
