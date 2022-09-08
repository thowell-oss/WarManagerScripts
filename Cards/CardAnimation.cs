/* CardAnimation.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using UnityEngine;

namespace WarManager.Cards
{
    /// <summary>
    /// Handles the animation for the card
    /// </summary>
    public class CardAnimation
    {
        /// <summary>
        /// The reference animation state for the similarly named property
        /// </summary>
        private CardAnimationState _currentAnimationState = CardAnimationState.Start;

        /// <summary>
        /// The card reference
        /// </summary>
        private Card _card;

        /// <summary>
        /// Can the data on the card be displayed?
        /// </summary>
        public bool DisplayData { get; private set; }

        /// <summary>
        /// Can the card be deleted (after the end animation)?
        /// </summary>
        public bool CanDeleteCard { get; private set; } = false;


        /// <summary>
        /// The time it takes to animate
        /// </summary>
        public static readonly float AnimationTime = 1f;

        /// <summary>
        /// The Animation state of the card
        /// </summary>
        public CardAnimationState CurrentAnimationState
        {
            get
            {
                return _currentAnimationState;
            }

            private set
            {
                _currentAnimationState = value;

                CardUtility.AddCardToUpdateQueue(_card);

                if (_layout != null)
                {
                    switch (value)
                    {
                        case CardAnimationState.Start:
                            //set scale to 0
                            _layout.Scale = 0;
                            break;
                        case CardAnimationState.Idle:
                            CanDeleteCard = false;
                            break;
                    }
                }

                ApplyCardAnimationState();
            }
        }

        /// <summary>
        /// The card layout to manipulate
        /// </summary>
        private CardLayout _layout;

        /// <summary>
        /// The corrected idle scale to reference
        /// </summary>
        public float idleScale = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="layout">the card layout to manipulate</param>
        public CardAnimation(CardLayout layout, Card c)
        {
            _layout = layout;
            _card = c;
            CurrentAnimationState = CardAnimationState.Idle;
            ApplyCardAnimationState();
        }

        /// <summary>
        /// Refresh the card animation over time
        /// </summary>
        public void ApplyCardAnimationState()
        {

            switch (CurrentAnimationState)
            {
                //the card has just been spawned
                case CardAnimationState.Start:

                    CanDeleteCard = false;
                    if (_layout != null)
                        _layout.Scale = idleScale;
                    DisplayData = false;

                    CurrentAnimationState = CardAnimationState.Idle;
                    break;

                //nothing happening
                case CardAnimationState.Idle:

                    CanDeleteCard = false;
                    if (_layout != null)
                        _layout.Scale = idleScale;
                    DisplayData = true;

                    break;

                //mouse hover state
                case CardAnimationState.Hover:

                    CanDeleteCard = false;
                    if (_layout != null)
                        _layout.Scale = idleScale + .05f;

                    break;

                //the card is being removed
                case CardAnimationState.End:

                    if (_layout != null)
                        _layout.Scale = .1f;
                    DisplayData = false;

                    break;

                //The card is locked
                case CardAnimationState.Locked:

                    if (_layout != null)
                        _layout.Scale = idleScale;
                    DisplayData = true;

                    break;

                //the card is being dragged
                case CardAnimationState.Drag:

                    if (_layout != null)
                        _layout.Scale = idleScale + 0.05f;
                    DisplayData = true;

                    break;
            }
        }

        /// <summary>
        /// Sets the card to idle if possible
        /// </summary>
        public virtual void SetCardIdle()
        {
            CurrentAnimationState = CardAnimationState.Idle;
        }

        /// <summary>
        /// Locks the card animation when the card is set to locked
        /// </summary>
        /// <param name="locked">set the locked state to true or false</param>
        public virtual void SetCardLocked(bool locked)
        {
            if (CurrentAnimationState == CardAnimationState.Minimize)
                return;

            if (locked)
            {
                CurrentAnimationState = CardAnimationState.Locked;
            }
            else
            {
                CurrentAnimationState = CardAnimationState.Idle;
            }
        }

        /// <summary>
        /// Hide the card when not in use
        /// </summary>
        /// <param name="hidden">set the hidden state to true or false</param>
        public virtual void SetCardHidden(bool hidden)
        {
            if (CurrentAnimationState == CardAnimationState.Locked)
                return;

            if (hidden)
            {
                CurrentAnimationState = CardAnimationState.Minimize;
            }
            else
            {
                CurrentAnimationState = CardAnimationState.Idle;
            }
        }

        /// <summary>
        /// Set the card state to hover when the mouse is hovering over the card
        /// </summary>
        /// <param name="hover">set the hover state to true or false if possible</param>
        public virtual void SetCardHover()
        {
            if (CurrentAnimationState != CardAnimationState.Idle && CurrentAnimationState != CardAnimationState.Hover)
                return;

            CurrentAnimationState = CardAnimationState.Hover;
        }

        /// <summary>
        /// Set the card animation to a drag state
        /// </summary>
        /// <param name="drag"></param>
        public virtual void SetCardDrag()
        {
            if (CurrentAnimationState == CardAnimationState.Hover)
                CurrentAnimationState = CardAnimationState.Drag;

            if (CurrentAnimationState != CardAnimationState.Idle && CurrentAnimationState != CardAnimationState.Drag)
                return;

            CurrentAnimationState = CardAnimationState.Drag;
        }
    }
}
