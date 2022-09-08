using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public GameObject LoadingScreen;
    public TMPro.TMP_Text ContextText;

    public RectTransform Background;

    public List<GameObject> loadingElements = new List<GameObject>();

    public Image loadingSliderImage;


    /// <summary>
    /// The value of the slider to change between 0 and 1 to detect how much of the 'thing' has been loaded
    /// </summary>
    /// <value></value>
    public float LoadingSliderNormalized
    {
        get
        {
            return loadingSliderImage.fillAmount;
        }

        set
        {
            loadingSliderImage.fillAmount = value;
        }
    }

    private bool open = false;

    #region  singleton
    public static LoadingScreenManager main;

    public void Start()
    {
        main = this;

        // LeanTween.delayedCall(1, () =>
        // {
        //     AnimateOpen();
        // });

        // LeanTween.delayedCall(3, () =>
        // {
        //     AnimateClose();
        // });

    }

    #endregion

    public void SetLoadingScreen(bool active, string context)
    {
        if (active && !open)
        {
            AnimateOpen();
        }
        else
        {
            AnimateClose();
        }

        ContextText.text = context;
    }

    public void AnimateOpen()
    {
        LeanTween.cancel(gameObject);
        SetElementsActive(false);

        if (!LeanTween.isTweening(gameObject))
        {
            Background.gameObject.SetActive(true);
            Background.sizeDelta = new Vector2(10, 10);
            LeanTween.value(this.gameObject, (x) => { ScaleOnX(x); }, 10, 580, .75f).setEaseOutCubic();

            LeanTween.delayedCall(.5f, () =>
            {
                LeanTween.value(this.gameObject, (x) => { ScaleOnY(x); }, 10, 30, .25f).setEaseOutCubic();

                LeanTween.delayedCall(.25f, () =>
                {
                    SetElementsActive(true);
                    open = true;
                });
            });
        }
    }


    private void AnimateClose()
    {
        LeanTween.cancel(gameObject);
        SetElementsActive(false);

        LeanTween.value(this.gameObject, (x) => { ScaleOnY(x); }, 30, 5, .25f).setEaseOutCubic();

        LeanTween.delayedCall(.125f, () =>
            {
                LeanTween.value(this.gameObject, (x) => { ScaleOnX(x); }, 580, 0, .5f).setEaseOutCubic();
                open = false;
            });
    }


    private void ScaleOnY(float value)
    {
        if (Background == null)
            Background = GetComponent<RectTransform>();

        Background.sizeDelta = new Vector2(Background.sizeDelta.x, value);
    }

    private void ScaleOnX(float value)
    {
        if (Background == null)
            Background = GetComponent<RectTransform>();

        Background.sizeDelta = new Vector2(value, Background.sizeDelta.y);
    }

    private void SetElementsActive(bool active)
    {
        foreach (var obj in loadingElements)
        {
            obj.SetActive(active);
        }
    }
}
