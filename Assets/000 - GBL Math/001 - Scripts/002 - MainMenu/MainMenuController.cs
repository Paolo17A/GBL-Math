using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private MainMenuCore MainMenuCore;
    private void Awake()
    {
        MainMenuCore.onMainMenuStateChange += MainMenuStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        MainMenuCore.onMainMenuStateChange -= MainMenuStateChange;
    }

    private void Start()
    {
        MainMenuCore.SetInitialVolume();
        if(GameManager.Instance.SceneController.LastScene ==  "")
        {
            MainMenuCore.CurrentMainMenuState = MainMenuCore.MainMenuStates.LOGIN;
        }
        else
            MainMenuCore.CurrentMainMenuState = MainMenuCore.MainMenuStates.MAINMENU;
    }

    private void MainMenuStateChange(object sender, EventArgs e)
    {
        switch(MainMenuCore.CurrentMainMenuState)
        {
            case MainMenuCore.MainMenuStates.LOGIN:
                MainMenuCore.ShowLoginPanel();
                break;
            case MainMenuCore.MainMenuStates.SIGNUP:
                MainMenuCore.ShowRegisterPanel();
                break;
            case MainMenuCore.MainMenuStates.MAINMENU:
                MainMenuCore.InitializePlayerData();
                MainMenuCore.ShowMainMenuPanel();
                break;
            case MainMenuCore.MainMenuStates.LESSON_SELECT:
                MainMenuCore.ShowLessonSelectPanel();
                break;
            case MainMenuCore.MainMenuStates.SETTINGS:
                MainMenuCore.ShowSettingsPanel();
                break;
            case MainMenuCore.MainMenuStates.QUIT:
                MainMenuCore.ShowQuitPanel();
                break;
            case MainMenuCore.MainMenuStates.FINISHED_LESSONS:
                MainMenuCore.ShowFinishedLessonsPanel();
                break;
            case MainMenuCore.MainMenuStates.REVIEW_VIDEO:
                MainMenuCore.ShowReviewVideoPanel();
                MainMenuCore.PlayVideo();
                break;
        }
    }

}

