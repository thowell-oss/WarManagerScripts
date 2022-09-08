
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System.Threading;
using System.Threading.Tasks;

using UnityEngine;
using WarManager.Backend.CardsElementData;
using WarManager.Special;


namespace WarManager.Backend
{
    /// <summary>
    /// Handles parsing the id and generating the data for system cards
    /// </summary>
    [Notes.Author("Handles parsing the id and generating the data for system cards")]
    public class SystemCardsManager
    {
        /*
            1. parse the id to see if the card is a system card
            2. if it is, get the needed data, and generate a data set, data entry
            3. build the associated data view(s) and elements
            4. kick the info out back to the source

            id length is 36
        */

        /// <summary>
        /// lightning yellow color
        /// </summary>
        public static string SystemColorHex = "#D90000";

        /// <summary>
        /// typical guid length
        /// </summary>
        private static readonly int IDLength = 36;


        /// <summary>
        /// The list of action cards
        /// </summary>
        /// <typeparam name="ActionCardProperties"></typeparam>
        /// <returns></returns>
        private List<Actor> _actionCards = new List<Actor>()
        {
            new NoteCardActor(),
            new TitleCardActor(),
            new TagSumCounter(),
            new DuplicateCardFinderActor(),
            new SumCardActor(),
            new TagCheckActor(),
            new TopTenOvertimeEmployeesActor(),
            new CrewCostActor(),
        };

        /// <summary>
        /// The action cards
        /// </summary>
        /// <value></value>
        public IEnumerable<Actor> ActionCards
        {
            get => _actionCards;
        }

        /// <summary>
        /// The count of the action cards
        /// </summary>
        public int ActionCardCount => _actionCards.Count;

        /// <summary>
        /// Get an array of action cards
        /// </summary>
        /// <returns></returns>
        public Actor[] ToArray => _actionCards.ToArray();

        /// <summary>
        /// Parse the id to get the associated card information for a system card
        /// </summary>
        /// <param name="setID">the card dataset ID</param>
        public DataEntry ParseID(string setID, string rowId)
        {
            if (setID == null)
                throw new NullReferenceException("The id cannot be null");

            if (setID.Trim().Length < 1)
                throw new ArgumentException("the id cannot be empty");

            if (setID.Length < IDLength)
                throw new ArgumentException("the id length is too short");

            if (setID.StartsWith("sys") && setID.Length > IDLength)
            {
                setID = setID.Remove(0, 3);
                string formalID = setID.Substring(0, IDLength);
                //Debug.Log(formalID);
                string args = setID.Remove(0, IDLength + 1);
                //Debug.Log(args);

                return FindCardType(formalID, args, rowId);
            }

            throw new ArgumentException("not a system card");
        }

        /// <summary>
        /// Get the data set
        /// </summary>
        /// <param name="setID">the data set</param>
        /// <returns></returns>
        public DataSet GetDataSet(string setID)
        {
            return ParseID(setID, "").DataSet;
        }

        /// <summary>
        /// start processing the card type
        /// </summary>
        /// <param name="dataSetID">the data set id of the card</param>
        /// <param name="args">the arguments associated with the card</param>
        private DataEntry FindCardType(string dataSetID, string args, string rowID)
        {

            #region old

            // switch (id)
            // {
            //     case "7fa7474f-5536-4fdd-b970-b435a6280584": //weather card
            //         return GetWeather(args);

            //     case "5bc9541b-27ee-4a70-8d06-cef398325e65": //duplicate card
            //         return GetDuplicateCard();

            //     case "941d5a04-03fe-4bfe-b4d1-f73440f02e79": //sum card
            //         return GetSumCard();

            //     case "b9697564-52e2-4ec8-9e58-3914232b2b4c": //tag check card
            //         return GetTagCheckCard();

            //     default:
            //         Debug.LogError("The id given does not match any particular ID " + id);
            //         throw new ArgumentException("The id given does not match any particular ID " + id);
            // }

            #endregion

            var actionCard = _actionCards.Find(x => x.DataSetID == dataSetID);

            if (actionCard != null)
            {
                return actionCard.GetDataEntry(rowID, args);
            }

            throw new ArgumentException("Cannot find card with the id " + dataSetID);
        }

