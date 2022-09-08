/* InfoContent.cs
 * Author: Taylor Howell
 */

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using WarManager.Unity3D.Windows;

namespace WarManager.Unity3D
{
	/// <summary>
	/// Handles the transferring of information in order to setup a window
	/// </summary>
	public class WindowContentQueue
	{
		[SerializeField] Queue<ISideWindowContent> _content = new Queue<ISideWindowContent>();

		public int Count
		{
			get
			{
				return _content.Count;
			}
		}

		public void EnqeueContent(ISideWindowContent newCont)
		{
			_content.Enqueue(newCont);
		}

		public ISideWindowContent DequeueContent()
		{
			if (_content.Count < 0)
			{
				return null;
			}
			else
			{
				return _content.Dequeue();
			}
		}
	}
}
