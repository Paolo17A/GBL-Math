using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MainMenuCore;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private PlayerData PlayerData;

    [Header("SETTINGS")]
    [SerializeField] private Slider BGMSlider;
    [SerializeField] private Slider SFXSlider;

    public void SetInitialVolume()
    {
        if (PlayerPrefs.HasKey("BGM"))
        {
            PlayerData.BGMVolume = PlayerPrefs.GetFloat("BGM");
            GameManager.Instance.AudioManager.SetBGMVolume(PlayerData.BGMVolume);
            BGMSlider.value = PlayerData.BGMVolume;
        }
        if (PlayerPrefs.HasKey("SFX"))
        {
            PlayerData.SFXVolume = PlayerPrefs.GetFloat("SFX");
            GameManager.Instance.AudioManager.SetSFXVolume(PlayerData.SFXVolume);
            SFXSlider.value = PlayerData.SFXVolume;
        }
    }

    public void SetBGMVolume()
    {
        PlayerData.BGMVolume = BGMSlider.value;
        PlayerPrefs.SetFloat("BGM", PlayerData.BGMVolume);
        GameManager.Instance.AudioManager.SetBGMVolume(PlayerData.BGMVolume);
    }

    public void SetSFXVolume()
    {
        PlayerData.SFXVolume = SFXSlider.value;
        PlayerPrefs.SetFloat("SFX", PlayerData.SFXVolume);
        GameManager.Instance.AudioManager.SetSFXVolume(PlayerData.SFXVolume);
    }

    public void ResetAudioSettings()
    {
        BGMSlider.value = 1;
        SFXSlider.value = 1;
        SetBGMVolume();
        SetSFXVolume();
        PlayerData.ResetData();
    }
}
