using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Video Settings References")]
    [Tooltip("The reference to the camera shake toggle")]
    [SerializeField] private Slider cameraShakeToggle;

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

    [Tooltip("The reference to the crosshair opacity slider")]
    [SerializeField] private Slider crosshairOpacitySlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI crosshairOpacityText;


    [Header("Audio Settings References")]
    [Tooltip("The reference to the volume master slider")]
    [SerializeField] private Slider volumeMasterSlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI volumeMasterText;

    [Tooltip("The reference to the volume sfx slider")]
    [SerializeField] private Slider volumeSfxSlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI volumeSfxText;

    [Tooltip("The reference to the volume music slider")]
    [SerializeField] private Slider volumeMusicSlider;

    [Tooltip("The reference to the textbox next to the slider displaying the number")]
    [SerializeField] private TextMeshProUGUI volumeMusicText;




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
        LoadVideoSettings();
        LoadGameplaySettings();
        LoadAudioSettings();  
    }

    #region VideoSettings
    public void LoadVideoSettings()
    {
        // load in camera shake toggle value
        cameraShakeToggle.value = (PlayerPrefs.GetInt("cameraShake", 1) == 1) ? 1 : 0;
        SetCameraShakeText(cameraShakeToggle.value);

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
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void CameraShakeToggleInput()
    {
        float val = cameraShakeToggle.value;
        CameraManager.instance.ToggleCameraShake(cameraShakeToggle.value);
        SetCameraShakeText(cameraShakeToggle.value);
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void SetCameraShakeText(float val)
    {
        cameraShakeText.text = val == 1 ? "Enabled" : "Disabled";
    }

    #endregion

    #region AudioSettings

    /// <summary>
    /// Load values for all of the audio settings and their text from PlayerPrefs
    /// </summary>
    public void LoadAudioSettings()
    {
        // load in master volume settings
        volumeMasterSlider.value = PlayerPrefs.GetFloat("volumeMaster", .8f);
        volumeMasterText.text = (volumeMasterSlider.value * 100).ToString("F2");

        // load in SFX volume settings
        volumeSfxSlider.value = PlayerPrefs.GetFloat("volumeSFX", .8f);
        volumeSfxText.text = (volumeSfxSlider.value * 100).ToString("F2");

        // load in music volume settings
        volumeMusicSlider.value = PlayerPrefs.GetFloat("volumeMusic", .8f);
        volumeMusicText.text = (volumeMusicSlider.value * 100).ToString("F2");
    }

    /// <summary>
    /// UI portion of adjusting the players MASTER volume
    /// Calls the AudioManager ChangeVolume function and multiplies the input value from the 
    /// slider (1-10) by the scale set in AudioManager
    /// </summary>
    public void VolumeMasterSliderInput()
    {
        float val = volumeMasterSlider.value;
        volumeMasterText.text = (val * 100).ToString("F2");
        AudioManager.instance.ChangeVolume(val * AudioManager.instance.volumeScale, AudioManager.AudioType.Master);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// UI portion of adjusting the players SFX volume
    /// Calls the AudioManager ChangeVolume function and multiplies the input value from the 
    /// slider (1-10) by the scale set in AudioManager
    /// </summary>
    public void VolumeSFXSliderInput()
    {
        float val = volumeSfxSlider.value;
        volumeSfxText.text = (val * 100).ToString("F2");
        AudioManager.instance.ChangeVolume(val * AudioManager.instance.volumeScale, AudioManager.AudioType.SFX);
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// UI portion of adjusting the players MUSIC volume
    /// Calls the AudioManager ChangeVolume function and multiplies the input value from the 
    /// slider (1-10) by the scale set in AudioManager
    /// </summary>
    public void VolumeMusicSliderInput()
    {
        float val = volumeMusicSlider.value;
        volumeMusicText.text = (val * 100).ToString("F2");
        AudioManager.instance.ChangeVolume(val * AudioManager.instance.volumeScale, AudioManager.AudioType.Music);
        EventSystem.current.SetSelectedGameObject(null);
    }

    #endregion

    #region GameplaySettings
    public void LoadGameplaySettings()
    {
        // load in sensitivity slider input
        sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity", .1f);
        sensitivityText.text = (sensitivitySlider.value * 10).ToString("F2");

        // load in aim sensitivity slider input
        aimSensitivitySlider.value = PlayerPrefs.GetFloat("aimSensitivity", .1f);
        aimSensitivityText.text = (aimSensitivitySlider.value * 10).ToString("F2");

        // load in crosshair opacity slider input
        crosshairOpacitySlider.value = PlayerPrefs.GetFloat("crosshairOpacity", .65f);
        crosshairOpacityText.text = (crosshairOpacitySlider.value * 100).ToString("F2");
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

        EventSystem.current.SetSelectedGameObject(null);
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

        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// UI portion of adjusting the players crosshair visibility
    /// Passes the input slider value through to the player controller where it adjusts the camera sensitivity
    /// </summary>
    public void CrosshairOpacitySliderInput()
    {
        float val = crosshairOpacitySlider.value;
        crosshairOpacityText.text = (val * 100).ToString("F2");
        UIManager.instance.GetComponent<CrosshairManager>().ChangeCrosshairOpacity(val);
        EventSystem.current.SetSelectedGameObject(null);
    }

    #endregion

}
