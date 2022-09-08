
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Cards;
using WarManager.Backend;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    /// <summary>
    /// Handles the storage and maintenance of all actors
    /// </summary>
    [Notes.Author("Handles the storage and maintenance of all actors")]
    public class CardActors
    {

        /// <summary>
        /// the list of all active actors
        /// </summary>
        /// <typeparam name="Actor"></typeparam>
        /// <returns></returns>
        private Dictionary<Sheet<Card>, List<Actor>> _allActors = new Dictionary<Sheet<Card>, List<Actor>>();

        /// <summary>
        /// The count of actors currently running
        /// </summary>
        public int Count => _allActors.Count;

        /// <summary>
        /// The actors
        /// </summary>
        public IEnumerable<Sheet<Card>> Sheets => _allActors.Keys;


        private bool cardsBeingDraggedEventChanged = false;
        private bool cardsBeingDraggedState = false;


        /// <summary>
        /// Activate an actor and set it in the war manager lifecycle
        /// </summary>
        /// <param name="card">the card the actor is associated with</param>
        /// <param name="makeup">the card makeup</param>
        /// <param name="actor">the actor</param>
        public void Add(Card card)
        {
            if (card == null)
                throw new NullReferenceException("the card does not exist");

            if (SheetsManager.TryGetActiveSheet(card.SheetID, out var sheet))
            {
                if (!_allActors.ContainsKey(sheet))
                {
                    _allActors.Add(sheet, new List<Actor>());
                }

                _allActors[sheet].Add(card.Entry.Actor);
                card.Entry.Actor.OnInit(card);
            }
            else
            {
                throw new ArgumentException("Cannot find the active sheet with id " + card.SheetID);
            }
        }

        /// <summary>
        /// Remove an actor out of the War Manager lifecycle
        /// </summary>
        /// <param name="card">the card</param>
        /// <returns>returns true if the removal was successful, false if not</returns>
        public bool Remove(Card card)
        {
            if (card == null)
                throw new NullReferenceException("the card does not exist");

            if (SheetsManager.TryGetActiveSheet(card.SheetID, out var sheet))
            {
                if (_allActors.ContainsKey(sheet))
                {
                    var cards = _allActors[sheet];
                    cards.Remove(card.Entry.Actor);
                    card.Entry.Actor.OnDestroy();

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Remove actors that are contained in a specific sheet
        /// </summary>
        /// <param name="sheet">the sheet to remove the actors</param>
        public void RemoveActorsBySheet(Sheet<Card> sheet)
        {
            if (_allActors.ContainsKey(sheet))
            {
                List<Actor> actors = _allActors[sheet];

                for (int i = actors.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        actors[i].OnRemoveFromSheet();
                        actors.RemoveAt(i);
                    }
                    catch (Exception ex)
                    {
                        WarSystem.WriteToLog(ex.Message + " (" + actors[i].Name + ")", Logging.MessageType.error);

#if UNITY_EDITOR
                        Debug.LogError(ex.Message + " (" + actors[i].Name + ")");
#endif
                    }
                }
            }
        }

        /// <summary>
        /// tick all active actors
        /// </summary>
        public void Tick()
        {
            foreach (var t in _allActors)
            {
                var actors = t.Value;

                foreach (var x in actors)
                {

                    if (x.Enabled)
                    {

                        try
                        {
                            if (x.Initialized)
                            {
                                x.Tick();

                                if (x.Dragging)
                                    x.Drag();

                                if (cardsBeingDraggedEventChanged)
                                {
                                    cardsBeingDraggedEventChanged = false;
                                    x.OtherCardsDragStateChanged(cardsBeingDraggedState);
                                }

                                if (!x.Sleeping)
                                    x.Act();

                                x.LateTick();
                            }
                        }
                        catch (Exception ex)
                        {

                            if (x != null && x.Name != null)
                            {
#if UNITY_EDITOR

                                Debug.LogError(ex.Message + " (" + x.Name + ")");
#endif
                                WarSystem.WriteToLog(ex.Message + " (" + x.Name + ")", Logging.MessageType.error);
                            }
                            else
                            {
#if UNITY_EDITOR

                                Debug.LogError(ex.Message);
#endif
                                WarSystem.WriteToLog(ex.Message, Logging.MessageType.error);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Called when a drag card state has changed
        /// </summary>
        /// <param name="dragCard">is the card being dragged?</param>
        /// <param name="startCard">the card the dragging is being acted upon</param>
        /// <param name="sender">the sender object</param>
        public void OnDragCardChanged(bool dragCard, Card startCard, object sender)
        {
            foreach (var t in _allActors)
            {
                var actors = t.Value;

                foreach (var x in actors)
                {

                    try
                    {
                        if (x.Initialized)
                        {
                            x.OtherCardDragStateChanged(dragCard, startCard, sender);
                        }
                    }
                    catch (Exception ex)
                    {
#if UNITY_EDITOR
                        Debug.Log(ex.Message);
#endif
                        WarSystem.WriteToLog(ex.Message, Logging.MessageType.error);
                    }
                }
            }

            cardsBeingDraggedEventChanged = true;
            cardsBeingDraggedState = dragCard;
        }
    }
}

