using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public enum UIType
    {
        Game,
        Pause,
        End,
        Shop,
        Settings
    }

    // The current UI in effect
    private UIType _currentUI = UIType.Game;

    [Header("Parent References")]

    [Tooltip("The reference to the game UI parent")]
    [SerializeField] private GameObject _gameUIParent;

    [Tooltip("The reference to the pause UI parent")]
    [SerializeField] private GameObject _pauseUIParent;

    [Tooltip("The reference to the settings UI parent")]
    [SerializeField] private GameObject _settingsUIParent;

    [Header("Game References")]

    [Tooltip("The reference to the ammo counter text")]
    [SerializeField] private TextMeshProUGUI _ammoCounter;

    [Tooltip("The reference to the health counter text")]
    [SerializeField] private TextMeshProUGUI _healthCounter;

    [Tooltip("The reference to the current round text")]
    [SerializeField] private TextMeshProUGUI _currentRound;

    [Tooltip("The reference to the objective text")]
    [SerializeField] private TextMeshProUGUI _objectiveText;

    [Tooltip("The reference to the world space canvas")]
    public Canvas _worldSpaceCanvas;

    [Header("Pause References")]

    [Tooltip("The reference to the continue button")]
    [SerializeField] private GameObject _continueButton;

    [Tooltip("The reference to the settings button")]
    [SerializeField] private GameObject _settingsButton;

    [Header("Settings References")]


    [Tooltip("The reference to the sensitivity slider")]
    [SerializeField] private Slider _sensitivitySlider;

    [Tooltip("The reference to the aim sensitivity slider")]
    [SerializeField] private Slider _aimSensitivitySlider;

    // General References
    // Reference to the player gameobject
    private PlayerController _controller;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        _controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        if (_controller == null)
        {
            Debug.LogWarning("Missing a player in the scene!");
        }
    }

    /// <summary>
    /// Updates the ammo counter text
    /// </summary>
    /// <param name="bulletsLeft">How many bullets left in current mag</param>
    /// <param name="magSize">The total size of the mag</param>
    public void UpdateAmmoText(int bulletsLeft, int magSize)
    {
        if (_ammoCounter != null)
        {
            //_ammoCounter.SetText(bulletsLeft + " / " + magSize);
            _ammoCounter.SetText($"{bulletsLeft}");
        }
    }

    /// <summary>
    /// Updates current health of player
    /// </summary>
    /// <param name="currentHealth">The current player health to be displayed</param>
    public void UpdateHealthText(float currentHealth)
    {
        if (_healthCounter != null)
        {
            _healthCounter.SetText($"{currentHealth}");
        }
    }

    /// <summary>
    /// Updates the round UI
    /// </summary>
    /// <param name="currRound">What number round is it</param>
    /// <param name="roundName">The name of the current round</param>
    public void UpdateRound(int currRound, string roundName, int loop)
    {
        if (_currentRound != null)
        {
            if (currRound == -1)
            {
                _currentRound.SetText("Incoming hostiles...");
            }
            else
            {
                _currentRound.SetText($"Round {currRound}");
            }
        }

        if (_objectiveText != null)
        {
            _objectiveText.SetText($"{roundName} {ToRoman(loop)}");
        }
    }

    public string ToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) return "";
        if (number < 1) return string.Empty;
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);

        return "";
    }

    public void ToggleOnScreenUI(UIType newType)
    {
        // Toggle off all prior UI
        _gameUIParent.SetActive(false);
        _pauseUIParent.SetActive(false);
        _settingsUIParent.SetActive(false);

        _currentUI = newType;

        switch (newType)
        {
            case UIType.Game:
                _gameUIParent.SetActive(true);
                break;

            case UIType.Pause:
                _continueButton.SetActive(true);
                _settingsButton.SetActive(true);
                _pauseUIParent.SetActive(true);
                break;

            case UIType.Settings:
                _settingsUIParent.SetActive(true);
                break;

            case UIType.End:
                _continueButton.SetActive(false);
                _settingsButton.SetActive(false);
                _pauseUIParent.SetActive(true);
                break;

            case UIType.Shop:
                break;
        }
    }

    /// <summary>
    /// Continues the game after unpausing
    /// </summary>
    public void Continue()
    {
        ToggleOnScreenUI(UIType.Game);
        GameManager.instance.Unpause();
    }

    /// <summary>
    /// Opens the settings menu from the pause screen
    /// </summary>
    public void Settings()
    {
        ToggleOnScreenUI(UIType.Settings);
    }

    public void ReturnToPause()
    {
        ToggleOnScreenUI(UIType.Pause);
    }

    public void SensitivitySliderInput()
    {
        _controller.ChangeSensitivity(_sensitivitySlider.value);
    }

    public void AimSensitivitySliderInput()
    {
        _controller.ChangeSensitivity(_aimSensitivitySlider.value);
    }

}
