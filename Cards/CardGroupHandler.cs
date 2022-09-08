using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager;
using WarManager.Backend;

namespace WarManager.Cards
{
    public class CardGroupHandler : ICollection<Card>
    {
        public Point CardOffsetFromLeader { get; set; }

        public int Count => _cards.Count;

        public bool IsReadOnly => false;

        private List<Card> _cards = new List<Card>();

        /// <summary>
        /// The card offset handler constructor
        /// </summary>
        /// <param name="offset">the card offset</param>
        /// <param name="card">the card</param>
        public CardGroupHandler(Point offset, Card card)
        {
            CardOffsetFromLeader = offset;
        }

        public void Add(Card item)
        {
            _cards.Add(item);
        }

        public void Clear()
        {
            _cards.Clear();
        }

        public bool Contains(Card item)
        {
            return _cards.Contains(item);
        }

        public void CopyTo(Card[] array, int arrayIndex)
        {
            _cards.CopyTo(array, arrayIndex);
        }

        public bool Remove(Card item)
        {
            return _cards.Remove(item);
        }

        public IEnumerator<Card> GetEnumerator()
        {
            return _cards.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _cards.GetEnumerator();
        }
    }
}