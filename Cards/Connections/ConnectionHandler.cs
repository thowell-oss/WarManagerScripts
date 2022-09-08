
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles the connections with the cards
    /// </summary>
    [Notes.Author("Handles the connections with the cards")]
    public class ConnectionHandler
    {
        /// <summary>
        /// Is the card visible?
        /// </summary>
        /// <value></value>
        public bool Visible { get => Card.CardRepresented; }

        /// <summary>
        /// The ID of the card
        /// </summary>
        /// <value></value>
        public string ID { get => Card.ID; }

        /// <summary>
        /// The location the card is in
        /// </summary>
        public Point point => Card.point;

        /// <summary>
        /// The card reference
        /// </summary>
        /// <value></value>
        public Card Card { get; private set; }

        private List<CardConnection> _connections = new List<CardConnection>();

        /// <summary>
        /// get the connections
        /// </summary>
        /// <value></value>
        public IEnumerable<CardConnection> Connections
        {
            get => _connections;
        }

        /// <summary>
        /// the count of all connections
        /// </summary>
        public int Count => _connections.Count;

        /// <summary>
        /// Initialize the connection handler
        /// </summary>
        /// <param name="card"></param>
        public void Init(Card card)
        {
            if (Card != null)
                throw new ArgumentException("The card is already initialized!");

            Card = card;
        }

        /// <summary>
        /// Add a card connection to the card
        /// </summary>
        private void AddCardConnection(CardConnection newConnection)
        {
            if (newConnection == null)
                throw new NullReferenceException("the new connection cannot be null");

            _connections.Add(newConnection);
        }

        /// <summary>
        /// Remove a card connection
        /// </summary>
        /// <param name="oldConnection"></param>
        private void RemoveCardConnection(CardConnection oldConnection)
        {
            if (oldConnection == null)
                throw new NullReferenceException("the connection cannot be null");

            _connections.Remove(oldConnection);
        }

        /// <summary>
        /// Replace an existing card connection
        /// </summary>
        /// <param name="oldConnection">the old card connection</param>
        /// <param name="newConnection">the new card connection</param>
        private void ReplaceConnection(CardConnection oldConnection, CardConnection newConnection)
        {
            if (oldConnection == null)
                throw new NullReferenceException("the connection cannot be null");

            if (newConnection == null)
                throw new NullReferenceException("the new connection cannot be null");

            RemoveCardConnection(oldConnection);

            AddCardConnection(newConnection);
        }

        /// <summary>
        /// Deconstruct the card handler
        /// </summary>
        public void Deconstruct()
        {
            for (int i = _connections.Count - 1; i >= 0; i--)
            {
                CardConnection connection = _connections[i];
                RemoveCardConnection(connection);
            }

            Card = null;
        }

        public void SetVisible(bool visible)
        {

        }
    }
}
