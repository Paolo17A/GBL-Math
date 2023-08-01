using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishedLessonDataHandler : MonoBehaviour
{
    //=============================================================================================
    [SerializeField] private LessonData ThisLessonData;
    [SerializeField] private MainMenuCore MainMenuCore;
    //=============================================================================================

    public void SelectThisFinishedLesson()
    {
        MainMenuCore.SelectedFinishedLessonData = ThisLessonData;
        MainMenuCore.InitializeVideoPlayer();
        MainMenuCore.CurrentMainMenuState = MainMenuCore.MainMenuStates.REVIEW_VIDEO;
    }
}
