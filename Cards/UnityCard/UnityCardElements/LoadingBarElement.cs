using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{

    [RequireComponent(typeof(RectTransform))]
    public class LoadingBarElement : PoolableObject, IUnityCardElement
    {
        public int ID { get; set; }

        public string ElementTag => "bar";

        public bool Active { get => this.gameObject.activeInHierarchy; set => this.gameObject.SetActive(value); }
        public bool Critical { get; set; }

        public LoadingBarElementData Data { get; private set; }

        RectTransform _rectTransform;

        public Image BarImage;
        public Image BackgroundImage;

        public Image LoadingBarImage;
        public TMPro.TMP_Text PercentTextRight, PercentTextLeft;

        private TMPro.TMP_Text _selectedText;

        public TMPro.TMP_Text TitleText;

        [SerializeField] Transform Top, Bottom;

        public float Value { get; private set; } = 0;

        public string Title { get; set; } = "";

        public CardElementViewData ElementViewData { get; private set; }

        [SerializeField] float _testValue;
        public float min;
        public float max;

        public bool test;


        void Update()
        {
            if (test)
            {
                test = false;
                Data = new LoadingBarElementData()
                {
                    Min = min,
                    Max = max,
                    UsePercentText = "right"
                };

                var id = System.Guid.NewGuid().ToString();

                var values = new DataValue[2]
                {
                    new DataValue((1,id), "id", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full, ""),
                    new DataValue((1,id), "value", "", ColumnInfo.GetValueTypeOfParagraph, ValuePermissions.Full, "")
                };

                Value = _testValue;
                Title = "Overall Budget";

                SetStaticBarValues();
                SetBar();
            }
        }

        private void SetTransform()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            transform.localPosition = new Vector3((float)Data.Location[0], (float)Data.Location[1], (float)Data.Location[2]);
            _rectTransform.sizeDelta = new Vector2((float)Data.Scale[0], (float)Data.Scale[1]);
            _rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, (float)Data.Rotation[0]));
        }

        private void SetStaticBarValues()
        {
            switch (Data.UsePercentText)
            {
                case "left":
                    PercentTextLeft.gameObject.SetActive(true);
                    PercentTextRight.gameObject.SetActive(false);
                    _selectedText = PercentTextLeft;
                    break;
                case "right":
                    PercentTextLeft.gameObject.SetActive(false);
                    PercentTextRight.gameObject.SetActive(true);
                    _selectedText = PercentTextRight;
                    break;

                default:
                    _selectedText = null;
                    PercentTextLeft.gameObject.SetActive(false);
                    PercentTextRight.gameObject.SetActive(false);
                    break;
            }

            if (ColorUtility.TryParseHtmlString(Data.TitleTextColor, out var col))
            {
                TitleText.color = col;
            }

            TitleText.fontSize = Data.TitleTextFontSize;

            LoadingBarImage.gameObject.SetActive(Data.UseLine);

            if (_selectedText != null)
            {
                _selectedText.fontSize = Data.PercentTextFontSize;

                if (ColorUtility.TryParseHtmlString(Data.PercentTextColor, out var _selectedTextCol))
                {
                    _selectedText.color = _selectedTextCol;
                }
            }
        }

        private void SetBar()
        {
            TitleText.text = Title;
            float barPercent = 0;

            float min = (float)Data.Min;
            float max = (float)Data.Max;

            if (max - min > 0 && Value - min >= 0)
            {
                barPercent = (Value - min) / (max - min);
            }
            else
            {
                barPercent = 0;
            }

            LeanTween.value(this.gameObject, SetValue, 0, barPercent, .5f).setEaseOutBack();
            //LeanTween.value(this.gameObject, (x) => { LoadingBarImage.color = x; }, b )

            Color bottomColor = Color.black;

            if (ColorUtility.TryParseHtmlString(Data.BarColorHex, out var barHexCol))
            {
                bottomColor = barHexCol;
            }

            if (Data.UseGradientColor)
            {
                //Debug.Log(barPercent);

                //Debug.Log("top color" + Data.BarColorGradientTop);

                if (ColorUtility.TryParseHtmlString(Data.BarColorGradientTop, out var topColor))
                {
                    BarImage.color = Color.Lerp(bottomColor, topColor, barPercent);
                }
                else
                {
                    BarImage.color = bottomColor;
                }
            }
            else
            {
                BarImage.color = bottomColor;
            }


            if (ColorUtility.TryParseHtmlString(Data.BackgroundColorHex, out var backgroundColor))
            {
                BackgroundImage.color = backgroundColor;
            }
        }

        private void SetValue(float normalizedValue)
        {
            BarImage.fillAmount = normalizedValue;
            LoadingBarImage.transform.position = Vector3.Lerp(Bottom.position, Top.position, normalizedValue);

            if (_selectedText == null)
                return;

            if (normalizedValue >= 0)
            {
                _selectedText.text = string.Format("{0:00.0}%", normalizedValue * 100);
            }
            else
            {
                _selectedText.text = "0";
            }

            _selectedText.transform.rotation = Quaternion.Euler(Vector3.zero);
        }

        /// <summary>
        /// Set the color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="hexColor"></param>
        public void SetColor(Color color, string hexColor)
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out var col))
            {
                color = col;
            }
        }

        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other.ID);
        }

        public void DisableNonEssential()
        {
            throw new System.NotImplementedException();
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is LoadingBarElementData d)
            {
                Data = d;
                ElementViewData = d;

                if (entry.TryGetValueAt(data.GetFirstColumn(), out var result))
                {
                    Title = result.HeaderName;

                    if (result.Value != null && result.Value.ToString().Trim().Length > 0)
                    {

                        if (float.TryParse(result.Value.ToString(), out var value))
                        {
                            Value = value;

                            SetTransform();
                            SetStaticBarValues();
                            SetBar();

                            return true;
                        }
                    }
                    else
                    {
                        Value = 0;
                        SetTransform();
                        SetStaticBarValues();
                        SetBar();

                        return result.Value != null;
                    }
                }
            }
            return false;
        }
    }
}
