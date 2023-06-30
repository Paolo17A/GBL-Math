using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleData", menuName = "Stat 7/Data/ModuleData")]
public class ModuleData : ScriptableObject
{
    [field: Header("MODULE VARIABLES")]
    [field: SerializeField][field: TextArea(minLines: 15, maxLines: 30)] public string LeftPageText;
    [field: SerializeField][field: TextArea(minLines: 15, maxLines: 30)] public string RightPageText;
}
