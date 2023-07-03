using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Stat 7/Data/PlayerData")]
public class PlayerData : ScriptableObject
{
    public enum Gender { NONE, MALE, FEMALE }

    [field: Header("PLAYER DATA")]
    [field: SerializeField] public string PlayFabID { get; set; }
    [field: SerializeField] public string GUID { get; set; }
    [field: SerializeField] public Gender PlayerGender {  get; set; }
    [field: SerializeField] public int CurrentLessonIndex { get; set; }
    [field: SerializeField] public List<LevelStarsData> LevelStars { get; set; }

    [field: Header("SLIME DATA")]
    [field: SerializeField] public string SlimeCharacterID { get; set; }
    [field: SerializeField] public int SlimeMaxHealth { get; set; }
    [field: SerializeField] public OwnedHat EquippedHat { get; set; }

    [field: Header("INVENTORY")]
    [field: SerializeField] public int EnergyCount { get; set; }
    [field: SerializeField] public int CoinCount { get; set; }
    [field: SerializeField] public List<OwnedHat> OwnedHats { get; set; }


    [field: Header("VOLUME")]
    [field: SerializeField] public float BGMVolume { get; set; }
    [field: SerializeField] public float SFXVolume { get; set; }

    public void ResetData()
    {
        PlayFabID = "";
        GUID = "";
        PlayerGender = Gender.NONE;
        CurrentLessonIndex = 1;
        LevelStars.Clear();
        SlimeCharacterID = "";
        SlimeMaxHealth = 5;
        EnergyCount = 0;
        CoinCount = 0;
        OwnedHats.Clear();
    }

    #region LEVEL STARS
    [Serializable]
    public class LevelStarsData
    {
        public int LevelIndex;
        public int LevelStars;

        public LevelStarsData(int levelIndex, int levelStars)
        {
            LevelIndex = levelIndex;
            LevelStars = levelStars;
        }
    }

    public LevelStarsData GetLevelStar(int levelIndex)
    {
        foreach(var levelStar in LevelStars)
            if(levelStar.LevelIndex == levelIndex)
                return levelStar;
        
        return null;
    }

    public int GetTotalStars()
    {
        int total = 0;
        for(int i = 0; i < LevelStars.Count; i++)
        {
            total += LevelStars[i].LevelStars;
        }

        return total;
    }
    #endregion

    #region HATS
    [Serializable]
    public class OwnedHat
    {
        public string HatInstanceID;
        public HatData ThisHatData;

        public OwnedHat(string hatInstanceID, HatData thisHatData)
        {
            HatInstanceID = hatInstanceID;
            ThisHatData = thisHatData;
        }
    }

    public OwnedHat GetSelectedHat(string hatInstanceID)
    {
        foreach (OwnedHat hat in OwnedHats)
            if (hat.HatInstanceID == hatInstanceID)
                return hat;
        return null;
    }
    #endregion
}
