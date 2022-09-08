using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager;
using WarManager.Backend;
using WarManager.Cards;
using WarManager.Unity3D;

namespace WarManager.Testing
{
    [Notes.Author("tesing the card clustering")]
    public class Test_CardClustering : MonoBehaviour
    {
        [SerializeField] private bool TestActive = true;

        [SerializeField] List<string> clusteredCards = new List<string>();

        private void Update()
        {
            if (!TestActive)
                return;
            TestActive = false;

            GetCardClusteringBounds();
            CardClusteringTesting();

        }

        private void GetCardClusteringBounds()
        {
            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var bounds = CardUtility.GetCardClusterBoundingBoxes(sheet, sheet.CurrentLayer);

                foreach (var x in bounds)
                {

                    var topLeft = x.Value.TopLeftCorner;
                    var bottomRight = x.Value.BottomRightCorner;

                    Debug.Log(topLeft.ToString() + " " + bottomRight.ToString());

                    //if (x.Value != null)
                    //{
                    //    var pnts = x.Value.GetPerimeter();

                    //    foreach (var p in pnts)
                    //    {
                    //        Debug.DrawLine(new Vector3(p.x, p.y, 0), new Vector3(p.x, p.y - 5, 0), Color.green);
                    //    }
                    //}
                }
            }
        }

        private void CardClusteringTesting()
        {
            clusteredCards.Clear();

            if (SheetsManager.TryGetCurrentSheet(out var sheet))
            {
                var obj = sheet.GetAllObj();

                var cards = CardUtility.GetCardCluster(obj[0], sheet.CurrentLayer, sheet.ID);

                foreach (var x in cards)
                {
                    clusteredCards.Add(x.point.ToString());
                }
            }
        }
    }
}