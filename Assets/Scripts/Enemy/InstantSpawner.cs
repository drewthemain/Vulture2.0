using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantSpawner : MonoBehaviour
{
    #region Variables

    [Header("References")]

    [Tooltip("The types of enemies to spawn")]
    [SerializeField] private List<GameObject> enemies;

    [Tooltip("The delay between new enemies being spawned")]
    [SerializeField] private float spawnDelay = 5f;

    #endregion

    #region Methods

    private void Start()
    {
        if (enemies.Count == 0)
        {
            Debug.LogWarning($"Spawner {transform.name} is missing enemies to spawn!");
            return;
        }

        // Currently spawn based on a timer
        // WIP WILL BECOME A SMARTER, ROOMBASED SYSTEM
        InvokeRepeating("Spawn", Random.Range(spawnDelay / 2, spawnDelay + spawnDelay / 2), spawnDelay);
    }

    /// <summary>
    /// Spawns a random enemy from the pool
    /// </summary>
    private void Spawn()
    {
        int index = Random.Range(0, enemies.Count);

        GameObject newEnemy = Instantiate(enemies[index], transform.position, Quaternion.identity);
        newEnemy.transform.parent = this.transform;
    }

    #endregion
}
