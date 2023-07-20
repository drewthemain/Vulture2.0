using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartSpawner : MonoBehaviour
{
    #region Variables

    [Tooltip("The types of enemies to spawn")]
    public Order.EnemyTypes _enemyType;

    [Tooltip("The amount of time inbetween spawns")]
    [SerializeField] private float _maxSpawnBuffer = 3;

    [Header("References")]

    [Tooltip("The current pool of enemies")]
    [SerializeField] private List<GameObject> _enemies;

    // Reference to the Room this belongs to
    private Room _parentRoom;

    // The current number of enemies waiting to be spawned
    private int _orderAmount = 0;

    // The time inbetween spawns
    private float _spawnBuffer = 0;

    // The timer for the spawn buffer
    private float _spawnTimer = 0;

    #endregion

    #region Methods

    private void Awake()
    {
        if (GetComponentInParent<Room>())
        {
            _parentRoom = GetComponentInParent<Room>();
        }
        else
        {
            Debug.LogWarning($"Smart Spawner {transform.name} is not a proper child of a room!");
        }

        _spawnBuffer = Random.Range(_maxSpawnBuffer / 2, _maxSpawnBuffer);
    }

    private void Update()
    {
        if (_orderAmount > 0)
        {
            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= _spawnBuffer)
            {
                _spawnTimer = 0;
                _spawnBuffer = Random.Range(_maxSpawnBuffer / 2, _maxSpawnBuffer);

                if (RoundManager._instance.CanSpawn())
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

        RoundManager._instance.RecordEnemySpawn();

        _orderAmount -= 1;
    }

    /// <summary>
    /// Selects the enemy prefab based on the type
    /// </summary>
    /// <returns>Prefab</returns>
    public GameObject SelectEnemy()
    {
        switch (_enemyType)
        {
            case Order.EnemyTypes.Soldier:

                return _enemies[0];

            case Order.EnemyTypes.Swarm:

                return _enemies[1];

        }

        return null;
    }

    /// <summary>
    /// Adds numbers onto the order pile
    /// </summary>
    /// <param name="amount">The amount of enemies to be added to the queue</param>
    public void AcceptOrder(int amount)
    {
        _orderAmount += amount;
    }

    public int ClearOrderRemaining()
    {
        int remaining = _orderAmount;
        _orderAmount = 0;
        return remaining;
    }

    #endregion
}
