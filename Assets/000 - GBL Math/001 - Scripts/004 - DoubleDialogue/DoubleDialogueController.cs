using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDialogueController : MonoBehaviour
{
    [SerializeField] private DoubleDialogueCore DoubleDialogueCore;
    [SerializeField] private string CorrespondingScene;

    private void Awake()
    {
        DoubleDialogueCore.onDoubleDialogueStateChange += DoubleDialogueChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        DoubleDialogueCore.onDoubleDialogueStateChange -= DoubleDialogueChange;
    }

    private void Start()
    {
        DoubleDialogueCore.CurrentDoubleDialogueState = DoubleDialogueCore.DoubleDialogueStates.DOUBLE_DIALOGUE;
    }

    private void DoubleDialogueChange(object sender, EventArgs e)
    {
        switch(DoubleDialogueCore.CurrentDoubleDialogueState)
        {
            case DoubleDialogueCore.DoubleDialogueStates.DOUBLE_DIALOGUE:
                DoubleDialogueCore.BeginDialogue();
                break;
            case DoubleDialogueCore.DoubleDialogueStates.SENDOFF:
                DoubleDialogueCore.BeginDialogue();
                break;
            case DoubleDialogueCore.DoubleDialogueStates.DONE:
                GameManager.Instance.SceneController.CurrentScene = CorrespondingScene;
                break;

        }
    }
}
