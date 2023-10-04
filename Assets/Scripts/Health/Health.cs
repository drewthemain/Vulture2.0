using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    #region Variables

    [Header("Values")]

    [Tooltip("The maximum amount of health")]
    [SerializeField] protected float _maxHealth = 100f;

    [Tooltip("The maximum amount of health")]
    public float _currentHealth = 100f;

    protected float _currentMaxHealth = 100f;

    #endregion

    #region Methods

    private void Start()
    {
        _currentMaxHealth = _maxHealth;
    }

    /// <summary>
    /// Subtract damage from health and check for death
    /// </summary>
    /// <param name="dmg">The amount of damage done</param>
    public virtual void TakeDamage(float dmg, float multiplier=1)
    {
        _currentHealth -= dmg * multiplier;

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals between 0 and maximum health
    /// </summary>
    /// <param name="heal">The amount of healing done</param>
    protected virtual void Heal(float heal)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + heal, 0f, _currentMaxHealth);
    }

    /// <summary>
    /// Increases max health and heals
    /// </summary>
    /// <param name="increase">The amount of HP to increase by</param>
    public virtual void IncreaseMax(float increase)
    {
        _currentMaxHealth += increase;

        _currentHealth = _currentMaxHealth;
    }

    /// <summary>
    /// Increases max health and heals
    /// </summary>
    /// <param name="increase">The amount of HP to increase by</param>
    public virtual void ResetMax()
    {
        _currentMaxHealth = _maxHealth;

        _currentHealth = _maxHealth;
    }

    /// <summary>
    /// Function called upon death
    /// </summary>
    protected virtual void Die()
    {
    }

    /// <summary>
    /// Returns the current max health
    /// </summary>
    public virtual float GetMaxHealth()
    {
        return _currentMaxHealth;
    }

    #endregion
}
