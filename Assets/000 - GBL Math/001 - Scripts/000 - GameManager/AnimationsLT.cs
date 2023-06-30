using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationsLT : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("PANELS ANIMATION")]
    [SerializeField] private float animationSpeed;
    [SerializeField] private LeanTweenType easeType;

    [Header("BUTTON ANIMATION")]
    [SerializeField] private float speedButton;
    [SerializeField] private LeanTweenType easeTypeButton;

    #region PANELS
    public void ShowHidePanel(RectTransform objToShow, RectTransform objToHide, Vector2 startPosObjToShow)
    {
        objToShow.anchoredPosition = startPosObjToShow;
        objToShow.gameObject.SetActive(true);

        LeanTween.move(objToShow, Vector2.zero, animationSpeed).setEase(easeType).setOnComplete(() => 
        {
            objToHide.gameObject.SetActive(false);
            gameManager.CanUseButtons = true;
        });
    }

    public void HideShowPanel(RectTransform objToShow, RectTransform objToHide, Vector2 destPosObjToHide)
    {
        objToShow.anchoredPosition = Vector3.zero;
        objToShow.gameObject.SetActive(true);

        LeanTween.move(objToHide, destPosObjToHide, animationSpeed).setEase(easeType).setOnComplete(() => 
        {
            objToHide.gameObject.SetActive(false);
            gameManager.CanUseButtons = true;
        });
    }

    public void ShowHideSlide(RectTransform objToShow, RectTransform objToHide, Vector2 destPosObjToHide, Action action)
    {
        objToShow.anchoredPosition = new Vector3(-375f, 0f, 0f);
        objToShow.gameObject.SetActive(true);

        LeanTween.move(objToHide, destPosObjToHide, animationSpeed).setEase(easeType);

        LeanTween.move(objToShow, Vector2.zero, animationSpeed).setEase(easeType).setOnComplete(() =>
        {
            gameManager.CanUseButtons = true;
            action?.Invoke();
            objToHide.gameObject.SetActive(false);
        });
    }

    public void HideShowSlide(RectTransform objToShow, RectTransform objToHide, Vector2 destPosObjToHide, Action action)
    {
        objToShow.anchoredPosition = new Vector3(375f, 0f, 0f);
        objToShow.gameObject.SetActive(true);

        LeanTween.move(objToHide, destPosObjToHide, animationSpeed).setEase(easeType);

        LeanTween.move(objToShow, Vector2.zero, animationSpeed).setEase(easeType).setOnComplete(() =>
        {
            gameManager.CanUseButtons = true;
            action?.Invoke();
            objToHide.gameObject.SetActive(false);
        });
    }

    public void ShowSlide(RectTransform objToShow, Vector2 startPosition, Vector2 destination, Action action)
    {
        objToShow.anchoredPosition = startPosition;
        objToShow.gameObject.SetActive(true);

        LeanTween.move(objToShow, destination, animationSpeed).setEase(easeType).setOnComplete(() =>
        {
            gameManager.CanUseButtons = true;
            action?.Invoke();
        });

    }

    public void FadePanel(RectTransform objToShow, RectTransform objToHide, CanvasGroup cg, float from, float to, Action action)
    {
        //objToShow.anchoredPosition = Vector2.zero;
        cg.alpha = from;
        objToShow.gameObject.SetActive(true);

        LeanTween.alphaCanvas(cg, to, animationSpeed).setEase(easeType).setOnComplete(() =>
        {
            gameManager.CanUseButtons = true;
            action?.Invoke();
            if(objToHide != null)
                objToHide.gameObject.SetActive(false);
        });
    }

    #endregion

    #region BUTTONS

    public void FillAnimation(Image image, float from, float to, Action action)
    {
        LeanTween.value(image.gameObject, f => image.fillAmount = f, from, to, speedButton).setEase(easeTypeButton).setOnComplete(() => 
        {
            action?.Invoke();
        });
    }

    public void ChangeButtonColor(Image image, Color from, Color to, Action action)
    {
        LeanTween.value(image.gameObject, c => image.color = c, from, to, speedButton).setEase(easeTypeButton).setOnComplete(() => 
        {
            action?.Invoke();
        });
    }

    #endregion
}
