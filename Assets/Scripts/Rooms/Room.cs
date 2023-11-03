using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Room : MonoBehaviour
{
    #region Variables 

    [Header("Identification")]

    [Tooltip("The unique ID of this room")]
    [SerializeField] private string roomID = "";

    [Header("Spawning")]

    [Tooltip("Rooms that should be considered 'connected' to this room")]
    [SerializeField] private List<Room> neighboringRooms;

    [Tooltip("The odds that an enemy will be spawned in a neighboring room")]
    [Range(0, 1)]
    [SerializeField] private float neighboringSpawnOdds = 0.4f;

    [Tooltip("The chance a window will be fixed in-between rounds")]
    [SerializeField] private float windowFixChance = 0.3f;

    [Header("Cover")]

    [Tooltip("List of cover transforms")]
    [SerializeField] private Transform coverParent;

    // A record of which covers are taken
    private Dictionary<Transform, Transform> coverRecords = new Dictionary<Transform, Transform>();

    // The transforms of every cover position
    private List<Transform> coverTransforms = new List<Transform>();

    // The list of all possible spawners
    private List<SmartSpawner> groundSpawners = new List<SmartSpawner>();

    // Reference to the breakable window
    private List<Window> windows = new List<Window>();

    // Is the room currently depressurized?
    private bool depressurized = false;

    // The list of all possible spawners
    private List<SmartSpawner> wallSpawners = new List<SmartSpawner>();

    // Is the player currently inside this room?
    private bool playerInside = false;

    #endregion

    #region Methods

    void Start()
    {
        if (roomID.Length == 0)
        {
            Debug.LogWarning("Each room should have a unique identifier. Try naming it after a function or landmark of the room.");
        }

        // Populate records with length of covers
        foreach (Transform cover in coverParent)
        {
            coverTransforms.Add(cover);
            coverRecords[cover] = null;
        }

        foreach (SmartSpawner spawner in GetComponentsInChildren<SmartSpawner>())
        {
            if (spawner.isGroundSpawner)
            {
                groundSpawners.Add(spawner);
            }
            else
            {
                wallSpawners.Add(spawner);
            }
        }

        foreach(Window window in GetComponentsInChildren<Window>())
        {
            windows.Add(window);
        }
    }

    /// <summary>
    /// Getter for a room's ID
    /// </summary>
    /// <returns>The room ID string</returns>
    public string GetRoomID()
    {
        return roomID;
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
        List<Transform> temp = new List<Transform>(coverTransforms);
        temp.Sort((p1, p2) => Vector3.Distance(p1.position, enemy.position).CompareTo(Vector3.Distance(p2.position, enemy.position)));

        for (int i = 0; i < temp.Count; i++)
        {
            // If the cover is already being used, continue
            if (coverRecords[temp[i]] != null)
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
            coverRecords[temp[i]] = enemy;
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
        if (coverRecords[returnedCover] != null)
        {
            coverRecords[returnedCover] = null;
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
        playerInside = isInside;

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
        for (int i = 0; i < order.enemyAmount * multiplier; i++)
        {
            // Start by getting a neighboring room
            Room neighborRoom = CheckNeighbors(order.enemy);

            if (neighborRoom)
            {
                // RNG to see if enemy will spawn in neighboring room instead of player room
                if (Random.Range(0f, 1f) <= neighboringSpawnOdds)
                {
                    neighborRoom.CommandSpawn(order.enemy);
                    continue;
                }
            }

            // Try to spawn in player room. If failure, try spawning in neighbor
            if (!CommandSpawn(order.enemy))
            {
                if (neighborRoom)
                {
                    neighborRoom.CommandSpawn(order.enemy);
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
    /// <param name="type">The type of enemy to be spawned</param>
    /// <returns>A room if one fits, else null</returns>
    public Room CheckNeighbors(Order.EnemyTypes type)
    {
        List<Room> possibleRooms = new List<Room>();
        foreach (Room room in neighboringRooms)
        {
            if (room.CheckSpawnerCapacity(type))
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
    /// <param name="type">The type of enemy to be spawned</param>
    /// <returns>True if spawned, else false</returns>
    public bool CommandSpawn(Order.EnemyTypes type)
    {
        List<SmartSpawner> spawners = new List<SmartSpawner>();

        switch (type)
        {
            case Order.EnemyTypes.Soldier:
            case Order.EnemyTypes.Splicer:
                spawners = groundSpawners;
                break;
            case Order.EnemyTypes.Swarm:
                spawners = wallSpawners;
                break;
        }

        if (spawners.Count == 0)
        {
            return false;
        }

        spawners[Random.Range(0, spawners.Count)].AcceptOrder(type, 1);

        return true;
    }

    /// <summary>
    /// Checker to see if a room has that type of spawner available
    /// </summary>
    /// <param name="type">The type of enemy that the spawner can handle</param>
    /// <returns>True if the room has spawners, else false</returns>
    public bool CheckSpawnerCapacity(Order.EnemyTypes type)
    {
        switch (type)
        {
            case Order.EnemyTypes.Soldier:
            case Order.EnemyTypes.Splicer:
                return groundSpawners.Count > 0;
            case Order.EnemyTypes.Swarm:
                return wallSpawners.Count > 0;
        }

        return false;
    }

    /// <summary>
    /// Tells the spawners in this room to shut down
    /// </summary>
    /// <returns>The remaining enemies to be spawned in this room</returns>
    public Dictionary<Order.EnemyTypes, int> Shutdown()
    {
        Dictionary<Order.EnemyTypes, int> carryOn = new Dictionary<Order.EnemyTypes, int>();

        foreach (SmartSpawner spawner in groundSpawners.Union(wallSpawners))
        {
            Dictionary<Order.EnemyTypes, int> leftOver = spawner.ClearOrderRemaining();

            foreach (Order.EnemyTypes type in leftOver.Keys)
            {
                int currentCount;
                carryOn.TryGetValue(type, out currentCount);
                carryOn[type] = currentCount + leftOver[type];
            }
        }

        return carryOn;
    }

    /// <summary>
    /// Getter for the window
    /// </summary>
    /// <returns>Window reference</returns>
    public Window GetBrokenWindow()
    {
        Window window = GameManager.instance.GetPullingWindow();

        if (window && windows.Contains(window))
        {
            return GameManager.instance.GetPullingWindow();
        }

        return null;
    }

    /// <summary>
    /// Toggles the depressurization state
    /// </summary>
    public void Depressurize(Window window)
    {
        depressurized = true;

        GameManager.instance.ToggleGravity(true, window);
    }

    /// <summary>
    /// Getter for the depressurization status
    /// </summary>
    /// <returns>Is the room depressurized?</returns>
    public bool GetPressureStatus()
    {
        return GetBrokenWindow() != null;
    }

    /// <summary>
    /// Fixes a certain amount of windows
    /// </summary>
    /// <param name="fixAll">Should all of the windows be fixed?</param>
    /// <param name="num">The number of windows to be fixed</param>
    public void FixWindows()
    {
        foreach(Window window in windows)
        {
            if (Random.Range(0,1) < windowFixChance)
            {
                window.FixWindow();
            }
        }
    }

    /// <summary>
    /// Determines whether the room should be focused in on
    /// </summary>
    /// <returns>True if player is inside</returns>
    public bool isActiveRoom()
    {
        return playerInside;
    }

    #endregion
}
