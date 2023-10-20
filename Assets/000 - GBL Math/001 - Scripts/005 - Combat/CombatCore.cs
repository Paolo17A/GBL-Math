using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;

public class CombatCore : MonoBehaviour
{
    #region STATE MACHINE
    public enum CombatStates
    {
        NONE,
        COUNTDOWN,
        TIMER,
        PLAYERTURN,
        ENEMYTURN,
        GAMEOVER
    }

    private event EventHandler combatStateChange;
    public event EventHandler onCombatStateChange
    {
        add
        {
            if (combatStateChange == null || !combatStateChange.GetInvocationList().Contains(value))
                combatStateChange += value;
        }
        remove { combatStateChange -= value; }
    }

    public CombatStates CurrentCombatState
    {
        get => combatState;
        set
        {
            combatState = value;
            combatStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private CombatStates combatState;
    #endregion

    #region VARIABLES
    //==================================================================================================================
    [SerializeField] private PlayerData PlayerData;
    [SerializeField] private List<HatData> AllAvailableHats;

    [Header("LESSON")]
    [SerializeField] private TextMeshProUGUI LessonIndexTMP;
    [SerializeField] private SpriteRenderer BackgroundSprite;

    [Header("COUNTDOWN VARIABLES")]
    [SerializeField] private GameObject CountdownPanel;
    [SerializeField] private TextMeshProUGUI CountdownTMP;
    [SerializeField] private int MaxCountdownValue;
    [SerializeField][ReadOnly] private float CountdownValue;

    [Header("TIMER")]
    [SerializeField] private GameObject TimerContainer;
    [SerializeField] private TextMeshProUGUI TimerTMP;
    [SerializeField] private int MaxTimerValue;
    [ReadOnly] public int CurrentTimerValue;
    [SerializeField][ReadOnly] private float TimerValueLeft;

    [Header("TEXT QUESTION 2 CHOICE")]
    [SerializeField] private GameObject TwoChoiceQuestionContainer;
    [SerializeField] private TextMeshProUGUI QuestionTMPTwoChoice;
    [SerializeField] private List<ChoiceButtonHandler> TwoChoiceButtons;

    [Header("TEXT QUESTION 4 CHOICE")]
    [SerializeField] private GameObject FourChoiceQuestionContainer;
    [SerializeField] private TextMeshProUGUI QuestionTMP;
    [SerializeField] private List<ChoiceButtonHandler> ChoiceButtons;
    [ReadOnly] public int CurrentQuestionIndex;

    [Header("IMAGE QUESTION")]
    [SerializeField] private GameObject ImageQuestionContainer;
    [SerializeField] private Image ImageSprite;
    [SerializeField] private Button PreviousImageChoiceBtn;
    [SerializeField] private Button NextImageChoiceBtn;
    [SerializeField][ReadOnly] private int CurrentImageChoiceIndex;

    [Header("NUMBER INPUT QUESTION")]
    [SerializeField] private GameObject NumberInputQuestionContainer;
    [SerializeField] private TextMeshProUGUI NumericalQuestionTMP;
    [SerializeField] private TMP_InputField NumberTMPInput;
    [SerializeField] private Button SubmitNumberBtn;

    [Header("CHARACTERS")]
    [SerializeField] private GameObject GumballPrefab;
    [ReadOnly] public CharacterCombatCore PlayerCharacter;
    [SerializeField] private Transform PlayerHeartContainer;
    [ReadOnly] public CharacterCombatCore EnemyCharacter;
    [SerializeField] private Transform EnemyHeartContainer;

    [Header("PAUSE")]
    [SerializeField] private GameObject PausePanel;
    [SerializeField] private AudioClip CombatBGMusic;
    [SerializeField] private SettingsManager SettingsManager;

    [Header("POST GAME")]
    [SerializeField] private PostGameCore PostGameCore;

    [Header("DEBUGGER")]
    GameObject player;
    GameObject enemy;
    int failedCallbackCounter;
    //==================================================================================================================
    #endregion

    #region INITIALIZATION
    public IEnumerator GetCharacterData()
    {
        GetCharacterDataPlayFab();
        yield return null;
    }

    private void GetCharacterDataPlayFab()
    {
        GetCharacterDataRequest getCharacterData = new GetCharacterDataRequest();
        getCharacterData.CharacterId = PlayerData.SlimeCharacterID;

        PlayFabClientAPI.GetCharacterData(getCharacterData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerData.SlimeMaxHealth = int.Parse(resultCallback.Data["MaxHealth"].Value);
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetCharacterDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    public IEnumerator GetCharacterInventory()
    {
        GetCharacterInventoryPlayFab();
        yield return null;  
    }

    private void GetCharacterInventoryPlayFab()
    {
        GetCharacterInventoryRequest getCharacterInventory = new GetCharacterInventoryRequest();
        getCharacterInventory.CharacterId = PlayerData.SlimeCharacterID;
        PlayFabClientAPI.GetCharacterInventory(getCharacterInventory,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Inventory.Count == 0)
                {
                    PlayerData.EquippedHat.HatInstanceID = "";
                    PlayerData.EquippedHat.ThisHatData = null;
                }
                else
                {
                    PlayerData.EquippedHat.HatInstanceID = resultCallback.Inventory[0].ItemInstanceId;
                    PlayerData.EquippedHat.ThisHatData = GetProperHat(resultCallback.Inventory[0].ItemId);
                }
                CurrentCombatState = CombatStates.COUNTDOWN;
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetCharacterInventoryPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    public void InitializeQuizGame()
    {
        LessonIndexTMP.text = "Lesson " + GameManager.Instance.CurrentLesson.LessonIndex;
        BackgroundSprite.sprite = GameManager.Instance.CurrentLesson.BackgroundSprite;
        SettingsManager.SetInitialVolume();
        GameManager.Instance.AudioManager.SetBackgroundMusic(CombatBGMusic);
        #region COUNTDOWN
        CountdownPanel.SetActive(true);
        CountdownValue = MaxCountdownValue;
        CountdownTMP.text = CountdownValue.ToString();
        #endregion

        #region QUESTIONS
        Shuffle(GameManager.Instance.CurrentLesson.LessonQuestions);
        CurrentQuestionIndex = 0;
        ToggleQuestionObjects(false);
        #endregion

        #region CHARACTERS
        if(PlayerCharacter != null) 
            Destroy(PlayerCharacter.gameObject);
        player = Instantiate(GumballPrefab);
        PlayerCharacter = player.GetComponent<CharacterCombatCore>();
        PlayerCharacter.InitializeCharacter(PlayerHeartContainer, this, PostGameCore);

        if (EnemyCharacter != null)
            Destroy(EnemyCharacter.gameObject);

        enemy = Instantiate(GameManager.Instance.CurrentLesson.EnemyPrefab);
        EnemyCharacter = enemy.GetComponent<CharacterCombatCore>();
        EnemyCharacter.InitializeCharacter(EnemyHeartContainer, this, PostGameCore);
        #endregion

        #region GAMEOVER
        PostGameCore.ResetPostGame();
        #endregion
    }

    public void ReduceCountdownTimer()
    {
        if (CountdownValue > 1)
        {
            CountdownValue -= Time.deltaTime;
            CountdownTMP.text = Mathf.FloorToInt(Math.Max(0, CountdownValue)).ToString();
        }
        else
        {
            CountdownPanel.SetActive(false);
            CurrentCombatState = CombatStates.TIMER;
        }
    }
    #endregion

    #region TIMER
    public void ResetTimer()
    {
        CurrentTimerValue = MaxTimerValue;
        TimerValueLeft = CurrentTimerValue;
        TimerTMP.text = CurrentTimerValue.ToString();
        DisplayCurrentQuestion();
    }

    public void DecreaseTimer()
    {
        //  Timer is ongoing
        if (CurrentTimerValue > 0)
        {
            TimerValueLeft -= Time.deltaTime;
            CurrentTimerValue = (int)TimerValueLeft;
            TimerTMP.text = CurrentTimerValue.ToString();

            if (GameManager.Instance.CheatsActivated && CurrentTimerValue == 25)
            {
                AssignNewQuestion();
                CurrentCombatState = CombatStates.PLAYERTURN;
            }
        }
        //  Out of Time
        else
        {
            ToggleQuestionObjects(false);
            TimerContainer.SetActive(false);
            CurrentCombatState = CombatStates.ENEMYTURN;
        }
    }
    #endregion

    #region QUESTIONS
    private void ToggleQuestionObjects(bool _bool)
    {
        TwoChoiceQuestionContainer.SetActive(_bool);
        FourChoiceQuestionContainer.SetActive(_bool);
        ImageQuestionContainer.SetActive(_bool);
        NumberInputQuestionContainer.SetActive(_bool);
    }

    private void DisplayCurrentQuestion()
    {
        TimerContainer.SetActive(true);
        if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].ThisQuestionType == QuestionData.QuestionType.TEXT)
        {
            Shuffle(GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices);
            if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices.Count == 2)
            {
                TwoChoiceQuestionContainer.SetActive(true);
                QuestionTMPTwoChoice.text = GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Question;
                for (int i = 0; i < TwoChoiceButtons.Count; i++)
                {
                    TwoChoiceButtons[i].AssignAnswer(GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices[i]);
                    if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Answer == GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices[i])
                        TwoChoiceButtons[i].IsCorrectAnswer = true;
                    else
                        TwoChoiceButtons[i].IsCorrectAnswer = false;
                }
            }
            else if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices.Count == 4)
            {
                FourChoiceQuestionContainer.SetActive(true);
                QuestionTMP.text = GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Question;
                for (int i = 0; i < ChoiceButtons.Count; i++)
                {
                    ChoiceButtons[i].AssignAnswer(GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices[i]);
                    if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Answer == GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Choices[i])
                        ChoiceButtons[i].IsCorrectAnswer = true;
                    else
                        ChoiceButtons[i].IsCorrectAnswer = false;
                }
            }
        }
        else if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].ThisQuestionType == QuestionData.QuestionType.IMAGE)
        {
            ImageQuestionContainer.SetActive(true);
            CurrentImageChoiceIndex = 0;
            DisplayCurrentImageChoice();
        }
        else if (GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].ThisQuestionType == QuestionData.QuestionType.NUMBER)
        {
            NumberInputQuestionContainer.SetActive(true);
            NumericalQuestionTMP.text = GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].Question;
            NumberTMPInput.text = "";
            SubmitNumberBtn.interactable = false;
        }
    }

    public void AssignNewQuestion()
    {
        CurrentQuestionIndex++;
        if (CurrentQuestionIndex == GameManager.Instance.CurrentLesson.LessonQuestions.Count)
        {
            CurrentQuestionIndex = 0;
            Shuffle(GameManager.Instance.CurrentLesson.LessonQuestions);
        }
        ToggleQuestionObjects(false);
        TimerContainer.SetActive(false);
    }

    #region IMAGE QUESTION
    private void DisplayCurrentImageChoice()
    {
        ImageSprite.sprite = GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].ImageChoices[CurrentImageChoiceIndex];
        if(CurrentImageChoiceIndex == 0)
        {
            PreviousImageChoiceBtn.interactable = false;
            NextImageChoiceBtn.interactable = true;
        }
        else if (CurrentImageChoiceIndex == GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].ImageChoices.Count - 1)
        {
            PreviousImageChoiceBtn.interactable = true;
            NextImageChoiceBtn.interactable = false;
        }
        else
        {
            PreviousImageChoiceBtn.interactable = true;
            NextImageChoiceBtn.interactable = true;
        }
    }

    public void DisplayPreviousImageChoice()
    {
        CurrentImageChoiceIndex--;
        DisplayCurrentImageChoice();
    }

    public void DisplayNextImageChoice()
    {
        CurrentImageChoiceIndex++;
        DisplayCurrentImageChoice();
    }

    public void SelectThisImageChoice()
    {
        if (CurrentImageChoiceIndex == GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].ImageAnswerIndex)
        {
            CurrentCombatState = CombatStates.PLAYERTURN;
        }
        else
        {
            CurrentCombatState = CombatStates.ENEMYTURN;
        }
        AssignNewQuestion();
    }
    #endregion

    #region NUMBER QUESTION
    public void HandleSubmitNumberButtonInteractability()
    {
        if (NumberTMPInput.text == string.Empty)
            SubmitNumberBtn.interactable = false;
        else
            SubmitNumberBtn.interactable = true;
    }
    public void SubmitThisNumericalInput()
    {
        if (float.Parse(NumberTMPInput.text) == GameManager.Instance.CurrentLesson.LessonQuestions[CurrentQuestionIndex].NumberAnswer)
        {
            CurrentCombatState = CombatStates.PLAYERTURN;
        }
        else
        {
            CurrentCombatState = CombatStates.ENEMYTURN;
        }
        AssignNewQuestion();
    }
    #endregion
    #endregion

    #region PAUSE
    public void PauseGame()
    {
        Time.timeScale = 0;
        PausePanel.SetActive(true);
        GameManager.Instance.AudioManager.PauseBGM();
        GameManager.Instance.AudioManager.PauseSFX();
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        GameManager.Instance.AudioManager.ResumeSFX();
        GameManager.Instance.AudioManager.ResumeBGM();
    }

    public void RestartGame()
    {
        if(PlayerData.EnergyCount > 0)
        {
            CurrentCombatState = CombatStates.COUNTDOWN;
            ResumeGame();
        }
        else
        {
            GameManager.Instance.AudioManager.KillBackgroundMusic();
            GameManager.Instance.SceneController.CurrentScene = "DiscussionScene";
        }
    }

    public void ReturnToMainMenu()
    {
        ResumeGame();
        GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
    }

    public void ContinueToNextLevel()
    {
        //  The user has finished all available levels already
        if (GameManager.Instance.CurrentLesson.LessonIndex >= GameManager.Instance.AllLessons.Count && PlayerData.CurrentLessonIndex > GameManager.Instance.AllLessons.Count)
        {
            GameManager.Instance.AudioManager.KillBackgroundMusic();
            GameManager.Instance.SceneController.CurrentScene = "EndingScene";
        }
        else
        {
            //  Set the new current lesson
            GameManager.Instance.CurrentLesson = GameManager.Instance.AllLessons[GameManager.Instance.CurrentLesson.LessonIndex];

            //  Check if the user has enough stars to play the next level
            if (GameManager.Instance.CurrentLesson.StarQuota > PlayerData.GetTotalStars())
            {
                Debug.Log("NOT ENOUGH STARS");
                GameManager.Instance.CurrentLesson = null;
                GameManager.Instance.SceneController.CurrentScene = "MainMenuScene";
            }
            else
            {
                if (GameManager.Instance.CurrentLesson.LessonIndex == PlayerData.CurrentLessonIndex)
                {
                    GameManager.Instance.AudioManager.KillBackgroundMusic();
                    if (GameManager.Instance.CurrentLesson.HasDialogueScene)
                        GameManager.Instance.SceneController.CurrentScene = "DoubleDialogueScene";
                    else
                        GameManager.Instance.SceneController.CurrentScene = "DiscussionScene";
                }
                else
                {
                    CurrentCombatState = CombatStates.COUNTDOWN;
                    ResumeGame();
                }
            }
        }
    }
    #endregion

    #region UTILITY
    public void Shuffle<Transform>(List<Transform> ts)
    {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }

    public HatData GetProperHat(string hatID)
    {
        foreach (HatData hat in AllAvailableHats)
            if (hat.HatID == hatID)
                return hat;
        return null;
    }

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
