using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "LessonData", menuName = ("Stat 7/Data/LessonData"))]
public class LessonData : ScriptableObject
{
    [field: Header("LESSON DATA")]
    [field: SerializeField] public int LessonIndex { get; set; }
    [field: SerializeField][field: TextArea(minLines: 5, maxLines: 10)] public string LessonDetails{ get; set; }
    [field: SerializeField] public int CoinReward { get; set; }
    [field: SerializeField] public int StarQuota { get; set; }

    [field: Header("DISCUSSION DATA")]
    [field: SerializeField][field: TextArea] public List<string> IntroductoryMessages { get; set; }
    [field: SerializeField] public float IntroTypeSpeed { get; set; }
    [field: SerializeField] public VideoClip LessonVideo { get; set; }
     
    [field: Header("GAME DATA")]
    [field: SerializeField] public GameObject EnemyPrefab { get; set; }
    [field: SerializeField] public List<QuestionData> LessonQuestions { get; set; }
}
