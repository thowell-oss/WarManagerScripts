
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Special;
using WarManager.Backend.CardsElementData;
using WarManager.Cards;
using WarManager.Backend;

namespace WarManager
{
    /// <summary>
    /// The weather card actor
    /// </summary>
    [Notes.Author("The weather card actor")]
    public sealed class WeatherCardActor : Actor
    {
        public override string Name => "Weather";

        public override string Description => "This card displays relevant weather information.";

        public override string DataSetID => "7fa7474f-5536-4fdd-b970-b435a6280584";

        DateTime lastTimeRefreshed = DateTime.Now.AddHours(-2);

        public override void OnInit(Card card)
        {
            base.OnInit(card);

            RefreshUI();

            //Card.Entry.UpdateValueAt(new ValueTypePair("loading...", ColumnInfo.GetValueTypeOfParagraph), 0);
            //UpdateUI();

            lastTimeRefreshed = DateTime.Now;

        }

        /// <summary>
        /// Update and display weather info
        /// </summary>
        public override void Act()
        {
            base.Act();

            if (lastTimeRefreshed < DateTime.Now.AddHours(-1))
            {
                RefreshUI();
            }
        }


        /// <summary>
        /// refresh the UI
        /// </summary>
        private void RefreshUI()
        {
            WeatherData.Refresh(39.099728, -94.578568);

            HourlyWeather selectedWeather = WeatherData.GetWeather()[0];
            long min = long.MaxValue;

            DateTime closestDate = DateTime.Now.AddYears(-5);

            foreach (var x in WeatherData.Weather)
            {
                if (Math.Abs(x.Time.Ticks - DateTime.Now.Ticks) < min)
                {
                    min = Math.Abs(x.Time.Ticks - DateTime.Now.Ticks);
                    closestDate = x.Time;
                }
            }

            Card.Entry.UpdateValueAt(new ValueTypePair($"{selectedWeather.Precipitation}% chance rain\n{string.Format("{0:0.##}", selectedWeather.Gust)} MPH gusts\n({selectedWeather.Time.ToShortDateString()})", ColumnInfo.GetValueTypeOfParagraph), 0);
            UpdateUI();
        }

        /// <summary>
        /// Get the instance of the actor class
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override DataEntry GetDataEntry(string RowID, string args)
        {
            DataFileInstance instance = new DataFileInstance(null, new string[1] { "Precipitation" }, new List<string[]>() { new string[1] { "" } });

            return new DataEntry(RowID, GetDefaultDataValues(RowID).ToArray(),
            GetDataSet(GetDataSetId(), GetElementViewData(), instance, new List<string>() { "Precipitation" }))
            { Actor = new WeatherCardActor() };
        }

        protected override List<DataValue> GetDefaultDataValues(string rowID)
        {
            var value = new DataValue((1, rowID), "Precipitation", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full);
            return new List<DataValue>() { value };
        }

        protected override List<CardElementViewData> GetElementViewData()
        {
            List<CardElementViewData> data = new List<CardElementViewData>()
            {
                new CardBackgroundElementData()
                {
                    Scale = new double[2] {4.5, 2.75},
                    ColorHex = "#1d1d1d"
                },

                new CardElementTextData()
                {
                    Location = new double[3] {-9, 0, 1},
                    Scale = new double[2] {7,5},
                    TextJustification = "center",
                    FontSize = 14,
                    ColorHex = "#eee",
                    SetColumns = new List<int>() {0},
                },
            };

            return data;
        }

        public override void RemoveEntry()
        {

        }
    }
}