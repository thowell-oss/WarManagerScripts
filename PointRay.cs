
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;

namespace WarManager
{
    public class PointRay
    {
        /// <summary>
        /// The sheet the ray will be preformed on
        /// </summary>
        public string SheetID { get; private set; }

        /// <summary>
        /// The layer the ray will be preformed on
        /// </summary>
        public Layer Layer { get; private set; }

        /// <summary>
        /// The start point of the ray
        /// </summary>
        public Point Start { get; private set; }

        /// <summary>
        /// The direction of the ray
        /// </summary>
        public Point Direction { get; private set; }

        /// <summary>
        /// The distance the ray will last (if <= 0 -> the ray will stop at null)
        /// </summary>
        public int Distance { get; private set; }

        private RayNullCardBehavior _nullBehavior = RayNullCardBehavior.StopAtFirstNull;

        /// <summary>
        /// How should the ray interact with null card locations?
        /// </summary>
        /// <value></value>
        public RayNullCardBehavior NullBehavior
        {
            get
            {
                if (Distance <= 0)
                    return RayNullCardBehavior.StopAtFirstNull;

                return _nullBehavior;
            }
            private set
            {
                _nullBehavior = value;
            }
        }

        /// <summary>
        /// Should the card at the start point be included?
        /// </summary>
        /// <value></value>
        public bool IncludeStartCard { get; private set; } = true;

