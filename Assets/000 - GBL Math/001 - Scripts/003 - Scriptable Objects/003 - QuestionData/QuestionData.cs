using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionData", menuName = "Stat 7/Data/QuestionData")]
public class QuestionData : ScriptableObject
{
    public enum QuestionType { NONE, TEXT, IMAGE, NUMBER }

    [field: Header("UNIVERSAL")]
    [field: SerializeField] public QuestionType ThisQuestionType;
    [field: SerializeField][field: TextArea(minLines: 5, maxLines:10)] public string Question;
    [field: SerializeField] public List<string> Choices;
    [field: SerializeField] public string Answer;

    [field: Header("IMAGE")]
    [field: SerializeField] public List<Sprite> ImageChoices;
    [field: SerializeField] public int ImageAnswerIndex;

    [field: Header("NUMBER")]
    [field: SerializeField] public float NumberAnswer;

}
