using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Backend;
using WarManager.Cards;
using WarManager.Backend.CardsElementData;

namespace WarManager
{
    public class CrewCostActor : Actor
    {
        public override string Name => "Crew Cost Gismos";

        public override string Description => "Calculates crew cost and displays the results under a card cluster.";

        public override string DataSetID => "0d727f0d-9164-48b9-8295-0b631a1fe9a2";

        private DataSet _dataSet;
        private DataEntry _entry;

        private int crewCostColumn = 7;
        private static readonly string employeesID = "326bd75f-12cc-407c-8275-09ce8b5206dd";

        Dictionary<List<Card>, Rect> _clusters = new Dictionary<List<Card>, Rect>();

        Dictionary<Point, WorldGismo> _crewCostGismos = new Dictionary<Point, WorldGismo>();


        public override void OnAwake()
        {
            base.OnAwake();
            RecalculateCardClusters();
        }

        public override void OnDrop()
        {
            base.OnDrop();

            RecalculateCardClusters();
        }

        public override void OtherCardsDragStateChanged(bool dragging)
        {
            base.OtherCardsDragStateChanged(dragging);

            RecalculateCardClusters();
        }

        /// <summary>
        /// recalculate the card clusters 
        /// </summary>
        private void RecalculateCardClusters()
        {

            if (GismosManager.Main.GismosCount > 0)
                GismosManager.Main.ClearGismos();

            if (!SheetsManager.TryGetCurrentSheet(out var sheet))
                return;

            //Debug.Log("got the sheet");

            _clusters = CardUtility.GetCardClusterBoundingBoxes(sheet, sheet.CurrentLayer);

            foreach (var x in _clusters)
            {
                //Debug.Log("handling clusters");
                HandleClusterCalculations(x.Key, x.Value, sheet);
            }
        }

        /// <summary>
        /// Get calculations per column in a cluster
        /// </summary>
        /// <param name="cards">the cards</param>
        /// <param name="rect">the rect</param>
        /// <param name="sheet">the sheet</param>
        private void HandleClusterCalculations(List<Card> cards, Rect rect, Sheet<Card> sheet)
        {
            Dictionary<Point, string> results = new Dictionary<Point, string>();

            //Debug.Log(rect.BottomLeftCorner);

            for (int i = 0; i < rect.Height; i++)
            {


                // Debug.Log(" i " + i);

                float costSum = 0;

                for (int k = 0; k < rect.Width; k++)
                {
                    //Debug.Log("k " + k);

                    var selectedLocation = rect.BottomLeftCorner + new Point(i, -k);

                    //Debug.Log(selectedLocation.ToString());

                    var card = CardUtility.GetCard(selectedLocation, sheet.CurrentLayer, sheet.ID);

                    // Debug.Log("got card");

                    if (card != null && card.DataSet != null && card.DataSet.ID != null && card.DataSet.ID == employeesID)
                    {
                        /// Debug.Log("card data set valid");

                        if (card.Entry.TryGetValueAt(crewCostColumn, out var value))
                        {
                            // Debug.Log("got value");

                            if (float.TryParse(value.Value.ToString(), out var result))
                            {
                                costSum += (result * 40);

                                // Debug.Log("applying cost");
                            }
                        }
                    }
                }

                results.Add(new Point(rect.BottomLeftCorner.x + i, rect.TopLeftCorner.y - 2), costSum.ToString());
            }

            if (results.Count > 0)
                DisplayGismos(results);
        }

        private void DisplayGismos(Dictionary<Point, string> results)
        {
            foreach (var x in results)
            {
                if (x.Value != "0" || x.Value != " ")
                {
                    var gismo = GismosManager.Main.SetWorldGismo(x.Key, false);

                    if (gismo != null)
                        gismo.SetWorldGismoContent("$$/Week:\n$" + string.Format("{0:0,000.00}", x.Value), new Color(.8f, .8f, .8f), 12);
                }
            }
        }

        /// <summary>
        /// Get the data entry
        /// </summary>
        /// <param name="rowID">the row id</param>
        /// <param name="args">the arguments (if needed for some reason)</param>
        /// <returns></returns>
        public override DataEntry GetDataEntry(string rowID, string args)
        {

            //DataFileInstance instance = new DataFileInstance(null, new string[1] { "id" }, new List<string[]>() { new string[0] }); //not saving the data file instance (just calculations via gismos)

            _dataSet = GetDataSet(GetDataSetId(), GetElementViewData(), GetDataFilePath(), new List<string>(), new string[] { "id" });
            if (_dataSet.EntryExists(rowID))
            {
                var entry = _dataSet.GetEntry(rowID);
                entry.Actor = this;

                return entry;
            }

            _entry = CreateNewDataEntry<CrewCostActor>(rowID, _dataSet, this);
            return _entry;
        }

        public override void RemoveEntry()
        {
            if (_entry != null)
                _dataSet.RemoveEntry(_entry);

            foreach (var x in _crewCostGismos)
                GismosManager.Main.ClearGismo(x.Key);
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            return new List<DataValue>(); //default to daily and allow for weekly
        }

        /// <summary>
        /// get element view data
        /// </summary>
        /// <returns></returns>
        protected override List<CardElementViewData> GetElementViewData()
        {
            var data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    ColorHex = "#222",
                    Scale = new double[2] {4.5, 2.75},
                },
                new CardElementTextData()
                {
                    Location = new double[] {-7.5, 0, 1},
                    Scale = new double[2] { 7,1 },
                    ColorHex = "#aaa",
                    FontSize = 14,
                    Italics = true,
                    LeadingWords = "Crew Cost Gismos"
                }
            };

            return data;
        }
    }
}
