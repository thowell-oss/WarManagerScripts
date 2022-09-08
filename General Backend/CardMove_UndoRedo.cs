
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Cards
{
    public class CardMove_UndoRedo : IUndoable
    {
        public UndoRedoTag Tag => UndoRedoTag.CardAction;

        private List<SnapShot> _cardActivitySnapShotList;

        /// <summary>
        /// Redo
        /// </summary>
        public void RedoAction()
        {
            for (int i = 0; i < _cardActivitySnapShotList.Count; i++)
            {
                var a = _cardActivitySnapShotList[i];
                var c = a.CardReference;

                c.ApplySnapShot(a);

                if (a.Removed)
                {

                    //CardManager.RemoveCard(c, c.ID);
                }

                if (a.Added)
                {
                    CardUtility.TryAddCard(c);
                }
            }
        }

        /// <summary>
        /// Undo
        /// </summary>
        public void UndoAction()
        {
            for (int i = _cardActivitySnapShotList.Count - 1; i >= 0; i--)
            {
                var a = _cardActivitySnapShotList[i];
                var c = a.CardReference;

                c.ApplySnapShot(a);

                if (a.Removed)
                {
                    CardUtility.TryAddCard(c);
                }

                if (a.Added)
                {
                    //CardManager.RemoveCard(c, c.ID);
                }
            }
        }

        public CardMove_UndoRedo(List<SnapShot> activity) => SetAction(activity);

        public void SetAction(List<SnapShot> cardActivity)
        {
            if (cardActivity == null)
                throw new NullReferenceException("The card activity list must not be null");

            _cardActivitySnapShotList = cardActivity;
        }
    }
}
