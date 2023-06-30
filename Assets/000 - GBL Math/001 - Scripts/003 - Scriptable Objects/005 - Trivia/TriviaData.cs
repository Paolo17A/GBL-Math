using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TriviaData", menuName = ("Stat 7/Data/TriviaData"))]
public class TriviaData : ScriptableObject
{
    [field: SerializeField][field: TextArea(minLines: 5, maxLines: 10)] public string Trivia { get; set; }
}
