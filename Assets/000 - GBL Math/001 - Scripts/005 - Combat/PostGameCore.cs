using Newtonsoft.Json;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab.ClientModels;
using System;
using TMPro;

public class PostGameCore : MonoBehaviour
{
    //=====================================================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private CombatCore CombatCore;

    [Header("GAME OVER")]
    [ReadOnly] public GameManager.Result FinalResult;
    [SerializeField] private GameObject VictoryPanel;
    [SerializeField] private Transform EarnedStarsContainer;
    [SerializeField] private TextMeshProUGUI RewardedCoinsTMP;
    [SerializeField] private GameObject DefeatPanel;

    private int failedCallbackCounter;
    //=====================================================================================================================================

    #region GAME OVER
    public void ResetPostGame()
    {
        FinalResult = GameManager.Result.NONE;
        VictoryPanel.SetActive(false);
        DefeatPanel.SetActive(false);
    }

    public void ProcessVictory()
    {
        VictoryPanel.SetActive(true);
        if (GameManager.Instance.DebugMode)
        {
            if (GameManager.Instance.CurrentLesson.LessonIndex == PlayerData.CurrentLessonIndex)
                PlayerData.CurrentLessonIndex++;

            if (CombatCore.PlayerCharacter.GetStarCount() > PlayerData.GetLevelStar(GameManager.Instance.CurrentLesson.LessonIndex).LevelStars)
                PlayerData.GetLevelStar(GameManager.Instance.CurrentLesson.LessonIndex).LevelStars = CombatCore.PlayerCharacter.GetStarCount();
        }
        else GetCurrentUserDataPlayFab();
    }


    public void ProcessDefeat()
    {
        DefeatPanel.SetActive(true);
        if (GameManager.Instance.DebugMode)
            PlayerData.EnergyCount--;  
        else
            GetCurrentUserDataPlayFab();
    }

    private void GetCurrentUserDataPlayFab()
    {
        GameManager.Instance.LoadingPanel.SetActive(true);
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data["GUID"].Value != PlayerData.GUID)
                {
                    GameManager.Instance.DisplayErrorPanel("You have logged into multiple devices");
                    return;
                }
                PlayerData.CurrentLessonIndex = int.Parse(resultCallback.Data["CurrentLevel"].Value);
                PlayerData.LevelStars = JsonConvert.DeserializeObject<List<PlayerData.LevelStarsData>>(resultCallback.Data["LevelStars"].Value);

