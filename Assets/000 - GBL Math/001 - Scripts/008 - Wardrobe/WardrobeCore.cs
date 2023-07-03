using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeCore : MonoBehaviour
{
    //====================================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private List<HatData> AllAvailableHats;

    [Header("WARDROBE VARIABLES")]
    [SerializeField] private TextMeshProUGUI OwnedCoinsTMP;
    [SerializeField] private List<HatDataHandler> DisplayedHats;
    [SerializeField] private Button PreviousShelfBtn;
    [SerializeField] private Button NextShelfBtn;
    [SerializeField][ReadOnly] private int StartingHatIndex;
    [ReadOnly] public HatDataHandler SelectedHat;

    [Header("PURCHASE VARIABLES")]
    [SerializeField] private PurchaseCore PurchaseCore;

    [Header("GUMBALL VARIABLES")]
    [SerializeField] public GameObject AnimatedHat;
    [SerializeField] public SpriteRenderer HatSprite;

    private int failedCallbackCounter;
    //====================================================================================================================

    #region WARDROBE INITIALIZATION
    public IEnumerator GetUserDataCoroutine()
    {
        GetUserDataPlayFab();
        yield return null;
    }

    private void GetUserDataPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data["GUID"].Value != PlayerData.GUID)
                {
                    GameManager.Instance.DisplayErrorPanel("You have logged into multiple devices. Please restart the game");
                    return;
                }
                GameManager.Instance.SceneController.AddActionLoadinList(GetUserInventoryCoroutine());
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetUserDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    public IEnumerator GetUserInventoryCoroutine()
    {
        GetUserInventoryPlayFab(true);
        yield return null;
    }

    public void GetUserInventoryPlayFab(bool isInitializing)
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.CoinCount = resultCallback.VirtualCurrency["CO"];
                DisplayOwnedCoins(PlayerData.CoinCount);

                PlayerData.OwnedHats.Clear();
                foreach (ItemInstance item in resultCallback.Inventory)
                    if (item.ItemClass == "HAT")
                        PlayerData.OwnedHats.Add(new PlayerData.OwnedHat(item.ItemInstanceId, GetProperHat(item.ItemId)));

                if(isInitializing)
                {
                    GameManager.Instance.SceneController.AddActionLoadinList(GetCharacterInventoryCoroutine());
                    StartingHatIndex = 0;
                }
                else
                {
                    PurchaseCore.HidePurchasePanel();
                    GameManager.Instance.LoadingPanel.SetActive(false);
                }
                DisplayHatsOnShelf();

            },
            errorCallback => ErrorCallback(errorCallback.Error, () => GetUserInventoryPlayFab(isInitializing), () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private IEnumerator GetCharacterInventoryCoroutine()
    {
        GetCharacterInventoryPlayFab();
        yield return null;
    }

    private void GetCharacterInventoryPlayFab()
    {
        GetCharacterInventoryRequest getCharacterInventory = new GetCharacterInventoryRequest();
        getCharacterInventory.CharacterId = PlayerData.SlimeCharacterID;
        PlayFabClientAPI.GetCharacterInventory(getCharacterInventory,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Inventory.Count == 0)
                {
                    AnimatedHat.SetActive(false);
                    PlayerData.EquippedHat.HatInstanceID = "";
                    PlayerData.EquippedHat.ThisHatData = null;
                }
                else
                {
                    PlayerData.EquippedHat.HatInstanceID = resultCallback.Inventory[0].ItemInstanceId;
                    PlayerData.EquippedHat.ThisHatData = GetProperHat(resultCallback.Inventory[0].ItemId);
                    AnimatedHat.SetActive(true);
                    HatSprite.sprite = PlayerData.EquippedHat.ThisHatData.HatSprite;
                }
                GameManager.Instance.SceneController.ActionPass = true;
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetCharacterInventoryPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }
    #endregion

    #region SHELF
    public void DisplayHatsOnShelf()
    {
        for (int i = 0; i < DisplayedHats.Count; i++)
        {
            DisplayedHats[i].InitializeThisHat(AllAvailableHats[StartingHatIndex + i], IsHatOwned(AllAvailableHats[StartingHatIndex + i].HatID) || IsHatEquipped(AllAvailableHats[StartingHatIndex + i].HatID), 
                IsHatEquipped(AllAvailableHats[StartingHatIndex + i].HatID));
        }

        if (StartingHatIndex == 0)
        {
            PreviousShelfBtn.interactable = false;
            NextShelfBtn.interactable = true;
        }
        else if (StartingHatIndex + 9 >= AllAvailableHats.Count)
        {
            PreviousShelfBtn.interactable = true;
            NextShelfBtn.interactable = false;
        }
        else
        {
            PreviousShelfBtn.interactable = true;
            NextShelfBtn.interactable = true;
        }
    }

    public void DisplayNextShelf()
    {
        StartingHatIndex += 9;
        DisplayHatsOnShelf();
    }

    public void DisplayPreviousShelf()
    {
        StartingHatIndex -= 9;
        DisplayHatsOnShelf();
    }
    #endregion

    #region UTILITY
    public void DisplayOwnedCoins(int coins)
    {
        OwnedCoinsTMP.text = coins.ToString("n0");
    }
    public HatData GetProperHat(string hatID)
    {
        foreach (HatData hat in AllAvailableHats)
            if (hat.HatID == hatID)
                return hat;
        return null;
    }

    private bool IsHatOwned(string hatID)
    {
        foreach (var ownedHat in PlayerData.OwnedHats)
            if (ownedHat.ThisHatData.HatID == hatID)
                return true;
        return false;
    }

    private bool IsHatEquipped(string hatID)
    {
        if (PlayerData.EquippedHat.HatInstanceID != "" && PlayerData.EquippedHat.ThisHatData.HatID == hatID)
            return true;
        else
            return false;
    }

    public void ReturnToMainMenu()
    {
        GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
    }

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