        #region old

        /// <summary>
        /// Get all action data sets
        /// </summary>
        /// <returns>returns a list of data sets that are apart of the action data set</returns>
        // public List<DataSet> GetAllActionDataSets()
        // {
        //     List<DataSet> setList = new List<DataSet>();

        //     for (int i = 0; i < _actionCards.Count; i++)
        //     {
        //         //_actionCards[i].GetDataSet()

        //         //get the data set to be stored with all other data sets
        //         //when spawning another action card, we link it to the data set
        //         //data set should locate an existing csv file which then we can store pertinent information
        //         //save and fetch data.

        //         //is there a way to not use the same data set or rather not have to deal with saving large files?? WE NEED A DATABASE!!!
        //     }
        // }

        #endregion

        /// <summary>
        /// Get the action search data for all action cards
        /// </summary>
        /// <returns></returns>
        public QuickSearchData GetActionSearchData()
        {
            Dictionary<IList<string>, DataEntry> data = new Dictionary<IList<string>, DataEntry>();

            foreach (var x in ActionCards)
            {
                var entry = x.GetDataEntry(Guid.NewGuid().ToString(), "");
                data.Add(new List<string>() { x.Name }, entry);
            }

            QuickSearchData quickSearch = new QuickSearchData("Action Cards", Color.clear, data, -1);

            return quickSearch;
        }

        #region old

        // /// <summary>
        // /// Get the weather data and associated card info
        // /// </summary>
        // /// <param name="args">the args</param>
        // private DataEntry GetWeather(string args)
        // {

        //     DataFileInstance instance = new DataFileInstance(null, new string[1] { "Precipitation" }, new List<string[]>() { new string[1] { "" } });


        //     List<CardElementViewData> data = new List<CardElementViewData>()
        //     {
        //         new CardBackgroundElementData()
        //         {
        //             Scale = new double[2] {4.5, 2.75},
        //             ColorHex = "#1d1d1d"
        //         },

        //         new CardElementTextData()
        //         {
        //             Location = new double[3] {-9, 0, 1},
        //             Scale = new double[2] {7,5},
        //             TextJustification = "center",
        //             FontSize = 14,
        //             ColorHex = "#eee",
        //             SetColumns = new List<int>() {0},
        //         },
        //     };

        //     return new DataEntry(0, new DataValue[1] { new DataValue(new Point(1, 1), "Precipitation", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full) },
        //     SystemDataSet("sys7fa7474f-5536-4fdd-b970-b435a6280584:", data, instance, new List<string>() { "Precipitation" }))
        //     { Actor = new WeatherCardActor() };
        // }

        // /// <summary>
        // /// Create a duplicate card finder
        // /// </summary>
        // /// <returns></returns>
        // public DataEntry GetDuplicateCard()
        // {
        //     List<CardElementViewData> data = new List<CardElementViewData>() { new CardBackgroundElementData()
        //     {
        //         ColorHex = "#0000",
        //         Scale = new double[2] {3,2.5}
        //     } };

        //     return new DataEntry(0, new DataValue[0], SystemDataSet("sys5bc9541b-27ee-4a70-8d06-cef398325e65:", data))
        //     { Actor = new DuplicateCardFinderActor() }; //create the entry here (add the data set) and set the actor. This should return the actor back to the makeup for use
        // }

        // /// <summary>
        // /// Create a sum card
        // /// </summary>
        // /// <returns></returns>
        // public DataEntry GetSumCard()
        // {
        //     string tagName = "sum";

        //     DataFileInstance instance = new DataFileInstance(null, new string[1] { tagName }, new List<string[]>() { new string[1] { "" } });

        //     List<CardElementViewData> data = new List<CardElementViewData>()
        //     {
        //         new CardBackgroundElementData()
        //         {
        //             Scale = new double[2] {4.5, 2.75},
        //             ColorHex = "#1d1d1d"
        //         },

