using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //===============================================================================================================
    [SerializeField] private AudioSource BGMSource;
    [SerializeField] private AudioSource SFXSource;
    //===============================================================================================================

    public void SetBGMVolume(float volume)
    {
        BGMSource.volume = volume;
    }

    public void PauseBGM()
    {
        BGMSource.Pause();
    }

    public void ResumeBGM()
    {
        BGMSource.Play();
    }

    public void SetSFXVolume(float volume)
    {
        SFXSource.volume = volume;
    }

    public void PauseSFX()
    {
        SFXSource.Pause();
    }

    public void ResumeSFX()
    {
        SFXSource.Play();
    }


    public void KillBackgroundMusic()
    {
        BGMSource.Stop();
    }

    public void KillSoundEffect()
    {
        SFXSource.Stop();
    }

    public void PlayAudioClip(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void SetBackgroundMusic(AudioClip clip)
    {
        KillBackgroundMusic();
        BGMSource.clip = clip;
        BGMSource.Play();
    }

    public bool IsStillTalking()
    {
        if (SFXSource.isPlaying)
            return true;
        else
            return false;
    }
}
