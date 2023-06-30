using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectGenderHandler : MonoBehaviour
{
    //============================================================================================================
    [SerializeField] private MainMenuCore mainMenuCore;
    [SerializeField] private PlayerData.Gender thisGender;
    [SerializeField] private Image outlineImage;
    [SerializeField] private SelectGenderHandler otherGenderHandler;
    //============================================================================================================

    private void Start()
    {
        DisableOutline();
    }

    public void DisableOutline()
    {
        outlineImage.enabled = false;
    }

    public void EnableOutline()
    {
        outlineImage.enabled = true;
        otherGenderHandler.DisableOutline();
        //mainMenuCore.SelectedGender = thisGender;
    }
}
