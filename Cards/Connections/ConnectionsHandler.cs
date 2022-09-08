
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Backend;

namespace WarManager.Cards
{
    [Notes.Author("Handles the list of card connections for a given sheet")]
    public class ConnectionsHandler
    {
        /// <summary>
        /// the list of card connections
        /// </summary>
        /// <typeparam name="CardConnection"></typeparam>
        /// <returns></returns>
        private List<CardConnection> _cardConnectionsList = new List<CardConnection>();

        /// <summary>
        /// The card connections count
        /// </summary>
        /// <value></value>
        public IEnumerable<CardConnection> CardConnections
        {
            get => _cardConnectionsList;
        }

        /// <summary>
        /// The count of the card connections
        /// </summary>
        /// <value></value>
        public int Count
        {
            get => _cardConnectionsList.Count;
        }

        /// <summary>
        /// Empty Constructor
        /// </summary>
        public ConnectionsHandler()
        {

        }

        /// <summary>
        /// The card connections
        /// </summary>
        /// <param name="connection"></param>
        public void AddConnection(CardConnection connection)
        {
            if (connection == null)
                throw new NullReferenceException("the connection cannot be null");

            _cardConnectionsList.Add(connection);
        }

        /// <summary>
        /// Get connections visible to the camera
        /// </summary>
        /// <returns>returns an array of connections, an empty array if non are found visible</returns>
        public CardConnection[] GetVisibleCardConnections()
        {
            List<CardConnection> connections = new List<CardConnection>();

            var cameraController = WarManagerCameraController.MainController;

            foreach (var x in connections)
            {
                var locationA = x.CardA.point.ToVector2();
                var locationB = x.CardB.point.ToVector2();
                if (cameraController.IsInOrthographicCameraBounds(locationA, 1))
                {
                    connections.Add(x);
                }
                else if (cameraController.IsInOrthographicCameraBounds(locationB, 1))
                {
                    connections.Add(x);
                }
            }

            return connections.ToArray();
        }
    }
}