        public Sheet<Card> Sheet { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="start">the start point of the ray</param>
        /// <param name="direction">the direction the ray will fire</param>
        /// <param name="layer">the layer the ray will fire</param>
        /// <param name="sheetId">the sheet the ray will fire</param>
        /// <param name="distance">the distance of the ray (-1 or less will go until there are no cards on the sheet)</param>
        /// <param name="stopAtNull">should the ray stop at a null card?</param>
        public PointRay(Point start, Point direction, Layer layer, string sheetId, int distance = -1, RayNullCardBehavior stopAtNull = RayNullCardBehavior.StopAtFirstNull, bool includeStartCard = true)
        {
            if (start == null)
                throw new NullReferenceException("The start point cannot be null");

            if (direction == null)
                throw new NullReferenceException("The direction cannot be null");

            if (layer == null)
                throw new NullReferenceException("The layer cannot be null");

            if (sheetId == null)
                throw new NullReferenceException("the sheetid cannot be null");

            Sheet = SheetsManager.GetActiveSheet(sheetId);

            if (Sheet == null)
                throw new NullReferenceException("The sheet cannot be null");


            Start = start;
            Direction = direction;
            Layer = layer;
            SheetID = sheetId;
            Distance = distance;
            NullBehavior = stopAtNull;
            IncludeStartCard = includeStartCard;
        }

        /// <summary>
        /// Returns a list of cards along the ray
        /// </summary>
        /// <returns>returns a list of cards, if there are no cards, then the list returns empty</returns>
        /// <remarks>The list of cards are not ordered in any particular way</remarks>
        public List<Card> GetCardsAlongRay(List<Card> doNotIncludeCards)
        {
            Card c = null;
            List<Card> cards = new List<Card>();

            if (IncludeStartCard)
            {
                c = CardUtility.GetCard(Start, Layer, SheetID);

                if (c == null && !HandleNullCard(c, cards))
                {
                    return cards;
                }
                else if (c != null)
                {
                    bool canContinue = true;

                    cards.AddRange(HandleAddingCard(c, 0, doNotIncludeCards, out canContinue));

                    if (!canContinue)
                        return CheckForDuplicates(cards);
                }
            }

            Point nextLocation = new Point(Start.x + Direction.x, Start.y + Direction.y);
            int distance = 0;

            bool contineRay = true;

            while (contineRay)
            {
                bool canContinue = false;

                c = CardUtility.GetCard(nextLocation, Layer, SheetID);

                if (c != null)
                {
                    cards.AddRange(HandleAddingCard(c, distance, doNotIncludeCards, out canContinue));
                }
                else
                {
                    canContinue = HandleNullCard(c, cards);
                }

                contineRay = canContinue && (distance <= Distance || Distance <= -1);

                nextLocation = new Point(nextLocation.x + Direction.x, nextLocation.y + Direction.y);
                distance++;

            }

            return CheckForDuplicates(cards);
        }

        private List<Card> CheckForDuplicates(List<Card> cards)
        {

            for (int i = 0; i < cards.Count; i++)
            {
                var duplicateChecklist = cards.FindAll((x) => x == cards[i]);

                if (duplicateChecklist.Count > 1)
                {
                    for (int k = 1; k < duplicateChecklist.Count; k++)
                    {
                        cards.Remove(duplicateChecklist[k]);
                    }
                }
            }

            cards.Sort();
            return cards;
        }

        /// <summary>
        /// Handles groups and no no cards
        /// </summary>
        /// <param name="c">the given question</param>
        /// <param name="distance"></param>
        /// <param name="doNotInclude">the cards not to include in the ray</param>
        private List<Card> HandleAddingCard(Card c, int distance, List<Card> doNotInclude, out bool canContinue)
        {
            List<Card> cards = new List<Card>();

            bool grouped = false;
            bool stretched = false;

            if (CanIncludeCard(c, doNotInclude))
            {
                if (c.Grouped)
                {
                    cards.AddRange(HandleGroupedCardsRays(distance, c.GroupCardsList));
                    cards.AddRange(c.GroupCardsList);
                    grouped = true;
                }

                //if (c.CardStretched)
                //{
                //    cards.AddRange(HandleGroupedCardsRays(distance, c.GroupStretchCardsList));
                //    cards.AddRange(c.GroupStretchCardsList);
                //    stretched = true;
                //}

                if (!grouped && !stretched)
                    cards.Add(c);
            }

            cards.Sort();

            if (grouped || stretched)
            {
                canContinue = false;
            }
            else
            {
                canContinue = true;
            }

            return cards;
        }

        /// <summary>
        /// Checks to see if the card is not on the no no list
        /// </summary>
        /// <param name="newCard">the card in question</param>
        /// <param name="doNotIncludeCardList">the no no card list reference</param>
        /// <returns>returns true if the card can be added to the list, false if not</returns>
        private bool CanIncludeCard(Card newCard, List<Card> doNotIncludeCardList)
        {
            if (newCard == null || doNotIncludeCardList == null || doNotIncludeCardList.Count < 1)
                return true;

            for (int i = 0; i < doNotIncludeCardList.Count; i++)
            {
                var card = doNotIncludeCardList.Find((x) => x.ID == newCard.ID);

                if (card != null)
                {
                    Debug.Log("Card not added to list " + card);
                    return false;
                }
            }

            return true;
        }


        public List<Card> HandleGroupedCardsRays(int distanceCompleted, List<Card> cardGrouplist)
        {
            //fire new rays -> include cards that do not contain existing grouped cards

            List<Card> finalCardsList = new List<Card>();

            int completedDistance = -1;

            if (distanceCompleted > 0)
            {
                completedDistance = Distance - distanceCompleted;
            }


            foreach (var card in cardGrouplist)
            {
                PointRay ray = new PointRay(card.point, Direction, Layer, SheetID, completedDistance, NullBehavior, false);
                List<Card> cards = ray.GetCardsAlongRay(cardGrouplist);

                finalCardsList.AddRange(cards.ToArray());
            }

            return finalCardsList;
        }

        /// <summary>
        /// Handles the behavior of null cards
        /// </summary>
        /// <param name="c">the null card</param>
        /// <param name="cards">the list of cards</param>
        /// <returns>returns true if the ray can continue, false if not</returns>
        private bool HandleNullCard(Card c, List<Card> cards)
        {
            bool canContinue = false;

            switch (NullBehavior)
            {
                case RayNullCardBehavior.ContinueAndSkipNull:
                    canContinue = true;
                    break;

                case RayNullCardBehavior.ContinueAndIncludeNull:
                    cards.Add(c);//bug
                    canContinue = true;
                    break;
                default:
                    canContinue = false;
                    break;
            }

            return canContinue;
        }
    }

    /// <summary>
    /// Dictates how the ray will handle null behavior
    /// </summary>
    public enum RayNullCardBehavior
    {
        ContinueAndIncludeNull,
        ContinueAndSkipNull,
        StopAtFirstNull,
    }
}
