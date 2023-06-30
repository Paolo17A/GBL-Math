using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    //============================================================================
    [field: Header("PANELS")]
    [field: SerializeField] private GameObject FrontPanel { get; set; }
    [field: SerializeField] private GameObject LastPanel { get; set; }

    [field: Header("CONSTANTS")]
    [field: SerializeField] private float windowConstant;
    [field: SerializeField] private float spawnConstant;

    [field: Header("DEBUGGER")]
    [field: SerializeField][field: ReadOnly] private GameObject holder;
    [field: SerializeField][field: ReadOnly] private GameObject stageHolder;
    //============================================================================

    void Update()
    {
        FrontPanel.transform.Translate(Vector3.left * 2 * Time.deltaTime);
        LastPanel.transform.Translate(Vector3.left * 2 * Time.deltaTime);
        if (LastPanel.transform.position.x <= windowConstant)
        {
            FrontPanel.transform.position = new Vector3(spawnConstant, FrontPanel.transform.position.y, FrontPanel.transform.position.z);
            holder = FrontPanel;
            FrontPanel = LastPanel;
            LastPanel = holder;
            holder = null;
        }
    }
}
