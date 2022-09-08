using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;

using WarManager.Unity3D.ObjectPooling;
using WarManager.Unity3D;
using WarManager.Unity3D.Windows;
using WarManager.Sharing;

namespace WarManager
{
    /// <summary>
    /// Handles calculating the locations of all context cards
    /// </summary>
    [Notes.Author("Handles calculating the locations of all context cards")]
    public class SheetCardClusterContextHandler : MonoBehaviour
    {

        private bool _activateCluster = true;
        public bool ActivateCluster
        {
            get => _activateCluster;
            set
            {
                _activateCluster = value;
            }
        }

        [SerializeField] private HashSet<Point> AddCardPoints = new HashSet<Point>();
        [SerializeField] private List<Point> SelectCardPoints = new List<Point>();

        [SerializeField] private SheetCardClusterContextCard Prefab;

        private List<SheetCardClusterContextCard> cards = new List<SheetCardClusterContextCard>();

        [SerializeField] private Sprite AddSprite, SelectRowSprite;

        [SerializeField] private Color AddColor, SelectRowColor;

        public int AddCardCount;
        public int ContextCardCount;

        void Start()
        {

            LeanTween.delayedCall(2, () =>
            {
                StartCoroutine(UpdateLocations());
            });
        }

        public static SheetCardClusterContextHandler Main;

        public void Awake()
        {
            if (Main == null)
                Main = this;
            else throw new System.ArgumentException("cannot have multiple static sheet card cluster context handlers");
        }

        /// <summary>
        /// update the locations every second
        /// </summary>
        /// <returns></returns>
        IEnumerator UpdateLocations()
        {
            while (true)
            {
                SetCards();
                yield return new WaitForSecondsRealtime(1);
            }
        }

        /// <summary>
        /// Set cards
        /// </summary>
        public void SetCards()
        {
            ClearPoints();

            if (!ActivateCluster)
            {
                ClearCards();
                return;
            }

            CalculateLocations();

            // for (int i = 0; i < AddCardPoints.Count; i++)
            // {
            //     SheetCardClusterContextCard card;

            //     if (i < cards.Count)
            //     {
            //         cards[i].SetCard(AddCardPoints[i], AddSprite, AddCard);
            //         card = cards[i];
            //     }
            //     else
            //     {
            //         var x = PoolManager.Main.CheckOutGameObject<SheetCardClusterContextCard>(Prefab.gameObject, false, this.transform);
            //         x.gameObject.SetActive(true);
            //         cards.Add(x);
            //         x.SetCard(AddCardPoints[i], AddSprite, AddCard);
            //         card = cards[i];
            //     }

            //     //Debug.DrawRay(card.transform.position, Vector3.up * .5f, Color.green, 1);
            // }

            while (cards.Count < AddCardPoints.Count)
            {
                var x = PoolManager.Main.CheckOutGameObject<SheetCardClusterContextCard>(Prefab.gameObject, false, this.transform);
                x.gameObject.SetActive(true);
                cards.Add(x);
            }

            int k = 0;
            foreach (var x in AddCardPoints)
            {

                cards[k].SetCard(x, AddSprite, AddColor, AddCard);
                k++;
            }

            if (AddCardPoints.Count < cards.Count)
            {
                for (int i = cards.Count - 1; i >= AddCardPoints.Count; i--)
                {
                    PoolManager.Main.TryCheckInObject(Prefab.gameObject, cards[i].gameObject, this.transform);
                }
            }
        }

