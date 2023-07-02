using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HatData", menuName = "Stat 7/Data/HatData")]
public class HatData : ScriptableObject
{
    [field: Header("HAT DATA")]
    [field: SerializeField] public string HatID { get; set; }
    [field: SerializeField] public string HatName { get; set; }
    [field: SerializeField] public Sprite HatSprite { get; set; }
    [field: SerializeField] public int HatPrice { get; set; }
}
