using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{

    #region Variables

    [Header("Options")]
    [SerializeField] private float _timeTillDeath = 1f;

    [Tooltip("The color for a lesser multiplier")]
    [SerializeField] private Color _lesserMultiplier;

    [Tooltip("The color for an equal multiplier")]
    [SerializeField] private Color _equalMultiplier;

    [Tooltip("The color for a greater multiplier")]
    [SerializeField] private Color _greaterMultiplier;

    // Reference to the damage text
    private TextMeshProUGUI _damageText;

    // Has the number been setup yet?
    private bool _initialized = false;

    // Internal timer for movement and deletion
    private float _timer = 0;

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

        _damageText = GetComponent<TextMeshProUGUI>();

        if (_damageText)
        {
            _damageText.SetText($"{damageAmount * multiplier}");

            if (multiplier > 1 || multiplier == -1)
            {
                _damageText.color = _greaterMultiplier;

                if (multiplier == -1)
                {
                    _timeTillDeath *= 2;
                    transform.localScale *= 50;

                    _damageText.SetText($"{damageAmount}");
                }
            }
            else if (multiplier == 1)
            {
                _damageText.color = _equalMultiplier;
            }
            else
            {
                _damageText.color = _lesserMultiplier;
            }
        }

        _initialized = true;
    }

    private void Update()
    {
        if (_initialized)
        {
            // Lower number position and scale over time
            if (_timer < _timeTillDeath)
            {
                transform.position -= Vector3.up * Time.deltaTime;
                transform.localScale -= transform.localScale * (Time.deltaTime * 2);

                _timer += Time.deltaTime;

                return;
            }

            Destroy(this.gameObject);
        }
    }

    #endregion
}
