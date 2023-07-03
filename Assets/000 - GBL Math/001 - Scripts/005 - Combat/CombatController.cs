using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [SerializeField] private CombatCore CombatCore;
    [SerializeField] private PostGameCore PostGameCore;

    private void Awake()
    {
        GameManager.Instance.SceneController.GetActionLoadingList.Clear();
        GameManager.Instance.SceneController.AddActionLoadinList(CombatCore.GetCharacterData());
        GameManager.Instance.SceneController.AddActionLoadinList(CombatCore.GetCharacterInventory());
        GameManager.Instance.SceneController.ActionPass = true;

        CombatCore.onCombatStateChange += CombatStateChange;
    }

    private void OnDisable()
    {
        CombatCore.onCombatStateChange -= CombatStateChange;
    }

    private void CombatStateChange(object sender, EventArgs e)
    {
        switch (CombatCore.CurrentCombatState)
        {
            case CombatCore.CombatStates.COUNTDOWN:
                CombatCore.InitializeQuizGame();
                break;
            case CombatCore.CombatStates.TIMER:
                CombatCore.ResetTimer();
                break;
            case CombatCore.CombatStates.PLAYERTURN:
                CombatCore.PlayerCharacter.CurrentTravelState = CharacterCombatCore.TravelState.APPROACH;
                break;
            case CombatCore.CombatStates.ENEMYTURN:
                CombatCore.EnemyCharacter.CurrentTravelState = CharacterCombatCore.TravelState.APPROACH;
                break;
            case CombatCore.CombatStates.GAMEOVER:
                if (PostGameCore.FinalResult == GameManager.Result.VICTORY)
                    PostGameCore.ProcessVictory();
                else if (PostGameCore.FinalResult == GameManager.Result.DEFEAT)
                    PostGameCore.ProcessDefeat();
                break;
        }
    }

    private void Update()
    {
        if (CombatCore.CurrentCombatState == CombatCore.CombatStates.COUNTDOWN)
            CombatCore.ReduceCountdownTimer();
        else if (CombatCore.CurrentCombatState == CombatCore.CombatStates.TIMER)
            CombatCore.DecreaseTimer();
    }
}
