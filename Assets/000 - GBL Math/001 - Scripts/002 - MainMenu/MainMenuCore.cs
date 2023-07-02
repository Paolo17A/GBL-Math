using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;

public class MainMenuCore : MonoBehaviour
{
    #region STATE MACHINE
    //================================================================================================================
    public enum MainMenuStates
    {
        NONE,
        LOGIN,
        SIGNUP,
        MAINMENU,
        LESSON_SELECT,
        SETTINGS,
        QUIT,

    }

    private event EventHandler mainMenuStateChange;
    public event EventHandler onMainMenuStateChange
    {
        add
        {
            if (mainMenuStateChange == null || !mainMenuStateChange.GetInvocationList().Contains(value))
                mainMenuStateChange += value;
        }
        remove { mainMenuStateChange -= value; }
    }

    public MainMenuStates CurrentMainMenuState
    {
        get => mainMenuStates;
        set
        {
            mainMenuStates = value;
            mainMenuStateChange?.Invoke(this, EventArgs.Empty);
        }
    }
    [SerializeField][ReadOnly] private MainMenuStates mainMenuStates;
    //=============================================================================================================
    #endregion

    #region VARIABLES
    //=============================================================================================================
    [SerializeField] private PlayerData PlayerData;

    [Header("PANELS")]
    [SerializeField] private RectTransform LoginRT;
    [SerializeField] private CanvasGroup LoginCG;
    [SerializeField] private RectTransform RegisterRT;
    [SerializeField] private CanvasGroup RegisterCG;
    [SerializeField] private RectTransform MainMenuRT;
    [SerializeField] private CanvasGroup MainMenuCG;
    [SerializeField] private RectTransform LessonSelectRT;
    [SerializeField] private CanvasGroup LessonSelectCG;
    [SerializeField] private RectTransform SettingsRT;
    [SerializeField] private CanvasGroup SettingsCG;
    [SerializeField] private RectTransform QuitRT;
    [SerializeField] private CanvasGroup QuitCG;

    [Header("LOGIN")]
    [SerializeField] private TMP_InputField LoginEmailTMPInput;
    [SerializeField] private TMP_InputField LoginPasswordTMPInput;

    [Header("REGISTER")]
    [SerializeField] private TMP_InputField RegisterEmailTMPInput;
    [SerializeField] private TMP_InputField RegisterUsernameTMPInput;
    [SerializeField] private TMP_InputField RegisterPasswordTMPInput;
    [SerializeField] private TMP_InputField RegisterConfirmPasswordTMPInput;

    [Header("SETTINGS")]
    [SerializeField] private SettingsManager SettingsManager;

    [Header("LESSON SELECT")]
    [SerializeField] private TextMeshProUGUI StarCountTMP;
    [SerializeField] private TextMeshProUGUI EnergyCountTMP;
    [SerializeField] private GameObject LessonButtonsContainer;
    [SerializeField] private GameObject DiscussionButtonsContainer;
    [SerializeField] private List<LessonDataHandler> AllLessons;

    [Header("SELECTED LESSON")]
    [SerializeField] private GameObject PopUpLessonPanel;
    [SerializeField] private TextMeshProUGUI LessonDetailsTMP;
    [SerializeField] private TextMeshProUGUI CoinRewardTMP;
    [SerializeField] private TextMeshProUGUI StarQuotaTMP;
    [SerializeField] private TextMeshProUGUI SelectLevelButtonTMP;
    [ReadOnly] public LessonData SelectedLessonData;

    [Header("BACKGROUND MUSIC")]
    [SerializeField] private AudioClip MainMenuBGMusic;

    int failedCallbackCounter;
    //=============================================================================================================
    #endregion

