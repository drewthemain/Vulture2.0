using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SmartSpawner : MonoBehaviour
{
    #region Variables

    [Tooltip("Whether this spawner will spawn grounded enemies or not")]
    public bool isGroundSpawner;

    [Tooltip("The amount of time inbetween spawns")]
    [SerializeField] private float maxSpawnBuffer = 3;

    // TO DO
    private Transform eggParent;
    private List<GameObject> eggSpawns = new List<GameObject>();

    // Reference to the Room this belongs to
    private Room parentRoom;

    // The current number of enemies waiting to be spawned
    private Dictionary<Order.EnemyTypes, int> orders = new Dictionary<Order.EnemyTypes, int>();

    // The time inbetween spawns
    private float spawnBuffer = 0;

    // The timer for the spawn buffer
    private float spawnTimer = 0;

    #endregion

    #region Methods

    private void Awake()
    {
        if (GetComponentInParent<Room>())
        {
            parentRoom = GetComponentInParent<Room>();
        }
        else
        {
            Debug.LogWarning($"Smart Spawner {transform.name} is not a proper child of a room!");
        }

        spawnBuffer = Random.Range(maxSpawnBuffer / 2, maxSpawnBuffer);

        if (transform.GetChild(0))
        {
            eggParent = transform.GetChild(0);

            foreach(Transform child in eggParent.transform)
            {
                eggSpawns.Add(child.gameObject);
            }
        }
    }

    private void Update()
    {
        if (orders.Count > 0)
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnBuffer)
            {
                spawnTimer = 0;
                spawnBuffer = Random.Range(maxSpawnBuffer / 2, maxSpawnBuffer);

                if (RoundManager.instance.CanSpawn())
                {
                    Spawn();
                }
            }
        }
    }

    /// <summary>
    /// Spawns a random enemy from the pool
    /// </summary>
    public void Spawn()
    {
        Order.EnemyTypes[] availableTypes = orders.Keys.ToArray();
        Order.EnemyTypes chosenType = availableTypes[Random.Range(0, availableTypes.Length)];

        GameObject enemyPrefab = EnemyManager.instance.GetPrefabByEnemyType(chosenType);

        if (!enemyPrefab)
        {
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        newEnemy.transform.parent = this.transform;

        RoundManager.instance.RecordEnemySpawn();

        int currentCount;
        orders.TryGetValue(chosenType, out currentCount);
        orders[chosenType] = currentCount - 1;

        if (orders[chosenType] <= 0)
        {
            orders.Remove(chosenType);
        }
    }

    /// <summary>
    /// Adds numbers onto the order pile
    /// </summary>
    /// <param name="amount">The amount of enemies to be added to the queue</param>
    public void AcceptOrder(Order.EnemyTypes enemyType, int amount)
    {
        int currentCount;
        orders.TryGetValue(enemyType, out currentCount);
        orders[enemyType] = currentCount + 1;
    }

    /// <summary>
    /// Clears the current order remaining for a reset
    /// </summary>
    /// <returns>Number of remaining enemies in the order</returns>
    public Dictionary<Order.EnemyTypes, int> ClearOrderRemaining()
    {
        Dictionary<Order.EnemyTypes, int> copy = new Dictionary<Order.EnemyTypes, int>(orders);
        orders.Clear();
        return copy;
    }

    #endregion
}
