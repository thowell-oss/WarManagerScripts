
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using System.Text;
using System.Text.RegularExpressions;

using WarManager.Backend;

using Sirenix.OdinInspector;

namespace WarManager.Unity3D.Windows
{
    [Notes.Author("Describes the behavior of the editable label")]
    public class SlideWindow_Element_EditableLabel : MonoBehaviour, ISlideWindow_Element
    {
        public TMPro.TMP_Text Tag;
        public TMPro.TMP_InputField InputField;
        public TMPro.TMP_Dropdown ContentDropDown;
        public USPhoneNumberInputHandler PhoneNumberInput;
        [Space]
        public Image Border;
        public Color BackgroundErrorColor = Color.yellow;
        public Color DefaultColor = Color.gray;
        public Color RequiredColor = Color.red;
        [Space]

        public GameObject EmailInput;

        public TooltipTrigger desciptionTrigger;

        public Button EditButton;

        public SlideWindow_Element_ContentInfo info { get; set; }

        public GameObject targetGameObject => this.gameObject;
        public string SearchContent { get; set; } = "";

        Action<string> strCallBack;

        public string DescriptionHeader;
        public string InfoHeader;

        string _selectedData = "";
        List<string> _values = new List<string>();

        /// <summary>
        /// the type image (handles the icons)
        /// </summary>
        public Image InputTypeImage;

        public TooltipTrigger InputTypeToolTip;

        [FoldoutGroup("Paragraph")]
        public Sprite ParagraphSprite;

        [FoldoutGroup("Paragraph")]
        public Color ParagraphColor;

