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
    [Notes.Author("the unity card pie chart")]
    public class PieChartCardElement : PoolableObject, IUnityCardElement
    {
        public int ID { get; set; }

        public string ElementTag => "pie chart";

        public bool Active { get => this.gameObject.activeInHierarchy; set => this.gameObject.SetActive(value); }
        public bool Critical { get; set; }

        public CardElementViewData ElementViewData { get; private set; }

        public bool Test;

        public List<PieChartData> PieChartDataList = new List<PieChartData>();

        PieChartElementData ElementProperties;

        public List<Image> ActiveRings = new List<Image>();
        public Image RingPrefab;

        public PivotLabel PivotPrefab;

        [SerializeField] TMPro.TMP_Text TitleText;

        private List<DataValue> values = new List<DataValue>();

        private List<PivotLabel> ActiveLabels = new List<PivotLabel>();

        RectTransform _rectTransform;

        public static bool TrySetProperties(PieChartCardElement element, PieChartElementData data, DataEntry entry)
        {

            if (element._rectTransform == null)
                element._rectTransform = element.GetComponent<RectTransform>();

            element.ElementViewData = data;

            element.transform.localPosition = new Vector3((float)data.Location[0], (float)data.Location[1], (float)data.Location[2]);
            element._rectTransform.sizeDelta = new Vector2((float)data.Scale[0], (float)data.Scale[1]);
            element._rectTransform.rotation = Quaternion.Euler(new Vector3(0, 0, (float)data.Rotation[0]));

            element.PieChartDataList = data.Data;

            element.values = new List<DataValue>();
            element.values.AddRange(entry.GetAllowedDataValues());

            element.SetPieChart();

            element.ElementProperties = data;

            return true;
        }


        void Update()
        {
            if (Test)
            {
                Test = false;

                PieChartDataList = new List<PieChartData>()
                {
                    new PieChartData()
                    {
                        Column = 1,
                        HexColor = "#eeaaaa",
                        Min = 0,
                        Max = 1,
                        ClampValue = "json"
                    },
                    new PieChartData()
                    {
                        Column = 2,
                        HexColor = "#aaaaee",
                        Min = 0,
                        Max = 1,
                        ClampValue = "json"
                    },
                    new PieChartData()
                    {
                        Column = 3,
                        HexColor = "#33aa33",
                        Min = 0,
                        Max = 1,
                        ClampValue = "json"
                    },
                    new PieChartData()
                    {
                        Column = 4,
                        HexColor = "#aaaaee",
                        Min = 0,
                        Max = 1,
                        ClampValue = "json"
                    },
                };

                string rowId = System.Guid.NewGuid().ToString();

                values = new List<DataValue>()
                {
                    new DataValue((0, rowId), "id", "", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    new DataValue((1, rowId), "10 Mo Comm", ".5", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    new DataValue((2, rowId), "Sheet Metal", ".25", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    new DataValue((3, rowId), "GC", ".32", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    new DataValue((4, rowId), "Solar", ".1", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    new DataValue((6, rowId), "Solar", ".1", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    new DataValue((5, rowId), "title", "Labor Cost", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                    //new DataValue((3, rowId), "data 3", "4", ColumnInfo.GetValueTypeOfRational, ValuePermissions.Full),
                };

                ElementProperties = new PieChartElementData()
                {
                    LabelOffset = new double[2] { .5, .5 },
                    LabelFontSize = 12,
                    LabelOrbitDistanceFromCenter = 2.5,
                    LabelValueFormat = "percent",
                    LabelColorHex = "#aaa",
                    TitleColorHex = "#eee"
                };

                ElementProperties.SetColumns = new List<int>() { 5 };

                SetPieChart();
            }
        }

        private void SetPieChart()
        {

            ClearPieChart();
            var dataDict = GetData(PieChartDataList, out var sum);

            TitleText.fontSize = ElementProperties.TitleFontSize;
            var titleValue = values.Find(x => x.CellLocation.column == ElementProperties.ToColumnArray[0]);

            if (titleValue != null)
            {
                TitleText.text = titleValue.ParseToParagraph();
            }
            else
            {
                TitleText.text = "<error>";
            }

            if (ColorUtility.TryParseHtmlString(ElementProperties.TitleColorHex, out var labelColor))
            {
                TitleText.color = labelColor;
            }

            float currentAddedTotal = 0;

            foreach (var x in dataDict)
            {
                var ringPercent = x.Value.amt / sum;

                //Debug.Log(ringPercent + " " + x.Value + " " + sum);

                var newRing = Instantiate<Image>(RingPrefab, transform);

                LeanTween.value(newRing.gameObject, (x) => { TweenRotation(newRing, x); }, 0, currentAddedTotal, 1f).setEaseOutSine();

                if (ColorUtility.TryParseHtmlString(x.Key.HexColor, out var color))
                {
                    newRing.color = color;
                }

                //newRing.fillAmount = ringPercent;

                LeanTween.value(newRing.gameObject, (x) => { TweenFillAmount(newRing, x); }, 0, ringPercent, 1f).setEaseInOutCubic();

                var newLabel = Instantiate<PivotLabel>(PivotPrefab, transform);
                ActiveLabels.Add(newLabel);

                string valueFormat = "";
                if (ElementProperties.LabelValueFormat == "percent")
                    valueFormat = string.Format("{0:0.##}%", ringPercent * 100);
                else if (ElementProperties.LabelValueFormat == "actual")
                    valueFormat = x.Value.total.ToString();
                else
                    valueFormat = "<incorrect>";

                newLabel.SetPivot(currentAddedTotal, (float)ElementProperties.LabelOrbitDistanceFromCenter, (float)ElementProperties.LabelOffset[0], (float)ElementProperties.LabelOffset[1], ringPercent,
                x.Value.header, valueFormat, ElementProperties.TitleFontSize);

                if (ColorUtility.TryParseHtmlString(ElementProperties.LabelColorHex, out Color col))
                {
                    newLabel.LabelText.color = col;
                }

                currentAddedTotal += ringPercent;

                ActiveRings.Add(newRing);
            }
        }

        private void TweenRotation(Image obj, float currentAddedTotal)
        {
            obj.transform.rotation = Quaternion.Euler(new Vector3(obj.transform.rotation.x, obj.transform.rotation.y,
             360 - (360 * currentAddedTotal)));
        }


        private void TweenFillAmount(Image obj, float amt)
        {
            obj.fillAmount = amt;
        }

        private void ClearPieChart()
        {
            for (int i = 0; i < ActiveRings.Count; i++)
            {
                Destroy(ActiveRings[i].gameObject);
            }

            ActiveRings.Clear();

            for (int i = 0; i < ActiveLabels.Count; i++)
            {
                Destroy(ActiveLabels[i].gameObject);
            }

            ActiveLabels.Clear();
        }

        /// <summary>
        /// Get the current values with the pie chart data as the key
        /// </summary>
        /// <param name="data">the data</param>
        /// <returns>returns a non null dictionary when complete</returns>
        private Dictionary<PieChartData, (float amt, string header, float total)> GetData(List<PieChartData> data, out float sum)
        {

            if (data == null)
                data = new List<PieChartData>();

            Dictionary<PieChartData, (float amt, string header, float total)> pieData = new Dictionary<PieChartData, (float amt, string header, float total)>();

            float total = 0;

            for (int i = 0; i < data.Count; i++)
            {
                var dataValue = values.Find(x => x.CellLocation.column == data[i].Column);

                var value = dataValue.ParseToParagraph();
                var header = dataValue.HeaderName;

                if (float.TryParse(value, out var val))
                {

                    float max = data[i].Max - data[i].Min;

                    float current = 0;

                    if (val > data[i].Min)
                        current = val - data[i].Min;

                    if (max == 0)
                        max = 1; //prevent divide by zero issues

                    float percent = current / max;

                    pieData.Add(data[i], (percent, dataValue.HeaderName, val));
                    total += val;
                }
            }

            sum = total;
            return pieData;
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
            if (data is PieChartElementData d)
            {
                return TrySetProperties(this, d, entry);
            }

            return false;
        }
    }
}
