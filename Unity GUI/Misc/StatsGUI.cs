/* StatsGUI.cs
 * Author: Taylor Howell
 */
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Unity3D
{
	/// <summary>
	/// Handles the stats gui in the bottom left corner of the screen
	/// </summary>
	public class StatsGUI : MonoBehaviour
	{
		private List<string> StatsToDisplay = new List<string>();
		public TMPro.TMP_Text TextObject;

		private string lastStatAdded;

		// Start is called before the first frame update
		void Start()
		{
			StatsToDisplay.Add("Loading Stats...");
		}

		// Update is called once per frame
		void Update()
		{
			string finalString = "";

			foreach (string s in StatsToDisplay)
			{
				if (!string.IsNullOrEmpty(finalString))
				{
					finalString = finalString + ", " + s;
				}
				else
				{
					finalString = s;
				}
			}

			if (StatsToDisplay.Count > 5)
				StatsToDisplay.RemoveAt(0);

			TextObject.text = finalString;
		}

		/// <summary>
		/// Add a stat to be displayed
		/// </summary>
		/// <param name="s"></param>
		public void AddStat(string s)
		{
			if (lastStatAdded != s)
			{
				StatsToDisplay.Add(s);
				lastStatAdded = s;
			}
		}
	}
}
