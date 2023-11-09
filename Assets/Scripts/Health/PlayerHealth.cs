using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Damage Overlay")]
    [Tooltip("The damage overlay GameObject")]
    public Image overlay;
    [Tooltip("How long the image stays fully opaque")]
    public float duration;
    [Tooltip("How quickly the image will fade")]
    public float fadeSpeed;
    [Tooltip("The multiplier for the blood overlay alpha (alpha = damage * multiplier)")]
    [SerializeField] private float bloodMultiplier;
    // timer to check against the duration
    private float durationTimer; 

    // status for if the player is currently in combat
    private bool inCombat;
    // Should the player take damage?
    public bool godMode;

    // References
    private PlayerController controller;
    // Current alpha level of the blood overlay
    private float currentBloodAlpha;

    #endregion

    #region Methods

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);
    }

    void Update()
    {
        if (overlay.color.a > 0)
        {
            durationTimer += Time.deltaTime;
            if (durationTimer > duration)
            {
                // fade the image
                currentBloodAlpha -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, currentBloodAlpha);
            }
        }
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
        if (godMode)
        {
            return;
        }

        base.TakeDamage(dmg, multiplier);

        if (speedDecreaseOnDamage != 0 && controller)
        {
            controller.speedMultiplier -= speedDecreaseOnDamage;
        }

        if (speedIncreaseOnDamage != 0 && controller)
        {
            controller.speedMultiplier += speedIncreaseOnDamage;
        }

        UpdateBloodOverlay(dmg);


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

    /// <summary>
    /// Update the blood overlay based on the damage taken
    /// </summary>
    /// <param name="damage">The amount of damage from an attack</param>
    private void UpdateBloodOverlay(float damage)
    {
        float newAlpha = currentBloodAlpha + (damage / 100) * bloodMultiplier;

        if (newAlpha < 1)
        {
            currentBloodAlpha = newAlpha;
        }
        else if (newAlpha >= 1)
        {
            currentBloodAlpha = 1;
        }
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, currentBloodAlpha);
        durationTimer = 0;
    }
    #endregion
}
