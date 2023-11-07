using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public enum AudioType
    {
        Master,
        SFX,
        Music
    }

    [Tooltip("Multiplier applied to the volume values passed into the sliders (0-1 * scale)")]
    public float volumeScale;

    // Volume settings used to keep track of the individual volumes
    // Current master volume
    private float volumeMaster;
    // Current SFX volume
    private float volumeSFX;
    // Current music volume
    private float volumeMusic;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        UpdateVolumes();
    }

    /// <summary>
    /// Set each of the volume settings + WWise RTPC's to their values stored in PlayerPrefs, called on starting the game
    /// </summary>
    public void UpdateVolumes()
    {
        volumeMaster = PlayerPrefs.GetFloat("volumeMaster", .8f);
        volumeSFX = PlayerPrefs.GetFloat("volumeSFX", .8f);
        volumeMusic = PlayerPrefs.GetFloat("volumeMusic", .8f);
        AkSoundEngine.SetRTPCValue("UI_Volume_MST", volumeMaster);
        AkSoundEngine.SetRTPCValue("UI_Volume_SFX", volumeSFX);
        AkSoundEngine.SetRTPCValue("UI_Volume_MUS", volumeMusic);
    }

    /// <summary>
    /// Change a specific volume value from the AudioTypes and save it to PlayerPrefs
    /// </summary>
    /// <param name="newVolume">The value that the volume is being set to</param>
    /// <param name="type">The audio type to determine which volume is changed</param>
    public void ChangeVolume(float newVolume, AudioType type)
    {
        switch (type)
        {
            case AudioType.Master:
                volumeMaster = newVolume;
                AkSoundEngine.SetRTPCValue("UI_Volume_MST", volumeMaster);
                PlayerPrefs.SetFloat("volumeMaster", volumeMaster);
                break;

            case AudioType.SFX:
                volumeSFX = newVolume;
                AkSoundEngine.SetRTPCValue("UI_Volume_SFX", volumeSFX);
                PlayerPrefs.SetFloat("volumeSFX", volumeSFX);
                break;

            case AudioType.Music:
                volumeMusic = newVolume;
                AkSoundEngine.SetRTPCValue("UI_Volume_MUS", volumeMusic);
                PlayerPrefs.SetFloat("volumeMusic", volumeMusic);
                break;

            default:
                break;
        }
    }

    public void AnimEventPostWwiseEvent(string wwiseEventName, GameObject audioSource)
    {
        AkSoundEngine.PostEvent(wwiseEventName, audioSource);
    }
}
