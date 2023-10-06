using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{

    #region Variables

    [Header("Options")]
    [SerializeField] private float timeTillDeath = 1f;

    [Tooltip("The color for a lesser multiplier")]
    [SerializeField] private Color lesserMultiplier;

    [Tooltip("The color for an equal multiplier")]
    [SerializeField] private Color equalMultiplier;

    [Tooltip("The color for a greater multiplier")]
    [SerializeField] private Color greaterMultiplier;

    [Tooltip("The color for the greatest weakpoint")]
    [SerializeField] private Color weakPointMultiplier;

    // Reference to the damage text
    private TextMeshProUGUI damageText;

    // Has the number been setup yet?
    private bool initialized = false;

    // Internal timer for movement and deletion
    private float timer = 0;

    #endregion

    #region Methods

    /// <summary>
    /// Initializes number with values and references
    /// </summary>
    /// <param name="damageAmount"></param>
    public void Initialize(float damageAmount, float multiplier)
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(Vector3.up * 180);

        damageText = GetComponent<TextMeshProUGUI>();

        if (damageText)
        {
            damageText.SetText($"{damageAmount * multiplier}");

            if (multiplier >= 1.5 || multiplier == -1)
            {
                damageText.color = weakPointMultiplier;
                transform.localScale *= 1.2f;

                if (multiplier == -1)
                {
                    timeTillDeath *= 2;
                    transform.localScale *= 50;

                    damageText.SetText($"{damageAmount}");
                }
            }
            else if (multiplier > 1)
            {
                damageText.color = greaterMultiplier;
            }
            else if (multiplier == 1)
            {
                damageText.color = equalMultiplier;
            }
            else
            {
                damageText.color = lesserMultiplier;
            }
        }

        initialized = true;
    }

    private void Update()
    {
        if (initialized)
        {
            // Lower number position and scale over time
            if (timer < timeTillDeath)
            {
                transform.position -= Vector3.up * Time.deltaTime;
                transform.localScale -= transform.localScale * (Time.deltaTime * 2);

                timer += Time.deltaTime;

                return;
            }

            Destroy(this.gameObject);
        }
    }

    #endregion
}
