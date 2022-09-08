/* UnityHeapViewer.cs
 * Author: Taylor Howell
 */

using System.Collections.Generic;

using UnityEngine;

using WarManager.Cards;

namespace WarManager.Testing
{
	/// <summary>
	/// Handles viewing the heap for debugging purposes
	/// </summary>
	public class UnityHeapViewer : MonoBehaviour
	{
		///// <summary>
		///// The reference to the data manager from the cardManager
		///// </summary>
		//CardDataManager _dataManager = CardManager.CardData;

		///// <summary>
		///// The heap view (for the inspector)
		///// </summary>
		//public List<string> HeapView = new List<string>();

		//public Vector2 UpperLeft;

		//public Vector2 LowerRight;

		//// Update is called once per frame
		//void Update()
		//{
		//	var heap = _dataManager.Heap.structure;
		//	string str = "";
		//	HeapView.Clear();

		//	for (int i = 0; i < heap.Count; i++)
		//	{
		//		List<Card> cards = heap[i].ToList();

		//		string val = "";

		//		for (int k = cards.Count - 1; k >= 0; k--)
		//		{
		//			if (cards[k] != null)
		//			{
		//				str = cards[k].ID.Substring(0, 4);

		//				val = val + ", " + cards[k].point.ToString() + " " + str;
		//			}
		//			else
		//			{
		//				val = val + "null";
		//			}
		//		}

		//		HeapView.Add(val);
		//	}

		//	((int x, int y) upperLeft, (int x, int y) lowerRight) rect = _dataManager.GetBounds();

		//	UpperLeft = new Vector2(rect.upperLeft.x, rect.upperLeft.y);
		//	LowerRight = new Vector2(rect.lowerRight.x, rect.lowerRight.y);
		//}
	}
}