                if (FinalResult == GameManager.Result.VICTORY)
                {
                    //  Playing the lesson for the first time
                    if (PlayerData.CurrentLessonIndex == GameManager.Instance.CurrentLesson.LessonIndex)
                    {
                        UpdateCharacterMaxHealthPlayFab();
                        UpdateCurrentLessonPlayFab();
                    }

                    //  Playing the lesson for the nth time
                    else
                    {
                        //  The user has an improved performance
                        if (CombatCore.PlayerCharacter.GetStarCount() > PlayerData.GetLevelStar(GameManager.Instance.CurrentLesson.LessonIndex).LevelStars)
                        {
                            PlayerData.GetLevelStar(GameManager.Instance.CurrentLesson.LessonIndex).LevelStars = CombatCore.PlayerCharacter.GetStarCount();
                            UpdateLevelStarsPlayFab();
                        }
                        else
                        {
                            GameManager.Instance.LoadingPanel.SetActive(false);
                            DisplayVictoryVisuals();
                        }    
                    }
                }
                else if (FinalResult == GameManager.Result.DEFEAT)
                    SubtractEnergyPlayFab();
                
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetCurrentUserDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void UpdateCharacterMaxHealthPlayFab()
    {
        UpdateCharacterDataRequest updateCharacterData = new UpdateCharacterDataRequest();
        updateCharacterData.CharacterId = PlayerData.SlimeCharacterID;
        updateCharacterData.Data = new Dictionary<string, string>
        {
            {"MaxHealth", (PlayerData.SlimeMaxHealth + 1).ToString() }
        };

        PlayFabClientAPI.UpdateCharacterData(updateCharacterData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.SlimeMaxHealth++;
            },
            errorCallback => ErrorCallback(errorCallback.Error, UpdateCharacterMaxHealthPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    #region VICTORY CALLS
    //  This function increments the CurrentLesson in PlayFab. This function will only be called if it's the first time for the player to beat this level.
    private void UpdateCurrentLessonPlayFab()
    {
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>()
        {
            { "CurrentLevel", (PlayerData.CurrentLessonIndex + 1).ToString() }
        };

        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.CurrentLessonIndex++;
                PlayerData.LevelStars.Add(new PlayerData.LevelStarsData(GameManager.Instance.CurrentLesson.LessonIndex, CombatCore.PlayerCharacter.GetStarCount()));
                UpdateLevelStarsPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, UpdateCurrentLessonPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void UpdateLevelStarsPlayFab()
    {
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>()
        {
            { "LevelStars", JsonConvert.SerializeObject(PlayerData.LevelStars) }
        };
        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (CombatCore.PlayerCharacter.GetStarCount() > 1)
                    GrantCoinsToUser();
                else
                {
                    GameManager.Instance.LoadingPanel.SetActive(false);
                    DisplayVictoryVisuals();
                }
            },
            errorCallback => ErrorCallback(errorCallback.Error, UpdateLevelStarsPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    //  Coins will only be granted to the user if they earned at least 2 stars
    private void GrantCoinsToUser()
    {
        AddUserVirtualCurrencyRequest addUserVirtualCurrency = new AddUserVirtualCurrencyRequest();
        addUserVirtualCurrency.VirtualCurrency = "CO";
        addUserVirtualCurrency.Amount = GameManager.Instance.CurrentLesson.CoinReward;

        PlayFabClientAPI.AddUserVirtualCurrency(addUserVirtualCurrency,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GetUpdatedVirtualCurrency();
            },
            errorCallback => ErrorCallback(errorCallback.Error, GrantCoinsToUser, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }
    #endregion

    #region DEFEAT CALLS
    private void SubtractEnergyPlayFab()
    {
        SubtractUserVirtualCurrencyRequest subtractUserVirtualCurrency = new SubtractUserVirtualCurrencyRequest();
        subtractUserVirtualCurrency.VirtualCurrency = "EN";
        subtractUserVirtualCurrency.Amount = 1;

        PlayFabClientAPI.SubtractUserVirtualCurrency(subtractUserVirtualCurrency,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GetUpdatedVirtualCurrency();
            },
            errorCallback => ErrorCallback(errorCallback.Error, SubtractEnergyPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }
    #endregion

    private void GetUpdatedVirtualCurrency()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.CoinCount = resultCallback.VirtualCurrency["CO"];
                PlayerData.EnergyCount = resultCallback.VirtualCurrency["EN"];
                GameManager.Instance.LoadingPanel.SetActive(false);
                DisplayVictoryVisuals();
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetUpdatedVirtualCurrency, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void DisplayVictoryVisuals()
    {
        //  Display Stars
        for (int i = 0; i < EarnedStarsContainer.transform.childCount; i++)
        {
            if (i < CombatCore.PlayerCharacter.GetStarCount())
                EarnedStarsContainer.transform.GetChild(i).gameObject.SetActive(true);
            else
                EarnedStarsContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        //  Display Coins
        if (CombatCore.PlayerCharacter.GetStarCount() < 2)
            RewardedCoinsTMP.text = "Earned Coins: 0";
        else
            RewardedCoinsTMP.text = "Earned Coins: " + GameManager.Instance.CurrentLesson.CoinReward.ToString("n0");
    }

    #endregion

    #region UTILITY
    public void ErrorCallback(PlayFabErrorCode errorCode, Action restartAction, Action errorAction)
    {
        if (errorCode == PlayFabErrorCode.ConnectionError)
        {
            failedCallbackCounter++;
            if (failedCallbackCounter >= 5)
                GameManager.Instance.DisplayErrorPanel("Connectivity error. Please connect to strong internet");
            else
                restartAction();
        }
        else
            errorAction();
    }
    #endregion
}
