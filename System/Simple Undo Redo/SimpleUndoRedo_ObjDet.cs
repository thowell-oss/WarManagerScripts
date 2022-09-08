
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;

namespace WarManager
{
    /// <summary>
    /// Provides object details for each object
    /// </summary>
    public class SimpleUndoRedo_ObjDet
    {
        /// <summary>
        /// The card
        /// </summary>
        /// <value></value>
        public Card Card { get; private set; }

        /// <summary>
        /// The point the card is located at
        /// </summary>
        /// <value></value>
        public Point CardLocation { get; private set; }

        /// <summary>
        /// The layer the card is located on
        /// </summary>
        /// <value></value>
        public Layer Layer { get; private set; }

        /// <summary>
        /// The sheet the card is on
        /// </summary>
        /// <value></value>
        public Sheet<Card> CardSheet { get; private set; }

        /// <summary>
        /// Is the card locked?
        /// </summary>
        /// <value></value>
        public bool Locked { get; private set; }

        /// <summary>
        /// Is this card selected?
        /// </summary>
        /// <value></value>
        public bool Selected {get; private set;}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="c">the card</param>
        /// <param name="p">the location the card was at</param>
        /// <param name="cardSheet">the sheet the card is located on</param>
        public SimpleUndoRedo_ObjDet(Card c, Point p, Layer layer, Sheet<Card> cardSheet)
        {
            if (c == null)
                throw new NullReferenceException("The card cannot be null");

            if (p == null)
                throw new NullReferenceException("The point cannot be null");

            //for some odd reason the layer is always null??

            if (cardSheet == null)
                throw new NullReferenceException("The card sheet cannot be null");

            Card = c;
            CardLocation = p;
            Layer = layer;
            CardSheet = cardSheet;
            Locked = c.CardLocked;

            // if(Locked)
            //     UnityEngine.Debug.Log("Locked");

            Selected = c.Selected;
        }
       
    }
}
