
using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using WarManager;
using WarManager.Backend;
using WarManager.Unity3D;
using WarManager.Cards;

namespace WarManager.Testing.Unit
{
    [Notes.Author("Handles the testing for the card utility class")]
    public class test_CardUtility : MonoBehaviour
    {
        [Test]
        public void testCardClusteringSuccessful()
        {
            var sheet = GetClusterOfCardsInSheet();

        }

        /// <summary>
        /// Get a sample test sheet to use for testing anything in card utility
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        private Sheet<Card> GetSampleSheet(List<Card> cards)
        {
            var sheet = new Sheet<Card>(Guid.NewGuid().ToString(), "test sheet", "test sheet", false);

            for (int i = 0; i < cards.Count; i++)
            {
                sheet.AddObj(cards[i]);
            }

            return sheet;
        }

        /// <summary>
        /// Get a sample card cluster for a sample sheet
        /// </summary>
        /// <returns></returns>
        private Sheet<Card> GetClusterOfCardsInSheet()
        {


            var sheet = GetSampleSheet(new List<Card>());
            var dataset = DataSet.GetDataSet(Guid.NewGuid().ToString(), new List<DataValue>(), new List<Backend.CardsElementData.CardElementViewData>(), new DataFileInstance(), new List<string>());

            var entry = new DataEntry();

            dataset.AppendEntry(entry);

            for (int i = 0; i < 10; i++)
            {
                sheet.AddObj(new Card(new Point(5 + i, -5), Guid.NewGuid().ToString(), sheet.ID, sheet.CurrentLayer.ID, entry));
            }

            return sheet;

        }
    }
}
