using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Stat 7/Data/DialogueData")]
public class DialogueData : ScriptableObject
{
    [field: Header("DIALOGUE DATA")]
    [field: SerializeField] public string DialogueSpeaker { get; set; }
    [field: SerializeField][field: TextArea(minLines:5, maxLines: 10)] public string DialogueContent { get; set; }
    [field: SerializeField] public float TypeSpeed { get; set; }
}
