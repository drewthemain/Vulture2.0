using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartMap : MonoBehaviour
{
    #region Variables

    // Instance for singleton
    public static SmartMap instance;

    // List of all the rooms in the scene (filled at runtime)
    private List<Room> _rooms = new List<Room>();

    [Header("Room Values")]

    [Tooltip("The current active room (with the player in it)")]
    [SerializeField] private Room _activeRoom;

    #endregion

    #region Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            // Start by finding all rooms
            foreach (GameObject room in GameObject.FindGameObjectsWithTag("Room"))
            {
                _rooms.Add(room.GetComponent<Room>());
            }

            if (_rooms.Count == 0)
            {
                Debug.LogWarning("Your scene is missing rooms - spawning will not work properly!");
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Setter for the active room (with the player in it)
    /// </summary>
    /// <param name="room">Reference to the active room</param>
    public void SetActiveRoom(Room room)
    {
        if (_activeRoom != null)
        {
            Vector2 backlog = _activeRoom.Shutdown();

            Segment backlogSegment = new Segment();
            backlogSegment._orders = new List<Order>();
            
            if (backlog.x != 0)
            {
                backlogSegment._orders.Add(CreateOrder(Order.EnemyTypes.Soldier, (int)backlog.x));
            }

            if (backlog.y != 0)
            {
                backlogSegment._orders.Add(CreateOrder(Order.EnemyTypes.Swarm, (int)backlog.y));
            }

            AcceptSegment(backlogSegment, 1);
        }

        _activeRoom = room;
    }

    /// <summary>
    /// Takes a segment and gives the spawning to the current active room
    /// </summary>
    /// <param name="segment">The current segment of the round</param>
    public void AcceptSegment(Segment segment, int multiplier)
    {
        foreach (Order order in segment._orders)
        {
            if (_activeRoom.SmartSpawn(order, multiplier))
            {
                continue;
            }

            bool roomFound = false;

            foreach (Room room in _rooms)
            {
                if (room.SmartSpawn(order, multiplier))
                {
                    roomFound = true;
                    break;
                }
            }

            if (!roomFound)
            {
                Debug.LogWarning("No rooms available to fulfill this segment's order!");
            }
        }
    }

    /// <summary>
    /// Fixes a certain amount of windows
    /// </summary>
    /// <param name="fixAll">Should all of the windows be fixed?</param>
    /// <param name="num">The number of windows to be fixed</param>
    public void FixWindows()
    {
        foreach (Room room in _rooms)
        {
            room.FixWindows();
        }
    }

    /// <summary>
    /// Respawns a single enemy
    /// </summary>
    /// <param name="enemyType">The enemy type to be respawned</param>
    public void RespawnEnemy(Order.EnemyTypes enemyType)
    {
        RoundManager._instance._totalEnemiesSpawned--;

        Segment backlogSegment = new Segment();
        backlogSegment._orders = new List<Order>();

        backlogSegment._orders.Add(CreateOrder(enemyType, 1));

        AcceptSegment(backlogSegment, 1);
    }

    /// <summary>
    /// Creates a new order from a type and amount
    /// </summary>
    /// <param name="enemyType">The type of enemy</param>
    /// <param name="amount">The amount of the enemy</param>
    /// <returns></returns>
    public Order CreateOrder(Order.EnemyTypes enemyType, int amount)
    {
        Order newOrder = new Order();
        newOrder._enemy = enemyType;
        newOrder._enemyAmount = amount;

        return newOrder;
    }

    #endregion
}
