using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChoiceButtonHandler : MonoBehaviour
{
    //=================================================================================================================
    [SerializeField] private CombatCore CombatCore;

    [Header("CHOICE VARIABLES")]
    [SerializeField] private TextMeshProUGUI ChoiceTMP;
    [ReadOnly] public bool IsCorrectAnswer;
    //=================================================================================================================

    public void AssignAnswer(string answer)
    {
        ChoiceTMP.text = answer;
    }

    public void ProcessAnswer()
    {
        CombatCore.AssignNewQuestion();
        if (IsCorrectAnswer)
        {
            CombatCore.CurrentCombatState = CombatCore.CombatStates.PLAYERTURN;
        }
        else
        {
            CombatCore.CurrentCombatState = CombatCore.CombatStates.ENEMYTURN;
        }
    }
}
