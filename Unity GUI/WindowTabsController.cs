/* WindowTabsController.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WarManager.Unity3D.Windows
{
	/// <summary>
	/// Manages toggling tabs for war manager
	/// </summary>
	public class WindowTabsController : MonoBehaviour
	{
		public List<Image> Tabs = new List<Image>();
		public List<GameObject> TabWindows = new List<GameObject>();

		public Color DefaultColor;

		public Color ActiveColor;

		bool next = false;

		public void Awake()
		{
			next = true;
			ToggleNewTab(0);
		}
	
		/// <summary>
		/// Toggle a tab by the image selected
		/// </summary>
		/// <param name="obj">the selected tab image</param>
		public void ToggleNewTab(Image obj)
		{
			if (!next)
			{
				return;
			}

			if (Tabs.Count != TabWindows.Count)
				Debug.LogError("Not every tab has its own window");

			for (int i = 0; i < Tabs.Count; i++)
			{
				if (Tabs[i] != obj)
				{
					Tabs[i].color = DefaultColor;
					TabWindows[i].SetActive(false);
				}
				else
				{
					TabWindows[i].SetActive(true);
				}
			}

			obj.color = ActiveColor;

			next = false;

		}

		/// <summary>
		/// Toggle a tab by the int
		/// </summary>
		/// <param name="tabInt">the given int</param>
		public void ToggleNewTab(int tabInt)
		{
			if (!next)
			{
				return;
			}

			if (Tabs.Count != TabWindows.Count)
				Debug.LogError("Not every tab has its own window");

			for (int i = 0; i < Tabs.Count; i++)
			{
				if (i != tabInt)
				{
					Tabs[i].color = DefaultColor;
					TabWindows[i].SetActive(false);
				}
				else
				{
					TabWindows[i].SetActive(true);
					Tabs[i].color = ActiveColor;
				}
			}

			next = false;
		}

		public void AnimComplete()
		{
			next = true;
		}
	}
}
