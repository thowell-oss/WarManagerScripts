/* CardPoolManager.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;
using System;

namespace WarManager.Unity3D
{
    /// <summary>
    /// Handles pooling the unity GUI cards and related managing preformance
    /// </summary>
    [Notes.Author("Handles pooling the unity GUI cards and related managing preformance")]
    public class CardPoolManager : MonoBehaviour
    {
        // public Vector2 Offset;
        // public Vector2 Scale = new Vector2(7, 3);   //7.5f, 3

        private WarGrid _grid;

        public float CameraBoundsFudging = .5f;

        public float IdleRefreshTimer = 1f;

        /// <summary>
        /// The total amount of unity cards that can be created during initialization
        /// </summary>
        private int totalAmtCreated = 3;

        /// <summary>
        /// Stores all instantiated unity cards
        /// </summary>
        /// <typeparam name="UnityCard"></typeparam>
        /// <returns></returns>
        private List<UnityCard> _pool = new List<UnityCard>();
        /// <summary>
        /// The list of unity cards that are being used
        /// </summary>
        /// <typeparam name="UnityCard">the type of unity card that </typeparam>
        /// <returns></returns>
        public List<UnityCard> _usedUnityCardsList = new List<UnityCard>();

        /// <summary>
        /// The queue of the unusedUnity cards
        /// </summary>
        /// <typeparam name="UnityCard"></typeparam>
        /// <returns></returns>
        private Queue<UnityCard> _unusedUnityCardQueue = new Queue<UnityCard>();

        private List<GridPartition<Card>> _selectedPartitions = new List<GridPartition<Card>>();

        /// <summary>
        /// The unity card prefab
        /// </summary>
        public UnityCard CardPrefab;

        /// <summary>
        /// The world anchor to parent all unity cards
        /// </summary>
        public GameObject WorldAnchor;

        /// <summary>
        /// The war manager camera controller
        /// </summary>
        public WarManagerCameraController CameraController;

        public static CardPoolManager Main;

        /// <summary>
        /// Is the card pool manager loading a sheet?
        /// </summary>
        private bool _loading = false;

        void Awake() => Init();

        /// <summary>
        /// Spawn unity cards in order to be used
        /// </summary>
        public void Init()
        {
            if (Main != null)
            {
                Destroy(this);
            }
            else
            {
                InstantiateCard((int)Mathf.Pow(totalAmtCreated, 2));
                Main = this;
            }

        }

        /// <summary>
        /// Instantiate cards and add them to the card pool
        /// </summary>
        /// <param name="amt">the amount of cards you want to instantiate</param>
        void InstantiateCard(int amt)
        {
            for (int i = 0; i < amt; i++)
            {
                GameObject go = Instantiate(CardPrefab.gameObject, WorldAnchor.transform) as GameObject;
                UnityCard card = go.GetComponent<UnityCard>();

                card.WarCamera = CameraController;
                _pool.Add(card);
                card.ResetCard(null);
                _unusedUnityCardQueue.Enqueue(card);

            }
        }

        /// <summary>
        /// for testing
        /// </summary>
        /// <param name="sheetID">the id of the sheet</param>
        public void AddTestCards(string sheetID)
        {
            var sheet = SheetsManager.GetActiveSheet(sheetID);

            float x = UnityEngine.Random.Range(5, 10);
            float y = UnityEngine.Random.Range(5, 10);


            for (int i = 0; i < Mathf.RoundToInt(x); i++)
            {
                for (int k = 0; k < Mathf.RoundToInt(y); k++)
                {
                    Card c = new Card(new Point(i, -k), i.ToString() + "" + k.ToString(), sheetID, "d");
                    c.CanHide = false;
                    sheet.AddObj(c);
                }
            }

            CheckPartitionVisibility();
        }

        /// <summary>
        /// checks to see if the card can be seen by the camera - turns the card off if the card is out of the camera bounds
        /// </summary>
        private void CheckPartitionVisibility()
        {

            if (_loading) return;

            string id = SheetsManager.CurrentSheetID;

            if (id == null)
            {
                WarSystem.WriteToLog("id is null", Logging.MessageType.error);
                Clear();
                return;
            }

            var sheet = SheetsManager.GetActiveSheet(id);

            var offset = SheetsManager.GetGridOffset(id);
            var gridSize = SheetsManager.GetGridScale(id);

            _grid = new WarGrid(offset, gridSize);

            if (sheet == null)
            {
                WarSystem.WriteToLog("sheet is null", Logging.MessageType.error);
                Clear();
                return;
            }

            foreach (var dict in sheet.Partitions)
            {
                var partitions = dict.Value;

                for (int i = 0; i < partitions.Length; i++)
                {
                    for (int j = 0; j < partitions[i].Length; j++)
                    {
                        bool configureCards = false;

                        if (partitions[i][j] != null)
                        {

                            Vector2 camPosition = CameraController.GetCamera.transform.position;
                            (Vector2 top, Vector2 bottom) camBoundsLocation = CameraController.GetCameraOrthographicBounds(CameraBoundsFudging);

                            var bounds = partitions[i][j].GridBounds;

                            // DebugPartition(bounds, offset, gridSize);

                            if (IsInPartitionBounds(camBoundsLocation.top, bounds) || IsInPartitionBounds(camBoundsLocation.bottom, bounds))
                            {
                                configureCards = true;
                            }

                            if (IsInPartitionBounds(new Vector2(camBoundsLocation.top.x, camBoundsLocation.bottom.y), bounds) || IsInPartitionBounds(new Vector2(camBoundsLocation.bottom.x,
                             camBoundsLocation.top.y), bounds))
                            {
                                configureCards = true;
                            }

                            if (IsInPartitionBounds(camPosition, bounds))
                            {
                                configureCards = true;

                            }

                            if (IsInPartitionBounds(new Vector2(camPosition.x, camBoundsLocation.top.y), bounds) || IsInPartitionBounds(new Vector2(camPosition.x, camBoundsLocation.bottom.y), bounds))
                            {
                                configureCards = true;
                            }

                            if (IsInPartitionBounds(new Vector2(camBoundsLocation.top.x, camPosition.y), bounds) || IsInPartitionBounds(new Vector2(camBoundsLocation.bottom.x, camPosition.y), bounds))
                            {
                                configureCards = true;
                            }

                            Vector2 pointA = new Vector2(camPosition.x - (camPosition.x - camBoundsLocation.top.x) / 2, camPosition.y);
                            Vector2 pointB = new Vector2(camPosition.x + (camBoundsLocation.bottom.x - camPosition.x) / 2, camPosition.y);

                            if (IsInPartitionBounds(pointA, bounds) || IsInPartitionBounds(pointB, bounds))
                            {
                                configureCards = true;
                            }


                            List<Card> cards = new List<Card>();

                            if (configureCards)
                            {
                                cards.AddRange(partitions[i][j].GetAllObjects());

                                var partition = _selectedPartitions.Find((x) => (x == partitions[i][j]));

                                if (partition == null)
                                {
                                    _selectedPartitions.Add(partitions[i][j]);
                                }
                            }
                            else
                            {
                                if (_selectedPartitions.Remove(partitions[i][j]))
                                {
                                    cards.AddRange(partitions[i][j].GetAllObjects());
                                }
                            }

                            StartCoroutine(ConfigureCardsInPartition(cards.ToArray(), id));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// draw lines on the screen to show the partitioning at work
        /// </summary>
        private void DebugPartition(Rect bounds, double[] offset, double[] gridSize)
        {
            float topLeftX = Point.GridToWorld(bounds.TopLeftCorner.x, (float)offset[0], (float)gridSize[0]);
            float topLeftY = Point.GridToWorld(bounds.TopLeftCorner.y, (float)offset[1], (float)gridSize[1]);

            float bottomRightX = Point.GridToWorld(bounds.BottomRightCorner.x, (float)offset[0], (float)gridSize[0]);
            float bottomRightY = Point.GridToWorld(bounds.BottomRightCorner.y, (float)offset[1], (float)gridSize[1]);

            Debug.DrawLine(new Vector2(topLeftX, topLeftY), new Vector2(bottomRightX, topLeftY), Color.green);
            Debug.DrawLine(new Vector2(bottomRightX, bottomRightY), new Vector2(bottomRightX, topLeftY), Color.green);
            Debug.DrawLine(new Vector2(bottomRightX, bottomRightY), new Vector2(topLeftX, bottomRightY), Color.green);
            Debug.DrawLine(new Vector2(topLeftX, bottomRightY), new Vector2(topLeftX, topLeftY), Color.green);
        }

        /// <summary>
        /// Configure many cards in the partition
        /// </summary>
        /// <param name="cards">the list of cards for that partition</param>
        IEnumerator ConfigureCardsInPartition(Card[] cards, string sheetID)
        {
            if (cards == null || cards.Length < 1)
            {

            }
            else
            {

                _loading = true;

                WarSystem.GetSheetMetaData(sheetID, out var sheet);

                //Debug.Log("configuring cards for " + sheet.SheetName);

                foreach (var card in cards)
                {
                    System.Func<bool> function = () => ConfigureCardInPartition(card, sheetID);
                    yield return new WaitUntil(function);
                }

                _loading = false;
            }
        }

        /// <summary>
        /// Detect if the card can appear when the camera is in view of the card
        /// </summary>
        /// <param name="card"></param>
        /// <param name="sheetID"></param>
        private bool ConfigureCardInPartition(Card card, string sheetID)
        {
            Pointf f = Point.GridToWorld(card.point, _grid);

            if (CameraController.IsInOrthographicCameraBounds(new Vector2(f.x, f.y), CameraBoundsFudging))
            {
                if (_unusedUnityCardQueue.Count < 1)
                {
                    InstantiateCard(100);
                }

                SetUnityCard(card, sheetID);
            }
            else
            {
                DeactivateCard(card);
            }

            return true;
        }

        /// <summary>
        /// Deactivate the card in the partition and reset it (should trigger a reset which will update the information)
        /// </summary>
        /// <param name="card">the card to reset</param>
        /// <param name="SheetID">the sheet that the card is located on</param>
        public void ReconfigureCard(Card card, string SheetID)
        {
            if (_loading) return;

            try
            {
                DeactivateCard(card);
                ConfigureCardInPartition(card, SheetID);
            }
            catch (System.Exception ex)
            {
                NotificationHandler.Print(ex.Message);
            }
        }

        /// <summary>
        /// Is a certain vector2 within the bounds of the partition?
        /// </summary>
        /// <param name="location">the location vector2</param>
        /// <param name="bounds">the bounds ofthe partiton</param>
        /// <returns>returns true if the bounds are in the partition, false if not</returns>
        private bool IsInPartitionBounds(Vector2 location, Rect bounds)
        {
            Point topLeft = bounds.TopLeftCorner;
            Point bottomRight = bounds.BottomRightCorner;

            float topLeftX = Point.GridToWorld(topLeft.x, _grid.Offset.x, _grid.GridScale.x);
            float topLeftY = Point.GridToWorld(topLeft.y, _grid.Offset.y, _grid.GridScale.y);

            float bottomRightX = Point.GridToWorld(bottomRight.x, _grid.Offset.x, _grid.GridScale.x);
            float bottomRightY = Point.GridToWorld(bottomRight.y, _grid.Offset.y, _grid.GridScale.y);

            if (location.y < topLeftY && location.y > bottomRightY)
            {
                if (location.x > topLeftX && location.x < bottomRightX)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Activates a unity card and initializes it
        /// </summary>
        /// <param name="card">the backend card reference</param>
        private void SetUnityCard(Card card, string sheetID)
        {
            //Debug.Log("start card");

            try
            {

                if (card.CardRepresented || card == null)
                    return;

                //Debug.Log("start card");

                UnityCard unityCard = _unusedUnityCardQueue.Dequeue();
                _usedUnityCardsList.Add(unityCard);
                unityCard.gameObject.SetActive(true);

                //Debug.Log("card");

                unityCard.Grid = _grid;

                unityCard.ResetCard(card);
                card.CardRepresented = true;

                //Debug.Log("card 1");

                if (!card.DatasetID.StartsWith("sys"))
                    card.ClearCache();

                if (card.MakeUp != null)
                    card.MakeUp.OnAwake();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR
                Debug.LogError(ex.Message);
#endif
            }

            //Debug.Log("card 2");
        }

        /// <summary>
        /// Deactivates a unity card
        /// </summary>
        /// <param name="card">the backend card reference</param>
        private void DeactivateCard(Card card)
        {
            if (!card.CardRepresented || card == null)
                return;

            //check to make sure all cards in a stretched group are out of bounds before turning it off

            _usedUnityCardsList.RemoveAll((x) => x.Card == null);

            var unityCard = _usedUnityCardsList.Find((x) => x.Card == card);
            if (unityCard != null)
            {
                DeactivateDisplayCard(unityCard);
            }

            card.MakeUp.OnSleep();
        }

        /// <summary>
        /// Remove display card from visibility
        /// </summary>
        /// <param name="unityCard">the unity card to deactive</param>
        private void DeactivateDisplayCard(UnityCard unityCard)
        {
            if (unityCard == null)
            {
                Debug.Log("Card is null");
                return;
            }

            _usedUnityCardsList.Remove(unityCard);

            if (unityCard.Card != null)
                unityCard.Card.CardRepresented = false;

            unityCard.ResetCard(null);

            _unusedUnityCardQueue.Enqueue(unityCard);

            //Debug.Log("Removing card " + unitycard.Card.point);
        }

        /// <summary>
        /// Refresh a list of cards that are being updated without updating everything
        /// </summary>
        /// <param name="cards">the list of cards to update</param>
        /// <param name="sheetID">the sheet id of the current card</param>
        public void RefreshCards(List<Card> cards, string sheetID)
        {
            if (_loading) return;

            if (string.IsNullOrEmpty(sheetID))
                throw new System.ArgumentException("Cannot use the given sheet id");

            if (cards == null || cards.Count < 1) throw new System.ArgumentException("Cannot use the list because it is either" +
                 " empty or null");

            foreach (var card in cards)
            {
                if (card != null) ReconfigureCard(card, sheetID);
            }
        }

        /// <summary>
        /// Refresh a card that is already actives
        /// </summary>
        /// <param name="cardId">the id of the card</param>
        /// <exception cref="System.ArgumentException">Error thrown when the id is  either null, empty, or white space</exception>
        [Obsolete("untested")]
        public void RefreshActiveCard(string cardId)
        {
            if (_loading) return;

            if (string.IsNullOrWhiteSpace(cardId) || cardId == string.Empty)
            {
                throw new System.ArgumentException("The card id is either null, empty, or whitespace");
            }

            List<UnityCard> unityCards = _usedUnityCardsList.FindAll((x) => x.ID == cardId);

            List<Card> cards = new List<Card>();

            if (unityCards.Count > 0)
            {
                foreach (var c in unityCards)
                {
                    if (c.Card != null)
                    {
                        cards.Add(c.Card);
                    }
                }

                RefreshCards(cards, SheetsManager.CurrentSheetID);
            }
        }

        private void Clear()
        {
            if (_loading) return;

            for (int i = _usedUnityCardsList.Count - 1; i >= 0; i--)
            {
                DeactivateDisplayCard(_usedUnityCardsList[i]);
            }
        }

        private void CameraChanged()
        {
            CheckPartitionVisibility();
        }

        private void OnOpenSheet(string id)
        {
            Clear();
            CheckPartitionVisibility();
        }

        private void ChangeSheet(string id)
        {
            Clear();
            CheckPartitionVisibility();
        }

        /// <summary>
        /// add a card
        /// </summary>
        /// <param name="p">The point it was added</param>
        /// <param name="id">the id of the card</param>
        public void AddNewCard(Point p, string id)
        {
            CheckPartitionVisibility();
        }

        public void OnCloseCardSheet(string id)
        {
            Clear();
        }

        public void OnPasteCard()
        {
            CheckPartitionVisibility();
        }

        public void OnRefresh()
        {
            Clear();
            CheckPartitionVisibility();
        }

        void OnEnable()
        {
            WarManagerCameraController.OnCameraChange += CameraChanged;
            SheetsManager.OnSetSheetCurrent += ChangeSheet;
            SheetsManager.OnCloseCardSheet += OnCloseCardSheet;
            //WarManager.Unity3D.Windows.DataSetViewer.OnAddNewCard += AddNewCard;
            CopyPasteDuplicate.OnPasteCard += OnPasteCard;
            SimpleUndoRedoManager.OnUndoRedo += OnRefresh;
        }

        void OnDisable()
        {
            WarManagerCameraController.OnCameraChange -= CameraChanged;
            SheetsManager.OnSetSheetCurrent -= ChangeSheet;
            SheetsManager.OnCloseCardSheet -= OnCloseCardSheet;
            //WarManager.Unity3D.Windows.DataSetViewer.OnAddNewCard -= AddNewCard;
            CopyPasteDuplicate.OnPasteCard -= OnPasteCard;
            SimpleUndoRedoManager.OnUndoRedo -= OnRefresh;
        }
    }
}
