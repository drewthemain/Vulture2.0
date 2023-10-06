using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Health
{
    #region Variables
    [Header("Passive Healing")]
    [Tooltip("The amount that the heal over time will increment by when passively healing")]
    [SerializeField] private float passiveHealingIncrement;
    [Tooltip("The delay between passive healing increments (in seconds)")]
    [SerializeField] private float passiveHealTickDelay;
    [Tooltip("How long the player waits to heal after taking damage (in seconds)")]
    [SerializeField] private float inCombatTime;

    [Header("Round End Healing")]
    [Tooltip("How long the player waits to heal after taking damage (in seconds)")]
    [SerializeField] private float roundEndHealIncrement;
    [Tooltip("How long the player waits to heal after taking damage (in seconds)")]
    [SerializeField] private float roundEndHealDelay;

    [Header("Camera Shake")]
    [Tooltip("Intensity of the camera shake for taking damage")]
    [SerializeField] private float onDamageShakeIntensity;
    [Tooltip("Duration of the camera shake for taking damage")]
    [SerializeField] private float onDamageShakeDuration;

    // status for if the player is currently in combat
    private bool inCombat;

    // References

    private PlayerController controller;

    #endregion

    #region Methods

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    protected override void Die()
    {
        base.Die();

        if (GameManager.instance)
        {
            GameManager.instance.LoseGame();
        }
    }

    public override void TakeDamage(float dmg, float multiplier)
    {
        base.TakeDamage(dmg, multiplier);

        if (speedDecreaseOnDamage != 0 && controller)
        {
            Debug.Log(speedDecreaseOnDamage);
            controller.speedMultiplier -= speedDecreaseOnDamage;
        }

        CameraManager.instance.CameraShake(onDamageShakeIntensity, onDamageShakeDuration);
        UIManager.instance.UpdateHealth(currentHealth);
        EnterCombat();
    }

    protected override void Heal(float heal)
    {
        base.Heal(heal);
        UIManager.instance.UpdateHealth(currentHealth);
    }

    public override float GetMaxHealth()
    {
        return base.GetMaxHealth();
    }

    /// <summary>
    /// Start the healing delay to account for being in combat
    /// End healing / restart the combat timer when entering or reentering combat
    /// </summary>
    private void EnterCombat()
    {
        inCombat = true;
        StopAllCoroutines();
        StartCoroutine(InCombatDelay());
    }

    /// <summary>
    /// Exit the combat healing delay and begin slowly healing up to 50% of the max health
    /// </summary>
    private void ExitCombat()
    {
        inCombat = false;
        StartCoroutine(LerpHealth(passiveHealingIncrement, passiveHealTickDelay, (maxHealth / 2)));
    }

    /// <summary>
    /// Begin the coroutine for healing the player health after the round ends
    /// Goes to the maximum health instead of the standard half
    /// </summary>
    public void EndOfTurnHeal()
    {
        StartCoroutine(LerpHealth(roundEndHealIncrement, roundEndHealDelay, maxHealth));
    }

    /// <summary>
    /// Coroutine for the delay after taking damage to account for being in combat
    /// </summary>
    IEnumerator InCombatDelay()
    {
        yield return new WaitForSeconds(inCombatTime);
        inCombat = false;
        ExitCombat();
    }

    /// <summary>
    /// Coroutine for the slowly increasing the health by a rate of one (increment / delay) up to a maximum value
    /// </summary>
    IEnumerator LerpHealth(float increment, float delay, float maxHealthLimit)
    {
        while (currentHealth < maxHealthLimit)
        {
            Heal(increment);
            yield return new WaitForSeconds(delay);
        }
        currentHealth = maxHealthLimit;
    }

    #endregion
}
