using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WarManager.Backend;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Forms
{
    /// <summary>
    /// The input field for the forms system
    /// </summary>
    [Notes.Author("The input field for the forms system")]
    public class FormsInputField : IFormsInput, ICSV_FormsInput
    {
        /// <summary>
        /// The id of the input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("id")]
        public string ID { get; set; }

        /// <summary>
        /// The title of the input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// The info container button of the input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("info")]
        public string Info { get; set; }

        /// <summary>
        /// The type of input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("type")]
        public string Type { get; set; }

        /// <summary>
        /// The example input of the input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("example")]
        public string Example { get; set; }

        /// <summary>
        /// The width of the input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("width")]
        public float Width { get; set; }

        /// <summary>
        /// Should the input field be verified before the input is submitted?
        /// </summary>
        /// <value></value>
        [JsonPropertyName("critical")]
        public bool Critical { get; set; }

        /// <summary>
        /// The path of the icon (if applicable) that will be used
        /// </summary>
        /// <value></value>
        [JsonPropertyName("icon path")]
        public string IconPath { get; set; }

        /// <summary>
        /// The starting state of the input field
        /// </summary>
        /// <value></value>
        [JsonPropertyName("start state")]
        public string StartState { get; set; }

        /// <summary>
        /// Has the input been verified?
        /// </summary>
        /// <value></value>
        [JsonIgnore]
        public bool Verified { get; set; }

        [JsonIgnore]
        public IFormsInput Previous { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        [JsonIgnore]
        public IFormsInput Next { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        [JsonIgnore]
        public FormsInputState CurrentState { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        [JsonIgnore]
        public string GroupID { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        [JsonIgnore]
        public int GroupOrder { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        [JsonPropertyName("input options")]
        public ICSV_FormsInputOptions InputOptions { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        [JsonPropertyName("existing edit location")]
        public List<Point> ExistingEditLocation { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


        public FormsInputField(IFormsInput previous, IFormsInput next, string groupID, int groupOrder)
        {
            Previous = previous;
            Next = next;
            GroupID = groupID;
            GroupOrder = groupOrder;
        }

        public List<string> GetDataSetIDs()
        {
            throw new System.NotImplementedException();
        }

        public DataValue GetDataValue()
        {
            throw new System.NotImplementedException();
        }

        public void SetInputFocused(bool focused)
        {
            throw new System.NotImplementedException();
        }

        public FormsInputState SetState()
        {
            throw new System.NotImplementedException();
        }
    }
}
