using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/* The GameManager is the central core of the game. It persists all throughout run-time 
 * and stores universal game objects and variables that need to be used in multiple scenes. */
public class GameManager : MonoBehaviour
{
    #region VARIABLES
    //===========================================================
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

                if (_instance == null)
                    _instance = new GameObject().AddComponent<GameManager>();
            }

            return _instance;
        }
    }

    public enum Result { NONE, VICTORY, DEFEAT };
    [field: SerializeField] public List<GameObject> GameMangerObj { get; set; }

    [field: SerializeField] public bool DebugMode { get; set; }
    [SerializeField] private string SceneToLoad;
    [field: SerializeField][field: ReadOnly] public bool CanUseButtons { get; set; }
    [SerializeField] public bool CheatsActivated;

    [field: Header("CAMERA")]
    [field: SerializeField] public Camera MainCamera { get; set; }
    [field: SerializeField] public Camera MyUICamera { get; set; }

    [field: Header("MISCELLANEOUS SCRIPTS")]
    [field: SerializeField] public SceneController SceneController { get; set; }
    [field: SerializeField] public AnimationsLT AnimationsLT { get; set; }
    [field: SerializeField] public AudioManager AudioManager { get; set; }
    [field: SerializeField] public LessonData CurrentLesson { get; set; }
    [field: SerializeField] public List<LessonData> AllLessons { get; set; }
    [SerializeField] private PlayerData PlayerData;

    [field: Header("LOADING")]
    [field: SerializeField] public GameObject LoadingPanel { get; set; }

    [Header("ERROR")]
    [SerializeField] private GameObject ErrorPanel;
    [SerializeField] private TextMeshProUGUI ErrorTMP;
    int failedCallbackCounter;
    //===========================================================
    #endregion

    #region CONTROLLER FUNCTIONS
    private void Awake()
    {
        PlayerData.ResetData();
        if (_instance != null)
        {
            for (int a = 0; a < GameMangerObj.Count; a++)
                Destroy(GameMangerObj[a]);
        }

        for (int a = 0; a < GameMangerObj.Count; a++)
            DontDestroyOnLoad(GameMangerObj[a]);
    }

    private void Start()
    {
        if (DebugMode)
            SceneController.CurrentScene = SceneToLoad;
        else
            SceneController.CurrentScene = "MainMenuScene";
    }


    #region ERROR
    public void DisplayErrorPanel(string _message)
    {
        LoadingPanel.SetActive(false);
        ErrorPanel.SetActive(true);
        ErrorTMP.text = _message;
    }

    public void HideErrorPanel()
    {
        LoadingPanel.SetActive(false);
        ErrorPanel.SetActive(false);
    }
    #endregion

    private void OnApplicationQuit()
    {
        PlayerData.ResetData();
    }
    #endregion
}
