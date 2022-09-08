
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Backend;

using UnityEngine.UI;

namespace WarManager.Unity3D
{
    /// <summary>
    /// behavior for a sheet cluster card
    /// </summary>
    [Notes.Author("Behavior for a sheet cluster card")]
    public class SheetCardClusterContextCard : PoolableObject
    {
        [SerializeField] SpriteRenderer Renderer;
        [SerializeField] BoxCollider2D Collider;
        [SerializeField] SpriteRenderer IconImage;

        [SerializeField] TMPro.TMP_Text MessageText;

        [Space]
        [SerializeField]
        Color Default, Hover;

        private Action<Point> _buttonPressEventCallback;

        private Point _location;

        void Start()
        {
            SetColor(Default);
        }

        /// <summary>
        /// Set the card location, icon and callback
        /// </summary>
        /// <param name="p">the point</param>
        /// <param name="icon">the icon </param>
        /// <param name="buttonPressEventCallback">the button callback</param>
        public void SetCard(Point p, Sprite icon, Color col, Action<Point> buttonPressEventCallback)
        {
            if (icon == null)
                throw new NullReferenceException("the icon cannot be null");

            if (buttonPressEventCallback == null)
                throw new NullReferenceException("the button call cannot be null");

            if (PlaceCard(p, buttonPressEventCallback))
                SetImage(icon, col);

        }

        /// <summary>
        /// Set the card
        /// </summary>
        /// <param name="p">the location that the card needs to be at</param>
        /// <param name="message">the message that the card will display</param>
        /// <param name="buttonPressEventCallback">the event call back</param>
        /// <param name="messageColor">the color of the message</param>
        public void SetCard(Point p, string message, Color messageColor, Action<Point> buttonPressEventCallback)
        {
            if (message == null || message.Trim() == string.Empty)
                message = "<empty>";

            if (PlaceCard(p, buttonPressEventCallback))
                SetMessage(message, messageColor);

        }


        /// <summary>
        /// place the card
        /// </summary>
        /// <param name="p"></param>
        /// <param name="buttonPressEventCallback"></param>
        /// <returns></returns>
        private bool PlaceCard(Point p, Action<Point> buttonPressEventCallback)
        {
            if (SheetsManager.CurrentSheetID != null)
            {
                PlaceCard(p, SheetsManager.CurrentSheetID);

                _buttonPressEventCallback = buttonPressEventCallback;
                _location = p;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Place the card
        /// </summary>
        /// <param name="p">the point</param>
        /// <param name="sheetID">the sheet id</param>
        private void PlaceCard(Point p, string sheetID)
        {
            if (sheetID == null)
                throw new NullReferenceException("sheet id cannot be null");

            if (sheetID.Trim().Length < 1)
                throw new ArgumentException("sheet id is empty");

            if (sheetID != SheetsManager.CurrentSheetID)
                return;

            var grid = SheetsManager.GetWarGrid(sheetID);

            var loc = Point.GridToWorld(p, grid);
            transform.position = loc.ToVector3();

            Renderer.size = grid.GridScale.ToVector2() / 2;
            Collider.size = grid.GridScale.ToVector2() / 2;
        }


        /// <summary>
        /// Set the image of the card
        /// </summary>
        /// <param name="sprite">the sprite to show in the card</param>
        /// <param name="col">the color</param>
        private void SetImage(Sprite sprite, Color col)
        {
            MessageText.text = "";

            IconImage.gameObject.SetActive(true);
            IconImage.sprite = sprite;
            IconImage.color = col;
        }

        /// <summary>
        /// Set the message of the card
        /// </summary>
        /// <param name="message">the message to display</param>
        /// <param name="color">the color</param>
        private void SetMessage(string message, Color color)
        {
            IconImage.gameObject.SetActive(false);

            MessageText.text = message;
            MessageText.color = color;
        }

        void OnMouseUp()
        {
            //Debug.Log("Mouse Down");

            if (_buttonPressEventCallback != null && _location.IsInGridBounds)
            {
                _buttonPressEventCallback(_location);
            }
        }

        void OnMouseEnter()
        {
            LeanTween.cancel(this.gameObject);
            LeanTween.value(this.gameObject, SetColor, Renderer.color, Hover, .125f);
        }

        void OnMouseExit()
        {
            LeanTween.cancel(this.gameObject);
            LeanTween.value(this.gameObject, SetColor, Renderer.color, Default, .125f);
        }

        private void SetColor(Color color)
        {
            //Renderer.color = color;
            IconImage.color = color;
        }

        void OnDisable()
        {
            //Debug.Log("disabled");
        }
    }
}
