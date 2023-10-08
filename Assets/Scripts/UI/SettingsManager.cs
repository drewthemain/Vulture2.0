using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Video Settings References")]
    [Tooltip("The reference to the camera shake toggle")]
    [SerializeField] private Toggle cameraShakeToggle;

    [Tooltip("The reference to the textbox next to the toggle displaying the state")]
    [SerializeField] private TextMeshProUGUI cameraShakeText;

    [Tooltip("The UI aspects that are always displayed and can change opacity")]
    [SerializeField] private GameObject GUI;

    [Tooltip("The reference to the GUI opacity slider")]
    [SerializeField] private Slider GUIOpacitySlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI GUIOpacityText;


    [Header("Gameplay Settings References")]
    [Tooltip("The reference to the sensitivity slider")]
    [SerializeField] private Slider sensitivitySlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI sensitivityText;

    [Tooltip("The reference to the aim sensitivity slider")]
    [SerializeField] private Slider aimSensitivitySlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI aimSensitivityText;


    // General References
    // Reference to the player gameobject
    private PlayerController controller;


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

        if (controller == null)
        {
            Debug.LogWarning("Missing a player in the scene!");
        }
    }

    private void Start()
    {
        LoadSettings();
    }

    /// <summary>
    /// Load all of the individual settings values
    /// </summary>
    public void LoadSettings()
    {
        LoadGameplaySettings();
        LoadVideoSettings();
    }

    #region GameplaySettings
    public void LoadGameplaySettings()
    {
        // load in sensitivity slider input
        sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity", .1f);
        sensitivityText.text = (sensitivitySlider.value * 10).ToString("F2");

        // load in aim sensitivity slider input
        aimSensitivitySlider.value = PlayerPrefs.GetFloat("aimSensitivity", .1f);
        aimSensitivityText.text = (aimSensitivitySlider.value * 10).ToString("F2");
    }

    /// <summary>
    /// UI portion of adjusting the players overall sensitivity
    /// Passes the input slider value through to the player controller where it adjusts the camera sensitivity
    /// </summary>
    public void SensitivitySliderInput()
    {
        float sens = sensitivitySlider.value;
        if (sensitivitySlider.value <= 0.01f)
        {
            sens = 0.01f;
        }

        sensitivityText.text = (sens * 10).ToString("F2");
        controller.ChangeSensitivity(sens);
    }

    /// <summary>
    /// UI portion of adjusting the players overall aim sensitivity
    /// Passes the input slider value through to the player controller where it adjusts the camera sensitivity for ADS
    /// </summary>
    public void AimSensitivitySliderInput()
    {
        float sens = aimSensitivitySlider.value;
        if (aimSensitivitySlider.value <= 0.01f)
        {
            sens = 0.01f;
        }

        aimSensitivityText.text = (sens * 10).ToString("F2");
        controller.ChangeAimSensitivity(sens);
    }

    #endregion

    #region VideoSettings
    public void LoadVideoSettings()
    {
        // load in camera shake toggle value
        cameraShakeToggle.isOn = (PlayerPrefs.GetInt("cameraShake", 1) == 1) ? true : false;
        SetCameraShakeText(cameraShakeToggle.isOn);

        // load in gui opacity slider input
        GUIOpacitySlider.value = PlayerPrefs.GetFloat("GUIOpacity", 1f);
        GUIOpacityText.text = (GUIOpacitySlider.value * 100).ToString("F2");
    }

    /// <summary>
    /// UI portion of adjusting the players overall sensitivity
    /// Passes the input slider value through to the player controller where it adjusts the camera sensitivity
    /// </summary>
    public void GUIOpacitySliderInput()
    {
        float val = GUIOpacitySlider.value;
        GUIOpacityText.text = (val * 100).ToString("F2");
        UIManager.instance.ChangeGUIOpacity(val);
    }

    public void CameraShakeToggleInput()
    {
        CameraManager.instance.ToggleCameraShake(cameraShakeToggle.isOn);
        SetCameraShakeText(cameraShakeToggle.isOn);
    }

    private void SetCameraShakeText(bool state)
    {
        cameraShakeText.text = state ? "On" : "Off";
    }

    #endregion
}
