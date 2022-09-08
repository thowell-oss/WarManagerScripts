using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingLastSessionGUI : MonoBehaviour
{

    [SerializeField] Transform startLocation;
    [SerializeField] Transform endLocation;

    [SerializeField] private float speed = .125f;

    [SerializeField] private TMPro.TMP_Text greeting;

    [SerializeField] List<GameObject> _items = new List<GameObject>();

    [SerializeField] RectTransform Background;

    [SerializeField] Vector2 OpenScale;
    [SerializeField] Vector2 ClosedScale;

    void Awake()
    {
        this.transform.position = endLocation.position;
        //LeanTween.move(this.gameObject, endLocation.transform.position, speed).setEaseInOutCubic();

        if (WarManager.WarSystem.CurrentActiveAccount != null)
            greeting.text = $"Hey {WarManager.WarSystem.CurrentActiveAccount.FirstName}";

        LeanTween.value(Background.gameObject, TweenBackgroundScale, Background.sizeDelta, OpenScale, speed / 4).setEaseInOutBack();

        LeanTween.delayedCall(speed / 2, () =>
        {
            foreach (var x in _items)
            {
                x.SetActive(true);
            }
        });

        LeanTween.delayedCall(speed * 1.5f, () =>
        {
            LeanTween.value(Background.gameObject, TweenBackgroundScale, Background.sizeDelta, ClosedScale, speed / 2).setEaseInOutBack();

            foreach (var x in _items)
            {
                x.SetActive(false);
            }
        });
    }

    private void TweenBackgroundScale(Vector2 scale)
    {
        Background.sizeDelta = scale;
    }
}
