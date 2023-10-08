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

    public enum SettingsMenuType
    {
        Main,
        Accessibility,
        Gameplay,
        Sound,
        Video,
    }

    // The current UI in effect
    private UIType currentUI = UIType.Game;

    // The current UI in effect
    private SettingsMenuType currentSettingsMenu = SettingsMenuType.Main;

    [Header("Parent References")]

    [Tooltip("The reference to the game UI parent")]
    [SerializeField] private GameObject gameUIParent;

    [Tooltip("The reference to the pause UI parent")]
    [SerializeField] private GameObject pauseUIParent;

    [Tooltip("The reference to the lose game UI parent")]
    [SerializeField] private GameObject loseUIParent;

    [Tooltip("The reference to the settings UI parent")]
    [SerializeField] private GameObject settingsUIParent;

    [Tooltip("The UI aspects that are always displayed and can change opacity")]
    [SerializeField] private GameObject GUI;

    [Header("Game References")]

    [Tooltip("The reference to the ammo counter text")]
    [SerializeField] private TextMeshProUGUI ammoCounter;

    [Tooltip("The reference to the ammo slider")]
    [SerializeField] private Slider ammoSlider;

    [Tooltip("The reference to the health counter text")]
    [SerializeField] private TextMeshProUGUI healthCounter;

    [Tooltip("The reference to the health slider")]
    [SerializeField] private Slider healthSlider;

    [Tooltip("The reference to the current round text")]
    [SerializeField] private TextMeshProUGUI currentRound;

    [Tooltip("The reference to the objective text")]
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Tooltip("The reference to the warning banner")]
    [SerializeField] private GameObject warningBanner;

    [Tooltip("The reference to the world space canvas")]
    public Canvas worldSpaceCanvas;

    [Header("Pause References")]

    [Tooltip("The reference to the continue button")]
    [SerializeField] private GameObject continueButton;

    [Tooltip("The reference to the settings button")]
    [SerializeField] private GameObject settingsButton;

    // General References
    // Reference to the player gameobject
    private PlayerController controller;
    // Reference to the player's input actions
    private InputManager input;
    //Reference to animator on pause menu
    private Animator PauseAnim;
    //Reference to animator on settings menu
    private Animator settingsAnim;

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

        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        PauseAnim = pauseUIParent.GetComponent<Animator>();
        settingsAnim = settingsUIParent.GetComponent<Animator>();
        input = InputManager.instance;

        if (controller == null)
        {
            Debug.LogWarning("Missing a player in the scene!");
        }
    }

    private void Update()
    {
        if (InputManager.instance.PlayerPressedEscape())
        {
            if (currentUI == UIType.Game || currentUI == UIType.Pause)
            {
                PauseGame();
            }
            else if (currentUI == UIType.Settings)
            {
                if(currentSettingsMenu == SettingsMenuType.Main)
                {
                    ExitSettings();
                }
                else
                {
                    ExitSettingsMenus();
                }
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
        if (ammoCounter != null)
        {
            //ammoCounter.SetText(bulletsLeft + " / " + magSize);
            ammoCounter.SetText($"{bulletsLeft}");
        }
        if (ammoSlider != null)
        {
            ammoSlider.value = ((float)bulletsLeft/(float)magSize);
        }
    }

    /// <summary>
    /// Updates current health of player
    /// </summary>
    /// <param name="currentHealth">The current player health to be displayed</param>
    public void UpdateHealth(float currentHealth)
    {
        if (healthCounter != null)
        {
            healthCounter.SetText($"{currentHealth}");
        }
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / controller.GetComponent<PlayerHealth>().GetMaxHealth();
        }
    }

    /// <summary>
    /// Updates the round UI
    /// </summary>
    /// <param name="currRound">What number round is it</param>
    /// <param name="roundName">The name of the current round</param>
    public void UpdateRound(int currRound, string roundName, int loop)
    {
        if (currentRound != null)
        {
            if (currRound == -1)
            {
                currentRound.SetText("Incoming hostiles...");
            }
            else
            {
                currentRound.SetText($"Round {currRound}");
            }
        }

        if (objectiveText != null)
        {
            objectiveText.SetText($"{roundName} {ToRoman(loop)}");
        }
    }

    public void ToggleWarningBanner(bool activeStatus = false)
    {
        warningBanner.SetActive(activeStatus);
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
        gameUIParent.SetActive(false);
        pauseUIParent.SetActive(false);
        settingsUIParent.SetActive(false);

        currentUI = newType;

        switch (newType)
        {
            case UIType.Game:
                currentUI = UIType.Game;
                gameUIParent.SetActive(true);
                break;

            case UIType.Pause:
                currentUI = UIType.Pause;
                continueButton.SetActive(true);
                settingsButton.SetActive(true);
                pauseUIParent.SetActive(true);
                break;

            case UIType.Settings:
                currentUI = UIType.Settings;
                settingsUIParent.SetActive(true);
                break;

            case UIType.End:
                currentUI = UIType.End;
                continueButton.SetActive(false);
                settingsButton.SetActive(false);
                pauseUIParent.SetActive(false);
                loseUIParent.SetActive(true);
                break;

            case UIType.Shop:
                currentUI = UIType.Shop;
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
        if (currentUI == UIType.Game)
        {
            ToggleOnScreenUI(UIType.Pause);
            GameManager.instance.Pause();
        }
        // unpause
        else if (currentUI == UIType.Pause)
        {
            PauseAnim.SetTrigger("Continue");
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
        settingsAnim.SetTrigger("EndSettings");
        StartCoroutine(ExitSettingsCoroutine());
    }

    /// <summary>
    /// Return to the main settings menu from each of the sub menus
    /// </summary>
    public void ExitSettingsMenus()
    {
        // capitalize first letter for formatting
        settingsAnim.SetTrigger("End" + currentSettingsMenu.ToString());
        StartCoroutine(ExitSettingsMenusCoroutine());
    }

    /// <summary>
    /// Allows the Settings menu to continue animating before moving to pause or main menu
    /// </summary>
    private IEnumerator ExitSettingsCoroutine()
    {
        yield return new WaitForSecondsRealtime(animDuration);

        ReturnToPause();
        PauseAnim.SetTrigger("FromSettings");
    }

    /// <summary>
    /// Allows the Settings sub menus to continue animating before moving to settings main
    /// </summary>
    private IEnumerator ExitSettingsMenusCoroutine()
    {
        yield return new WaitForSecondsRealtime(animDuration);

        ReturnToSettings();
        //settingsAnim.SetTrigger("FromSettingsMenu");
    }

    /// <summary>
    /// Return to the pause menu.
    /// </summary>
    public void ReturnToPause()
    {
        ToggleOnScreenUI(UIType.Pause);
    }

    /// <summary>
    /// Return to the settings menu.
    /// </summary>
    public void ReturnToSettings()
    {
        ToggleOnScreenUI(UIType.Settings);
        UpdateSettingsSubMenuState("Main");
    }

    /// <summary>
    /// Changes the current settings submenu state based on the string entered
    /// </summary>
    /// <param name="subMenuType">The EXACT name of the sub menu type from the enum that you want to switch to</param>
    public void UpdateSettingsSubMenuState(string subMenuType)
    {
        switch(subMenuType)
        {
            case "Video":
                currentSettingsMenu = SettingsMenuType.Video;
                break;
            case "Sound":
                currentSettingsMenu = SettingsMenuType.Sound;
                break;
            case "Gameplay":
                currentSettingsMenu = SettingsMenuType.Gameplay;
                break;
            case "Accessibility":
                currentSettingsMenu = SettingsMenuType.Accessibility;
                break;
            case "Main":
                currentSettingsMenu = SettingsMenuType.Main;
                break;
            default:
                currentSettingsMenu = SettingsMenuType.Main;
                break;
        }
    }

    public void ChangeGUIOpacity(float opacityPercentage)
    {
        PlayerPrefs.SetFloat("GUIopacity", opacityPercentage);
        Image[] GUIElements = GUI.GetComponentsInChildren<Image>();
        foreach(Image img in GUIElements)
        {
            Color imgColor = img.color;
            imgColor.a = opacityPercentage;
            img.color = imgColor;
        }
        TextMeshProUGUI[] textElements = GUI.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (TextMeshProUGUI text in textElements)
        {
            Color textColor = text.color;
            textColor.a = opacityPercentage;
            text.color = textColor;
        }
    }
}
