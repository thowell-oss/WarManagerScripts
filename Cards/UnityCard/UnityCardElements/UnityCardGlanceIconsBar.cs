

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using WarManager.Backend.CardsElementData;
using WarManager.Unity3D;
using WarManager.Backend;

namespace WarManager.Cards.Elements
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class UnityCardGlanceIconsBar : PoolableObject, IUnityCardElement
    {
        public int ID { get; private set; }

        public string ElementTag { get; } = "glance";

        public bool Active
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }
        public bool Critical { get; set; }

        public CardElementViewData ElementViewData { get; private set; }

        private Vector2 _barSize;
        private Vector3 _barLocation;

        private int _maxIconCount;
        private Vector2 _iconSize;

        private Dictionary<string, (string imageName, string color)> _imageLookup = new Dictionary<string, (string, string)>();
        private List<string> _imageOrder = new List<string>();

        public Sprite FallbackSprite;
        private bool reset = false;

        private Image ImageComponent;

        List<Image> activeIcons = new List<Image>();

        /// <summary>
        /// the data used in order to change the sticker
        /// </summary>
        private string[] _spawnData;

        public Image IconImagePrefab;

        private RectTransform _rectTransform;

        private HorizontalLayoutGroup verticalLayoutGroup;

        private string fillType = "right";

        private DataEntry _entry;

        public void Refresh()
        {

            if (activeIcons != null && activeIcons.Count > 0)
            {
                for (int j = activeIcons.Count - 1; j >= 0; j--)
                {
                    if (activeIcons != null)
                        Destroy(activeIcons[j].gameObject);
                }
            }

            activeIcons = new List<Image>();

            transform.localPosition = _barLocation;

            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            _rectTransform.sizeDelta = _barSize;

            if (verticalLayoutGroup == null)
                verticalLayoutGroup = GetComponent<HorizontalLayoutGroup>();

            switch (fillType)
            {
                case "left":
                    verticalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
                    break;

                case "center":
                    verticalLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
                    break;

                default:
                    verticalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
                    break;
            }

            List<(string keyword, Sprite icon, Color color)> icons = new List<(string keyword, Sprite icon, Color color)>();

            if (_spawnData != null && _spawnData.Length > 0)
            {
                List<string> keywords = new List<string>();

                for (int i = 0; i < _spawnData.Length; i++)
                {
                    if (_spawnData[i] != null)
                    {
                        if (_spawnData[i].Contains(","))
                        {
                            string[] splitStr = _spawnData[i].Split(',');
                            keywords.AddRange(splitStr);
                        }
                        else
                        {
                            keywords.Add(_spawnData[i]);
                        }
                    }
                }

                // Debug.Log(string.Join("|", keywords));

                foreach (var word in _imageOrder)
                {
                    if (keywords.Find(x => x == word) != null && _imageLookup.TryGetValue(word, out var result))
                    {
                        Color col = Color.clear;
                        Sprite icon = FallbackSprite;

                        string colorStr = result.color;

                        if (ColorUtility.TryParseHtmlString(colorStr, out var finalCol))
                        {
                            col = finalCol;
                        }

                        string iconName = result.imageName;

                        if (WarManager.Unity3D.ServerIconsManager.TryGetIcon(iconName, out var finalIcon))
                        {
                            icon = finalIcon;
                        }

                        icons.Add((word, icon, col));
                    }
                }
            }

            int k = 0;

            while (k < _maxIconCount && k < icons.Count)
            {

                Image img = Instantiate<Image>(IconImagePrefab, transform);

                TooltipTrigger t = img.gameObject.GetComponent<TooltipTrigger>();
                t.headerText = icons[k].keyword;

                t.size = _iconSize / 2.2f;

                img.color = icons[k].color;
                img.sprite = icons[k].icon;

                img.transform.localScale = _iconSize;

                activeIcons.Add(img);

                k++;
            }
        }

        IEnumerator GetIconWait(string name, Image image, Color col)
        {
            yield return new WaitUntil(() => GetIcon(name, image, col));
        }

        private bool GetIcon(string name, Image image, Color col)
        {
            if (WarManager.Unity3D.ServerIconsManager.TryGetIcon(name, out var finalIcon))
            {
                image.enabled = true;
                image.sprite = finalIcon;
                image.color = col;
                // tooltipTrigger.headerText = _spawnData[0];
            }
            else
            {
                image.enabled = false;
            }

            return true;
        }

        public int CompareTo(IUnityCardElement other)
        {
            return ID.CompareTo(other.ID);
        }

        public void DisableNonEssential()
        {

        }

        /// <summary>
        /// Attempt to set a sprite from data
        /// </summary>
        /// <param name="glanceElement">the image element</param>
        /// <param name="glanceData">the associated data</param>
        /// <returns>returns true if the 'Try Set' was possible, false if not</returns>a
        public static bool TrySetGlance(UnityCardGlanceIconsBar glanceElement, CardGlanceElementData glanceData, DataEntry entry)
        {

            #region old
            // if (element == null)
            //     throw new NullReferenceException("The element cannot be null");

            // if (data == null)
            //     throw new NullReferenceException("the data cannot be null");

            // if (element.ElementTag == null)
            //     throw new NullReferenceException("the element tag cannot be null");

            // if (element.ElementTag != data.ElementTag)
            //     return false;

            // element.gameObject.SetActive(true);

            // element.ID = data.ID;
            // element._barLocation = new Vector2(data.Location[0], data.Location[1]);
            // element.Critical = data.Critical;
            // element._imageLookup.Clear();
            // element._unityCard = card;

            // if (data.Payload != null && data.Payload != string.Empty)
            // {
            //     string[] payload = data.Payload.Split(',');

            //     element._spawnData = data.DisplayInfo;

            //     Vector2 size = new Vector2(1, 1);

            //     if (payload.Length >= 1)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[0], out amt))
            //         {
            //             amt = amt / 2;

            //             size = new Vector2(amt, size.y);
            //             // Debug.Log("x");
            //         }
            //         else
            //         {
            //             string message = "Property 1 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 2)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[1], out amt))
            //         {
            //             amt = amt / 2;

            //             size = new Vector2(size.x, amt);
            //             // Debug.Log("added border amt " + amt);
            //         }
            //         else
            //         {
            //             string message = "Property 2 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 3)
            //     {
            //         int amt = 4;
            //         if (Int32.TryParse(payload[2], out amt))
            //         {
            //             element._maxIconCount = amt;
            //         }
            //         else
            //         {
            //             string message = "Property 3 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     Vector2 iconSize = new Vector2(1, 1);

            //     if (payload.Length >= 4)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[3], out amt))
            //         {
            //             amt = amt / 2;
            //             iconSize = new Vector2(amt, iconSize.y);
            //             // Debug.Log("x");
            //         }
            //         else
            //         {
            //             string message = "Property 4 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 5)
            //     {
            //         float amt = 1;
            //         if (float.TryParse(payload[4], out amt))
            //         {
            //             amt = amt / 2;
            //             iconSize = new Vector2(iconSize.x, amt);
            //             // Debug.Log("added border amt " + amt);
            //         }
            //         else
            //         {
            //             string message = "Property 5 of the glance bar was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     element._iconSize = iconSize;

            //     //on payload location 5 there are options for more than just one column (gathered in card makeup)

            //     if (payload.Length >= 7)
            //     {
            //         string fillType = payload[6].Trim();

            //         if (fillType == "center" || fillType == "right" || fillType == "left")
            //         {
            //             element.fillType = fillType;
            //         }
            //     }
            //     else
            //     {
            //         string message = "Property 7 of the glance bar was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }

            //     if (payload.Length >= 8) //input example: some_tagId=iconName:hex_color,...
            //     {
            //         for (int i = 7; i < payload.Length; i++)
            //         {
            //             string str = payload[i];

            //             string[] split1 = str.Split('=');

            //             if (split1 != null && split1.Length > 0)
            //             {
            //                 if (split1[0] != null)
            //                 {
            //                     string tagId = split1[0].Trim();
            //                     string[] split2 = split1[1].Split(':');

            //                     if (split2 != null && split2.Length > 1)
            //                     {
            //                         if (split2[0] != null && split2[0] != string.Empty)
            //                         {
            //                             string iconName = split2[0].Trim();

            //                             if (split2[1] != null && split2[1] != string.Empty)
            //                             {
            //                                 string color = split2[1].Trim();
            //                                 element._imageLookup.Add(tagId, (iconName, color));
            //                                 element._imageOrder.Add(tagId);
            //                             }
            //                             else
            //                             {
            //                                 NotificationHandler.Print("Could not parse the color on section: " + (i - 7));
            //                             }
            //                         }
            //                         else
            //                         {
            //                             NotificationHandler.Print("Could not parse the image name on section: " + (i - 7));
            //                         }
            //                     }
            //                     else
            //                     {
            //                         NotificationHandler.Print("Could not parse the image name or color on section: " + (i - 7));
            //                     }
            //                 }
            //                 else
            //                 {
            //                     NotificationHandler.Print("Could not parse the image keyword on section: " + (i - 7));
            //                 }
            //             }
            //             else
            //             {
            //                 NotificationHandler.Print("Could not parse the image info on section: " + (i - 7));
            //             }
            //         }
            //     }
            //     else
            //     {
            //         string message = "Property 7 of the glance bar was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }

            //     element._barSize = size;

            //     element.Refresh();

            //     return true;
            // }

            #endregion

            glanceElement._barLocation = new Vector3((float)glanceData.Location[0], (float)glanceData.Location[1], (float)glanceData.Location[2]);
            glanceElement._barSize = new Vector2((float)glanceData.Scale[0], (float)glanceData.Scale[1]);
            glanceElement._iconSize = new Vector2((float)glanceData.IconSize[0], (float)glanceData.IconSize[1]);

            glanceElement.ElementViewData = glanceData;

            glanceElement._imageLookup.Clear();
            glanceElement._imageOrder.Clear();
            for (int i = 0; i < glanceData.GlanceIcons.Length; i++)
            {
                glanceElement._imageLookup.Add(glanceData.GlanceIcons[i].TagId, (glanceData.GlanceIcons[i].IconPath, glanceData.GlanceIcons[i].HexColor));
                glanceElement._imageOrder.Add(glanceData.GlanceIcons[i].TagId);
            }

            glanceElement._maxIconCount = glanceData.MaxIconCount;

            glanceElement._entry = entry;

            List<string> dataLocations = new List<string>();

            foreach (var col in glanceData.Columns)
            {
                if (entry.TryGetValueAt(col, out var value))
                {
                    string str = value.ParseToParagraph();

                    string[] dataTags = str.Split(',');
                    for (int i = 0; i < dataTags.Length; i++)
                    {
                        dataTags[i] = dataTags[i].Trim();
                    }

                    dataLocations.AddRange(dataTags);
                }
            }

            glanceElement._spawnData = dataLocations.ToArray();

            glanceElement.Refresh();

            return true;
        }

        void OnDisable()
        {
            for (int i = activeIcons.Count - 1; i >= 0; i--)
            {
                Destroy(activeIcons[i].gameObject);
                activeIcons.RemoveAt(i);
            }

            _imageLookup.Clear();
            _imageOrder.Clear();
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is CardGlanceElementData d)
            {
                return TrySetGlance(this, d, entry);
            }

            return false;
        }
    }
}