using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Cards;

namespace WarManager.Unity3D
{
	public class GUIGridPointMarkersDriver : MonoBehaviour
	{
		public List<GameObject> gridPointMarkers = new List<GameObject>();

		public Transform worldTransform;
		private Point _lastLocation;

		public float scaleAmt;

		private Vector2Int pntOffset = new Vector2Int(0, 2);

		public float clamp = 10;
		private float actualClamp;

		public bool active = false;

		public Vector2 centerPoint;
		public Vector2 startPoint;


		// Update is called once per frame
		void Update()
		{
			WarManagerDriver _sheet = WarManagerDriver.Main;

			Vector2 gridMultVector2 = _sheet.Scale;
			Vector2 offsetVector2 = _sheet.Offset;

			(float, float) gridMult = (gridMultVector2.x, gridMultVector2.y);
			(float, float) offset = (offsetVector2.x, offsetVector2.y);

			Point centerGridLocation = CardLayout.GetCardGridLocation(WarManagerDriver.ConvertVector2ToTuple(worldTransform.position), (0, 0), offset, gridMult);

			if (_lastLocation != centerGridLocation)
			{
				Reset(gridMult, offset);
				_lastLocation = centerGridLocation;
			}
		}

		private void Reset((float, float) gridMult, (float, float) offset)
		{
			if (active)
			{
				Point centerGridLocation = CardLayout.GetCardGridLocation(WarManagerDriver.ConvertVector2ToTuple(worldTransform.position), (0, 0), offset, gridMult);

				centerPoint = WarManagerDriver.ConvertPointToVector2(centerGridLocation);

				int listIterator = 0;

				if (actualClamp < clamp - 1)
				{
					actualClamp = Mathf.Lerp(actualClamp, clamp, .05f);
				}
				else
				{
					actualClamp = clamp;
				}

				for (int i = 0; i < 5; i++)
				{
					for (int k = 0; k < 7; k++)
					{
						Point location = new Point(centerGridLocation.x + i - 2, centerGridLocation.y + k - 3);

						startPoint = WarManagerDriver.ConvertPointToVector2(location);

						(float x, float y) finalWorldLocation = CardLayout.GetCardGlobalLocation(location, (0, 0), offset, gridMult);

						gridPointMarkers[listIterator].SetActive(true);
						gridPointMarkers[listIterator].transform.position = WarManagerDriver.ConvertTupleToVector2(finalWorldLocation);

						Vector2 dist = new Vector2(worldTransform.position.x - finalWorldLocation.x, worldTransform.position.y - finalWorldLocation.y);

						float mag = Vector2.SqrMagnitude(dist);

						mag = Mathf.Clamp(mag, 1, actualClamp);

						LeanTween.scale(gridPointMarkers[listIterator], Vector3.one * scaleAmt / mag, 1).setEaseOutBack();

						listIterator++;
					}

					listIterator++;
				}
			}
			else
			{

				foreach (GameObject g in gridPointMarkers)
				{
					g.SetActive(false);
					actualClamp = 4;
				}
			}
		}
	}
}
