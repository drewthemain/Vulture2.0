using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    #region Variables

    [Header("Collider Variables")]

    [Tooltip("The prefab that will be spawned as a projectile")]
    [SerializeField] protected GameObject colliderPrefab;

    [Tooltip("The damage done by this collider")]
    [SerializeField] protected float colliderDamage = 5;

    [Tooltip("The delay between attacking")]
    [SerializeField] protected float attackDelay = 1;

    [Tooltip("The offset for bullet instantiation, X being forward/backward, Y being up/down")]
    [SerializeField] protected Vector2 colliderOffset = Vector2.zero;

    // Is the weapon currently firing?
    private bool isFiring = false;

    // The current delay timer between firing
    private float currentDelay = 0;

    // Reference to the player transform
    protected Transform playerReference;

    #endregion

    #region Methods

    protected virtual void Start()
    {
        // Player reference needed for aiming matters, can be changed later if needed
        playerReference = GameManager.instance.GetPlayerReference();
        currentDelay = attackDelay;
    }

    protected virtual void Update()
    {
        if (isFiring)
        {
            currentDelay += Time.deltaTime;

            if (currentDelay >= attackDelay)
            {
                currentDelay = 0;
                Fire();
            }
        }
    }

    /// <summary>
    /// Toggles firing state on and off
    /// </summary>
    /// <param name="shouldFire">True if this enemy should be attacking</param>
    public virtual void ToggleFiring(bool shouldFire)
    {
        isFiring = shouldFire;
    }

    public bool IsFiring()
    {
        return isFiring;
    }

    /// <summary>
    /// Handles logic for creating, setup, aiming, and firing of a bullet
    /// </summary>
    protected virtual void Fire()
    {
    }

    #endregion
}
