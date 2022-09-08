

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
    [RequireComponent(typeof(Image))]
    public class UnityCardImage : PoolableObject, IUnityCardElement
    {
        public int ID { get; private set; }

        public string ElementTag { get; } = "sticker";

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

        private Vector2 _size;
        private Vector3 Location;

        private Dictionary<string, (string imageName, string color)> _ImageLookup = new Dictionary<string, (string, string)>();

        public Sprite FallbackSprite;
        private bool reset = false;

        private Image ImageComponent;

        /// <summary>
        /// the data used in order to change the sticker
        /// </summary>
        private string[] _spawnData;

        TooltipTrigger tooltipTrigger;
        BoxCollider2D collider2d;

        public void Refresh()
        {

            if (tooltipTrigger == null)
                tooltipTrigger = GetComponent<TooltipTrigger>();

            tooltipTrigger.size = _size;

            transform.localPosition = Location;
            transform.localScale = _size;

            ColorUtility.TryParseHtmlString("#FF00FF", out var col);
            Sprite icon = FallbackSprite;

            if (ImageComponent == null)
                ImageComponent = GetComponent<Image>();


            if (_spawnData != null && _spawnData.Length > 0 && _spawnData[0] != null && _spawnData[0].Length > 0)
            {
                // Debug.Log(string.Join(",", _spawnData[0]));

                if (_ImageLookup.TryGetValue(_spawnData[0], out var result))
                {
                    string colorStr = result.color;

                    if (ColorUtility.TryParseHtmlString(colorStr, out var finalCol))
                    {
                        col = finalCol;
                    }

                    string iconName = result.imageName;

                    // if (WarManager.Unity3D.ServerIconsManager.TryGetIcon(iconName, out var finalIcon))
                    // {
                    //     icon = finalIcon;
                    // }
                    // else
                    // {
                    //     throw new ArgumentException("could not get icon " + _spawnData[0]);
                    // }

                    // tooltipTrigger.headerText = _spawnData[0];

                    // ImageComponent.enabled = true;

                    StartCoroutine(GetIconWait(iconName, ImageComponent, col));
                }
                else
                {
                    //Debug.Log(string.Join(",",_spawnData));
                    //throw new ArgumentException("Could not get image " + _spawnData[0]);
                    ImageComponent.enabled = false;
                }
            }
            else
            {
                //throw new ArgumentException("The spawn data is not correct ");
                ImageComponent.enabled = false;
            }




            //ImageComponent.color = col;
            // ImageComponent.sprite = icon;
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
                tooltipTrigger.headerText = _spawnData[0];
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
        /// <param name="stickerElement">the image element</param>
        /// <param name="stickerData">the associated data</param>
        /// <returns>returns true if the 'Try Set' was possible, false if not</returns>a
        public static bool TrySetSticker(UnityCardImage stickerElement, CardStickerElementData stickerData, DataEntry entry)
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
            // element.Location = new Vector2(data.Location[0], data.Location[1]);
            // element.Critical = data.Critical;

            // element._ImageLookup.Clear();

            // if (data.Payload != null && data.Payload != string.Empty)
            // {
            //     string[] payload = data.Payload.Split(',');

            //     Vector2 size = new Vector2(1, 1);

            //     element._spawnData = data.DisplayInfo;

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
            //             string message = "Property 1 of the sticker was not parsed correctly.";
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
            //             string message = "Property 2 of the sticker was not parsed correctly.";
            //             NotificationHandler.Print(message);
            //         }
            //     }

            //     if (payload.Length >= 3) //input example: some_tagId=iconName:hex_color,...
            //     {
            //         for (int i = 2; i < payload.Length; i++)
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
            //                                 element._ImageLookup.Add(tagId, (iconName, color));
            //                             }
            //                             else
            //                             {
            //                                 NotificationHandler.Print("Could not parse the color");
            //                             }
            //                         }
            //                         else
            //                         {
            //                             NotificationHandler.Print("Could not parse the image name");
            //                         }
            //                     }
            //                     else
            //                     {
            //                         NotificationHandler.Print("Could not parse the image name or color");
            //                     }
            //                 }
            //                 else
            //                 {
            //                     NotificationHandler.Print("Could not parse the image keyword");
            //                 }
            //             }
            //             else
            //             {
            //                 NotificationHandler.Print("Could not parse the image info");
            //             }
            //         }
            //     }
            //     else
            //     {
            //         string message = "Property 3 of the sticker was not parsed correctly.";
            //         NotificationHandler.Print(message);
            //     }

            //     element._size = size;

            //     element.Refresh();

            //     return true;
            // }

            #endregion

            stickerElement.ElementViewData = stickerData;

            stickerElement.Location = new Vector3((float)stickerData.Location[0], (float)stickerData.Location[1],
             (float)stickerData.Location[2]);

            stickerElement._size = new Vector2((float)stickerData.Scale[0], (float)stickerData.Scale[1]);

            stickerElement._ImageLookup.Clear();

            for (int i = 0; i < stickerData.StickersData.Length; i++)
            {
                stickerElement._ImageLookup.Add(stickerData.StickersData[i].TagId,
                (stickerData.StickersData[i].IconPath, stickerData.StickersData[i].HexColor));
            }

            stickerElement.Critical = stickerData.Critical;

            List<string> dataLocations = new List<string>();

            if (entry != null && entry.ValueCount > 0)
            {

                foreach (var col in stickerData.Columns)
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
            }

            stickerElement._spawnData = dataLocations.ToArray();

            stickerElement.Refresh();

            return true;
        }

        public bool SetElementProperties<T>(T data, DataEntry entry) where T : CardElementViewData
        {
            if (data is CardStickerElementData d)
            {
                return TrySetSticker(this, d, entry);
            }

            return false;
        }
    }
}