        //         new CardElementTextData()
        //         {
        //             Location = new double[3] {-9, 0, 1},
        //             Scale = new double[2] {7,5},
        //             TextJustification = "center",
        //             FontSize = 14,
        //             ColorHex = "#eee",
        //             SetColumns = new List<int>() {0},
        //         },
        //     };

        //     return new DataEntry(0, new DataValue[1] { new DataValue(new Point(1, 1), tagName, "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full) },
        //     SystemDataSet("sys941d5a04-03fe-4bfe-b4d1-f73440f02e79:", data, instance, new List<string>() { tagName }))
        //     { Actor = new SumCardActor() };
        // }

        // /// <summary>
        // /// Get a Tag Check Card
        // /// </summary>
        // /// <returns></returns>
        // public DataEntry GetTagCheckCard()
        // {
        //     DataFileInstance instance = new DataFileInstance(null, new string[5] { "id", "Tags", "Direction", "Contain?", "Distance" }, new List<string[]>() { new string[5] { Guid.NewGuid().ToString(), "Foreman", "Down", "Contains", "100" } });


        //     List<CardElementViewData> data = new List<CardElementViewData>()
        //     {
        //         new CardBackgroundElementData()
        //         {
        //             Scale = new double[2] {4.5, 1},
        //             ColorHex = "#1d1d1d"
        //         },

        //         new CardElementTextData()
        //         {
        //             Location = new double[3] {-9, 0, 1},
        //             Scale = new double[2] {7,5},
        //             TextJustification = "center",
        //             FontSize = 14,
        //             ColorHex = "#eee",
        //             SetColumns = new List<int>() {0, 1},
        //             MultiColumnString = " - ",
        //         },
        //     };

        //     //DataValue Id = new DataValue(new Point(1, 1), "Id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full, "The keyword you are looking for");
        //     DataValue TagValue = new DataValue(new Point(1, 1), "Keyword", "", ColumnInfo.GetValueTypeOfWord, ValuePermissions.Full, "The keyword of interest.");
        //     DataValue DirectionValue = new DataValue(new Point(2, 1), "Direction", "Column", ColumnInfo.GetValueTypeOfBoolean, ValuePermissions.Full, "The direction to search - either \"Column\" or \"Row\" ", new string[2] { "Column", "Row" });
        //     DataValue ContainValue = new DataValue(new Point(3, 1), "Contain?", "Must Not Contain",
        //      ColumnInfo.GetValueTypeOfBoolean, ValuePermissions.Full, "The message box will show if the card either 'contains' or 'does not contain' a specified keyword", new string[2] { "Contains", "Must Not Contain" });
        //     DataValue DistanceValue = new DataValue(new Point(4, 1), "Distance", "100",
        //                 ColumnInfo.GetValueTypeOfInt, ValuePermissions.Full, "The max detection distance from this card");

        //     var entry = new DataEntry(0, new DataValue[4] { TagValue, DirectionValue, ContainValue, DistanceValue },
        //     SystemDataSet("sysb9697564-52e2-4ec8-9e58-3914232b2b4c:", data, instance, new List<string>() { "Keyword", "Direction", "Contain?", "Distance" }))
        //     { Actor = new TagCheckActor() };

        //     return entry;
        // }

        // public DataSet SystemDataSet(string id, List<CardElementViewData> elementData)
        // {
        //     return SystemDataSet(id, elementData, new DataFileInstance(), new List<string>());
        // }

        // public DataSet SystemDataSet(string id, List<CardElementViewData> elementData, DataFileInstance data, List<string> allowedTags)
        // {
        //     return new DataSet("System", "War Manager Assistant", allowedTags, string.Empty,
        //         new List<string>(), true, string.Empty, SystemColorHex, 1.0, 2.0, id, new List<string>() { "Default" }, string.Empty,
        //         new List<DataSetView>() { new DataSetView("System", "System View", "This view is for the system cards", Guid.NewGuid().ToString(),
        //         true, true, elementData, new string[1] {"Default"},
        //         true, 1)}, data, new List<ColumnInfo>());
        // }

        #endregion
    }
}
