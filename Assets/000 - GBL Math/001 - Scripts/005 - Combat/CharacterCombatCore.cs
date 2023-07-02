using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCombatCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum CharacterCombatState
    {
        NONE,
        IDLE,
        ATTACKING,
        ATTACKED,
        DYING,
        OFFENSE
    }

    private event EventHandler characterCombatStateChange;
    public event EventHandler onCharacterCombatStateChange
    {
        add
        {
            if (characterCombatStateChange == null || !characterCombatStateChange.GetInvocationList().Contains(value))
                characterCombatStateChange += value;
        }
        remove
        {
            characterCombatStateChange -= value;
        }
    }
    public CharacterCombatState CurrentCharacterCombatState
    {
        get { return currentCharacterCombatState; }
        set
        {
            currentCharacterCombatState = value;
            characterCombatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }

    [SerializeField][ReadOnly] private CharacterCombatState currentCharacterCombatState;
    #endregion

    #region VARIABLES
    //==========================================================================================================
    [SerializeField] private PlayerData PlayerData;
    public enum CharacterType { NONE, PLAYER, ENEMY }
    public enum AttackType { NONE, MELEE, RANGED }
    public enum TravelState { NONE, APPROACH, RETURN}
    [SerializeField] private CombatCore CombatCore;
    [SerializeField] private PostGameCore PostGameCore;
    [SerializeField] private CharacterCombatController CharacterCombatController;

    [Header("CHARACTER")]
    public CharacterType thisCharacterType;
    public AttackType thisAttackType;
    public Animator anim;
    [SerializeField] private Vector3 CharacterOriginPoint;
    [SerializeField] private Vector3 CharacterAttackPoint;

    [Header("HEALTH")]
    [SerializeField] private int MaxHealth;
    [SerializeField] private Transform HealthContainer;
    [SerializeField][ReadOnly] private int CurrentHealth;

    [Header("MELEE ONLY: TRAVEL")]
    [ReadOnly] public TravelState CurrentTravelState;

    [Header("SHAKE VARIABLES")]
    [ReadOnly] public bool isShaking;
    [SerializeField] private int shakeIndex;

    [Header("AUDIO")]
    [SerializeField] private AudioClip walkSFX;
    [SerializeField] private AudioClip hitSFX;
    //==========================================================================================================
    #endregion

    #region INITIALIZATION
    public void InitializeCharacter(Transform healthContainer, CombatCore combatCore, PostGameCore postGameCore)
    {
        CombatCore = combatCore;
        PostGameCore = postGameCore;
        CharacterCombatController.CombatCore = combatCore;
        transform.position = CharacterOriginPoint;
        CurrentCharacterCombatState = CharacterCombatState.IDLE;
        HealthContainer = healthContainer;

        #region HEALTH
        if (thisCharacterType == CharacterType.PLAYER)
            MaxHealth = PlayerData.SlimeMaxHealth;
        CurrentHealth = MaxHealth;
        SetHeartSprites();
        #endregion
    }
    #endregion

    #region HEALTH
    public void TakeDamage()
    {
        CurrentHealth--;
        SetHeartSprites();
        if (CurrentHealth <= 0)
        {
            CurrentCharacterCombatState = CharacterCombatState.DYING;
            if ((thisCharacterType == CharacterType.PLAYER && CombatCore.EnemyCharacter.thisAttackType == AttackType.RANGED) ||
               (thisCharacterType == CharacterType.ENEMY && CombatCore.PlayerCharacter.thisAttackType == AttackType.RANGED))
            {
                if (thisCharacterType == CharacterType.PLAYER)
                    PostGameCore.FinalResult = GameManager.Result.DEFEAT;
                else if (thisCharacterType == CharacterType.ENEMY)
                    PostGameCore.FinalResult = GameManager.Result.VICTORY;
                CombatCore.CurrentCombatState = CombatCore.CombatStates.GAMEOVER;
            }
        }
        else
        {
            CurrentCharacterCombatState = CharacterCombatState.IDLE;
            if ((thisCharacterType == CharacterType.PLAYER && CombatCore.EnemyCharacter.thisAttackType == AttackType.RANGED) ||
               (thisCharacterType == CharacterType.ENEMY && CombatCore.PlayerCharacter.thisAttackType == AttackType.RANGED))
                CombatCore.CurrentCombatState = CombatCore.CombatStates.TIMER;
        }
    }

    public void HealCharacter()
    {
        CurrentHealth += 2;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        SetHeartSprites();

        CombatCore.CurrentCombatState = CombatCore.CombatStates.ENEMYTURN;
    }

    public void StartShake()
    {
        float initialX = transform.position.x;
        GameManager.Instance.AudioManager.PlayAudioClip(hitSFX);
        LeanTween.moveX(transform.gameObject, thisCharacterType == CharacterType.PLAYER ? initialX - 1 : initialX + 1, 0.15f).setOnComplete(() =>
        {
            LeanTween.moveX(transform.gameObject, initialX, 0.15f);
        });
    }

    public int GetDamageDealt()
    {
        return MaxHealth - CurrentHealth;
    }

    private void SetHeartSprites()
    {
        for (int i = 0; i < HealthContainer.childCount; i++)
        {
            if (i < CurrentHealth)
                HealthContainer.GetChild(i).gameObject.SetActive(true);
            else
                HealthContainer.GetChild(i).gameObject.SetActive(false);
        }
    }

    public int GetStarCount()
    {
        if ((float)CurrentHealth / (float)MaxHealth > 0 && (float)CurrentHealth / (float)MaxHealth <= 0.33)
            return 1;
        else if ((float)CurrentHealth / (float)MaxHealth > 0.33 && (float)CurrentHealth / (float)MaxHealth <= 0.66)
            return 2;
        else
            return 3;
    }
    #endregion

    #region TRAVEL
    public void ApproachOpponent()
    {
        if (Vector2.Distance(transform.position, CharacterAttackPoint) > Mathf.Epsilon)
            transform.position = Vector2.MoveTowards(transform.position, CharacterAttackPoint, 8 * Time.deltaTime);
        else
        {
            CurrentTravelState = TravelState.NONE;
            CurrentCharacterCombatState = CharacterCombatState.ATTACKING;
        }
    }

    public void ReturnToOrigin()
    {
        if (Vector2.Distance(transform.position, CharacterOriginPoint) > Mathf.Epsilon)
            transform.position = Vector2.MoveTowards(transform.position, CharacterOriginPoint, 8 * Time.deltaTime);
        else
        {
            CurrentTravelState = TravelState.NONE;
            if (CombatCore.PlayerCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING || CombatCore.EnemyCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING)
            {
                if (CombatCore.PlayerCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING)
                    PostGameCore.FinalResult = GameManager.Result.DEFEAT;
                else if (CombatCore.EnemyCharacter.CurrentCharacterCombatState == CharacterCombatState.DYING)
                    PostGameCore.FinalResult = GameManager.Result.VICTORY;
                CombatCore.CurrentCombatState = CombatCore.CombatStates.GAMEOVER;
            }
            else
                CombatCore.CurrentCombatState = CombatCore.CombatStates.TIMER;
        }
    }

    public void PlayWalkSFX()
    {
        if (GameManager.Instance.SceneController.CurrentScene != "CombatScene")
            return;

        if (CurrentTravelState == TravelState.NONE)
            return;

        GameManager.Instance.AudioManager.PlayAudioClip(walkSFX);
    }

    public void FinishMeleeAttack()
    {
        CurrentCharacterCombatState = CharacterCombatState.IDLE;
        CurrentTravelState = TravelState.RETURN;
    }
    #endregion

    #region UTILITY
    public void AttackOpponent()
    {
        if (thisCharacterType == CharacterType.PLAYER)
        {
            //GameManager.Instance.AudioManager.PlayAudioClip(hitSFX);
            CombatCore.EnemyCharacter.CurrentCharacterCombatState = CharacterCombatState.ATTACKED;
        }
        else if (thisCharacterType == CharacterType.ENEMY)
            CombatCore.PlayerCharacter.CurrentCharacterCombatState = CharacterCombatState.ATTACKED;
    }
    #endregion
}
