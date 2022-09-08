
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WarManager.Cards;
using WarManager.Backend;

namespace WarManager.Unity3D
{
    [RequireComponent(typeof(Image))]
    public class MenuBarDragCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler
    {
        public UnityCardDisplay UnityCardPrefab;

        public Outline outline;

        public Color defaultColor = Color.white;
        public Color HoverColor = Color.red;

        public UnityEvent OnUnityDrag;

        private bool recievedObject = false;

        public void Start()
        {
            defaultColor = outline.effectColor;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!recievedObject)
            {
                if (OnUnityDrag != null)
                    OnUnityDrag.Invoke();

                string id = SheetsManager.CurrentSheetID;
                var layer = SheetsManager.GetActiveSheet(id).CurrentLayer;

                Debug.Log("OnDrag Object (menu bar Drag Card)");
                recievedObject = true;
                Card c = new Card(new Point(1, -1), Guid.NewGuid().ToString(), id, layer.ID);

                WarManagerDriver.Main.SpawnCard(UnityCardPrefab, c);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            recievedObject = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            outline.effectColor = HoverColor;
            outline.effectDistance = new Vector2(6, 6);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            outline.effectColor = defaultColor;
            outline.effectDistance = new Vector2(2, 2);
        }
    }
}