using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroStoryController : MonoBehaviour
{
    [SerializeField] private IntroStoryCore IntroStoryCore;

    private void Awake()
    {
        IntroStoryCore.onIntroStoryStateChange += IntroStoryStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        IntroStoryCore.onIntroStoryStateChange -= IntroStoryStateChange;
    }

    private void Start()
    {
        GameManager.Instance.AudioManager.KillBackgroundMusic();
        IntroStoryCore.CurrentIntroStoryState = IntroStoryCore.IntroStoryStates.INTRO_SELF_DIALOGUE;
    }

    private void IntroStoryStateChange(object sender, EventArgs e)
    {
        switch (IntroStoryCore.CurrentIntroStoryState)
        {
            case IntroStoryCore.IntroStoryStates.INTRO_SELF_DIALOGUE:
                IntroStoryCore.BeginDialogue();
                break;
            case IntroStoryCore.IntroStoryStates.SLIME_WALK:
                IntroStoryCore.GrantBasicSlimePlayFab();
                break;
            case IntroStoryCore.IntroStoryStates.GIVE_SLIME_DIALOGUE:
                IntroStoryCore.BeginDialogue();
                break;
            case IntroStoryCore.IntroStoryStates.DONE:
                GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
                break;
        }
    }

    private void Update()
    {
        if (IntroStoryCore.CurrentIntroStoryState == IntroStoryCore.IntroStoryStates.SLIME_WALK)
            IntroStoryCore.MoveGumball();
    }
}
