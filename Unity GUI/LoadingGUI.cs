/* LoadingGUI.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager.Unity3D
{
	/// <summary>
	/// Handles the Triangle thinking loading gui
	/// </summary>
	public class LoadingGUI : MonoBehaviour
	{
		/// <summary>
		/// Get or set the loading icon to be active or not
		/// </summary>
		public bool IsLoading = true;

		public RectTransform Triangle;
		//public RectTransform Text;

		public RectTransform TriangleOffLocation;
		public RectTransform TextOffLocation;

		public RectTransform TActiveLocation;
		public RectTransform TextActiveLocation;

		public GameObject Background;

		public Animator anim;

		private bool start = true;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (IsLoading)
			{
				if (start)
				{
					anim.gameObject.SetActive(true);
					//Text.gameObject.SetActive(true);
					anim.SetTrigger("t");
					start = false;
				}

				Triangle.anchoredPosition = Vector2.Lerp(Triangle.anchoredPosition, TActiveLocation.anchoredPosition, .125f);
				//Text.anchoredPosition = Vector2.Lerp(Text.anchoredPosition, TextActiveLocation.anchoredPosition, .05f);
				Background.SetActive(true);

			}
			else
			{
				Triangle.anchoredPosition = TriangleOffLocation.anchoredPosition;
				//Text.anchoredPosition = TextOffLocation.anchoredPosition;
				Background.SetActive(false);

				if (!start)
				{
					anim.gameObject.SetActive(false);
					//Text.gameObject.SetActive(false);
					start = true;
				}
			}
		}
	}
}
