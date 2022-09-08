/* SlideWindow_Elements_Spacer.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D.Windows
{
	/// <summary>
	/// Handles the behavior of the spacer element
	/// </summary>
	public class SlideWindow_Element_Spacer : MonoBehaviour, ISlideWindow_Element
	{
		/// <summary>
		/// The information to process
		/// </summary>
		public SlideWindow_Element_ContentInfo info { get; set; }

		public GameObject targetGameObject => this.gameObject;

        public string SearchContent
		{
			get
			{
				return info.ElementType.ToString();
			}
		}

        RectTransform spacer;

		public void UpdateElement()
		{
			info.Height = Mathf.Clamp(info.Height, 20, 500);

			if (spacer == null)
				spacer = GetComponent<RectTransform>();
			spacer.sizeDelta = new Vector2(spacer.sizeDelta.x, info.Height);
		}
    }
}
