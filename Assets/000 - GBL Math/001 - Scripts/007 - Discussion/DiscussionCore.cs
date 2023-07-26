using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using PlayFab;
using PlayFab.ClientModels;

public class DiscussionCore : MonoBehaviour, IDialogue
{
    #region STATE MACHINE
    //================================================================================================================
    public enum DiscussionStates
    {
        NONE,
        WELCOME_BACK,
        INTRODUCTION,
        VIDEO,
        INVITATION
    }

    private event EventHandler discussionStateChange;
    public event EventHandler onDiscussionStateChange
    {
        add
        {
            if (discussionStateChange == null || !discussionStateChange.GetInvocationList().Contains(value))
                discussionStateChange += value;
        }
        remove { discussionStateChange -= value; }
    }

    public DiscussionStates CurrentDiscussionState
    {
        get => discussionState;
        set
        {
            discussionState = value;
            discussionStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private DiscussionStates discussionState;
    //=============================================================================================================
    #endregion

    #region VARIABLES
    //=============================================================================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("WIZARD SPEECH BUBBLES")]
    [SerializeField] private GameObject WizardSpeechBubble;
    [SerializeField] private TextMeshProUGUI WizardSpeechTMP;
    [SerializeField][TextArea] private List<string> WelcomeBackText;
    [SerializeField][TextArea] private string InvitationText;
    [SerializeField][ReadOnly] private int CurrentDialogueIndex;

    [Header("VIDEO")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoPlaybackImage;
    [SerializeField] private Slider VolumeSlider;

    [Header("USER INPUT")]
    [SerializeField] private Button ProceedBtn;
    [SerializeField] private GameObject PausePanel;

    private int failedCallbackCounter;
    //=============================================================================================================
    #endregion

    #region DIALOGUE
    public void BeginDialogue()
    {
        videoPlaybackImage.gameObject.SetActive(false);
        WizardSpeechTMP.text = string.Empty;
        WizardSpeechBubble.SetActive(true);
        StartCoroutine(TypeDialogueContent());
    }

    public IEnumerator TypeDialogueContent()
    {
        ProceedBtn.gameObject.SetActive(false);
        if (CurrentDiscussionState == DiscussionStates.WELCOME_BACK)
        {
            foreach (char c in WelcomeBackText[CurrentDialogueIndex])
            {
                WizardSpeechTMP.text += c;
                yield return new WaitForSeconds(GameManager.Instance.CurrentLesson.IntroTypeSpeed);
            }
        }
        else if (CurrentDiscussionState == DiscussionStates.INTRODUCTION)
        {
            if (GameManager.Instance.CurrentLesson.IntroductoryMessages.Count > 0)
            {
                foreach (char c in GameManager.Instance.CurrentLesson.IntroductoryMessages[CurrentDialogueIndex])
                {
                    WizardSpeechTMP.text += c;
                    yield return new WaitForSeconds(GameManager.Instance.CurrentLesson.IntroTypeSpeed);
                }
            }
            else
                CurrentDiscussionState = DiscussionStates.VIDEO;
        }
        else if (CurrentDiscussionState == DiscussionStates.INVITATION)
        {
            foreach (char c in InvitationText)
            {
                WizardSpeechTMP.text += c;
                yield return new WaitForSeconds(GameManager.Instance.CurrentLesson.IntroTypeSpeed);
            }
        }
        ProceedBtn.gameObject.SetActive(true);
        yield return null;
    }
    #endregion

    #region VIDEO
    public void InitializeVideoPlayer()
    {
        videoPlaybackImage.gameObject.SetActive(false);
        videoPlayer.clip = GameManager.Instance.CurrentLesson.LessonVideo;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinish;
    }

    private void OnVideoPrepared(VideoPlayer player)
    {
        player.prepareCompleted -= OnVideoPrepared;
    }

    public void OnVideoFinish(VideoPlayer player)
    {
        player.loopPointReached -= OnVideoFinish;
        ProceedBtn.gameObject.SetActive(true);
    }

    public void PlayVideo()
    {
        WizardSpeechBubble.gameObject.SetActive(false);
        videoPlaybackImage.gameObject.SetActive(true);
        videoPlayer.Play();
    }

    public void RewindVideo()
    {

    }
    #endregion

    #region PLAYFAB API
    private void GetUserDataPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data["GUID"].Value != PlayerData.GUID)
                {
                    GameManager.Instance.DisplayErrorPanel("You have logged into multiple devices");
                    return;
                }
                GetUserVirtualCurrencyPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetUserDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void GetUserVirtualCurrencyPlayFab()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.CoinCount = resultCallback.VirtualCurrency["CO"];
                PlayerData.EnergyCount = resultCallback.VirtualCurrency["EN"];
                if(PlayerData.EnergyCount == 3)
                {
                    GameManager.Instance.LoadingPanel.SetActive(false);
                    CurrentDiscussionState = DiscussionStates.INVITATION;
                }
                else
                    GrantEnergyPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetUserVirtualCurrencyPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }
    private void GrantEnergyPlayFab()
    {
        AddUserVirtualCurrencyRequest addUserVirtualCurrency = new AddUserVirtualCurrencyRequest();
        addUserVirtualCurrency.VirtualCurrency = "EN";
        addUserVirtualCurrency.Amount = 3 - PlayerData.EnergyCount;

        PlayFabClientAPI.AddUserVirtualCurrency(addUserVirtualCurrency,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GameManager.Instance.LoadingPanel.SetActive(false);
                PlayerData.EnergyCount = resultCallback.Balance;
                CurrentDiscussionState = DiscussionStates.INVITATION;
            },
            errorCallback => ErrorCallback(errorCallback.Error, GrantEnergyPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }
    #endregion

    #region USER INPUT
    public void ProceedDialogue()
    {
        if (CurrentDiscussionState == DiscussionStates.WELCOME_BACK)
        {
            if (WelcomeBackText.Count == 0 || CurrentDialogueIndex == WelcomeBackText.Count - 1)
                DisplayAnimatedVideo();
            else
                DisplayNextDialogue();
        }
        else if (CurrentDiscussionState == DiscussionStates.INTRODUCTION)
        {
            if (GameManager.Instance.CurrentLesson.IntroductoryMessages.Count == 0 || CurrentDialogueIndex == GameManager.Instance.CurrentLesson.IntroductoryMessages.Count - 1)
                DisplayAnimatedVideo();
            else
                DisplayNextDialogue();
        }
        else if (CurrentDiscussionState == DiscussionStates.VIDEO)
        {
            videoPlaybackImage.gameObject.SetActive(false);
            if(GameManager.Instance.DebugMode)
                CurrentDiscussionState = DiscussionStates.INVITATION;
            else
            {
                GameManager.Instance.LoadingPanel.SetActive(true);
                GetUserDataPlayFab();
            }
        }
        else if (CurrentDiscussionState == DiscussionStates.INVITATION)
            GameManager.Instance.SceneController.CurrentScene = "CombatScene";
    }

    public void DisplayPausePanel()
    {
        Time.timeScale = 0;
        PausePanel.SetActive(true);

        if(CurrentDiscussionState == DiscussionStates.VIDEO && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }

    public void HidePausePanel()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);

        if (CurrentDiscussionState == DiscussionStates.VIDEO && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
    }

    public void AdjustVideoVolume()
    {
        videoPlayer.SetDirectAudioVolume(0, VolumeSlider.value);
    }
    #endregion

    #region UTILITY
    private void DisplayAnimatedVideo()
    {
        CurrentDialogueIndex = 0;
        ProceedBtn.gameObject.SetActive(false);
        CurrentDiscussionState = DiscussionStates.VIDEO;
    }

    public void DisplayNextDialogue()
    {
        CurrentDialogueIndex++;
        WizardSpeechTMP.text = string.Empty;
        StartCoroutine(TypeDialogueContent());
    }
    #endregion

    #region ERROR
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
