using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ServerModels;
using System;

public class HatDataHandler : MonoBehaviour
{
    //===============================================================================================================
    [ReadOnly] public HatData ThisHatData;
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private WardrobeCore WardrobeCore;
    [SerializeField] private PurchaseCore PurchaseCore;

    [Header("COIN VISUALS")]
    [SerializeField] private GameObject CoinPrice;
    [SerializeField] private TextMeshProUGUI PriceTMP;

    [Header("HAT VISUALS")]
    [SerializeField] private Image HatImage;
    [SerializeField] private Button HatButton;

    [Header("BUTTON VISUALS")]
    [SerializeField] private Image HatButtonImage;
    [SerializeField] private Sprite GreenButtonSprite;
    [SerializeField] private Sprite RedButtonSprite;
    [SerializeField] private TextMeshProUGUI HatButtonTMP;

    [Header("HAT STATE")]
    [ReadOnly] public string hatInstanceID;
    [ReadOnly] public bool isOwned;
    [ReadOnly] public bool isEquipped;
    int failedCallbackCounter;
    //===============================================================================================================

    public void InitializeThisHat(HatData hatData, bool isOwned, bool isEquipped)
    {
        ThisHatData = hatData;
        this.isOwned = isOwned;
        this.isEquipped = isEquipped;
        HatImage.sprite = ThisHatData.HatSprite;
        PriceTMP.text = ThisHatData.HatPrice.ToString();

        if (isOwned)
        {
            CoinPrice.SetActive(false);
            if (isEquipped)
            {
                hatInstanceID = PlayerData.EquippedHat.HatInstanceID;
                HatButtonImage.sprite = RedButtonSprite;
                HatButtonTMP.text = "REMOVE";
            }
            else
            {
                hatInstanceID = GetOwnedHatInstanceID(ThisHatData.HatID);
                HatButtonImage.sprite = GreenButtonSprite;
                HatButtonTMP.text = "EQUIP";
            }
        }
        else
        {
            CoinPrice.SetActive(true);
            HatButtonImage.sprite = GreenButtonSprite;
            HatButtonTMP.text = "PURCHASE";
        }
    }

    public void SelectThisHat()
    {
        if(isOwned)
        {
            if(isEquipped)
                RemoveCurrentHat(false);
            else
                WearSelectedHat();
        }
        else
        {
            if(ThisHatData.HatPrice > PlayerData.CoinCount)
            {
                GameManager.Instance.DisplayErrorPanel("You do not have enough coins to purchase this hat");
                return;
            }
            else
            {
                WardrobeCore.SelectedHat = this;
                PurchaseCore.DisplayPurchasePanel();
            }
        }
    }

    private void RemoveCurrentHat(bool isReplacingCurrentHat)
    {
        if(GameManager.Instance.DebugMode)
        {
            WardrobeCore.AnimatedHat.SetActive(false);
            PlayerData.EquippedHat.HatInstanceID = "";
            PlayerData.EquippedHat.ThisHatData = null;
            PlayerData.OwnedHats.Add(new PlayerData.OwnedHat(hatInstanceID, ThisHatData));
            HatButtonImage.sprite = GreenButtonSprite;
            HatButtonTMP.text = "EQUIP";
        }
        else
        {
            MoveItemToUserFromCharacterRequest moveItemToUser = new MoveItemToUserFromCharacterRequest();
            moveItemToUser.ItemInstanceId = PlayerData.EquippedHat.HatInstanceID;
            moveItemToUser.CharacterId = PlayerData.SlimeCharacterID;
            moveItemToUser.PlayFabId = PlayerData.PlayFabID;

            PlayFabServerAPI.MoveItemToUserFromCharacter(moveItemToUser,
                resultCallback =>
                {
                    failedCallbackCounter = 0;
                    isOwned = true;
                    isEquipped = false;
                    WardrobeCore.AnimatedHat.SetActive(false);
                    PlayerData.OwnedHats.Add(new PlayerData.OwnedHat(PlayerData.EquippedHat.HatInstanceID, PlayerData.EquippedHat.ThisHatData));
                    PlayerData.EquippedHat.HatInstanceID = "";
                    PlayerData.EquippedHat.ThisHatData = null;
                    HatButtonImage.sprite = GreenButtonSprite;
                    HatButtonTMP.text = "EQUIP";

                    if (isReplacingCurrentHat)
                        TransferHatToCharacter();
                    else
                        PlayerData.OwnedHats.Add(new PlayerData.OwnedHat(hatInstanceID, ThisHatData));
                },
                errorCallback => ErrorCallback(errorCallback.Error, () => RemoveCurrentHat(isReplacingCurrentHat), () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
        }
    }

    private void WearSelectedHat()
    {
        if(GameManager.Instance.DebugMode)
        {
            if (PlayerData.EquippedHat.HatInstanceID != "")
                RemoveCurrentHat(true);

            WardrobeCore.AnimatedHat.SetActive(true);
            WardrobeCore.HatSprite.sprite = ThisHatData.HatSprite;
            PlayerData.EquippedHat.HatInstanceID = hatInstanceID;
            PlayerData.EquippedHat.ThisHatData = ThisHatData;
            HatButtonImage.sprite = RedButtonSprite;
            HatButtonTMP.text = "REMOVE";
        }
        else
        {
            if (PlayerData.EquippedHat.HatInstanceID != "")
                RemoveCurrentHat(true);
            else TransferHatToCharacter();
        }
    }

    private void TransferHatToCharacter()
    {
        GameManager.Instance.LoadingPanel.SetActive(true);
        MoveItemToCharacterFromUserRequest moveItemToCharacter = new MoveItemToCharacterFromUserRequest();
        moveItemToCharacter.ItemInstanceId = hatInstanceID;
        moveItemToCharacter.CharacterId = PlayerData.SlimeCharacterID;
        moveItemToCharacter.PlayFabId = PlayerData.PlayFabID;

        PlayFabServerAPI.MoveItemToCharacterFromUser(moveItemToCharacter,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                isOwned = true;
                isEquipped = true;
                WardrobeCore.AnimatedHat.SetActive(true);
                WardrobeCore.HatSprite.sprite = ThisHatData.HatSprite;
                PlayerData.EquippedHat.HatInstanceID = hatInstanceID;
                PlayerData.EquippedHat.ThisHatData = ThisHatData;
                PlayerData.OwnedHats.Remove(PlayerData.GetSelectedHat(hatInstanceID));
                HatButtonImage.sprite = RedButtonSprite;
                HatButtonTMP.text = "REMOVE";

                GameManager.Instance.LoadingPanel.SetActive(false);
                WardrobeCore.DisplayHatsOnShelf();

            },
            errorCallback => ErrorCallback(errorCallback.Error, TransferHatToCharacter, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    #region UTILITY
    private string GetOwnedHatInstanceID(string itemID)
    {
        foreach(var hat in PlayerData.OwnedHats)
            if(hat.ThisHatData.HatID == itemID)
                return hat.HatInstanceID;
        return null;
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
