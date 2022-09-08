using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Cards;
using WarManager.Unity3D;

namespace WarManager
{
    /// <summary>
    /// The World Gismo
    /// </summary>
    [Notes.Author("Handles the world gismos")]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider))]
    public class WorldGismo : PoolableObject
    {
        private SpriteRenderer _renderer;
        private BoxCollider _collider;

        [SerializeField] Canvas _canvas;
        [SerializeField] TMPro.TMP_Text _text;

        public WarManagerCameraController cameraController { get; set; }

        private WarGrid _grid;
        private int _fontSize;

        /// <summary>
        /// update the gui
        /// </summary>
        void LateUpdate()
        {
            if (cameraController.GetCamera == null || _grid == null)
                return;

            _renderer.size = _grid.GridScale.ToVector2() + new Vector2(cameraController.GetCamera.orthographicSize / 20, cameraController.GetCamera.orthographicSize / 20);
            _collider.size = _grid.GridScale.ToVector2() + new Vector2(cameraController.GetCamera.orthographicSize / 20, cameraController.GetCamera.orthographicSize / 20);
            _text.fontSize = _fontSize * cameraController.GetCamera.orthographicSize / 20;

        }

        /// <summary>
        /// Place the gismo 
        /// </summary>
        /// <param name="p">point p</param>
        public void SetWorldGizmoSetWorldGismoOnCurrentSheet(Point p, WarGrid grid)
        {

            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();

            if (_collider == null)
                _collider = GetComponent<BoxCollider>();

            if (!SheetsManager.TryGetCurrentSheet(out var sheet))
                return;
            _grid = grid;

            Vector3 worldPntBelow = Point.GridToWorld(p + Point.down, grid).ToVector3() + Vector3.up * grid.GridScale.y / 2;
            Vector3 worldPnt = Point.GridToWorld(p, grid).ToVector3();

            LeanTween.value(this.gameObject, SetLocation, worldPntBelow, worldPnt, .5f).setEaseInOutCubic();

            _text.text = "";
            _renderer.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set the gismo message and color
        /// </summary>
        /// <param name="message">the message</param>
        /// <param name="color">the color</param>
        /// <param name="fontSize">the font size</param>
        public void SetWorldGismoContent(string message, Color color, int fontSize)
        {
            _renderer.enabled = false;

            _text.text = message;
            _text.color = color;
            _fontSize = fontSize;
            _text.fontSize = fontSize;
        }

        /// <summary>
        /// Set the gismo icon and color
        /// </summary>
        /// <param name="icon">the sprite icon</param>
        /// <param name="color">the selected color of the icon</param>
        public void SetWorldGismoContent(Sprite icon, Color color)
        {
            _renderer.enabled = true;
            _renderer.sprite = icon;
            _renderer.color = color;

            _text.text = string.Empty;
        }

        /// <summary>
        /// Set the location of the world gizmo
        /// </summary>
        /// <param name="pnt">the vector3 point that the gismo will be placed</param>
        private void SetLocation(Vector3 pnt)
        {
            transform.position = pnt;
            _canvas.transform.position = transform.position;
        }
    }
}
