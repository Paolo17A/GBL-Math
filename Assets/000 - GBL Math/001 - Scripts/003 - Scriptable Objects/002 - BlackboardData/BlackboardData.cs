using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlackboardData", menuName = "Stat 7/Data/BlackboardData")]
public class BlackboardData : ScriptableObject
{
    public enum BlackBoardType { NONE, TEXT, IMAGE}
    [field: SerializeField] public BlackBoardType ThisBlackboardType { get; set; }
    [field: SerializeField] public bool IsTopicBeginning { get; set; }

    [field: Header("TEXT TYPE ONLY")]
    [field: SerializeField][field: TextArea(minLines: 10, maxLines: 20)] public string LessonText { get; set; }
    [field: SerializeField] public float LessonWriteSpeed { get; set; }
    [field: SerializeField] public AudioClip DialogueVoiceOver { get; set; }

}
