using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    #region Variables

    public static EnemyManager instance;

    [Header("References")]

    [Tooltip("The list of enemy prefabs to choose from")]
    [SerializeField] private List<GameObject> enemies;

    #endregion

    #region Methods

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public GameObject GetPrefabByEnemyType(Order.EnemyTypes enemyType)
    {
        switch (enemyType)
        {
            case Order.EnemyTypes.Soldier:
                return enemies[0];
            case Order.EnemyTypes.Swarm:
                return enemies[1];
            case Order.EnemyTypes.Splicer:
                return enemies[2];
            default:
                Debug.LogWarning("Invalid enemy type passed to EnemyManager");
                break;
        }

        return null;
    }

    #endregion
}
