
/* OpenURL.cs
 * Author: Taylor Howell
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarManager.Unity3D
{
	/// <summary>
	/// Open a URL or URI
	/// </summary>
	public class OpenURL : MonoBehaviour
	{
		/// <summary>
		/// The URL (or URI) to open
		/// </summary>
		public string Url;

		/// <summary>
		/// Open the Url associted with the class
		/// </summary>
		public void Open()
		{
			Application.OpenURL(Url);
		}
	}
}
