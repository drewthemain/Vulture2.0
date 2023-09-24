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
        Settings,
        Main
    }

    // The current UI in effect
    private UIType _currentUI = UIType.Game;

    [Header("Parent References")]

    [Tooltip("The reference to the game UI parent")]
    [SerializeField] private GameObject _gameUIParent;

    [Tooltip("The reference to the pause UI parent")]
    [SerializeField] private GameObject _pauseUIParent;

    [Tooltip("The reference to the lose game UI parent")]
    [SerializeField] private GameObject _loseUIParent;

    [Tooltip("The reference to the settings UI parent")]
    [SerializeField] private GameObject _settingsUIParent;

    [Header("Game References")]

    [Tooltip("The reference to the ammo counter text")]
    [SerializeField] private TextMeshProUGUI _ammoCounter;

    [Tooltip("The reference to the ammo slider")]
    [SerializeField] private Slider _ammoSlider;

    [Tooltip("The reference to the health counter text")]
    [SerializeField] private TextMeshProUGUI _healthCounter;

    [Tooltip("The reference to the health slider")]
    [SerializeField] private Slider _healthSlider;

    [Tooltip("The reference to the current round text")]
    [SerializeField] private TextMeshProUGUI _currentRound;

    [Tooltip("The reference to the objective text")]
    [SerializeField] private TextMeshProUGUI _objectiveText;

    [Tooltip("The reference to the warning banner")]
    [SerializeField] private GameObject _warningBanner;

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

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI _sensitivityText;

    [Tooltip("The reference to the aim sensitivity slider")]
    [SerializeField] private Slider _aimSensitivitySlider;

    [Header("Crosshair References")]
    [Tooltip("The reference to the continue button")]
    [SerializeField] private Canvas _crosshairCanvas;

    [Tooltip("The reference to the settings button")]
    [SerializeField] private GameObject _crosshair;

    [Tooltip("The reference to the crosshair canvas position transform")]
    [SerializeField] private Transform _crosshairCanvasPosition;

    // General References
    // Reference to the player gameobject
    private PlayerController _controller;
    // Reference to the player's input actions
    private InputManager input;
    //Reference to animator on pause menu
    private Animator _PauseAnim;
    //Reference to animator on settings menu
    private Animator _settingsAnim;

    // UI Animation References
    private float animDuration = 1 / 6f;


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
        _PauseAnim = _pauseUIParent.GetComponent<Animator>();
        _settingsAnim = _settingsUIParent.GetComponent<Animator>();
        input = InputManager.instance;

        if (_controller == null)
        {
            Debug.LogWarning("Missing a player in the scene!");
        }
    }

    private void Start()
    {
        LoadSettings();
    }

    private void Update()
    {
        if (InputManager.instance.PlayerPressedEscape())
        {
            if (_currentUI == UIType.Game || _currentUI == UIType.Pause)
            {
                PauseGame();
            }
            else if (_currentUI == UIType.Settings)
            {
                ExitSettings();
            }
            
        }
    }

    /// <summary>
    /// Updates the ammo counter text
    /// </summary>
    /// <param name="bulletsLeft">How many bullets left in current mag</param>
    /// <param name="magSize">The total size of the mag</param>
    public void UpdateAmmo(int bulletsLeft, int magSize)
    {
        if (_ammoCounter != null)
        {
            //_ammoCounter.SetText(bulletsLeft + " / " + magSize);
            _ammoCounter.SetText($"{bulletsLeft}");
        }
        if (_ammoSlider != null)
        {
            _ammoSlider.value = ((float)bulletsLeft/(float)magSize);
        }
    }

    /// <summary>
    /// Updates current health of player
    /// </summary>
    /// <param name="currentHealth">The current player health to be displayed</param>
    public void UpdateHealth(float currentHealth)
    {
        if (_healthCounter != null)
        {
            _healthCounter.SetText($"{currentHealth}");
        }
        if (_healthSlider != null)
        {
            _healthSlider.value = currentHealth / _controller.GetComponent<PlayerHealth>().GetMaxHealth();
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

    public void ToggleWarningBanner(bool activeStatus = false)
    {
        _warningBanner.SetActive(activeStatus);
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
                _currentUI = UIType.Game;
                _gameUIParent.SetActive(true);
                break;

            case UIType.Pause:
                _currentUI = UIType.Pause;
                _continueButton.SetActive(true);
                _settingsButton.SetActive(true);
                _pauseUIParent.SetActive(true);
                break;

            case UIType.Settings:
                _currentUI = UIType.Settings;
                _settingsUIParent.SetActive(true);
                break;

            case UIType.End:
                _currentUI = UIType.End;
                _continueButton.SetActive(false);
                _settingsButton.SetActive(false);
                _pauseUIParent.SetActive(false);
                _loseUIParent.SetActive(true);
                break;

            case UIType.Shop:
                _currentUI = UIType.Shop;
                break;
        }
    }

    /// <summary>
    /// Continues the game after unpausing
    /// </summary>
    public void Continue()
    {
        PauseGame();
    }

    /// <summary>
    /// Opens the settings menu from the pause screen
    /// </summary>
    public void Settings()
    {
        StartCoroutine(ToSettingsCoroutine());
    }

    /// <summary>
    /// Allows the previous menu to finish animating before moving to Settings
    /// </summary>
    private IEnumerator ToSettingsCoroutine()
    {
        yield return new WaitForSecondsRealtime(animDuration);

        ToggleOnScreenUI(UIType.Settings);
    }

    /// <summary>
    /// Toggles the pausing of the game
    /// </summary>
    public void PauseGame()
    {
        // pause
        if (_currentUI == UIType.Game)
        {
            ToggleOnScreenUI(UIType.Pause);
            GameManager.instance.Pause();
        }
        // unpause
        else if (_currentUI == UIType.Pause)
        {
            _PauseAnim.SetTrigger("Continue");
            StartCoroutine(PauseToGameCoroutine());
            
            //ToggleOnScreenUI(UIType.Game);
            //GameManager.instance.Unpause;
        }
        // return to pause
        else
        {
            ReturnToPause();
        }

    }

    /// <summary>
    /// Allows the Pause menu to finish animating before unpausing
    /// </summary>
    private IEnumerator PauseToGameCoroutine()
    {
        yield return new WaitForSecondsRealtime(animDuration);

        ToggleOnScreenUI(UIType.Game);
        GameManager.instance.Unpause();
    }

    /// <summary>
    /// Go from settings to either the main menu or the pause menu
    /// </summary>
    public void ExitSettings()
    {
        _settingsAnim.SetTrigger("EndSettings");
        StartCoroutine(ExitSettingsCoroutine());
    }

    /// <summary>
    /// Allows the Settings menu to continue animating before moving to pause or main menu
    /// </summary>
    private IEnumerator ExitSettingsCoroutine()
    {
        yield return new WaitForSecondsRealtime(animDuration);

        //ADD LOGIC HERE TO DETERMINE WHERE TO GO

        ReturnToPause();
        _PauseAnim.SetTrigger("FromSettings");
    }

    /// <summary>
    /// Return to the pause menu.
    /// </summary>
    public void ReturnToPause()
    {
        ToggleOnScreenUI(UIType.Pause);
    }

    /// <summary>
    /// UI portion of adjusting the players overall sensitivity
    /// Passes the input slider value through to the player controller where it adjusts the camera sensitivity
    /// </summary>
    public void SensitivitySliderInput()
    {
        float sens = _sensitivitySlider.value;
        if(_sensitivitySlider.value <= 0.01f)
        {
            sens = 0.01f;
        }

        _sensitivityText.text = (sens * 10).ToString("F2");
        _controller.ChangeSensitivity(sens);
    }

    /// <summary>
    /// Not yet implemented
    /// </summary>
    public void AimSensitivitySliderInput()
    {
        _controller.ChangeSensitivity(_aimSensitivitySlider.value);
    }

    public void LoadSettings()
    {
        // load in sensitivity slider input
        _sensitivitySlider.value = (PlayerPrefs.GetFloat("sensitivity", .5f));
        SensitivitySliderInput();
        
    }

    /// <summary>
    /// Switch the crosshair to display on the gun rather than on the camera
    /// </summary>
    public void EnableWorldspaceCrosshair()
    {
        _crosshairCanvas.renderMode = RenderMode.WorldSpace;
        Vector3 pos = _crosshairCanvasPosition.localPosition;
        _crosshairCanvas.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, pos.y, pos.z);
    }

    /// <summary>
    /// Switch the crosshair to display on the screenspace rather than on the gun
    /// </summary>
    public void EnableScreenCamCrosshair()
    {
        _crosshairCanvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

}
