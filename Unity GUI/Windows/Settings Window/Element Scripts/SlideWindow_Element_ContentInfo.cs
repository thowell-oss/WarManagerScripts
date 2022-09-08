/* SideWindow_Element_Content.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using WarManager.Backend;
using WarManager.Cards;

namespace WarManager.Unity3D.Windows
{
    /// <summary>
    /// Handles transferring content from a location to a window for dispatch
    /// </summary>
    public class SlideWindow_Element_ContentInfo : ISideWindowContent, IComparable<SlideWindow_Element_ContentInfo>
    {
        /// <summary>
        /// What the title of the string should be
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// what the content under the title is (resulting value)
        /// </summary>

        /// <summary>
        /// Set to help sort objects
        /// </summary>
        public string SortString = null;

        public string Content { get; set; }

        /// <summary>
        /// private backing field
        /// </summary>
        private UnitedStatesPhoneNumber _phoneNumber;

        /// <summary>
        /// the phone number
        /// </summary>
        public UnitedStatesPhoneNumber PhoneNumber
        {
            get
            {
                if (UnitedStatesPhoneNumber.TryParse(Content, out var phoneNumber))
                {
                    return phoneNumber;
                }

                var data = new UnitedStatesPhoneNumber("000", "000", "0000");
                data.Error = true;
                return data;
            }
            set
            {
                Content = value.FullNumberUS;
            }
        }

        /// <summary>
        /// for a drop down
        /// </summary>
        /// <value></value>
        public List<string> ContentOptions { get; set; } = new List<string>();

        public string ElementID { get; private set; } = "";

        /// <summary>
        /// The call back action for a button
        /// </summary>
        public Action<int> Callback { get; private set; }

        /// <summary>
        /// The call back action for advanced features
        /// </summary>
        /// <value></value>
        public Action<string> StringCallBack { get; private set; }

        public int CallBackActionType { get; private set; } = -1;

        /// <summary>
        /// The element that the content info is targeting
        /// </summary>
        private SideWindow_Element_Types _elementTarget;

        public DataEntry Entry;

        /// <summary>
        /// The image being transported
        /// </summary>
        public Sprite Sprite { get; private set; }


        public bool isPinned = false;

        /// <summary>
        /// Used for the editable label, display a tooltip description of what the user is editing (header)
        /// </summary>
        public string DescriptionHeader;

        /// <summary>
        /// Used for the editable label, display a tooltip description of what the user is editing (info)
        /// </summary>
        public string DescriptionInfo;

        /// <summary>
        /// The type of content
        /// </summary>
        public string ContentType;

        /// <summary>
        /// the custom regex (if needed)
        /// </summary>
        /// <value></value>
        public string Regex { get; set; }

        public SideWindow_Element_Types ElementType
        {
            get
            {
                return _elementTarget;

            }
        }

        public WarManager.Backend.DataValue SelectedDataValue;

        public float Height { get; set; }

        public Sheet<Card> OpenSheetReference;
        public SheetMetaData SheetMetaData;

        public List<(string, Action, bool)> Button_PickMenuMore { get; set; } = new List<(string, Action, bool)>();

        /// <summary>
        /// Constructor for the label or header
        /// </summary>
        /// <param name="label"></param>
        /// <param name="content"></param>
        public SlideWindow_Element_ContentInfo(string label, string content)
        {
            Label = label;
            Content = content;

            // if (content != null)
            //     Debug.Log(label + " " + content);
            // else
            // {
            //     Debug.Log(label + " (content is null)");
            // }

            Height = -1;

            ElementID = Guid.NewGuid().ToString();
            if (content == null)
                _elementTarget = SideWindow_Element_Types.Header;
            else
                _elementTarget = SideWindow_Element_Types.Label;

        }

        /// <summary>
        /// Constructor to display the data value
        /// </summary>
        /// <param name="selectedDataValue">the value to show</param>
        public SlideWindow_Element_ContentInfo(WarManager.Backend.DataValue selectedDataValue)
        {

            Label = "";
            Content = "";

            if (selectedDataValue == null)
            {
                Debug.LogError("The selected data value is null");
            }

            SelectedDataValue = selectedDataValue;
            _elementTarget = SideWindow_Element_Types.Label;
        }

        /// <summary>
        /// Set a header
        /// </summary>
        /// <param name="label">the text for the header to represent</param>
        /// <param name="fontSize">the size of the text</param>
        /// <remarks>the font size must be greater than 12</remarks>
        public SlideWindow_Element_ContentInfo(string label, int fontSize)
        {
            Height = fontSize;
            Label = label;

            _elementTarget = SideWindow_Element_Types.Header;
        }

        /// <summary>
        /// Create a paragraph of text
        /// </summary>
        /// <param name="title">the title of the paragraph</param>
        /// <param name="content">the content of the paragraph</param>
        /// <param name="height">the hight of the box of information</param>
        public SlideWindow_Element_ContentInfo(string title, string content, int height = 120)
        {
            Label = title;
            Content = content;
            Height = height;
            _elementTarget = SideWindow_Element_Types.Paragraph;
        }

        /// <summary>
        /// Constructor for spacer
        /// </summary>
        /// <param name="height">the height of the spacer</param>
        public SlideWindow_Element_ContentInfo(int height)
        {
            Height = height;
            _elementTarget = SideWindow_Element_Types.Spacer;
        }


        /// <summary>
        /// Constructor for a button
        /// </summary>
        /// <param name="label">the title of the button</param>
        /// <param name="callback">what the button will call when pressed</param>
        /// <param name="image">The sprite image for the button to show</param>
        public SlideWindow_Element_ContentInfo(string label, Action<int> callback, Sprite image = null)
        {
            Label = label;
            Callback = callback;
            Sprite = image;
            _elementTarget = SideWindow_Element_Types.Button;
            CallBackActionType = -1;
        }

        /// <summary>
        /// Constructor for a button
        /// </summary>
        /// <param name="label">the title of the button</param>
        /// <param name="actionType">the type of action that will be called when the specific button is pressed</param>
        /// <param name="callback">what the button will call when pressed</param>
        /// <param name="image">The sprite image for the button to show</param>
        public SlideWindow_Element_ContentInfo(string label, int actionType, Action<int> callback, Sprite image = null)
        {
            Label = label;
            Callback = callback;
            Sprite = image;
            _elementTarget = SideWindow_Element_Types.Button;
            CallBackActionType = actionType;
        }

        // <summary>
        /// Constructor for a button
        /// </summary>
        /// <param name="label">the title of the button</param>
        /// <param name="callback">what the button will call when pressed</param>
        /// <param name="image">The sprite image for the button to show</param>
        public SlideWindow_Element_ContentInfo(string label, Action callBack, Sprite icon = null)
        {
            Label = label;
            Callback = (x) => { callBack(); };
            _elementTarget = SideWindow_Element_Types.Button;
            CallBackActionType = 0;
            Sprite = icon;
        }

        /// <summary>
        /// Add a card representative 
        /// </summary>
        /// <param name="datasetID">the id of the data set</param>
        /// <param name="repId">the rep id of the specific card</param>
        /// <param name="callback">call back when the card button is pressed</param>
        [Obsolete("fix")]
        public SlideWindow_Element_ContentInfo(string datasetID, int repId, Action<int> callback)
        {
            Label = datasetID;
            CallBackActionType = repId;
            Callback = callback;
            _elementTarget = SideWindow_Element_Types.CardRep;
        }

        /// <summary>
        /// Add a card representative 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="callback"></param>
        public SlideWindow_Element_ContentInfo(DataEntry entry, Action<int> callback)
        {
            Label = entry.DataSet.ID;
            Callback = callback;
            _elementTarget = SideWindow_Element_Types.CardRep;

            Entry = entry;
        }

        /// <summary>
        /// Editable label
        /// </summary>
        /// <param name="tag">the tag of the label</param>
        /// <param name="content">the content pertaining to the tag</param>
        /// <param name="contentChangesCallBack">the call back to be made if the content is edited and changed</param>
        /// <param name="options">options (for using a drop down instead of a text box)</options>
        public SlideWindow_Element_ContentInfo(string tag, string content, Action<string> contentChangesCallBack, string[] options = null)
        {
            Label = tag;
            Content = content;
            StringCallBack = contentChangesCallBack;
            _elementTarget = SideWindow_Element_Types.EditLabel;

            ContentOptions.Clear();
            if (options != null)
                ContentOptions.AddRange(options);
        }

        /// <summary>
        /// Add a calculator
        /// </summary>
        /// <param name="a">entry a</param>
        /// <param name="b">entry b</param>
        /// <param name="calculator">the calculator strategy</param>
        public SlideWindow_Element_ContentInfo(string a, string b, CalculatorEntry entry)
        {
            Label = a;
            Content = b;

            _elementTarget = SideWindow_Element_Types.Calculator;
        }

        /// <summary>
        /// Reference an open card sheet
        /// </summary>
        /// <param name="sheetReference">the open card sheet to reference</param>
        /// <param name="metaData">the sheet meta data that is referenced</param>
        public SlideWindow_Element_ContentInfo(SheetMetaData metaData, Sheet<Card> sheetReference)
        {
            if (sheetReference == null)
            {
                throw new NullReferenceException("The sheet reference cannot be null");
            }

            if (metaData == null)
                throw new NullReferenceException("The sheet meta data cannot be null");

            OpenSheetReference = sheetReference;
            SheetMetaData = metaData;
            _elementTarget = SideWindow_Element_Types.SheetElement;
        }

        /// <summary>
        /// Reference a closed sheet
        /// </summary>
        /// <param name="sheetData">the sheet meta data</param>
        public SlideWindow_Element_ContentInfo(SheetMetaData sheetData)
        {
            if (sheetData == null)
                throw new NullReferenceException("The sheet meta data cannot be null");

            OpenSheetReference = null;
            SheetMetaData = sheetData;
            _elementTarget = SideWindow_Element_Types.SheetElement;
        }

        public override string ToString()
        {
            return Label + ": " + Content;
        }

        public int CompareTo(SlideWindow_Element_ContentInfo other)
        {
            if (other == null)
                return 1;

            if (SortString != null && SortString != string.Empty)
            {
                return SortString.CompareTo(other.SortString);
            }
            else if (Label != null)
            {
                return Label.CompareTo(other.Label);
            }
            else
            {
                return Height.CompareTo(other.Height);
            }
        }
    }
}