    #region PANELS
    public void ShowLoginPanel()
    {
        ResetLoginFields();
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, null, LoginCG, 0, 1, () =>
        {
            if (PlayerPrefs.HasKey("Username") && PlayerPrefs.HasKey("Password"))
            {
                LoginEmailTMPInput.text = PlayerPrefs.GetString("Username");
                LoginPasswordTMPInput.text = PlayerPrefs.GetString("Password");
                HandleLoginButton();
            }
        });
    }
    public void HideLoginPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(LoginRT, LoginRT, LoginCG, 1, 0, () => { });
    }
    public void ShowRegisterPanel()
    {
        ResetRegisterFields();
        GameManager.Instance.AnimationsLT.FadePanel(RegisterRT, null, RegisterCG, 0, 1, () => { });
    }
    public void HideRegisterPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(RegisterRT, RegisterRT, RegisterCG, 1, 0, () => { });
    }
    public void ShowMainMenuPanel()
    {
        EnergyCountTMP.text = PlayerData.EnergyCount.ToString();
        GameManager.Instance.AnimationsLT.FadePanel(MainMenuRT, null, MainMenuCG, 0, 1, () => { });
    }

    public void HideMainMenuPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(MainMenuRT, MainMenuRT, MainMenuCG, 1, 0, () => { });
    }

    public void ShowLessonSelectPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(LessonSelectRT, null, LessonSelectCG, 0, 1, () => { });
    }

    public void HideLessonSelectPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(LessonSelectRT, LessonSelectRT, LessonSelectCG, 1, 0, () => { });
    }

    public void ShowSettingsPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(SettingsRT, null, SettingsCG, 0, 1, () => { });
    }

    public void HideSettingsPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(SettingsRT, SettingsRT, SettingsCG, 1, 0, () => { });
    }

    public void ShowQuitPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(QuitRT, null, QuitCG, 0, 1, () => { });
    }

    public void HideQuitPanel()
    {
        GameManager.Instance.AnimationsLT.FadePanel(QuitRT, QuitRT, QuitCG, 1, 0, () => { });
    }

    public void MainMenuStateToIndex(int index)
    {
        switch (index)
        {
            case (int)MainMenuStates.LOGIN:
                CurrentMainMenuState = MainMenuStates.LOGIN;
                break;
            case (int)MainMenuStates.SIGNUP:
                CurrentMainMenuState = MainMenuStates.SIGNUP;
                break;
            case (int)MainMenuStates.MAINMENU:
                CurrentMainMenuState = MainMenuStates.MAINMENU;
                break;
            case (int)MainMenuStates.LESSON_SELECT:
                CurrentMainMenuState = MainMenuStates.LESSON_SELECT;
                break;
            case (int)MainMenuStates.SETTINGS:
                CurrentMainMenuState = MainMenuStates.SETTINGS;
                break;
            case (int)MainMenuStates.QUIT:
                CurrentMainMenuState = MainMenuStates.QUIT;
                break;
        }
    }
    #endregion

    #region LOGIN
    public void HandleLoginButton()
    {
        if (LoginEmailTMPInput.text == "" || LoginPasswordTMPInput.text == "")
        {
            GameManager.Instance.DisplayErrorPanel("Please fill up all fields");
            return;
        }

        if (GameManager.Instance.DebugMode)
        {
            HideLoginPanel();
            CurrentMainMenuState = MainMenuStates.MAINMENU;
        }
        else
            LoginWithEmailPlayFab();
    }

    private void LoginWithEmailPlayFab()
    {
        GameManager.Instance.LoadingPanel.SetActive(true);
        LoginWithEmailAddressRequest loginWithEmail = new()
        {
            Email = LoginEmailTMPInput.text,
            Password = LoginPasswordTMPInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginWithEmail,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerPrefs.SetString("Username", LoginEmailTMPInput.text);
                PlayerPrefs.SetString("Password", LoginPasswordTMPInput.text);
                PlayerData.PlayFabID = resultCallback.PlayFabId;
                SetPlayerGUIDPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, LoginWithEmailPlayFab, () => LoginWithUsernamePlayFab()));
    }

    private void LoginWithUsernamePlayFab()
    {
        LoginWithPlayFabRequest loginWithPlayFab = new()
        {
            Username = LoginEmailTMPInput.text,
            Password = LoginPasswordTMPInput.text
        };
        PlayFabClientAPI.LoginWithPlayFab(loginWithPlayFab,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                PlayerPrefs.SetString("Username", LoginEmailTMPInput.text);
                PlayerPrefs.SetString("Password", LoginPasswordTMPInput.text);
                PlayerData.PlayFabID = resultCallback.PlayFabId;
                SetPlayerGUIDPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, LoginWithUsernamePlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void SetPlayerGUIDPlayFab()
    {
        Guid guid = Guid.NewGuid();
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>
        {
            { "GUID", guid.ToString() }
        };

        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GetUserDataPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, SetPlayerGUIDPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void GetUserDataPlayFab()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (resultCallback.Data.ContainsKey("Gender"))
                    PlayerData.PlayerGender = (PlayerData.Gender)int.Parse(resultCallback.Data["Gender"].Value);
                if (resultCallback.Data.ContainsKey("CurrentLevel"))
                    PlayerData.CurrentLessonIndex = int.Parse(resultCallback.Data["CurrentLevel"].Value);
                if (resultCallback.Data.ContainsKey("GUID"))
                    PlayerData.GUID = resultCallback.Data["GUID"].Value;
                if (resultCallback.Data.ContainsKey("LevelStars"))
                {
                    PlayerData.LevelStars = JsonConvert.DeserializeObject<List<PlayerData.LevelStarsData>>(resultCallback.Data["LevelStars"].Value);
                    StarCountTMP.text = PlayerData.GetTotalStars().ToString("n0");
                }

                ListAllCharactersPlayFab();

            },
            errorCallback => ErrorCallback(errorCallback.Error, GetUserDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void ListAllCharactersPlayFab()
    {
        PlayFabClientAPI.GetAllUsersCharacters(new ListUsersCharactersRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GameManager.Instance.LoadingPanel.SetActive(false);
                if (resultCallback.Characters.Count == 0)
                    GameManager.Instance.SceneController.CurrentScene = "IntroStoryScene";
                else
                {
                    PlayerData.SlimeCharacterID = resultCallback.Characters[0].CharacterId;
                    GetCharacterDataPlayFab();
                }
            },
            errorCallback => ErrorCallback(errorCallback.Error, ListAllCharactersPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void GetCharacterDataPlayFab()
    {
        GameManager.Instance.LoadingPanel.SetActive(true);

        GetCharacterDataRequest getCharacterData = new GetCharacterDataRequest();
        getCharacterData.PlayFabId = PlayerData.PlayFabID;
        getCharacterData.CharacterId = PlayerData.SlimeCharacterID;

        PlayFabClientAPI.GetCharacterData(getCharacterData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                if (!resultCallback.Data.ContainsKey("MaxHealth"))
                {
                    GameManager.Instance.LoadingPanel.SetActive(false);
                    GameManager.Instance.DisplayErrorPanel("Slime has no Max Health property");
                    PlayerData.ResetData();
                    PlayFabClientAPI.ForgetAllCredentials();
                    ResetLoginFields();
                    return;
                }

                PlayerData.SlimeMaxHealth = int.Parse(resultCallback.Data["MaxHealth"].Value);
                GetUserInventoryPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetCharacterDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void GetUserInventoryPlayFab()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GameManager.Instance.LoadingPanel.SetActive(false);
                PlayerData.EnergyCount = resultCallback.VirtualCurrency["EN"];
                PlayerData.CoinCount = resultCallback.VirtualCurrency["CO"];
                HideLoginPanel();
                ResetLoginFields();
                CurrentMainMenuState = MainMenuStates.MAINMENU;
            },
            errorCallback => ErrorCallback(errorCallback.Error, GetUserInventoryPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void ResetLoginFields()
    {
        LoginEmailTMPInput.text = "";
        LoginPasswordTMPInput.text = "";
    }
    #endregion

    #region REGISTER
    public void HandleRegisterButton()
    {
        if (RegisterEmailTMPInput.text == "" || RegisterUsernameTMPInput.text == "" || RegisterPasswordTMPInput.text == "" || RegisterConfirmPasswordTMPInput.text == "")
        {
            GameManager.Instance.DisplayErrorPanel("Please fill up all fields");
            return;
        }

        if (!RegisterEmailTMPInput.text.Contains('@') || !RegisterEmailTMPInput.text.Contains(".com"))
        {
            GameManager.Instance.DisplayErrorPanel("Please enter a valid email address");
            return;
        }

        if (RegisterUsernameTMPInput.text.Length < 3 || RegisterUsernameTMPInput.text.Length > 20)
        {
            GameManager.Instance.DisplayErrorPanel("Username must be 3-20 characters only");
            return;
        }

        if (RegisterPasswordTMPInput.text != RegisterConfirmPasswordTMPInput.text)
        {
            GameManager.Instance.DisplayErrorPanel("Passwords do not match.");
            return;
        }

        if (RegisterPasswordTMPInput.text.Length < 6 || RegisterPasswordTMPInput.text.Length > 100)
        {
            GameManager.Instance.DisplayErrorPanel("Password must be 6-100 characters only");
            return;
        }

        if (GameManager.Instance.DebugMode)
        {
            HideRegisterPanel();
            CurrentMainMenuState = MainMenuStates.LOGIN;
        }
        else
            RegisterUserPlayFab();
    }

    private void RegisterUserPlayFab()
    {
        GameManager.Instance.LoadingPanel.SetActive(true);
        RegisterPlayFabUserRequest registerPlayFabUser = new RegisterPlayFabUserRequest
        {
            Username = RegisterUsernameTMPInput.text,
            DisplayName = RegisterUsernameTMPInput.text,
            Email = RegisterEmailTMPInput.text,
            Password = RegisterPasswordTMPInput.text
        };

        PlayFabClientAPI.RegisterPlayFabUser(registerPlayFabUser,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                InitializeUserDataPlayFab();
            },
            errorCallback => ErrorCallback(errorCallback.Error, RegisterUserPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void InitializeUserDataPlayFab()
    {
        UpdateUserDataRequest updateUserData = new UpdateUserDataRequest();
        updateUserData.Data = new Dictionary<string, string>
        {
            { "CurrentLevel", "1" },
            { "GUID", "" },
            { "LevelStars", "[]" }
        };

        PlayFabClientAPI.UpdateUserData(updateUserData,
            resultCallback =>
            {
                failedCallbackCounter = 0;
                GameManager.Instance.LoadingPanel.SetActive(false);
                HideRegisterPanel();
                CurrentMainMenuState = MainMenuStates.LOGIN;
            },
            errorCallback => ErrorCallback(errorCallback.Error, InitializeUserDataPlayFab, () => GameManager.Instance.DisplayErrorPanel(errorCallback.GenerateErrorReport())));
    }

    private void ResetRegisterFields()
    {
        RegisterEmailTMPInput.text = "";
        RegisterUsernameTMPInput.text = "";
        RegisterPasswordTMPInput.text = "";
        RegisterConfirmPasswordTMPInput.text = "";
    }
    #endregion

    #region MAIN MENU
    public void InitializePlayerData()
    {
        for (int i = 0; i < AllLessons.Count; i++)
        {
            if (i < PlayerData.CurrentLessonIndex)
            {
                AllLessons[i].UnlockThisLesson();
                AllLessons[i].GetComponent<Button>().interactable = true;
                if (PlayerData.GetLevelStar(i + 1) != null)
                    AllLessons[i].SetStarCount(PlayerData.GetLevelStar(i + 1).LevelStars);
                else
                    AllLessons[i].HideStars();
            }
            else
            {
                AllLessons[i].LockThisLesson();
                AllLessons[i].GetComponent<Button>().interactable = false;
                AllLessons[i].HideStars();
            }
        }
    }
    #endregion

    #region LESSON SELECT
    public void DisplayPopUpLessonPanel()
    {
        PopUpLessonPanel.SetActive(true);
        LessonDetailsTMP.text = SelectedLessonData.LessonDetails;
        CoinRewardTMP.text = "Coin Reward: " + SelectedLessonData.CoinReward.ToString("n0");
        StarQuotaTMP.text = "Minimum Stars Required: " + SelectedLessonData.StarQuota.ToString("n0");
        if (SelectedLessonData.LessonIndex == PlayerData.CurrentLessonIndex)
            SelectLevelButtonTMP.text = "LEARN";
        else
            SelectLevelButtonTMP.text = "PLAY";
    }
    public void ClosePopUpLessonPanel()
    {
        PopUpLessonPanel.SetActive(false);
        SelectedLessonData = null;
    }
    public void BeginSelectedLesson()
    {
        GameManager.Instance.CurrentLesson = SelectedLessonData;

        if(SelectedLessonData.StarQuota > PlayerData.GetTotalStars())
        {
            GameManager.Instance.DisplayErrorPanel("You must have a total of at least " + SelectedLessonData.StarQuota + " stars to play this level. You only have " + PlayerData.GetTotalStars() + " stars.");
            return;
        }

        if (SelectedLessonData.LessonIndex == PlayerData.CurrentLessonIndex || PlayerData.EnergyCount == 0)
        {
            GameManager.Instance.AudioManager.KillBackgroundMusic();
            GameManager.Instance.SceneController.CurrentScene = "DiscussionScene";
        }
        else
            GameManager.Instance.SceneController.CurrentScene = "CombatScene";
    }
    #endregion

    #region SETTINGS
    public void SetInitialVolume()
    {
        SettingsManager.SetInitialVolume();
        GameManager.Instance.AudioManager.SetBackgroundMusic(MainMenuBGMusic);
    }

    public void LogOutButton()
    {
        PlayerPrefs.DeleteKey("Username");
        PlayerPrefs.DeleteKey("Password");
        PlayerData.ResetData();
        SettingsManager.ResetAudioSettings();
        HideSettingsPanel();
        CurrentMainMenuState = MainMenuStates.LOGIN;
    }

    #region QUIT
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
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
