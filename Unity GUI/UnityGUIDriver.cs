/* UniyGuiDriver.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WarManager.Unity3D;

namespace WarManager
{
	public static class UnityGUIDriver
	{
		public static StatsGUI StatsObject { get; private set; }
		public static LoadingGUI Loading { get; private set; }

		public static GUIGridPointMarkersDriver pointMarkersDriver;

		/// <summary>
		/// Set the stats object to be referenced (in the bottom right corner)
		/// </summary>
		/// <param name="statsObject">the stats object</param>
		public static void SetStatsObject(StatsGUI statsObject)
		{
			StatsObject = statsObject;
		}

		/// <summary>
		/// Set the loading gui to be referenced
		/// </summary>
		/// <param name="gUI">the gui</param>
		public static void SetLoadingGUI(LoadingGUI gUI)
		{
			Loading = gUI;
		}

		/// <summary>
		/// Sets the gridpoint markers active and moves them around the tr grid position
		/// </summary>
		/// <param name="tr">the transform</param>
		public static void SetPointMarkersActive(Transform tr, bool active)
		{
			pointMarkersDriver.active = active;

			if(active)
			pointMarkersDriver.worldTransform = tr;
		}

	}
}
