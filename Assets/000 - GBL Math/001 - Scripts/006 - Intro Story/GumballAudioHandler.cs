using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GumballAudioHandler : MonoBehaviour
{
    //================================================================================================================
    [SerializeField] private IntroStoryCore IntroStoryCore;

    [Header("SOUND EFFECTS")]
    [SerializeField] private AudioClip GumballStep;
    [SerializeField] private AudioClip GumballHello;
    //================================================================================================================

    public void PlayGumballStep()
    {
        if (GameManager.Instance.SceneController.CurrentScene != "IntroStoryScene")
            return;

        if (IntroStoryCore.CurrentIntroStoryState != IntroStoryCore.IntroStoryStates.SLIME_WALK)
            return;

        if (GameManager.Instance.AudioManager.IsStillTalking())
            return;

        GameManager.Instance.AudioManager.PlayAudioClip(GumballStep);
    }

    public void PlayGumballHello()
    {
        GameManager.Instance.AudioManager.PlayAudioClip(GumballHello);
    }
}
