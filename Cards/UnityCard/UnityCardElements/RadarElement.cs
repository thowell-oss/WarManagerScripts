using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

using UnityEngine.UI;

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WarManager.Backend.CardsElementData
{

    public class RadarElement : PoolableObject, IUnityCardElement
    {
        public int ID { get; set; }

        public string ElementTag => "radar";

        public bool Active { get => this.gameObject.activeInHierarchy; set => this.gameObject.SetActive(value); }
        public bool Critical { get; set; }

        public CardElementViewData ElementViewData { get; private set; }

        [SerializeField] Image LinePrefab;
        [SerializeField] List<Image> ActiveLines = new List<Image>();

        public int TotalCriteria;
        public int TotalLevels;
        public float TotalSpacing = 50;
        public float fudge = 5;

        public bool Test;

        void Update()
        {
            if (Test)
            {
                Test = false;
                SetRadar();
            }
        }

        void SetRadar()
        {

            RemoveLines();

            if (TotalCriteria < 2)
                return;

            ElementViewData = null;

            Image firstLine = Instantiate<Image>(LinePrefab, this.transform);
            var lastLine = firstLine;

            float lastDegrees = 0;
            ActiveLines.Add(lastLine);

            for (int i = 1; i < TotalCriteria; i++)
            {
                Image line = Instantiate<Image>(LinePrefab, this.transform.position, Quaternion.identity);
                line.transform.parent = this.transform;
                ActiveLines.Add(line);

                float degrees = 360 / TotalCriteria * i;
                line.transform.rotation = Quaternion.Euler(0, 0, degrees);

                for (int k = 0; k < TotalLevels; k++)
                {
                    float currentLen = TotalSpacing / TotalLevels * k;

                    Vector3 currentLoc = ConvertFromDegreesToCoordinates(degrees, currentLen, transform.position);
                    Vector3 lastLoc = ConvertFromDegreesToCoordinates(lastDegrees, currentLen, transform.position);
                    Debug.DrawLine(currentLoc, lastLoc, Color.red, 5);
                }

                lastDegrees = degrees;
                lastLine = line;
            }

            for (int k = 0; k < TotalLevels; k++)
            {
                float currentLen = TotalSpacing / TotalLevels * k;

                Vector3 currentLoc = ConvertFromDegreesToCoordinates(0, currentLen, transform.position);
                Vector3 lastLoc = ConvertFromDegreesToCoordinates(lastDegrees, currentLen, transform.position);
                Debug.DrawLine(currentLoc, lastLoc, Color.red, 5);
            }
        }

        /// <summary>
        /// Convert from degrees to a cartesian coordinate
        /// </summary>
        /// <param name="degrees">the degree angle</param>
        /// <param name="r">r is the length from center the coordinate will be</param>
        /// <param name="center">the location of the pivot</param>
        /// <returns>returns a vector2 location</returns>
        public Vector3 ConvertFromDegreesToCoordinates(float degrees, float r, Vector3 center)
        {
            float theta = Mathf.PI / 180f * (degrees + fudge); // theta = pi / 180 deg * x deg

            float x = r * Mathf.Cos(theta); // x = r * cos(theta)
            float y = r * Mathf.Sin(theta); // y = r * sin(theta)

            return new Vector3(x, y, 0) + center;
        }


        private void RemoveLines()
        {
            if (ActiveLines.Count < 1)
                return;

            for (int i = 0; i < ActiveLines.Count; i++)
            {
                Destroy(ActiveLines[i]);
            }

            ActiveLines.Clear();
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
            SetRadar();
            return true;
        }
    }
}
