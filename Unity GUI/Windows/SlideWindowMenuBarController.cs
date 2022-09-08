using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Unity3D.Windows;
using WarManager;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles the slide window actions")]
    public class SlideWindowMenuBarController : MonoBehaviour
    {
        [SerializeField] Transform openTransform;
        [SerializeField] Transform closeTransform;
        [SerializeField] float TranslateSpeed = .25f;

        void Start()
        {
            this.transform.position = closeTransform.position;
            Open();
        }

        public void Open()
        {
            if (LeanTween.isTweening(this.gameObject))
                LeanTween.cancel(this.gameObject);

            LeanTween.move(this.gameObject, openTransform.position, TranslateSpeed).setEaseOutCubic();
        }

        public void Close()
        {
            SlideWindowsManager.main.CloseWindows(true);

            if (LeanTween.isTweening(this.gameObject))
            {
                LeanTween.cancel(this.gameObject);
            }

            LeanTween.delayedCall(.125f, () =>
            {
                LeanTween.move(this.gameObject, closeTransform.position, TranslateSpeed).setEaseOutCubic();
            });
        }

        public void SetOpenClose(bool open)
        {
            if (open)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        void OnDisable()
        {
            this.transform.position = closeTransform.position;
        }
    }
}
