/* CardManager.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections.Generic;

using WarManager.Cards;
using WarManager.Backend;
using System.Text;
using WarManager.Unity3D;

/// <summary>
/// War Manger namespace - contains custom code for War Manager
/// </summary>
namespace WarManager
{
    /// <summary>
    /// Handles manipulation of the backend cards related to its location on the sheet respectively
    /// </summary>
    [Notes.Author(8.11, "Handles the utility functions of card movement")]
    public static class CardUtility
    {

        /// <summary>
        /// The default shift when moving cards (either shift down or shift right (not up or left)).
        /// </summary>
        /// <returns></returns>
        public static Point DefaultShiftDirection = Point.down;

        /// <summary>
        /// A reference to the selected card sheet being edited
        /// </summary>
        public static Sheet<Card> CurrentSheet
        {
            get
            {
                return SheetsManager.GetActiveSheet(SheetsManager.CurrentSheetID);
            }
        }


        /// <summary>
        /// The bounds of the sheet
        /// </summary>
        public static Rect CameraBounds { get; private set; }

        public delegate void cardsChanged_delegate(Card[] cards);
        public static event cardsChanged_delegate OnCardsChanged;

        public delegate void generalUpdate_delegate();

        /// <summary>
        /// An event called every 5 seconds in order to generally update the sheet (if needed)
        /// </summary>
        public static event generalUpdate_delegate OnGeneralUpdate;

        /// <summary>
        /// Initialize the card manager
        /// </summary>
        public static void Init()
        {
            //UndoRedo.CreateNewSnapShot("initialize");
        }

        /// <summary>
        /// Called every 5 seconds
        /// </summary>
        public static void CallGeneralUpdate_Event()
        {
            if (OnGeneralUpdate != null)
                OnGeneralUpdate();
        }

        #region Card Data Management (Get, Move, Set, Remove)


        /// <summary>
        /// Stores a card and does not assign it to a sheet (card will not be saved to a sheet unless connectCards() is called)
        /// </summary>
        /// <param name="card">the card to store</param>
        [Obsolete("This method is not in use")]
        public static void SpawnDisconnectedCard(Card card)
        {
            // card.point = new Point(1, 1);

            // if (card == null)
            //     throw new NullReferenceException("The given card was null");

            // _cardsNotAssignedToSheet.Add(card);
        }

        /// <summary>
        /// Adds the cards not assigned to a sheet to the sheet
        /// </summary>
        [Obsolete("This method is not in use")]
        public static void ConnectCards()
        {
            // foreach (var card in _cardsNotAssignedToSheet)
            // {
            //     if (card.point.y < 1)
            //     {
            //         AddCard(card);
            //     }
            //     else
            //     {
            //         throw new NotSupportedException("Cards cannot live above 0 on the y axis");
            //     }
            // }
        }

        /// <summary>
        /// Adds a card to the sheet. If a card is in that location, the cards might overlap - insert a card if needed
        /// </summary>
        /// <param name="card">the card reference</param>
        /// <param name="location">the location of the card</param>
        /// <returns>returns true if the card can be placed in the dictionary, false if not</returns>
        public static bool TryAddCard(Card card, bool createSnapShot = false)
        {
            if (card == null)
                throw new NullReferenceException("The given card is null");

            if (SheetsManager.TryGetActiveSheet(card.SheetID, out var sheet))
            {
                if (sheet.ObjExists(card.point, card.Layer))
                    return false;

                if (!sheet.ContentDict.ContainsKey(card.ID))
                {
                    card.CardRepresented = false;

                    sheet.ContentDict.Add(card.ID, card);
                    sheet.AddObj(card);

                    CalculateBounds();

                    if (OnCardsChanged != null)
                        OnCardsChanged(new Card[1] { card });

                    if (createSnapShot)
                    {
                        SimpleUndoRedoManager.main.NewSnapShot();
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Add a card and make it take up more space than one grid point
        /// </summary>
        /// <param name="card">the card to add</param>
        /// <param name="bounds">the bounds of the card</param>
        [Obsolete("Stretch cards need to be reworked internally before they can be used")]
        public static bool TryAddStretchCard(Card card, Rect bounds)
        {
            if (!SheetsManager.TryGetActiveSheet(card.SheetID, out var sheet))
                return false;

            List<Point> points = bounds.SpacesTaken();

            foreach (var point in points)
            {
                if (sheet.ObjExists(point, card.Layer))
                    return false;
            }

            if (!TryAddCard(card))
            {
                UnityEngine.Debug.Log("Could not add card");
                return false;
            }

            List<Card> groupedCards = new List<Card>();
            groupedCards.Add(card);

            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] != card.point)
                {
                    Card c = new Card(points[i], Guid.NewGuid().ToString(), card.SheetID, card.Layer.ID);
                    if (!TryAddCard(c))
                    {
                        UnityEngine.Debug.Log("Could not add card");
                        return false;
                    }

                    groupedCards.Add(c);
                }
            }

            //foreach (var someCard in groupedCards)
            //{
            //    someCard.GroupStretchCardsList = groupedCards;
            //    someCard.CardStretchBounds = bounds;
            //}

            return true;

        }

        /// <summary>
        /// Get a card by location
        /// </summary>
        /// <param name="loc">the location of the card (float, float) tuple</param>
        /// <returns>returns the card if it exists, null if not</returns>
        public static Card GetCard(Point loc, Layer currentLayer, string sheetID = null)
        {
            // if (loc.x < 0 || loc.y > 0)
            //     return null;

            if (loc.IsInGridBounds)
            {
                if (sheetID == null || sheetID == string.Empty)
                {
                    return CurrentSheet.GetObj(loc, currentLayer);
                }
                else
                {
                    var sheet = SheetsManager.GetActiveSheet(sheetID);
                    return sheet.GetObj(loc, currentLayer);
                }
            }

            return null;
        }

        /// <summary>
        /// Get the adjacent card
        /// </summary>
        /// <param name="loc">the location of the card</param>
        /// <param name="vector">the normalized vector location of the selected card</param>
        /// <returns>returns the selected card localized from the original card, returns null if the card cannot be found</returns>
        public static Card GetCardAdjacent(Point loc, Point vector, Layer layer)
        {
            int localizedLocationX = loc.x + vector.x;
            int localizedLocationY = loc.y + vector.y;

            GetCard(new Point(localizedLocationX, localizedLocationY), layer);

            return null;
        }

        /// <summary>
        /// Attempts to get the card by ID, more expensive operation
        /// </summary>
        /// <param name="id">the guid</param>
        /// <param name="card">the out card to receive</param>
        /// <returns>returns true if the card is found, false if not</returns>
        public static Card GetCard(string id)
        {
            id = CheckID(id, "TryGetCard()");

            Card c;

            if (CurrentSheet.ContentDict.TryGetValue(id, out c))
            {
                return c;
            }

            return null;
        }

        /// <summary>
        /// Does the sheet at the current layer contain the card?
        /// </summary>
        /// <param name="location">the location of the card</param>
        /// <param name="layer">the layer of the card</param>
        /// <param name="sheetId">the sheet id</param>
        /// <returns>returns true if the card is found, false if not</returns>
        public static bool ContainsCard(Point location, Layer layer, string sheetId)
        {
            return GetCard(location, layer, sheetId) != null;
        }

        /// <summary>
        /// Return a full list of cards 
        /// </summary>
        /// <returns>returns a list of cards, empty or null list if otherwise</returns>
        public static List<Card> GetCardsFromCurrentSheet()
        {
            if (SheetsManager.SheetCount < 1)
                return new List<Card>();

            return CurrentSheet.GetAllObj();
        }

        /// <summary>
        /// Sorts the cards by a location using a given vector
        /// </summary>
        /// <param name="cardList">the list of cards to sort</param>
        /// <param name="direction">the vector to sort cards</param>
        /// <returns>returns the list of sorted cards</returns>
        public static List<Card> SortByGridAndVector(List<Card> cardList, Point direction)
        {
            if (cardList == null)
                throw new NullReferenceException("The list of cards cannot be null");

            if (direction == null)
                throw new NullReferenceException("The sort vector cannot be null");

            if (direction == Point.zero)
                throw new NotSupportedException("The cards cannot be sorted by a vector with a magnitude of zero");

            if (cardList.Count < 2)
                return cardList;

            cardList.Sort(delegate (Card a, Card b)
            {
                return Point.SortByDirection(a.point, b.point, direction);
            });

            return cardList;

        }

        /// <summary>
        /// Internal moving system - moves the card on the backend and shifts cards when needed
        /// </summary>
        /// <param name="card">the card being moved</param>
        /// <param name="newLoc">the new point where the card is located</param>
        /// <param name="nextLayer">the new layer where the card will be</param>
        /// <param name="nextSheetID">the new sheet id where the card will be placed</param>
        /// <param name="cardSnapshots">the list of cards that have changed as a result</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        private static bool TryMoveInternal(List<Card> selectedCardsList, Point newLoc, Layer nextLayer, string nextSheetID, out List<SnapShot> cardSnapshots)
        {
            List<SnapShot> snapShots = new List<SnapShot>();
            List<(string, Point, Point)> moveLocationsLogList = new List<(string, Point, Point)>();

            if (selectedCardsList == null)
                throw new NullReferenceException("The card cannot be null");

            if (selectedCardsList.Count < 1)
            {
                cardSnapshots = new List<SnapShot>();
                return false;
            }

            if (newLoc == null)
                throw new NullReferenceException("The new location cannot be null");

            if (nextSheetID == null)
            {
                throw new NullReferenceException("The sheet id cannot be null");
            }

            if (nextSheetID == string.Empty)
            {
                throw new NotSupportedException("The sheet id cannot be an empty string");
            }

            var nextSheet = SheetsManager.GetActiveSheet(nextSheetID);

            if (nextSheet == null)
            {
                //Debug.Log("next sheet is null");

                cardSnapshots = new List<SnapShot>();
                return false;
            }

            var grid = SheetsManager.GetWarGrid(nextSheetID);

            List<Card> finalCardsList = new List<Card>();

            //Debug.Log(selectedCardsList.Count);

            foreach (var card in selectedCardsList)
            {
                Point newCardPosition = Point.WorldToGrid(card.DragFinalLocation, grid);

                if (!newCardPosition.IsInGridBounds || card.CardLocked)
                {
                    //this card cannot be moved for some reason - falling back to original position
                    //Debug.Log("card cannot be moved");
                }
                else
                {
                    var currentSheet = SheetsManager.GetActiveSheet(card.SheetID);

                    if (currentSheet != null)
                    {
                        //Debug.Log("adding card to final cards list");
                        finalCardsList.Add(card);
                        currentSheet.Remove(card.point, card.Layer);
                    }
                    else
                    {
                        //Debug.Log("COuld not get the active sheet");
                    }
                }
            }

            if (finalCardsList.Count < 1)
            {
                //Debug.Log("no cards to move");

                cardSnapshots = new List<SnapShot>();
                return false;
            }

            finalCardsList = SortByGridAndVector(finalCardsList, -DefaultShiftDirection);

            bool canMove = false;

            //Debug.Log("cards sorted");

            foreach (var card in finalCardsList)
            {
                canMove = true;
                Point newCardPosition = Point.WorldToGrid(card.DragFinalLocation, grid);

                Card checkCard = nextSheet.GetObj(newCardPosition, nextLayer);

                if (checkCard != null)
                {
                    //Debug.Log("An object exists in that location");

                    Card checkCardAbove = nextSheet.GetObj(newCardPosition + -DefaultShiftDirection, nextLayer);
                    bool tryShift = true;

                    if (checkCard.Grouped)
                    {
                        if (checkCard.GroupCardsList.Find((x) => x == checkCardAbove) != null)
                        {
                            tryShift = false;
                        }
                    }

                    if (!tryShift && checkCard.CardStretched)
                    {
                        //if (checkCard.GroupStretchCardsList.Find((x) => x == checkCardAbove) != null)
                        //{
                        tryShift = false;
                        //}
                    }

                    if (!tryShift)
                    {
                        canMove = false;
                    }

                    if (tryShift && !TryShift(newCardPosition, DefaultShiftDirection, nextLayer, nextSheetID, 1, out snapShots))
                    {
                        canMove = false;
                    }
                }

                if (canMove)
                {
                    //Debug.Log("moving the card into the correct position");

                    card.point = newCardPosition;
                    card.Layer = nextLayer;
                    nextSheet.AddObj(card);

                    moveLocationsLogList.Add((card.ID, card.point, newLoc));
                    //WarSystem.WriteToLog($"Card {card.ID} moved from {card.point} to {newLoc}", logging.MessageType.logEvent);
                }
                else
                {
                    //Debug.Log("something didn't work - defaulting back to original position");
                    CurrentSheet.AddObj(card);

                    if (card.Grouped)
                    {
                        throw new NotSupportedException("The all the cards in the group need to be moved back");
                    }
                }


            }


            if (moveLocationsLogList.Count > 0)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var x in moveLocationsLogList)
                {
                    builder.AppendLine($"Card {x.Item1} moved from {x.Item2} to {x.Item3}");
                }

                WarSystem.WriteToLog($"Cards Moved from sheet {CurrentSheet.Name} to {nextSheet.Name}\n" + builder.ToString(), Logging.MessageType.info);
            }

            if (finalCardsList.Count > 0)
            {
                CalculateBounds();
                cardSnapshots = new List<SnapShot>();

                if (OnCardsChanged != null)
                    OnCardsChanged(finalCardsList.ToArray());

                return true;
            }
            else
            {
                cardSnapshots = new List<SnapShot>();
                return false;
            }


        }

        /// <summary>
        /// Move a list of cards
        /// </summary>
        /// <param name="cards">the list of cards to move</param>
        /// <param name="newLoc">the next location of the first card</param>
        /// <param name="nextLayer">the layer where the cards are going</param>
        /// <param name="sheetId">the sheet where the cards are going</param>
        /// <param name="createSnapShot">should a snapshot be created and registered with the undo/redo?</param>
        /// <returns>returns true if the operation is successful, false if not.</returns>
        public static bool MoveCards(List<Card> cards, Point newLoc, Layer nextLayer, string sheetId, bool createSnapShot = true)
        {
            if (TryMoveInternal(cards, newLoc, nextLayer, sheetId, out var snapShots))
            {
                if (createSnapShot)
                    SimpleUndoRedoManager.main.NewSnapShot();

                WarSystem.DeveloperPushNotificationHandler.MovedCards = true;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove a card from the card sheet
        /// </summary>
        /// <param name="point">the point on the sheet to remove the card</param>
        /// <param name="layer">the layer of which to remove the card</param>
        /// <param name="sheetID">the id of the sheet to remove the card</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool RemoveCard(Point point, Layer layer, string sheetID)
        {
            var sheet = SheetsManager.GetActiveSheet(sheetID);

            if (sheet != null)
            {
                var card = sheet.GetObj(point, layer);
                if (card != null)
                {
                    if (card.RemoveCallBack != null)
                        card.RemoveCallBack();

                    sheet.Remove(point, layer);
                    sheet.ContentDict.Remove(card.ID);
                    CalculateBounds();

                    WarSystem.DeveloperPushNotificationHandler.MovedCards = true;
                }

                return true;
            }

            return false;
        }

        #endregion

        #region active Card management
        /// <summary>
        /// Add the card to a list of cards that need to be refreshed on screen
        /// </summary>
        /// <param name="card">the card to add</param>
        [Obsolete("This method is not in use")]
        public static void AddCardToUpdateQueue(Card card)
        {
            // _activeCardQueue.Enqueue(card);
        }

        /// <summary>
        /// Remove a card from the list of cards that need to be refreshed on screen
        /// </summary>
        /// <param name="card">the card to remove</param>
        [Obsolete("This method is not in use")]
        public static Card DequeueActiveCard()
        {
            // if (_activeCardQueue.Count > 0)
            // {
            //     return _activeCardQueue.Dequeue();
            // }
            // else
            // {
            return null;
            // }
        }

        #endregion

        #region Get Adjacent Cards

        /// <summary>
        /// Gets the directions adjacent to the grid point (N, E, S, W - not diagonal)
        /// </summary>
        /// <param name="centralCard">the central location</param>
        /// <returns>returns a tuple Point of the adjacent cards</returns>
        public static Point[] GetCrossAdjacentGridPositions(Point centralCard)
        {
            Point[] directions = new Point[4] { Point.up, Point.right, Point.down, Point.left };
            Point[] gridPositions = new Point[4];

            for (int i = 0; i < directions.Length; i++)
            {
                gridPositions[i] = new Point(centralCard.x + directions[i].x, centralCard.y + directions[i].y);
            }

            return gridPositions;
        }

        /// <summary>
        /// Gets the diagonal directions adjacent to the grid point (NW, NE, SE, SW - diagonal)
        /// </summary>
        /// <param name="centralCard">the central grid point</param>
        /// <returns>returns a tuple Point of the adjacent cards</returns>
        public static Point[] GetDiagonalAdjacentGridPositions(Point centralCard)
        {
            Point[] directions = new Point[4] { Point.northWest, Point.northEast, Point.southEast, Point.southWest };
            Point[] gridPositions = new Point[4];

            for (int i = 0; i < directions.Length; i++)
            {
                gridPositions[i] = new Point(centralCard.x + directions[i].x, centralCard.y + directions[i].y);
            }

            return gridPositions;
        }

        /// <summary>
        /// Gets any positions adjacent to the card (including diagonal adjacent locations). 
        /// The list starts at NW and goes clockwise until it reaches W -> (NW, N, NE, E, SE, S, SW, W).
        /// </summary>
        public static List<Point> GetAllAdjacentGridPositions(Point location)
        {
            List<Point> pos = new List<Point>();

            pos.AddRange(GetCrossAdjacentGridPositions(location));
            pos.AddRange(GetDiagonalAdjacentGridPositions(location));

            List<Point> finalPos = new List<Point>();

            for (int i = 0; i < pos.Count / 2; i++) //correcting order of the points as we add them to the final list
            {
                finalPos.Add(pos[i]);
                finalPos.Add(pos[i + 4]);
            }

            return finalPos;
        }

        /// <summary>
        /// Get all cards along a line (vector and -vector) direction
        /// </summary>
        /// <param name="start">the start point of the rays</param>
        /// <param name="direction">the direction of the rays</param>
        /// <param name="distance">the distance the rays will last</param>
        /// <param name="layer">the layer the ray will preform on</param>
        /// <param name="sheetId">the sheet the ray will fire</param>
        /// <param name="behavior">how will the ray handle null cards?</param>
        /// <returns>returns a list of cards along the line</returns>
        public static List<Card> GetAdjacentCardsLine(Point start, Point direction, int distance, Layer layer, string sheetId, RayNullCardBehavior behavior = RayNullCardBehavior.StopAtFirstNull)
        {
            List<Card> cards = new List<Card>();

            PointRay ray = new PointRay(start, direction, layer, sheetId, distance, behavior, true);
            cards.AddRange(ray.GetCardsAlongRay(null));

            PointRay nextRay = new PointRay(ray.Start, -ray.Direction, ray.Layer, ray.SheetID, ray.Distance, ray.NullBehavior, false);
            cards.AddRange(nextRay.GetCardsAlongRay(null));
            return cards;
        }

        #endregion

        #region Camera Bounds

        /// <summary>
        /// Calculate the upper left and lower right corners of a rectangle bound for camera purposes
        /// </summary>
        private static void CalculateBounds()
        {
            CameraBounds = CurrentSheet.GetGridSheetBounds();
        }

        /// <summary>
        /// Get the upper left and lower right bounds of the sheet
        /// </summary>
        /// <param name="cardOffset"> the offset of all cards</param>
        /// <param name="gridMultiplier">the cards grid spacing multiplier</param>
        /// <returns>returns a tuple ((x, y)upper left, (x,y) lower right) </returns>
        public static ((float x, float y) upperLeft, (float x, float y) lowerRight) GetGlobalBounds((float, float) cardOffset, (float, float) gridMultiplier)
        {
            (float, float) ul = CardLayout.GetCardGlobalLocation(CameraBounds.TopLeftCorner, (0, 0), cardOffset, gridMultiplier);
            (float, float) lr = CardLayout.GetCardGlobalLocation(CameraBounds.BottomRightCorner, (0, 0), cardOffset, gridMultiplier);

            return (ul, lr);
        }

        #endregion

        #region offset
        /// <summary>
        /// Apply an arbitrary offset to a card
        /// </summary>
        /// <param name="c">the card to apply the offset to</param>
        /// <param name="offset">the offset to apply to the card</param>
        [Obsolete("Creating local grid offsets has been phased out in favor of creating cards that take up multiple grid spaces")]
        public static void ApplyOffset(Card c, (float, float) offset)
        {
            c.Layout.SetOffset(offset, true);

            ApplyOffsetRecursive(new Point(c.point.x + 1, c.point.y), c.Layer, offset, 2);
            ApplyOffsetRecursive(new Point(c.point.x, c.point.y - 1), c.Layer, offset, 2);
        }

        /// <summary>
        /// Apply the offset to cards recursively 
        /// </summary>
        /// <param name="location">the location of the card to apply</param>
        /// <param name="offset">the offset set to apply to the card</param>
        /// <param name="strategy">the strategy of the card application (0 = recursive along rows, 1 = recursive along columns, 2 = recursive along both rows and columns</param>
        [Obsolete("Creating local grid offsets has been phased out in favor of creating cards that take up multiple grid spaces")]
        private static void ApplyOffsetRecursive(Point location, Layer layer, (float x, float y) offset, int strategy)
        {
            Card currentCard = GetCard(location, layer);

            if (currentCard == null)
                return;

            currentCard.Layout.SetOffset(offset, false);
            (float x, float y) newOffset = (offset.x + currentCard.Layout.Offset.x, offset.y + currentCard.Layout.Offset.y);

            if (strategy == 0 || strategy == 2)
                ApplyOffsetRecursive(new Point(location.x + 1, location.y), layer, newOffset, strategy);

            if (strategy == 1 || strategy == 2)
                ApplyOffsetRecursive(new Point(location.x, location.y - 1), layer, newOffset, strategy);
        }

        #endregion

        #region shifting

        /// <summary>
        /// Shift a list of cards
        /// </summary>
        /// <param name="cards">the cards being shifted</param>
        /// <param name="pushCards">should the cards be pushed or pulled?</param>
        /// <param name="overrideShiftingRules">should the general settings be ignored?</param>
        [Obsolete("Please use Shift() instead", false)]
        public static void ShiftCards(List<Card> cards, Point vector, bool pushCards = true, bool overrideShiftingRules = false)
        {
            // cards.Sort();

            // if (vector.y < 0 || vector.x > 0)
            // {
            //     for (int i = cards.Count - 1; i >= 0; i--)
            //     {
            //         ShiftCards(cards[i].point, cards[i].Layer, vector, cards[i].SheetID, pushCards, overrideShiftingRules);
            //     }
            //     return;
            // }

            // foreach (Card c in cards)
            // {
            //     ShiftCards(c.point, c.Layer, vector, c.SheetID, pushCards, overrideShiftingRules);
            // }
        }


        /// <summary>
        /// Shift cards according to a 2D vector
        /// </summary>
        /// <param name="loc">the location to start</param>
        /// <param name="vector">the vector</param>
        /// <param name="pushCards">Should the cards be pushed away or pulled?</param>
        /// /// <param name="overrideShiftingRules">should the general settings be ignored?</param>
        [Obsolete("Please use Shift() instead", false)]
        public static bool ShiftCards(Point loc, Layer layer, Point vector, string sheetID, bool pushCards = true, bool overrideShiftingRules = false)
        {

            return false;
        }


        /// <summary>
        /// Shift a card
        /// </summary>
        /// <param name="card">the card</param>
        /// <param name="direction">the direction</param>
        /// <returns>returns the success of the shift</returns>
        public static bool TryShiftCard(Card card, Point direction, int distance, bool createSnapShot)
        {
            if (card == null)
                throw new NullReferenceException("The card cannot be null");

            if (direction == null)
                throw new NullReferenceException("The point cannot be null");

            if (direction == Point.zero)
                throw new NotSupportedException("The direction cannot have a magnitude of zero");

            bool success = TryShift(card.point, direction, card.Layer, card.SheetID, distance, out var snapShots);

            if (success && createSnapShot)
            {
                //create snapshot
            }

            return success;
        }

        /// <summary>
        /// Shift a list of cards
        /// </summary>
        /// <param name="cards">the list of cards to shift</param>
        /// <param name="direction">the direction the cards will shift</param>
        /// <param name="distance">The distance the card will shift</param>
        /// <param names="createSnapShot">Should a snap shot be created?</param>
        /// <returns>returns the success of the shift</returns>
        public static bool TryShiftCards(List<Card> cards, Point direction, int distance, bool createSnapShot = true)
        {
            if (cards == null)
                throw new NullReferenceException("The cards list cannot be null");

            if (direction == null)
                throw new NullReferenceException("The point cannot be null");

            if (direction == Point.zero)
                throw new NotSupportedException("The direction cannot have a magnitude of zero");

            if (cards.Count < 1)
                return false;

            if (cards.Count == 1)
                return TryShiftCard(cards[0], direction, distance, createSnapShot);

            cards = CardUtility.SortByGridAndVector(cards, direction);

            bool success = false;

            foreach (var card in cards)
            {
                if (CardUtility.TryShift(card.point, direction, card.Layer, card.SheetID, distance, out var snapshots))
                {
                    //UndoRedo.CreateNewSnapShot(new CardMove_UndoRedo(snapshots));
                    success = true;
                }
            }

            return success;

        }

        /// <summary>
        /// Shift Cards from one point to another in a certain direction and distance
        /// </summary>
        /// <param name="startPoint">the starting point to shift the cards</param>
        /// <param name="direction">the direction in which to shift the cards</param>
        /// <param name="layer">the layer where the cards will be shifted</param>
        /// <param name="sheetID">the sheet where the card will be shifted</param>
        /// <param name="shiftDistance">the amount of grid spaces the card will be shifted</param>
        /// <remarks>The shifting algorithm does not take shifting across multiple sheets and other layers into account</remarks>
        public static bool TryShift(Point startPoint, Point direction, Layer layer, string sheetID, int shiftDistance, out List<SnapShot> cardsAffected, bool includeStartCard = true)
        {
            PointRay ray = new PointRay(startPoint, direction, layer, sheetID, -1, RayNullCardBehavior.StopAtFirstNull, includeStartCard);

            var cards = ray.GetCardsAlongRay(null);

            if (!CanShift(cards, direction, shiftDistance))
            {
                cardsAffected = new List<SnapShot>();
                return false;
            }

            cardsAffected = new List<SnapShot>();

            for (int i = 0; i < cards.Count; i++)
            {
                ray.Sheet.Remove(cards[i].point, cards[i].Layer);
            }

            for (int j = 0; j < cards.Count; j++)
            {
                Point p = new Point(cards[j].point.x + (direction.x * shiftDistance), cards[j].point.y + (direction.y * shiftDistance));

                cardsAffected.Add(cards[j].GetSnapShot(false, false));

                cards[j].point = p;
                ray.Sheet.AddObj(cards[j]);
                cards[j].CardShifted(p);
            }

            if (OnCardsChanged != null)
                OnCardsChanged(cards.ToArray());

            return true;
        }


        /// <summary>
        /// Can the card be shifted?
        /// </summary>
        /// <param name="c">the card specifies the start point, sheet and layer</param>
        /// <param name="direction">the direction where the shift will happen</param>
        /// <param name="distance">the distance of the shift</param>
        /// <returns>returns true if the shift can happen false if not</returns>
        public static bool CanShift(Card c, Point direction, int distance)
        {
            if (c == null)
                throw new NullReferenceException("The card cannot be null");

            return CanShift(c.point, direction, c.Layer, c.SheetID, distance);
        }

        /// <summary>
        /// Can the cards be shifted?
        /// </summary>
        /// <param name="startPoint">the start point</param>
        /// <param name="direction">the direction where the shift will take place</param>
        /// <param name="layer">the layer where the shift will happen</param>
        /// <param name="sheetID">the id of the sheet</param>
        /// <param name="shiftDistance">the shift distance</param>
        /// <returns>returns true if the cards can be shifted, false if not</returns>
        public static bool CanShift(Point startPoint, Point direction, Layer layer, string sheetID, int shiftDistance)
        {
            PointRay ray = new PointRay(startPoint, direction, layer, sheetID, -1);
            var cards = ray.GetCardsAlongRay(null);

            if (cards.Count < 1)
                return false;

            if (!CanShift(cards, direction, shiftDistance))
                return false;

            return true;
        }


        /// <summary>
        /// Can the cards be shifted?
        /// </summary>
        /// <param name="cards">the list of cards</param>
        /// <param name="direction">the direction where the cards will be shifted</param>
        /// <param name="shiftDistance">the distance where the cards will shift</param>
        /// <returns>returns true if the cards can be shifted the direction, false if not</returns>
        public static bool CanShift(List<Card> cards, Point direction, int shiftDistance)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Point p = new Point(cards[i].point.x + (direction.x * shiftDistance), cards[i].point.y + (direction.y * shiftDistance));

                if (!cards[i].CanShift || !p.IsInGridBounds)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        /// <summary>
        /// Swap two adjacent cards' locations or at least shift the selected card if not cards are present...
        /// </summary>
        /// <param name="location">the location of the card</param>
        /// <param name="directionToSwap">the direction the cards are being swapped</param>
        /// <param name="layer">the layer</param>
        /// <param name="sheetID">the sheet id</param>
        public static void SwapOrShiftCard(Point location, Point directionToSwap, Layer layer, Sheet<Card> sheet, bool createSnapShot)
        {

            Point swapCardPoint = location + directionToSwap;

            //UnityEngine.Debug.Log(location + " " + directionToSwap + " " + swapCardPoint);

            Card cardToSwap = GetCard(swapCardPoint, layer, sheet.ID);

            Card cardToShift = GetCard(location, layer, sheet.ID);

            if (cardToShift != null)
            {

                if (CanShift(cardToShift, directionToSwap, 1))
                {
                    if (cardToSwap != null)
                    {
                        if (RemoveCard(cardToSwap.point, layer, sheet.ID))
                        {

                            //CardPoolManager.Main.OnUndoRedo();

                            if (TryShiftCard(cardToShift, directionToSwap, 1, false))
                            {

                                cardToSwap.point = location;

                                if (cardToSwap != null)
                                {
                                    TryDropCard(sheet, cardToSwap.point, sheet.CurrentLayer, cardToSwap.Entry, out var id);
                                    // CardPoolManager.Main.OnRefresh();
                                }
                                else
                                {
                                    UnityEngine.Debug.Log("cardToSwap = null");
                                }
                            }
                        }
                    }
                    else
                    {
                        TryShiftCard(cardToShift, directionToSwap, 1, false);
                    }
                }
            }

            if (createSnapShot)
            {
                SimpleUndoRedoManager.main.NewSnapShot();
            }
        }

        #region other
        /// <summary>
        /// Check the validity of the format of the ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sourceTag"></param>
        /// <returns></returns>
        private static string CheckID(string id, string sourceTag)
        {
            if (id == null)
                throw new NullReferenceException("The id null: " + sourceTag);

            if (string.Empty == id)
            {
                throw new NotSupportedException("The string is empty: " + sourceTag);
            }

            return id.Trim();
        }

        /// <summary>
        /// Returns the square distance of the the two 2d (float, float) tuple vectors
        /// </summary>
        /// <param name="a">first vector</param>
        /// <param name="b">second vector</param>
        /// <returns>returns the square distance (magnitude)</returns>
        public static float GetSqrMagnitude((float x, float y) a, (float x, float y) b)
        {
            float x = a.x - b.x;
            x = (float)Math.Pow(x, 2);

            float y = a.y - b.y;
            y = (float)Math.Pow(y, 2);

            (float x, float y) z = (x, y);
            return z.y + z.x;
        }

        /// <summary>
        /// Get the list of cards from list a that are not in list b
        /// </summary>
        /// <param name="a">the first given list of cards</param>
        /// <param name="b">the second given list of cards</param>
        /// <returns>returns a list of cards</returns>
        public static List<Card> Subtract(List<Card> a, List<Card> b)
        {
            List<Card> final = new List<Card>();

            for (int i = 0; i < a.Count; i++)
            {
                Card c = b.Find(x => x == a[i]);

                if (c == null)
                {
                    final.Add(a[i]);
                }
            }

            return final;
        }

        /// <summary>
        /// Get the cards that exist in both a and b
        /// </summary>
        /// <param name="a">the first list of cards</param>
        /// <param name="b">the second list of cards</param>
        /// <returns>returns a list of cards</returns>
        public static List<Card> Difference(List<Card> a, List<Card> b)
        {
            List<Card> final = new List<Card>();

            for (int i = 0; i < a.Count; i++)
            {
                Card c = b.Find(x => x == a[i]);

                if (c != null)
                {
                    final.Add(a[i]);
                }
            }

            return final;
        }

        #endregion

        /// <summary>
        /// Drop a card into a location on a given sheet. Adds a snapshot to the undo/redo manager
        /// </summary>
        /// <param name="sheet">the sheet to drop the card</param>
        /// <param name="dropLoc">the point location to drop the card</param>
        /// <param name="layer">the sheet layer to drop the card</param>
        /// <param name="entry">the data entry to spawn for the card to represent</param>
        /// <param name="cardID">the id of the card (out)</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryDropCard(Sheet<Card> sheet, Point dropLoc, Layer layer, DataEntry entry, out string cardID)
        {
            string datasetId = entry.DataSet.ID;
            string dataElementID = entry.RowID;

            // UnityEngine.Debug.Log(dropLoc);

            string someCardID = "";

            bool results = TryDropCard(sheet, dropLoc, layer, datasetId, dataElementID, out someCardID);

            cardID = someCardID;
            return results;
        }

        /// <summary>
        /// Drop a card into a location on a given sheet. Adds a snapshot to the undo/redo manager
        /// </summary>
        /// <param name="sheet">the sheet to drop the card</param>
        /// <param name="dropLoc">the point location to drop the card</param>
        /// <param name="layer">the sheet layer to drop the card</param>
        /// <param name="datasetId">the the id of the data set that the card will represent</param>
        /// <param name="dataElementID">the element id of the specific data that the card will show</param>
        /// <returns>returns true if adding the card was successful, false if not</returns>
        public static bool TryDropCard(Sheet<Card> sheet, Point dropLoc, Layer layer, string datasetId, string dataElementID, out string cardId)
        {
            if (GetCard(dropLoc, layer, sheet.ID) != null)
            {
                TryShift(dropLoc, DefaultShiftDirection, layer, sheet.ID, 1, out var snapShots);
            }

            if (GetCard(dropLoc, layer, sheet.ID) == null)
            {
                string id = Guid.NewGuid().ToString(); // definitely an easier way to generate ids... (get the largest id at the beginning of the creation of the cards and increment from there)
                cardId = id;


                Card c = new Card(dropLoc, id, sheet.ID, layer.ID, datasetId, dataElementID.ToString());


                if (TryAddCard(c, true))
                {
                    WarManager.Unity3D.CardPoolManager.Main.AddNewCard(dropLoc, id);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                cardId = "";
                return false;
            }
        }

        /// <summary>
        /// Try to drop a title card on a sheet
        /// </summary>
        /// <param name="sheet">the sheet to drop the title card</param>
        /// <param name="dropLoc">the location to drop the card</param>
        /// <param name="layer">the layer to drop the card on</param>
        /// <param name="cardID">the card id</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryDropTitleCard(Sheet<Card> sheet, Point dropLoc, Layer layer, out string cardID)
        {

            string entryId = Guid.NewGuid().ToString();

            TitleCardActor actor = new TitleCardActor();
            var entry = actor.GetDataEntry(entryId, "");


            bool result = TryDropCard(sheet, dropLoc, layer, entry, out var id);
            cardID = id;

            var card = GetCard(id);

            EditCard(GetCard(id), 1, "Edit Note Card");

            return result;
        }

        /// <summary>
        /// Try drop a note card on a sheet
        /// </summary>
        /// <param name="sheet">the sheet to drop the note card on</param>
        /// <param name="dropLoc">the location to drop the card</param>
        /// <param name="layer">the layer to drop the card on</param>
        /// <param name="cardID">the id of the card</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryDropNoteCard(Sheet<Card> sheet, Point dropLoc, Layer layer, out string cardID)
        {
            string entryId = Guid.NewGuid().ToString();

            NoteCardActor actor = new NoteCardActor();
            var entry = actor.GetDataEntry(entryId, "");


            bool result = TryDropCard(sheet, dropLoc, layer, entry, out var id);
            cardID = id;

            var card = GetCard(id);

            UnityEngine.Debug.Log("card");

            EditCard(GetCard(id), 1, "Edit Note Card");


            return result;
        }

        /// <summary>
        /// Edit the card
        /// </summary>
        /// <param name="card"></param>
        /// <param name="column"></param>
        /// <param name="contextTitle"></param>
        private static void EditCard(Card card, int column, string contextTitle)
        {
            EditTextMessageBoxController.OpenModalWindow("", contextTitle, (x) =>
            {
                UnityEngine.Debug.Log("opening modal window");

                if (x != string.Empty && x != null)
                {

                    //UnityEngine.Debug.Log("getting data entry");

                    var entry = card.Entry;

                    //UnityEngine.Debug.Log("getting data set");

                    var dataset = entry.DataSet;

                    //UnityEngine.Debug.Log("constructing new value");

                    ValueTypePair newPair = new ValueTypePair(x, ColumnInfo.GetValueTypeOfParagraph, column);


                    //UnityEngine.Debug.Log("updating new value");

                    entry.Actor.OnUpdateInput(new List<ValueTypePair>() { newPair });

                }
                else
                {
                    //UnityEngine.Debug.Log("attempting to remove card");

                    if (SheetsManager.TryGetActiveSheet(card.SheetID, out var sheet))
                    {
                        CardUtility.RemoveCard(card.point, sheet.CurrentLayer, sheet.ID);
                    }
                }
            });
        }

        /// <summary>
        /// Select all cards on the current sheet
        /// </summary>
        /// <returns>returns the list of selected cards</returns>
        public static List<Card> SelectAllCards()
        {
            return SelectOrDeselectAllCards(true);
        }

        /// <summary>
        /// Deselect all cards on the current sheet
        /// </summary>
        public static void DeslectAllCards()
        {
            SelectOrDeselectAllCards(false);
        }

        /// <summary>
        /// internal - handles the code for select all and deselect all
        /// </summary>
        /// <param name="selectAll">should this method select or deselect all cards?</param>
        /// <returns>returns a list of select cards (empty if selectAll is false)</returns>
        private static List<Card> SelectOrDeselectAllCards(bool selectAll)
        {
            CopyPasteDuplicate cardCommands = new CopyPasteDuplicate();
            cardCommands.SelectAll(selectAll);
            return SheetsCardSelectionManager.Main.GetSelectedCards(SheetsManager.CurrentSheetID);
        }

        /// <summary>
        /// Add the selected card to the current sheet list of selected cards
        /// </summary>
        /// <param name="card">the card to be selected</param>
        public static void SelectCard(Card card)
        {
            SheetsCardSelectionManager.Main.AddSelectedCard(card);
        }

        /// <summary>
        /// Used in order to drag a card from the list of cards (future feature)
        /// </summary>
        /// <param name="sheet">the card sheet</param>
        /// <param name="layer">the layer the card will be placed on</param>
        /// <param name="datasetId">the dataset id of the card</param>
        /// <param name="dataElementID">the element data id</param>
        /// <param name="cardID">the resulting card id</param>
        /// <returns>returns true if the operation was successful, false if not</returns>
        public static bool TryInsertDragCard(Sheet<Card> sheet, Layer layer, string datasetId, string dataElementID, out string cardID)
        {
            string cardId = "";

            Point p = Point.zero;


            bool foundEmpty = false;
            int i = 0;

            while (!foundEmpty)
            {
                if (GetCard(p, layer) != null)
                {
                    p = new Point(i + 1, 0);
                }
                else
                {
                    foundEmpty = true;
                }

                i++;
            }

            if (TryDropCard(sheet, p, layer, datasetId, dataElementID, out cardId))
            {

                Card card = GetCard(cardId);
                card.StartDrag(false);

                //the front end card functionality does not get set here (it should)... so where does that happen??

                card.SetCardToInputPosition();
                //set the card position to the position of the mouse...

                cardID = cardId;
                return true;
            }
            else
            {
                cardID = cardId;
                return false;
            }
        }

        /// <summary>
        /// Get cards with a specific data entry on the sheet
        /// </summary>
        /// <param name="entries">the data entries to find</param>
        /// <returns>Returns a list of card</returns>
        public static List<Card> FindCardsWithDataEntry(DataEntry[] entries)
        {
            List<Card> final = new List<Card>();

            var cards = GetCardsFromCurrentSheet();

            foreach (var card in cards)
            {
                foreach (var entry in entries)
                {
                    if (card.DatasetID == entry.DataSet.ID && card.RowID == entry.RowID)
                    {
                        final.Add(card);
                    }
                }
            }

            return final;
        }


        /// <summary>
        /// Locks all other cards on the current sheet except for the selected card dataset types
        /// </summary>
        /// <param name="card">the sample card</param>
        /// <returns>returns the list of cards</returns>
        public static List<Card> IsolateDataSetOnSheet(DataSet set)
        {
            List<Card> cards = GetCardsFromCurrentSheet();

            List<Card> lockedCards = new List<Card>();

            foreach (var x in cards)
            {
                if (x.DatasetID != set.ID)
                {
                    if (x.CanLockOrUnlock)
                    {
                        lockedCards.Add(x);
                        x.Lock(true);
                    }
                }
                else
                {
                    if (x.CanLockOrUnlock)
                    {
                        x.Lock(false);
                    }
                }
            }

            return lockedCards;
        }

        /// <summary>
        /// Gets the list of cards and finds all the data sets
        /// </summary>
        /// <param name="cards">the cards</param>
        /// <returns>returns the list of datasets</returns>
        public static List<DataSet> GetDataSetsFromCards(IList<Card> cards)
        {
            List<DataSet> dataSets = new List<DataSet>();

            foreach (var x in cards)
            {
                if (!dataSets.Contains(x.DataSet))
                    dataSets.Add(x.DataSet);
            }

            return dataSets;
        }

        /// <summary>
        /// Get the allowed tags from all cards
        /// </summary>
        /// <param name="cards">the list of cards</param>
        /// <returns>returns a string list of cards with selected tags</returns>
        public static List<KeyValuePair<string, DataSet>> GetAllowedTagsFromCards(IList<Card> cards)
        {
            var sets = GetDataSetsFromCards(cards);

            List<KeyValuePair<string, DataSet>> result = new List<KeyValuePair<string, DataSet>>();

            foreach (var x in sets)
            {
                List<string> data = x.AllowedTags;

                UnityEngine.Debug.Log(string.Join(" | ", data));

                foreach (var y in data)
                {

                    UnityEngine.Debug.Log(y);

                    KeyValuePair<string, DataSet> pair = new KeyValuePair<string, DataSet>(y, x);

                    UnityEngine.Debug.Log(pair.Key + " " + pair.Value.DatasetName);

                    result.Add(pair);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the cards that contain a selected data set
        /// </summary>
        /// <param name="cards">the cards</param>
        /// <param name="selectedDataSet">the dataset</param>
        /// <returns>returns the list of cards</returns>
        public static List<Card> GetCardsFromDataSet(IList<Card> cards, string selectedDataSetID)
        {

            if (selectedDataSetID == null)
                throw new NullReferenceException("The selected data set id cannot be null");

            if (selectedDataSetID.Trim() == string.Empty)
                throw new ArgumentException("The selected data set id cannot be empty");

            if (cards == null || cards.Count == 0)
                return new List<Card>();

            List<Card> selectedCards = new List<Card>();

            foreach (var x in cards)
            {
                if (x.DataSet.ID == selectedDataSetID)
                {
                    selectedCards.Add(x);
                }
            }

            return selectedCards;
        }

        /// <summary>
        /// Get the common info from all selected entries
        /// </summary>
        /// <param name="entries">the entries</param>
        /// <param name="searchType">the type of search</param>
        /// <returns>returns the list of column info</returns>
        public static List<ColumnInfo> GetCommonColumnInfoFromCards(IList<Card> entries, SearchType searchType)
        {
            List<ColumnInfo> data = new List<ColumnInfo>();
            List<DataSet> sets = new List<DataSet>();

            Dictionary<DataSet, List<ColumnInfo>> infos = new Dictionary<DataSet, List<ColumnInfo>>();

            foreach (var x in entries)
            {
                if (!sets.Contains(x.DataSet))
                {
                    sets.Add(x.DataSet);
                    infos.Add(x.DataSet, x.Entry.GetAllowedColumnInfo());
                }
            }

            for (int i = 0; i < sets.Count; i++)
            {
                var colInfo = infos[sets[i]];

                foreach (var col in colInfo)
                {
                    if (!data.Contains(col))
                    {
                        for (int k = 0; k < sets.Count; k++)
                        {
                            if (k != i)
                            {
                                var results = new List<ColumnInfo>();

                                if (searchType == SearchType.valueType)
                                    results = infos[sets[k]].FindAll(x => x.ValueType == col.ValueType);
                                else
                                    results = infos[sets[k]].FindAll(x => x.HeaderName.Contains(col.HeaderName));

                                foreach (var x in results)
                                {
                                    data.Add(x);
                                }

                                data.Add(col);
                            }
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Get column info that has a specific header or value type
        /// </summary>
        /// <param name="cards">the selected cards</param>
        /// <param name="type">the search type</param>
        /// <param name="tag">the value type or header</param>
        /// <returns>returns the list of column info</returns>
        public static List<ColumnInfo> GetCommonColumnInfoFromCards(IList<Card> cards, SearchType type, string tag)
        {

            if (cards == null)
                throw new NullReferenceException("the cards list cannot be null");

            if (cards.Count == 0)
                return new List<ColumnInfo>();

            if (tag == null)
                throw new NullReferenceException("the value cannot be null");

            if (tag == string.Empty) throw new ArgumentException("the value cannot be empty");

            List<ColumnInfo> info = new List<ColumnInfo>();

            foreach (var x in cards)
            {
                if (x != null && x.DataSet.SelectedView.CanViewCard)
                {
                    List<ColumnInfo> columnInfo = new List<ColumnInfo>();


                    if (type == SearchType.header)
                        columnInfo = x.Entry.GetAllowedColumnInfo().FindAll(y => y.HeaderName == tag);
                    else
                        columnInfo = x.Entry.GetAllowedColumnInfo().FindAll(y => y.ValueType == tag);

                    foreach (var y in columnInfo)
                    {
                        info.Add(y);
                    }
                }
            }

            return info;
        }

        /// <summary>
        /// Get clusters
        /// </summary>
        /// <param name="sheet">the card sheet</param>
        /// <param name="layer">the layer</param>
        /// <param name="includeOneCardInCluster">should a cluster definition include one card (without other cards adjacent to the card)?</param>
        /// <exception cref="NullReferenceException">thrown when the sheet or layer is null</exception>
        public static List<Card[]> GetCardClustersOnSheet(Sheet<Card> sheet, Layer layer, bool includeOneCardInCluster = false)
        {

            if (sheet == null)
                throw new NullReferenceException("the sheet cannot be null");

            if (layer == null)
                throw new NullReferenceException("the layer cannot be null");


            var allCards = sheet.GetAllObj(new List<Layer>() { layer });

            List<Card[]> cards = new List<Card[]>();

            int iterator = 0;

            while (allCards.Count > 0 && iterator < 100)
            {
                var cluster = GetCardCluster(allCards[0], layer, sheet.ID);

                if (includeOneCardInCluster)
                {
                    cards.Add(cluster.ToArray());
                }
                else
                {
                    if (cluster.Count > 1)
                    {
                        cards.Add(cluster.ToArray());
                    }
                }

                for (int k = 0; k < cluster.Count; k++)
                {
                    for (int i = allCards.Count - 1; i >= 0; i--)
                    {
                        if (allCards[i].ID == cluster[k].ID)
                        {
                            allCards.RemoveAt(i);
                        }
                    }
                }

                iterator++;
            }

            return cards;
        }


        /// <summary>
        /// Get the card cluster bounding boxes
        /// </summary>
        /// <param name="sheet">the sheet</param>
        /// <param name="layer">the layer to pull from</param>
        /// <returns>returns the dictionary of the list of cards and the rect location</returns>
        /// <exception cref="NullReferenceException">thrown when the sheet or layer is null</exception>
        public static Dictionary<List<Card>, Rect> GetCardClusterBoundingBoxes(Sheet<Card> sheet, Layer layer)
        {
            if (sheet == null)
                throw new NullReferenceException("The sheet cannot be null");

            if (layer == null)
                throw new NullReferenceException("the layer cannot be null");

            List<Card[]> cards = GetCardClustersOnSheet(sheet, layer);

            Dictionary<List<Card>, Rect> bounds = new Dictionary<List<Card>, Rect>();

            //UnityEngine.Debug.Log(cards.Count);

            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Length > 0)
                {

                    int lowestPointX = 100000;
                    int lowestPointY = 100000;

                    int highestPointX = -100000;
                    int highestPointY = -100000;

                    foreach (var x in cards[i])
                    {
                        if (x.point.x > highestPointX)
                        {
                            highestPointX = x.point.x;
                        }

                        if (x.point.x < lowestPointX)
                        {
                            lowestPointX = x.point.x;
                        }

                        if (x.point.y > highestPointY)
                        {
                            highestPointY = x.point.y;
                        }

                        if (x.point.y < lowestPointY)
                        {
                            lowestPointY = x.point.y;
                        }
                    }

                    Point a = new Point(lowestPointX, lowestPointY);
                    Point b = new Point(highestPointX, highestPointY);

                    Rect rect = new Rect(a, b);

                    List<Card> cardsList = new List<Card>();
                    cardsList.AddRange(cards[i]);

                    bounds.Add(cardsList, rect);
                }
            }

            return bounds;
        }

        /// <summary>
        /// Get the cluster of cards adjacent to the <paramref name="selectedCard"/>
        /// </summary>
        /// <param name="selectedCard">the card to find a cluster next to</param>
        /// <param name="layer">the sheet layer</param>
        /// <param name="sheetId">the id of the sheet</param>
        /// <returns>the list of cards in that cluster of the selected card</returns>
        public static List<Card> GetCardCluster(Card selectedCard, Layer layer, string sheetId)
        {
            return GetCardCluster_Recursive(selectedCard, layer, sheetId, new List<Card>());
        }

        /// <summary>
        /// Get the card cluster (internal and recursive)
        /// </summary>
        /// <param name="sampleCard">the sample card</param>
        /// <param name="layer">the sheet layer</param>
        /// <param name="sheetID">the id of the sheet</param>
        /// <param name="cardsInCluster">the list of selected cards</param>
        /// <returns>returns the list of clustered cards</returns>
        private static List<Card> GetCardCluster_Recursive(Card sampleCard, Layer layer, string sheetID, List<Card> cardsInCluster)
        {

            if (sampleCard == null || layer == null || sheetID == null || sheetID.Trim().Length == 0)
                return new List<Card>();

            var pnts = GetAllAdjacentGridPositions(sampleCard.point);

            cardsInCluster.Add(sampleCard);

            for (int i = 0; i < pnts.Count; i++)
            {
                var nextCard = GetCard(pnts[i], layer, sheetID);

                if (nextCard != null && !cardsInCluster.Contains(nextCard))
                {
                    cardsInCluster.Add(nextCard);
                    GetCardCluster_Recursive(nextCard, layer, sheetID, cardsInCluster);
                }
            }


            return cardsInCluster;

        }

        /// <summary>
        /// Get the selected cards of the current sheet
        /// </summary>
        /// <returns>the list of selected cards</returns>
        public static List<Card> GetSelectedCardsOnCurrentSheet()
        {
            var cards = SheetsCardSelectionManager.Main.GetCurrentSelectedCards();
            return cards;
        }
    }

    public enum SearchType
    {
        header,
        valueType
    }
}
