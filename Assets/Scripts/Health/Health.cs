using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    #region Variables

    [Header("Values")]

    [Tooltip("The maximum amount of health")]
    [SerializeField] protected float maxHealth = 100f;

    [Tooltip("The maximum amount of health")]
    public float currentHealth = 100f;

    protected float currentMaxHealth = 100f;

    #endregion

    #region Methods

    private void Start()
    {
        currentMaxHealth = maxHealth;
    }

    /// <summary>
    /// Subtract damage from health and check for death
    /// </summary>
    /// <param name="dmg">The amount of damage done</param>
    public virtual void TakeDamage(float dmg, float multiplier=1)
    {
        currentHealth -= dmg * multiplier;

        if (currentHealth <= 0f)
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
        currentHealth = Mathf.Clamp(currentHealth + heal, 0f, currentMaxHealth);
    }

    /// <summary>
    /// Increases max health and heals
    /// </summary>
    /// <param name="increase">The amount of HP to increase by</param>
    public virtual void IncreaseMax(float increase)
    {
        currentMaxHealth += increase;

        currentHealth = currentMaxHealth;
    }

    /// <summary>
    /// Increases max health and heals
    /// </summary>
    /// <param name="increase">The amount of HP to increase by</param>
    public virtual void ResetMax()
    {
        currentMaxHealth = maxHealth;

        currentHealth = maxHealth;
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
        return currentMaxHealth;
    }

    #endregion
}