        [FoldoutGroup("Paragraph")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string ParagraphDescription;

        [FoldoutGroup("Word")]
        public Sprite WordSprite;
        [FoldoutGroup("Word")]
        public Color WordColor;
        [FoldoutGroup("Word")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string WordDescription;

        [FoldoutGroup("Phone")]
        public Sprite PhoneSprite;
        [FoldoutGroup("Phone")]
        public Color PhoneColor;
        [FoldoutGroup("Phone")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string PhoneDescription;

        [FoldoutGroup("Integer")]
        public Sprite IntegerSprite;
        [FoldoutGroup("Integer")]
        public Color IntegerColor;
        [FoldoutGroup("Integer")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string IntegerDescription;

        [FoldoutGroup("Rational")]
        public Sprite RationalSprite;
        [FoldoutGroup("Rational")]
        public Color RationalColor;
        [FoldoutGroup("Rational")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string RationalDescription;

        [FoldoutGroup("Email")]
        public Sprite EmailSprite;
        [FoldoutGroup("Email")]
        public Color EmailColor;

        [FoldoutGroup("Email")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string EmailDescription;

        [FoldoutGroup("Keyword")]
        public Sprite KeywordSprite;
        [FoldoutGroup("Keyword")]
        public Color KeywordColor;
        [FoldoutGroup("Keyword")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string KeywordDescription;

        [FoldoutGroup("Sentence")]
        public Sprite SentenceSprite;
        [FoldoutGroup("Sentence")]
        public Color SentenceColor;
        [FoldoutGroup("Sentence")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string SentenceDescription;

        [FoldoutGroup("MultiSelect")]
        public Sprite MultiSelectSprite;
        [FoldoutGroup("MultiSelect")]
        public Color MultiSelectColor;
        [FoldoutGroup("MultiSelect")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string MultiSelectDescription;

        [FoldoutGroup("True False")]
        public Sprite TrueFalseSprite;
        [FoldoutGroup("True False")]
        public Color TrueFalseColor;

        [FoldoutGroup("True False")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string TrueFalseDescription;

        [FoldoutGroup("Data Entry")]
        public Sprite DataEntrySprite;
        [FoldoutGroup("Data Entry")]
        public Color DataEntryColor;
        [FoldoutGroup("Data Entry")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string DataEntryDescription;

        [FoldoutGroup("Custom")]
        public Sprite CustomSprite;
        [FoldoutGroup("Custom")]
        public Color CustomColor;
        [FoldoutGroup("Custom")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string CustomDescription;

        [FoldoutGroup("Error")]
        public Sprite Error;
        [FoldoutGroup("Error")]
        public Color ErrorColor = Color.yellow;
        [FoldoutGroup("Error")]
        [MultiLineProperty(4)]
        [HideLabel]
        [Title("Description", bold: false)]
        public string ErrorDescription;

        public void UpdateElement()
        {
            SearchContent = info.Content + info.Label + info.ElementType;

            if (info.ContentType == ColumnInfo.GetValueTypeOfCustom)
            {
                strCallBack = x =>
                {
                    if (x != null)
                    {
                        if (Regex.IsMatch(x, info.Regex)) // need to find the location of the regex
                        {
                            info.StringCallBack(x);
                        }
                        else
                        {
                            MessageBoxHandler.Print_Immediate("Could not change because your input does not match the correct format: " + x, "Error");
                        }
                    }
                };
            }
            else if (info.ContentType == ColumnInfo.GetValueTypeOfPhone)
            {
                strCallBack = x =>
                {
                    if (UnitedStatesPhoneNumber.TryParse(x, out var phone))
                    {
                        info.StringCallBack(phone.FullNumberUS);
                        info.PhoneNumber = phone;
                    }
                    else
                    {
                        MessageBoxHandler.Print_Immediate("Could not change because your input does not match the correct format: " + x, "Error");
                    }
                };
            }
            else
            {
                strCallBack = info.StringCallBack;
            }

            EditButton.interactable = strCallBack != null;

            Tag.text = info.Label;

            _selectedData = info.Content;

            if (info.DescriptionHeader != null && info.DescriptionHeader.Trim().Length > 0 &&
            info.DescriptionInfo != null && info.DescriptionInfo.Trim().Length > 0)
            {
                desciptionTrigger.gameObject.SetActive(true);
                desciptionTrigger.contentText = info.DescriptionInfo;
                desciptionTrigger.headerText = info.DescriptionHeader;
            }
            else
            {
                desciptionTrigger.gameObject.SetActive(false);
            }

            //ContentDropDown.gameObject.SetActive(info.ContentOptions != null && info.ContentOptions.Count > 0);

            //InputField.gameObject.SetActive(!(info.ContentOptions != null && info.ContentOptions.Count > 0));


            if (info.ContentType != null && info.ContentType.Length > 0)
            {
                HandleInputType(info.ContentType);
            }
            else
            {
                if (info.ContentOptions == null || info.ContentOptions.Count == 0)
                    HandleInputType(ColumnInfo.GetValueTypeOfParagraph);
                else if (info.ContentOptions.Count == 2)
                {
                    HandleInputType(ColumnInfo.GetValueTypeOfBoolean);
                }
                else
                    HandleInputType(ColumnInfo.GetValueTypeOfKeyword);
            }
        }

        /// <summary>
        /// Handle the input type
        /// </summary>
        /// <param name="type">the type of string input</param>
        private void HandleInputType(string type)
        {

            // Debug.Log(type);

            if (type == null || type == string.Empty)
            {
                //throw new ArgumentException("the content type is either null or empty");
                SetInputType(InputType.none);
                InputField.gameObject.SetActive(false);
                return;
            }


            // Debug.Log(type == ColumnInfo.GetValueTypeOfPhone + " \'" + type + "\'");

            ContentDropDown.gameObject.SetActive(type == ColumnInfo.GetValueTypeOfBoolean || type == ColumnInfo.GetValueTypeOfKeyword);
            PhoneNumberInput.gameObject.SetActive(type == ColumnInfo.GetValueTypeOfPhone);
            EmailInput.gameObject.SetActive(type == ColumnInfo.GetValueTypeOfEmail);

            if (type == ColumnInfo.GetValueTypeOfBoolean || type == ColumnInfo.GetValueTypeOfKeyword)
            {

                if (type == ColumnInfo.GetValueTypeOfBoolean)
                    SetInputType(InputType.trueFalse);
                else
                    SetInputType(InputType.keyword);

                _values = new List<string>();


                if (info.ContentOptions != null && info.ContentOptions.Count > 0)
                {

                    if (_selectedData == null || _selectedData.Length < 1)
                        _selectedData = "<nothing selected>";

                    _values.AddRange(info.ContentOptions);

                    _values.Remove(_selectedData);
                    _values.Insert(0, _selectedData);
                }
                else
                {
                    _values.Add("<nothing to select>");
                }

                ContentDropDown.ClearOptions();
                ContentDropDown.AddOptions(_values);

                InputField.gameObject.SetActive(false);
            }
            else if (type == ColumnInfo.GetValueTypeOfPhone)
            {

                if (info.PhoneNumber != null)
                {
                    SetInputType(InputType.phone);

                    //Debug.Log("using info phone number " + info.PhoneNumber);

                    PhoneNumberInput.SetPhoneNumber(info.PhoneNumber);

                    InputField.gameObject.SetActive(false);

                }
                else
                {
                    SetInputType(InputType.phone);

                    //Debug.Log("using info string for phone number " + info.PhoneNumber);

                    if (UnitedStatesPhoneNumber.TryParse(info.Content, out var phone))
                    {
                        info.PhoneNumber = phone;
                        PhoneNumberInput.SetPhoneNumber(phone);

                        //Debug.Log("success! using for phone " + phone.FullNumberUS);

                        InputField.gameObject.SetActive(false);
                    }
                    else
                    {

                        InputField.gameObject.SetActive(true);
                        InputField.text = info.Content;
                        PhoneNumberInput.gameObject.SetActive(false);
                        InputField.lineLimit = 1;
                        InputField.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
                    }

                    //Debug.Log("grabbing null");
                }
            }
            else if (type == ColumnInfo.GetValueTypeOfEmail)
            {
                SetInputType(InputType.email);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 1;
                InputField.contentType = TMPro.TMP_InputField.ContentType.EmailAddress;
            }
            else if (type == ColumnInfo.GetValueTypeOfInt)
            {
                SetInputType(InputType.integer);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 1;
                InputField.contentType = TMPro.TMP_InputField.ContentType.IntegerNumber;
            }
            else if (type == ColumnInfo.GetValueTypeOfRational)
            {
                SetInputType(InputType.rational);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 1;
                InputField.contentType = TMPro.TMP_InputField.ContentType.DecimalNumber;
            }

            else if (type == ColumnInfo.GetValueTypeOfWord)
            {
                SetInputType(InputType.word);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 1;
                InputField.contentType = TMPro.TMP_InputField.ContentType.Alphanumeric;
            }
            else if (type == ColumnInfo.GetValueTypeOfSentence)
            {
                SetInputType(InputType.sentence);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 1;
                InputField.contentType = TMPro.TMP_InputField.ContentType.Standard;
            }
            else if (type == ColumnInfo.GetValueTypeOfMultiSelectKeyword)
            {
                SetInputType(InputType.multiSelectKeyword);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 1;
                InputField.contentType = TMPro.TMP_InputField.ContentType.Standard;
            }
            else if (type == ColumnInfo.GetValueTypeOfButton)
            {
                throw new NotImplementedException();
            }
            else if (type == ColumnInfo.GetValueTypeOfDataEntry)
            {
                throw new NotImplementedException();
            }
            else if (type == ColumnInfo.GetValueTypeOfCustom)
            {
                InputField.gameObject.SetActive(transform);
                SetInputType(InputType.custom);
                InputField.text = info.Content;
                InputField.lineLimit = 0;
                InputField.contentType = TMPro.TMP_InputField.ContentType.Standard;
            }
            else //default to type of paragraph style
            {
                SetInputType(InputType.paragraph);
                InputField.gameObject.SetActive(true);
                InputField.text = info.Content;
                InputField.lineLimit = 0;
                InputField.contentType = TMPro.TMP_InputField.ContentType.Standard;
            }

        }

        /// <summary>
        /// Set the icon
        /// </summary>
        /// <param name="sprite">the sprite</param>
        /// <param name="color">the color that the image will be</param>
        private void SetIcon(Sprite sprite, Color color, string header, string description)
        {
            InputTypeImage.gameObject.SetActive(sprite != null);

            if (sprite != null)
            {
                InputTypeImage.sprite = sprite;
                InputTypeImage.color = color;
            }

            InputTypeToolTip.headerText = header;
            InputTypeToolTip.contentText = description;
        }

        /// <summary>
        /// set the input type
        /// </summary>
        /// <param name="type">the input type</param>
        private void SetInputType(InputType type)
        {

            switch (type)
            {
                case InputType.email:
                    SetIcon(EmailSprite, EmailColor, "Email", EmailDescription);
                    break;

                case InputType.keyword:
                    SetIcon(KeywordSprite, KeywordColor, "Keyword", KeywordDescription);
                    break;

                case InputType.trueFalse:
                    SetIcon(TrueFalseSprite, TrueFalseColor, "True False", TrueFalseDescription);
                    break;

                case InputType.phone:
                    SetIcon(PhoneSprite, PhoneColor, "Phone", PhoneDescription);
                    break;

                case InputType.custom:
                    SetIcon(CustomSprite, CustomColor, "*Custom", CustomDescription);
                    break;

                case InputType.paragraph:
                    SetIcon(ParagraphSprite, ParagraphColor, "Paragraph", ParagraphDescription);
                    break;

                case InputType.word:
                    SetIcon(WordSprite, WordColor, "Word", WordDescription);
                    break;

                case InputType.integer:
                    SetIcon(IntegerSprite, IntegerColor, "Integer", IntegerDescription);
                    break;

                case InputType.rational:
                    SetIcon(RationalSprite, RationalColor, "Rational", RationalDescription);
                    break;

                case InputType.sentence:
                    SetIcon(SentenceSprite, SentenceColor, "Sentence", SentenceDescription);
                    break;

                case InputType.multiSelectKeyword:
                    SetIcon(MultiSelectSprite, MultiSelectColor, "Multi-Select Keyword", MultiSelectDescription);
                    break;

                case InputType.dataEntry:
                    SetIcon(DataEntrySprite, DataEntryColor, "Data Entry", DataEntryDescription);
                    break;

                case InputType.Error:
                    SetIcon(Error, ErrorColor, "Error", ErrorDescription);
                    break;

                default:
                    SetIcon(null, Color.white, type.ToString(), "This item does not have a description yet.");
                    break;
            }
        }

        /// <summary>
        /// Edit the content
        /// </summary>
        public void Edit()
        {
            EditTextMessageBoxController.OpenModalWindow(info.Content, info.Label, strCallBack);
            // Debug.Log("Editing");
        }

        public void OnEndEditText(string str)
        {
            strCallBack(str);

        }

        public void OnEndEditPhoneNumber(UnitedStatesPhoneNumber phone)
        {
            // info.Content = phone.FullNumberUS;
            // info.PhoneNumber = phone;
            strCallBack(phone.FullNumberUS);
        }

        public void OnChangeValue(int x)
        {
            if (_values != null)
            {
                strCallBack(_values[x]);
                _selectedData = _values[x];
            }
            else throw new NullReferenceException("values are null");
        }
    }

    /// <summary>
    /// input type
    /// </summary>
    public enum InputType
    {
        paragraph,
        word,
        keyword,
        phone,
        integer,
        rational,
        email,
        trueFalse,
        custom,
        Error,
        none,
        sentence,
        multiSelectKeyword,
        dataEntry
    }
}
