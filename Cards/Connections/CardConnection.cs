
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Backend;

namespace WarManager.Cards
{
    [Notes.Author("Provides a connection between cards for transferring data and describing relationships")]
    public class CardConnection : IComparable<CardConnection>, IEquatable<CardConnection>
    {
        /// <summary>
        /// The ID of the card connection
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// private backing field
        /// </summary>
        private ConnectionHandler _cardA;

        /// <summary>
        /// Card A (the first card)
        /// </summary>
        /// <value></value>
        public ConnectionHandler CardA
        {
            get => _cardA;
            set
            {
                if (value == null)
                    throw new NullReferenceException("Card A cannot be null");

                _cardA = value;
            }
        }

        /// <summary>
        /// private backing field
        /// </summary>
        private ConnectionHandler _cardB;

        /// <summary>
        /// Card B (the second card)
        /// </summary>
        /// <value></value>
        public ConnectionHandler CardB
        {
            get => CardB;
            set
            {
                if (value == null)
                    throw new NullReferenceException("Card B cannot be null");

                _cardB = value;
            }
        }

        /// <summary>
        /// The color of the connection
        /// </summary>
        /// <value></value>
        public string HTMLColor { get; private set; }

        /// <summary>
        /// The connection id for card a
        /// </summary>
        /// <value></value>
        public string CardAConnectionID { get; private set; }

        /// <summary>
        /// The connection id for card b
        /// </summary>
        /// <value></value>
        public string CardBConnectionID { get; private set; }

        /// <summary>
        /// Is the card connection visible?
        /// </summary>
        /// <value></value>
        public bool Visible { get; private set; }

        /// <summary>
        /// Constructor - generates an ID
        /// </summary>
        /// <param name="cardA">card a of the connection</param>
        /// <param name="cardB">card b of the connection</param>
        /// <param name="htmlColor">the html color of the connection</param>
        public CardConnection(ConnectionHandler cardA, ConnectionHandler cardB, string htmlColor)
        {
            string id = Guid.NewGuid().ToString();
            InitConnection(id, cardA, cardB, htmlColor, "", "");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">the id of the connection</param>
        /// <param name="cardA">card a connection</param>
        /// <param name="cardB">card b connection</param>
        /// <param name="htmlColor">the html color</param>
        public CardConnection(string id, ConnectionHandler cardA, ConnectionHandler cardB, string htmlColor)
        {
            InitConnection(id, cardA, cardB, htmlColor, "", "");
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">the id of the connection</param>
        /// <param name="cardA">card a connection</param>
        /// <param name="cardB">card b connection</param>
        /// <param name="htmlColor">the html color</param>
        /// <param name="connectionIDA">the id of connection a</param>
        /// <param name="connectionIDB">the id of connection b</param>
        public CardConnection(string id, ConnectionHandler cardA, ConnectionHandler cardB, string htmlColor, string cardAConnectionID, string cardBConnectionID)
        {
            InitConnection(id, cardA, cardB, htmlColor, cardAConnectionID, cardBConnectionID);
        }

        /// <summary>
        /// Take values from the constructor to create a card connection
        /// </summary>
        /// <param name="id">the id</param>
        /// <param name="cardA">card a</param>
        /// <param name="cardB">card b</param>
        /// <param name="htmlColor">the color of the connection</param>
        /// <param name="connectionIDA">the id of connection a</param>
        /// <param name="connectionIDB">the id of connection b</param>
        private void InitConnection(string id, ConnectionHandler cardA, ConnectionHandler cardB, string htmlColor, string connectionIDA, string connectionIDB)
        {
            if (cardA == null)
                throw new NullReferenceException("Card A cannot be null");

            if (cardB == null)
                throw new NullReferenceException("Card B cannot be null");

            if (cardA.ID == cardB.ID)
                throw new ArgumentException("A card cannot be connected to itself");

            if (id == null)
                throw new NullReferenceException("the connection id is null");

            if (id.Trim() == string.Empty)
                throw new ArgumentException("the connection id is empty");

            if (htmlColor == null || htmlColor.Trim() == string.Empty)
                htmlColor = "#ddd"; //light gray

            ID = id;

            CardA = cardA;
            CardB = cardB;
            HTMLColor = htmlColor;

            if (connectionIDA == null)
                connectionIDA = "";

            if (connectionIDB == null)
                connectionIDB = "";

            CardAConnectionID = connectionIDA;
            CardBConnectionID = connectionIDB;

            SetVisibility();
        }

        public int CompareTo(CardConnection other)
        {
            if (other == null)
                return 1;

            int compareCardA = CardA.ID.CompareTo(other.CardA.ID);

            if (compareCardA == 0)
            {
                int compareCardB = CardB.ID.CompareTo(other.CardB.ID);
                if (compareCardB == 0)
                {
                    var compareColor = HTMLColor.CompareTo(other.HTMLColor);
                    return compareColor;
                }
                else
                {
                    return compareCardB;
                }
            }
            else
            {
                return compareCardA;
            }
        }

        public bool Equals(CardConnection other)
        {
            if (other == null)
                return false;

            if (CardA.ID == other.CardA.ID)
            {
                if (CardB.ID == other.CardB.ID)
                {
                    if (HTMLColor == other.HTMLColor)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Set the visibility
        /// </summary>
        public void SetVisibility()
        {
            if (CardA.Visible || CardB.Visible)
                Visible = true;
            else
                Visible = false;
        }
    }
}
