using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    #region Variables

    [Header("Collider Variables")]

    [Tooltip("The prefab that will be spawned as a projectile")]
    [SerializeField] protected GameObject _colliderPrefab;

    [Tooltip("The damage done by this collider")]
    [SerializeField] protected float _colliderDamage = 5;

    [Tooltip("The delay between attacking")]
    [SerializeField] protected float _attackDelay = 1;

    [Tooltip("The offset for bullet instantiation")]
    [SerializeField] protected float _colliderOffset = 1;

    // Is the weapon currently firing?
    private bool _isFiring = false;

    // The current delay timer between firing
    private float _currentDelay = 0;

    // Reference to the player transform
    private Transform _playerReference;

    #endregion

    #region Methods

    protected virtual void Start()
    {
        // Player reference needed for aiming matters, can be changed later if needed
        _playerReference = GameManager.instance.GetPlayerReference();
        _currentDelay = _attackDelay;
    }

    protected virtual void Update()
    {
        if (_isFiring)
        {
            _currentDelay += Time.deltaTime;

            if (_currentDelay >= _attackDelay)
            {
                _currentDelay = 0;
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
        _isFiring = shouldFire;
    }

    /// <summary>
    /// Handles logic for creating, setup, aiming, and firing of a bullet
    /// </summary>
    protected virtual void Fire()
    {
    }

    #endregion
}
