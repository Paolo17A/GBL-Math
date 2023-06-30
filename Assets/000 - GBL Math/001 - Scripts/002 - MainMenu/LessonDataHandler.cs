using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LessonDataHandler : MonoBehaviour
{
    //=================================================================================================================
    [SerializeField] private LessonData ThisLessonData;
    [SerializeField] private MainMenuCore MainMenuCore;

    [Header("SPRITES")]
    [SerializeField] private Button ThisButton;
    [SerializeField] private Image ButtonImage;
    [SerializeField] private Sprite UnlockedSprite;
    [SerializeField] private Sprite LockedSprite;

    [Header("STARS")]
    [SerializeField] private int StarCount;
    [SerializeField] private GameObject StarContainer;
    //=================================================================================================================

    public void SelectThisLesson()
    {
        MainMenuCore.SelectedLessonData = ThisLessonData;
        MainMenuCore.DisplayPopUpLessonPanel();
    }

    public void UnlockThisLesson()
    {
        ThisButton.interactable = true;
        ButtonImage.sprite = UnlockedSprite;
    }

    public void LockThisLesson()
    {
        ThisButton.interactable = false;
        ButtonImage.sprite = LockedSprite;
    }

    public void SetStarCount(int stars)
    {
        StarCount = stars;
        for(int i = 0; i < StarContainer.transform.childCount; i++)
        {
            if(i < StarCount)
                StarContainer.transform.GetChild(i).gameObject.SetActive(true);
            else
                StarContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
    public void HideStars()
    {
        StarContainer.SetActive(false);
    }
}
