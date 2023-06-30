using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscussionController : MonoBehaviour
{
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private DiscussionCore DiscussionCore;

    private void Awake()
    {
        DiscussionCore.onDiscussionStateChange += DiscussionStateChange;
        GameManager.Instance.SceneController.ActionPass = true;
    }

    private void OnDisable()
    {
        DiscussionCore.onDiscussionStateChange -= DiscussionStateChange;
    }

    private void Start()
    {
        DiscussionCore.InitializeVideoPlayer();
        if (PlayerData.EnergyCount == 0)
            DiscussionCore.CurrentDiscussionState = DiscussionCore.DiscussionStates.WELCOME_BACK;
        else
            DiscussionCore.CurrentDiscussionState = DiscussionCore.DiscussionStates.INTRODUCTION;
    }

    private void DiscussionStateChange(object sender, EventArgs e)
    {
        switch (DiscussionCore.CurrentDiscussionState)
        {
            case DiscussionCore.DiscussionStates.WELCOME_BACK:
                DiscussionCore.BeginDialogue();
                break;
            case DiscussionCore.DiscussionStates.INTRODUCTION:
                DiscussionCore.BeginDialogue();
                break;
            case DiscussionCore.DiscussionStates.VIDEO:
                DiscussionCore.PlayVideo();
                break;
            case DiscussionCore.DiscussionStates.INVITATION:
                DiscussionCore.BeginDialogue();
                break;
        }
    }
}
