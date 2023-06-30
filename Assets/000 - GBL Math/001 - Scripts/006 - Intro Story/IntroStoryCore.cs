using PlayFab;
using PlayFab.ServerModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IntroStoryCore : MonoBehaviour, IDialogue
{
    #region STATE MACHINE
    public enum IntroStoryStates
    {
        NONE,
        INTRO_SELF_DIALOGUE,
        SLIME_WALK,
        GIVE_SLIME_DIALOGUE,
        DONE
    }

    private event EventHandler introStoryStateChange;
    public event EventHandler onIntroStoryStateChange
    {
        add
        {
            if (introStoryStateChange == null || !introStoryStateChange.GetInvocationList().Contains(value))
                introStoryStateChange += value;
        }
        remove { introStoryStateChange -= value; }
    }

    public IntroStoryStates CurrentIntroStoryState
    {
        get => introStoryState;
        set
        {
            introStoryState = value;
            introStoryStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private IntroStoryStates introStoryState;
    #endregion

    #region VARIABLES
    //==================================================================================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("DIALOGUE")]
    [SerializeField] private GameObject DialogueContainer;
    [SerializeField] private TextMeshProUGUI DialogueTMP;
    [SerializeField][ReadOnly] private int CurrentDialogueIndex;
    private Coroutine dialogue;

    [Header("INTRODUCE SELF DIALOGUE")]
    [SerializeField] private List<BlackboardData> IntroductionDialogues;

    [Header("SLIME WALK")]
    [SerializeField] private GameObject Gumball;
    [SerializeField] private Vector3 GumballStartpoint;
    [SerializeField] private Vector3 GumballDestination;

    [Header("GIVE SLIME DIALOGUE")]
    [SerializeField] private List<BlackboardData> SlimeRelatedDialogues;

    [Header("DONE")]
    [SerializeField][TextArea(minLines:5, maxLines: 10)] private string DoneMessage;
    [SerializeField] private float TypeSpeed;

    [Header("USER INPUT")]
    [SerializeField] private GameObject DialogueButtons;
    [SerializeField] private Button SkipBtn;
    [SerializeField] private Button ContinueBtn;

    [Header("SCENE MANAGEMENT")]
    [SerializeField] private string CorrespondingScene;

    private int failedCallbackCounter;
    //==================================================================================================================
    #endregion

    #region INTRODUCE SELF DIALOGUE
    public void BeginDialogue()
    {
        if (dialogue != null)
            StopCoroutine(dialogue);
        DialogueTMP.text = string.Empty;
        if (IntroductionDialogues.Count == 0)
            CurrentIntroStoryState = IntroStoryStates.DONE;
        else
            dialogue = StartCoroutine(TypeDialogueContent());
    }

    public IEnumerator TypeDialogueContent()
    {
       DialogueContainer.SetActive(true);
        SkipBtn.interactable = true;
        ContinueBtn.interactable = false;
        if(CurrentIntroStoryState == IntroStoryStates.INTRO_SELF_DIALOGUE)
        {
            foreach (char c in IntroductionDialogues[CurrentDialogueIndex].LessonText)
            {
                DialogueTMP.text += c;
                yield return new WaitForSeconds(IntroductionDialogues[CurrentDialogueIndex].LessonWriteSpeed);
            }
        }
        else if (CurrentIntroStoryState == IntroStoryStates.GIVE_SLIME_DIALOGUE)
        {
            foreach (char c in SlimeRelatedDialogues[CurrentDialogueIndex].LessonText)
            {
                DialogueTMP.text += c;
                yield return new WaitForSeconds(SlimeRelatedDialogues[CurrentDialogueIndex].LessonWriteSpeed);
            }
        }
        SkipBtn.interactable = false;
        ContinueBtn.interactable = true;
    }

    public void SkipDialogue()
    {
        StopCoroutine(dialogue);
        if(CurrentIntroStoryState == IntroStoryStates.INTRO_SELF_DIALOGUE)
            DialogueTMP.text = IntroductionDialogues[CurrentDialogueIndex].LessonText;
        else if (CurrentIntroStoryState == IntroStoryStates.GIVE_SLIME_DIALOGUE)
            DialogueTMP.text = SlimeRelatedDialogues[CurrentDialogueIndex].LessonText;
        SkipBtn.interactable = false;
        ContinueBtn.interactable = true;
    }
    #endregion

    #region SLIME WALK
    public void MoveGumball()
    {
        if (Vector2.Distance(Gumball.transform.position, GumballDestination) > Mathf.Epsilon)
            Gumball.transform.position = Vector2.MoveTowards(Gumball.transform.position, GumballDestination, 5 * Time.deltaTime);
        else
        {
            DialogueButtons.SetActive(true);
            Gumball.GetComponent<GumballAudioHandler>().PlayGumballHello();
            CurrentIntroStoryState = IntroStoryStates.GIVE_SLIME_DIALOGUE;
        }
    }
    #endregion

    #region GIVE SLIME
    public void GrantBasicSlimePlayFab()
    {
        GrantCharacterToUserRequest grantCharacterToUser = new GrantCharacterToUserRequest();
        grantCharacterToUser.PlayFabId = PlayerData.PlayFabID;
        grantCharacterToUser.CharacterName = "Gumball";
        grantCharacterToUser.CharacterType = "Slime";
        PlayFabServerAPI.GrantCharacterToUser(grantCharacterToUser,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.SlimeCharacterID = resultCallback.CharacterId;
                InitializeBasicSlimeDataPlayFab(PlayerData.SlimeCharacterID);
            },
            errorCallback => ErrorCallback(errorCallback.Error, GrantBasicSlimePlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void InitializeBasicSlimeDataPlayFab(string characterID)
    {
        UpdateCharacterDataRequest updateCharacterDataRequest = new UpdateCharacterDataRequest();
        updateCharacterDataRequest.PlayFabId = PlayerData.PlayFabID;
        updateCharacterDataRequest.CharacterId = characterID;
        updateCharacterDataRequest.Data = new Dictionary<string, string>
        {
            { "MaxHealth", "5" }
        };

        PlayFabServerAPI.UpdateCharacterData(updateCharacterDataRequest,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.SlimeMaxHealth = 5;
            },
            errorCallback => ErrorCallback(errorCallback.Error, () => InitializeBasicSlimeDataPlayFab(characterID), () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }
    #endregion

    #region USER INPUT
    public void ProceedDialogue()
    {
        if (CurrentIntroStoryState == IntroStoryStates.INTRO_SELF_DIALOGUE)
        {
            if (IntroductionDialogues.Count == 0 || CurrentDialogueIndex == IntroductionDialogues.Count - 1)
            {
                CurrentDialogueIndex = 0;
                Gumball.transform.position = GumballStartpoint;
                CurrentIntroStoryState = IntroStoryStates.SLIME_WALK;
                DialogueButtons.SetActive(false);
            }
            else
                DisplayNextDialogue();
        }
        else if (CurrentIntroStoryState == IntroStoryStates.GIVE_SLIME_DIALOGUE)
        {
            if (SlimeRelatedDialogues.Count == 0 || CurrentDialogueIndex == SlimeRelatedDialogues.Count - 1)
                CurrentIntroStoryState = IntroStoryStates.DONE;
            else
                DisplayNextDialogue();
        }
    }
    #endregion

    #region UTILITY
    public void DisplayNextDialogue()
    {
        CurrentDialogueIndex++;
        BeginDialogue();
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
