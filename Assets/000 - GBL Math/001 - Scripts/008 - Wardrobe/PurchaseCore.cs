using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class PurchaseCore : MonoBehaviour
{
    //=======================================================================================================================================================
    [SerializeField] private WardrobeCore WardrobeCore;
    [SerializeField] private PlayerData PlayerData;

    [Header("PURCHASE VARIABLES")]
    [SerializeField] private GameObject PurchaseContainer;
    [SerializeField] private Image SelectedHatImage;
    [SerializeField] private TextMeshProUGUI PurchaseMessageTMP;

    private int failedCallbackCounter;
    //=======================================================================================================================================================

    #region PURCHASE
    public void DisplayPurchasePanel()
    {
        PurchaseContainer.SetActive(true);
        SelectedHatImage.sprite = WardrobeCore.SelectedHat.ThisHatData.HatSprite;
        PurchaseMessageTMP.text = "Would you like to purchase " + WardrobeCore.SelectedHat.ThisHatData.HatName + " for " + WardrobeCore.SelectedHat.ThisHatData.HatPrice + " coins?";
    }

    public void PurchaseSelectedHat()
    {
        GameManager.Instance.LoadingPanel.SetActive(true);
        PurchaseItemRequest purchaseItem = new PurchaseItemRequest();
        purchaseItem.CatalogVersion = "Hats";
        purchaseItem.ItemId = WardrobeCore.SelectedHat.ThisHatData.HatID;
        purchaseItem.VirtualCurrency = "CO";
        purchaseItem.Price = WardrobeCore.SelectedHat.ThisHatData.HatPrice;

        PlayFabClientAPI.PurchaseItem(purchaseItem,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                WardrobeCore.GetUserInventoryPlayFab(false);
            },
            errorCallback => ErrorCallback(errorCallback.Error, PurchaseSelectedHat, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    public void HidePurchasePanel()
    {
        PurchaseContainer.SetActive(false);
        WardrobeCore.SelectedHat = null;
    }
    #endregion

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
}
