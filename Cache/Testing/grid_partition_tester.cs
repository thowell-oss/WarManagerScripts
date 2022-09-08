
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager;
using WarManager.Cards;

namespace WarManager.Testing
{
    public class grid_partition_tester : MonoBehaviour
    {
        GridPartition<Card> Partition;

        float mult_x = 7;
        float mult_y = 5;

        // Start is called before the first frame update
        void Start()
        {
            // Partition = new GridPartition<Card>(new Point(0, 0));
            // Point p = new Point(5, -5);

            // Debug.Log(Partition.GridBounds.IsInBounds(p));

            // p = new Point(11, -11);
            // Debug.Log(Partition.GridBounds.IsInBounds(p));

            // p = new Point(-1, 1);
            // Debug.Log(Partition.GridBounds.IsInBounds(p));

            // p = new Point(10, 10);
            // Debug.Log("Bounds: " + Partition.GridBounds.IsInBounds(p));

            // Debug.Log("start complete");

            // Card c = new Card(new Point(0, -2), "new card");
            // Partition.Add(c);

            // Debug.Log(Partition.GetObj(new Point(0, -2)).ToString());

            // Debug.Log(Partition.Exist(new Point(0, -3)).ToString());
            // Debug.Log(Partition.Exist(new Point(0, -2)).ToString());
            // Partition.Remove(new Point(0, -2));
            // Debug.Log(Partition.Exist(new Point(0, -2)).ToString());

            // int x = 10;
            // int y = 10;

            // for (int i = 0; i < x; i++)
            // {
            //     for (int j = 0; j < y; j++)
            //     {
            //         Partition.Add(new Card(new Point(i, -j)));
            //     }
            // }

            // Card[] cards = Partition.GetRow(new Point(2, -2));

            // foreach (Card someCard in cards)
            // {
            //     Debug.Log("Card " + someCard.ToString());
            // }

            // cards = Partition.GetColumn(new Point(2, -2));

            // foreach (Card someCard in cards)
            // {
            //     Debug.Log("Card " + someCard.ToString());
            // }
        }

        // Update is called once per frame
        void Update()
        {
            DrawBounds(Partition.GridBounds);
        }

        /// <summary>
        /// Draw the partition bounds
        /// </summary>
        /// <param name="partition">the parition</param>
        private void DrawBounds(WarManager.Rect partition)
        {
            Debug.DrawLine(GetVector(partition.TopLeftCorner), GetVector(partition.TopRightCorner));
            Debug.DrawLine(GetVector(partition.TopRightCorner), GetVector(partition.BottomRightCorner));
            Debug.DrawLine(GetVector(partition.BottomRightCorner), GetVector(partition.BottomLeftCorner));
            Debug.DrawLine(GetVector(partition.BottomLeftCorner), GetVector(partition.TopLeftCorner));

            Debug.DrawLine(GetVector(partition.TopLeftCorner), GetVector(partition.BottomRightCorner));
            Debug.DrawLine(GetVector(partition.TopRightCorner), GetVector(partition.BottomLeftCorner));
        }

        private Vector2 GetVector(Point gridPoint)
        {
            return new Vector2(gridPoint.x * mult_x, gridPoint.y * mult_y);
        }
    }
}
