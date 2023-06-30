using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombatController : MonoBehaviour
{
    [SerializeField] private CharacterCombatCore CharacterCombatCore;
    [ReadOnly] public CombatCore CombatCore;

    private void Awake()
    {
        CharacterCombatCore.onCharacterCombatStateChange += CharacterCombatStateChange;
    }

    private void OnDisable()
    {
        CharacterCombatCore.onCharacterCombatStateChange -= CharacterCombatStateChange;
    }

    private void CharacterCombatStateChange(object sender, EventArgs e)
    {
        CharacterCombatCore.anim.SetInteger("index", (int)CharacterCombatCore.CurrentCharacterCombatState);
    }

    private void Update()
    {
        if ((CharacterCombatCore.thisCharacterType == CharacterCombatCore.CharacterType.PLAYER && CombatCore.CurrentCombatState == CombatCore.CombatStates.PLAYERTURN) || 
            (CharacterCombatCore.thisCharacterType == CharacterCombatCore.CharacterType.ENEMY && CombatCore.CurrentCombatState == CombatCore.CombatStates.ENEMYTURN))
        {
            if (CharacterCombatCore.CurrentTravelState == CharacterCombatCore.TravelState.APPROACH)
                CharacterCombatCore.ApproachOpponent();
            else if (CharacterCombatCore.CurrentTravelState == CharacterCombatCore.TravelState.RETURN)
                CharacterCombatCore.ReturnToOrigin();
        }
    }
}
