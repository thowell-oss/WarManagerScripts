using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Cards
{
    /// <summary>
    /// Pass event args to other cards within a group system
    /// </summary>
    public class CardGroupEventArgs
    {
        /// <summary>
        /// Pass a message to other cards
        /// </summary>
        public object message { get; private set; }

        /// <summary>
        /// The cards this message is supposed to go to
        /// </summary>
        public Card[] ToCards { get; private set; }


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="message">the message to pass to other cards</param>
        /// <param name="toCards">the cards to recive the action</param>
        public CardGroupEventArgs(object message, Card[] toCards)
        {
            this.message = message;
            ToCards = toCards;
        }
    }
}
