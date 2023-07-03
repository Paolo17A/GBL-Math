using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WardrobeController : MonoBehaviour
{
    [SerializeField] private WardrobeCore WardrobeCore;

    private void Awake()
    {
        GameManager.Instance.SceneController.GetActionLoadingList.Clear();
        GameManager.Instance.SceneController.AddActionLoadinList(WardrobeCore.GetUserDataCoroutine());

        GameManager.Instance.SceneController.ActionPass = true;
    }
}