        /// <summary>
        /// Calculate the locations of all the card cluster perimeter locations
        /// </summary>
        private void CalculateLocations()
        {
            if (!ActivateCluster)
            {
                return;
            }

            if (!SheetsManager.TryGetCurrentSheet(out var sheet))
                return;

            List<Card> cards = CardUtility.GetCardsFromCurrentSheet();


            foreach (var card in cards)
            {
                FindPoints(card.point);
            }

            List<Point> points = new List<Point>();
            points.AddRange(AddCardPoints);

            for (int i = points.Count - 1; i >= 0; i--)
            {
                if (cards.Find(y => y.point == points[i]) != null)
                {
                    points.RemoveAt(i);
                }
                else if (cards.Find(y => y.point == points[i] + Point.up) == null)
                {
                    if (cards.Find(y => y.point == points[i] + Point.northEast) != null)
                    {
                        points.RemoveAt(i);
                    }
                    else if (cards.Find(y => y.point == points[i] + Point.right) != null)
                    {
                        points.RemoveAt(i);
                    }
                }
            }

            AddCardPoints.Clear();

            foreach (var x in points)
            {
                AddCardPoints.Add(x);
            }

            // for (int i = SelectCardPoints.Count - 1; i >= 0; i--)
            // {
            //     if (cards.Find(y => y.point == SelectCardPoints[i]) != null)
            //     {
            //         SelectCardPoints.RemoveAt(i);
            //     }
            // }


            //worth the performance hit?

            // for (int i = AddCardPoints.Count - 1; i >= 0; i--)
            // {
            //     for (int k = AddCardPoints.Count - 1; k >= 0; k--)
            //     {
            //         if (i != k)
            //         {
            //             if (AddCardPoints[i] == AddCardPoints[k])
            //             {
            //                 cards.RemoveAt(k);
            //             }
            //         }
            //     }
            // }

            // for (int j = AddCardPoints.Count - 1; j >= 0; j--)
            // {
            //     if (SelectCardPoints.Contains(AddCardPoints[j]))
            //     {
            //         if (cards.Find(y => y.point == AddCardPoints[j] + Point.up) != null)
            //         {
            //             SelectCardPoints.Remove(AddCardPoints[j]);
            //         }
            //         else
            //         {
            //             SelectCardPoints.Remove(AddCardPoints[j]);
            //             AddCardPoints.RemoveAt(j);
            //         }
            //     }
            // }

#if UNITY_EDITOR

            //foreach (var x in AddCardPoints)
            //{
            //    var wordLoc = Point.GridToWorld(x, SheetsManager.GetWarGrid(sheet.ID));
            //    Debug.DrawRay(wordLoc.ToVector2(), Vector3.down * 2, Color.red, 1f);
            //}

            // foreach (var x in SelectCardPoints)
            // {
            //     var wordLoc = Point.GridToWorld(x, SheetsManager.GetWarGrid(sheet.ID));
            //     Debug.DrawRay(wordLoc.ToVector2(), Vector3.down * 2, Color.blue, .5f);
            // }
#endif
        }

        /// <summary>
        /// Find points from a specific location
        /// </summary>
        /// <param name="location">the point location</param>
        private void FindPoints(Point location)
        {

            Point up = location + Point.up;
            Point down = location + Point.down;

            Point left = location + Point.left;
            Point right = location + Point.right;

            if (up.IsInGridBounds)
            {
                SelectCardPoints.Add(up);
            }

            if (left.IsInGridBounds)
            {
                SelectCardPoints.Add(left);
            }

            AddCardPoints.Add(down);
            AddCardPoints.Add(right);
        }

        private void ClearPoints()
        {
            AddCardPoints.Clear();
            SelectCardPoints.Clear();
        }

        private void ClearCards()
        {
            foreach (var x in cards)
            {
                PoolManager.Main.TryCheckInObject(Prefab.gameObject, x.gameObject, this.transform);
            }

            cards.Clear();
        }

        /// <summary>
        /// Toggle the activate cluster
        /// </summary>
        public void ToggleActivateCluster()
        {

            var accountPrefs = UserPreferencesHandler.Preferences;
            accountPrefs.UseContextButtons = !accountPrefs.UseContextButtons;

            UserPreferencesHandler.SavePreferences();

            ActivateCluster = !ActivateCluster; //WarSystem.AccountPreferences.UseContextButtons;

            // Debug.Log("toggled activate cluster " + WarSystem.AccountPreferences.UseContextButtons);
        }

        public void AddCard(Point p)
        {
            //Debug.Log("Mouse Down - add card");
            if (ToolsManager.SelectedTool == ToolTypes.Edit)
                DataSetViewer.main.DropCardMainMenu(SheetsManager.CurrentSheetID, (x, y) => { }, new List<Point>() { p }, true, false);
        }

        public void SelectRow(Point p)
        {
            Debug.Log("Mouse Down - select row");
        }

        void NewSheetReset(string id)
        {
            ClearPoints();
            ClearCards();
        }

        void OnEnable()
        {
            SheetsManager.OnSetSheetCurrent += NewSheetReset;
        }

        void OnDisable()
        {
            SheetsManager.OnSetSheetCurrent -= NewSheetReset;
        }
    }
}
