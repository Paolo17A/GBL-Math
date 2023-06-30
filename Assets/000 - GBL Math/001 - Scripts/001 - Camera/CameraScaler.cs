using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraScaler : MonoBehaviour 
{
    [SerializeField] float leftRatio = 16.0f, rightRatio = 9.0f;
    [SerializeField] private bool isOrthographicCamera, debugging;

    [SerializeField] private TextMeshProUGUI debugger;

    float targetaspect;
    float windowaspect;

    float scaleheight;

    int width;
    int height;

    Camera cameraPlayer;

    // Use this for initialization
    void Awake ()
    {
        cameraPlayer = GetComponent<Camera>();

        if (isOrthographicCamera)
            cameraPlayer.orthographic = true;
        else
            cameraPlayer.orthographic = false;

        ScaleWithScreenSize();
    }

    private void Update()
    {
        ScaleWithScreenSize(); 
    }

    private void ScaleWithScreenSize()
    {
        targetaspect = leftRatio / rightRatio;

        if (debugging)
            StartCoroutine(ChangeScreen());

        width = Screen.width;
        height = Screen.height;

        windowaspect = (float)Screen.width / (float)Screen.height;
        scaleheight = windowaspect / targetaspect;

        if (scaleheight < 1.0f)
        {
            Rect rect = cameraPlayer.rect;

            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;

            cameraPlayer.rect = rect;
        }
        else // add pillarbox
        {
            float scalewidth = 1.0f / scaleheight;

            Rect rect = cameraPlayer.rect;

            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;

            cameraPlayer.rect = rect;
        }
    }

    IEnumerator ChangeScreen()
    {
        debugger.text = "Change screen";
        yield return new WaitForSecondsRealtime(5f);

        debugger.text = "";
    }
}
