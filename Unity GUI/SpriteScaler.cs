using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
    [Notes.Author("Handles scaling a sprite with the camera")]
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteScaler : MonoBehaviour
    {
        public Vector2 SpriteSizeBounds = new Vector2(1, 5);
        public float SpriteSizeMultiplier = 1;

        private SpriteRenderer _sprite;

        [SerializeField] private Vector2 _startingSize;

        [SerializeField] private WarManagerCameraController _cameraController;
        [SerializeField] private bool _animate = true;

        private bool started = false;

        /// <summary>
        /// Starts the timer upon being enabled
        /// </summary>
        /// <returns></returns>
        void Start()
        {

            if (SpriteSizeBounds.x > SpriteSizeBounds.y)
            {
                throw new System.Exception("The sprite size bounds lower bound (x) cannot be larger than the larger bound (y)");
            }

            _sprite = GetComponent<SpriteRenderer>();

            if (_sprite == null)
                throw new System.NullReferenceException("The sprite is null");

            _startingSize = transform.localScale;

            StartCoroutine(UpdateSpriteTimer());

            started = true;
        }



        /// <summary>
        /// The timer that periodically updates the size of the sprite
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateSpriteTimer()
        {
            transform.localScale = _startingSize;

            while (true)
            {
                yield return new WaitForSecondsRealtime(.25f);
                UpdateSpriteSize();
            }
        }

        /// <summary>
        /// Update the sprite size according to the camera
        /// </summary>
        private void UpdateSpriteSize()
        {


            if (_cameraController == null)
                _cameraController = WarManagerCameraController.MainController;

            float newSize = _cameraController.GetCamera.orthographicSize;

            var iconSize = new Vector2(_startingSize.x + newSize / SpriteSizeMultiplier, _startingSize.y + newSize / SpriteSizeMultiplier);

            iconSize = new Vector2(Mathf.Clamp(iconSize.x, SpriteSizeBounds.x, SpriteSizeBounds.y), Mathf.Clamp(iconSize.y, SpriteSizeBounds.x, SpriteSizeBounds.y));

            if (_animate)
            {
                if (LeanTween.isTweening(this.gameObject))
                    LeanTween.cancel(this.gameObject);

                LeanTween.value(this.gameObject, AnimateScale, (Vector2)transform.localScale, iconSize, .25f).setEaseOutCubic();
            }
            else
            {
                transform.localScale = iconSize;
            }
        }

        /// <summary>
        /// Animate the scale of the sprite
        /// </summary>
        /// <param name="size"></param>
        private void AnimateScale(Vector2 size)
        {
            transform.localScale = size;
        }

        void OnDisable()
        {
            StopCoroutine(UpdateSpriteTimer());
        }

        void OnEnable()
        {
            if (started)
                StartCoroutine(UpdateSpriteTimer());
        }
    }
}
