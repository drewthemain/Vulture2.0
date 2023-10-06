using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSpawner : MonoBehaviour
{
    #region Variables

    [Tooltip("The types of enemies to spawn")]
    public Order.EnemyTypes enemyType;

    [Tooltip("The amount of time inbetween spawns")]
    [SerializeField] private float maxSpawnBuffer = 3;

    [Header("References")]

    [Tooltip("The current pool of enemies")]
    [SerializeField] private List<GameObject> enemies;

    private Transform eggParent;

    private List<GameObject> eggSpawns = new List<GameObject>();

    // Reference to the Room this belongs to
    private Room parentRoom;

    // The current number of enemies waiting to be spawned
    private int orderAmount = 0;

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
        if (orderAmount > 0)
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
        GameObject newEnemy = Instantiate(SelectEnemy(), transform.position, Quaternion.identity);
        newEnemy.transform.parent = this.transform;

        RoundManager.instance.RecordEnemySpawn();

        orderAmount -= 1;
    }

    /// <summary>
    /// Selects the enemy prefab based on the type
    /// </summary>
    /// <returns>Prefab</returns>
    public GameObject SelectEnemy()
    {
        switch (enemyType)
        {
            case Order.EnemyTypes.Soldier:

                return enemies[0];

            case Order.EnemyTypes.Swarm:

                return enemies[1];

        }

        return null;
    }

    /// <summary>
    /// Adds numbers onto the order pile
    /// </summary>
    /// <param name="amount">The amount of enemies to be added to the queue</param>
    public void AcceptOrder(int amount)
    {
        orderAmount += amount;
    }

    /// <summary>
    /// Clears the current order remaining for a reset
    /// </summary>
    /// <returns>Number of remaining enemies in the order</returns>
    public int ClearOrderRemaining()
    {
        int remaining = orderAmount;
        orderAmount = 0;
        return remaining;
    }

    #endregion
}
