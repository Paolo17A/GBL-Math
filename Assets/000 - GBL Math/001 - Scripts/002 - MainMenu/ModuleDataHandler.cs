using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleDataHandler : MonoBehaviour
{
    //=================================================================================================================
    [SerializeField] private LessonData ThisLessonData;
    [SerializeField] private MainMenuCore MainMenuCore;
    //=================================================================================================================

    public void SelectThisLesson()
    {
        MainMenuCore.SelectedLessonData = ThisLessonData;
        MainMenuCore.HideModuleSelectPanel();
        //MainMenuCore.CurrentMainMenuState = MainMenuCore.MainMenuStates.SELECTED_MODULE;
    }
}
