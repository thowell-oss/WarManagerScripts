
using System;
using System.Collections;
using System.Collections.Generic;

using StringUtility;

using UnityEngine;

using WarManager.Sharing.Security;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles the tag bubbles and their object pooling system
    /// </summary>
    [Notes.Author(1.2, "Handles the tag bubbles and their object pooling system")]
    public class TagsBubblesManager : MonoBehaviour
    {
        /// <summary>
        /// The object prefab to pool
        /// </summary>
        [SerializeField] TagsBubbleButtonHandler BubblePrefab;

        /// <summary>
        /// The parent transform object
        /// </summary>
        [SerializeField] Transform _parentTransform;

        /// <summary>
        /// How many can be on at a time?
        /// </summary>
        [SerializeField] int _visibleBubbleLimit = 3;


        /// <summary>
        /// The limit of how many bubbles are visible at one time
        /// </summary>
        /// <value></value>
        public int VisibleBubbleLimit
        {
            get => _visibleBubbleLimit;
            set
            {
                if (value >= 0)
                {
                    _visibleBubbleLimit = value;
                }
            }
        }

        /// <summary>
        /// The active bubbles
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="_activeBubbles"></param>
        /// <param name="obj"></param>
        /// <param name="DisabledBubbles"></param>
        /// <typeparam name="(GameObject"></typeparam>
        /// <typeparam name="TMPro.TMP_Text)"></typeparam>
        /// <returns></returns>
        [SerializeField] List<TagsBubbleButtonHandler> _activeBubbles = new List<TagsBubbleButtonHandler>();
        private Queue<TagsBubbleButtonHandler> DisabledBubbles = new Queue<TagsBubbleButtonHandler>();

        /// <summary>
        /// Set a tag for each bubble
        /// </summary>
        /// <param name="tags">the array of tags</param>
        public void SetTags(Dictionary<string, Action> tags, bool useQuotes = true)
        {

            if (_activeBubbles.Count > 0)
            {
                ClearBubbles();
            }

            if (tags == null || tags.Count < 1)
                return;

            if (tags.Count <= VisibleBubbleLimit)
            {
                PopulateTagsLessThanOrEqualToActiveLimit(tags, useQuotes);
            }
            else
            {
                PopulateTagsGreaterThanActiveLimit(tags, useQuotes);
            }
        }

        /// <summary>
        /// Populate tags less than or equal to the active limit
        /// </summary>
        /// <param name="tags">the array of tags (string)</param>
        /// <param name="useQuotes">the use quotes boolean</param>
        private void PopulateTagsLessThanOrEqualToActiveLimit(Dictionary<string, Action> tags, bool useQuotes)
        {
            foreach (var x in tags)
            {
                EnableBubble(x.Key, useQuotes, x.Value);
            }
        }

        /// <summary>
        /// populate tags greater than the active limit
        /// </summary>
        /// <param name="tags">the tags and actions that will be displayed</param>
        /// <param name="useQuotes">the quotes</param>
        private void PopulateTagsGreaterThanActiveLimit(Dictionary<string, Action> tags, bool useQuotes)
        {

            int i = 0;

            foreach (var x in tags)
            {
                if (i < VisibleBubbleLimit)
                {
                    EnableBubble(x.Key, useQuotes, x.Value);
                }

                i++;
            }

            EnableBubble($"+{tags.Count - VisibleBubbleLimit}", false, () => { ShowAllTags(tags); });
        }

        /// <summary>
        /// remove a bubble from the disabled queue, add text and place the bubble in the active queue
        /// </summary>
        /// <param name="bubbleText">the text to add</param>
        private void EnableBubble(string bubbleText, bool useQuotes, Action btnAction)
        {

            if (useQuotes)
                bubbleText = bubbleText.SetStringQuotes();

            if (DisabledBubbles.Count <= 0)
                CreateNewBubble(1);

            var bubble = DisabledBubbles.Dequeue();

            bubble.SetActive(true);
            bubble.SetBubble(bubbleText, btnAction);
            _activeBubbles.Add(bubble);
        }


        /// <summary>
        /// Clear all active bubbles and put them in the disabled queue
        /// </summary>
        private void ClearBubbles()
        {
            for (int i = _activeBubbles.Count - 1; i >= 0; i--)
            {
                _activeBubbles[i].SetActive(false);
                DisabledBubbles.Enqueue(_activeBubbles[i]);
                _activeBubbles.RemoveAt(i);
            }
        }


        /// <summary>
        /// Instantiate a bubble and put it in the disabled queue
        /// </summary>
        /// <param name="amt">the amount of bubbles to add</param>
        private void CreateNewBubble(int amt)
        {

            for (int i = 0; i < amt; i++)
            {
                TagsBubbleButtonHandler go = Instantiate<TagsBubbleButtonHandler>(BubblePrefab, _parentTransform);
                TMPro.TMP_Text text = go.transform.GetChild(0).GetComponentInChildren<TMPro.TMP_Text>();

                DisabledBubbles.Enqueue(go);
            }
        }

        private void ShowAllTags(Dictionary<string, Action> tags)
        {

            List<string> tagList = new List<string>();
            foreach(var x in tags)
            {
                tagList.Add(x.Key);
            }

            PermissionsUI permissionsUI = new PermissionsUI();
            permissionsUI.ShowPermissionsByName(tagList, "References", ActiveSheetsDisplayer.main.ViewReferences);
        }
    }
}
