using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DoubleDialogueCore : MonoBehaviour, IDialogue
{
    #region STATE MACHINE
    public enum DoubleDialogueStates
    {
        NONE,
        DOUBLE_DIALOGUE,
        SENDOFF,
        DONE
    }

    private event EventHandler doubleDialogueStateChange;
    public event EventHandler onDoubleDialogueStateChange
    {
        add
        {
            if (doubleDialogueStateChange == null || !doubleDialogueStateChange.GetInvocationList().Contains(value))
                doubleDialogueStateChange += value;
        }
        remove { doubleDialogueStateChange -= value; }
    }

    public DoubleDialogueStates CurrentDoubleDialogueState
    {
        get => doubleDialogueState;
        set
        {
            doubleDialogueState = value;
            doubleDialogueStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private DoubleDialogueStates doubleDialogueState;
    #endregion

    #region VARIABLES
    //==================================================================================================================
    [Header("DIALOGUE")]
    [SerializeField] private GameObject DialogueContainer;
    [SerializeField] private TextMeshProUGUI DialogueSpeakerTMP;
    [SerializeField] private TextMeshProUGUI DialogueContentTMP;
    [SerializeField] private List<DialogueData> Dialogues;
    [SerializeField][ReadOnly] private int CurrentDialogueIndex;
    private Coroutine dialogue;

    [Header("SENDOFF")]
    [SerializeField][TextArea(minLines: 5, maxLines: 10)] private string SendOffMessage;
    [SerializeField] private float TypeSpeed;

    [Header("USER INPUT")]
    [SerializeField] private GameObject DialogueButtons;
    [SerializeField] private Button SkipBtn;
    [SerializeField] private Button ContinueBtn;
    //==================================================================================================================
    #endregion

    #region DOUBLE DIALOGUE
    public void BeginDialogue()
    {
        if (dialogue != null)
            StopCoroutine(dialogue);
        DialogueSpeakerTMP.text = Dialogues[CurrentDialogueIndex].DialogueSpeaker;
        DialogueContentTMP.text = string.Empty;
        if (Dialogues.Count == 0)
            CurrentDoubleDialogueState = DoubleDialogueStates.DONE;
        else
            dialogue = StartCoroutine(TypeDialogueContent());
    }

    public IEnumerator TypeDialogueContent()
    {
        DialogueContainer.SetActive(true);
        SkipBtn.interactable = true;
        ContinueBtn.interactable = false;
        
        if(CurrentDoubleDialogueState == DoubleDialogueStates.DOUBLE_DIALOGUE)
        {
            if (Dialogues[CurrentDialogueIndex].DialogueVoiceOver != null)
            {
                if (GameManager.Instance.AudioManager.IsStillTalking())
                    GameManager.Instance.AudioManager.KillSoundEffect();
                if (CurrentDialogueIndex == 0)
                    yield return new WaitForSeconds(0.5f);
                GameManager.Instance.AudioManager.PlayAudioClip(Dialogues[CurrentDialogueIndex].DialogueVoiceOver);
            }
            foreach (char c in Dialogues[CurrentDialogueIndex].DialogueContent)
            {
                DialogueContentTMP.text += c;
                yield return new WaitForSeconds(Dialogues[CurrentDialogueIndex].TypeSpeed);
            }
        }
        else if(CurrentDoubleDialogueState == DoubleDialogueStates.SENDOFF)
        {
            foreach (char c in SendOffMessage)
            {
                DialogueContentTMP.text += c;
                yield return new WaitForSeconds(TypeSpeed);
            }
        }

        SkipBtn.interactable = false;
        ContinueBtn.interactable = true;
    }
    #endregion

    #region USER INPUT
    public void SkipDialogue()
    {
        StopCoroutine(dialogue);
        if (CurrentDoubleDialogueState ==  DoubleDialogueStates.DOUBLE_DIALOGUE)
            DialogueContentTMP.text = Dialogues[CurrentDialogueIndex].DialogueContent;
        else if (CurrentDoubleDialogueState == DoubleDialogueStates.SENDOFF)
            DialogueContentTMP.text = SendOffMessage;
        SkipBtn.interactable = false;
        ContinueBtn.interactable = true;
    }
    public void ProceedDialogue()
    {
        if (GameManager.Instance.AudioManager.IsStillTalking())
            GameManager.Instance.AudioManager.KillSoundEffect();
        if (CurrentDoubleDialogueState == DoubleDialogueStates.DOUBLE_DIALOGUE)
        {
            if (Dialogues.Count == 0 || CurrentDialogueIndex == Dialogues.Count - 1)
                CurrentDoubleDialogueState = DoubleDialogueStates.SENDOFF;
            else
                DisplayNextDialogue();
        }
        else if (CurrentDoubleDialogueState == DoubleDialogueStates.SENDOFF)
            CurrentDoubleDialogueState = DoubleDialogueStates.DONE;
    }
    #endregion

    #region UTILITY
    public void DisplayNextDialogue()
    {
        CurrentDialogueIndex++;
        BeginDialogue();
    }
    #endregion
}
