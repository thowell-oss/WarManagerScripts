/* SaveButtonController.cs
 * Author: Taylor Howell
 */


using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WarManager.Unity3D
{
	/// <summary>
	/// Handles the GUI of the saving animations
	/// </summary>
	public class SaveButtonIconController : MonoBehaviour
	{
		/// <summary>
		/// The current saving animation
		/// </summary>
		public SavingStatus CurrentSavingStatus;
		Animator anim;
		Button b;

		public static UnityEvent OnChangeStatus;

		public void Start()
		{
			SetUpdateStatus(0);
		}

		/// <summary>
		/// Set the animation status and also tell if commands can be processed at that time
		/// </summary>
		/// <param name="newStatus">The status that the button will now show</param>
		public void SetUpdateStatus(SavingStatus newStatus)
		{
			CurrentSavingStatus = newStatus;
			if (anim == null)
				anim = GetComponent<Animator>();
			anim.SetInteger("Status", (int)newStatus);


			if (b == null)
				b = GetComponent<Button>();

			switch (newStatus)
			{
				case SavingStatus.NotConnected:
					b.interactable = false;
					break;

				case SavingStatus.Upload:
					b.interactable = true;
					break;

				case SavingStatus.Uploading:
					b.interactable = false;
					break;

				case SavingStatus.SaveAs:
					b.interactable = true;
					break;

				case SavingStatus.Download:
					b.interactable = false;
					break;

				case SavingStatus.Success:
					b.interactable = false;
					MoveToUploadOverTime();
					break;


				case SavingStatus.Syncing:
					b.interactable = false;
					break;
			}

			if (OnChangeStatus != null)
				OnChangeStatus.Invoke();
		}

		IEnumerator MoveToUploadOverTime()
		{
			yield return new WaitForSeconds(1);
			SetUpdateStatus(SavingStatus.Upload);
		}
	}

	/// <summary>
	/// The status of saving the file
	/// </summary>
	public enum SavingStatus
	{
		/// <summary>
		/// Cannot connect to the server/could not finish saving
		/// </summary>
		NotConnected,
		/// <summary>
		/// Save the file
		/// </summary>
		Upload,
		/// <summary>
		/// Saving animation
		/// </summary>
		Uploading,
		/// <summary>
		/// Save the file as...
		/// </summary>
		SaveAs,
		/// <summary>
		/// Get the most recent updates from the cloud
		/// </summary>
		Download,
		/// <summary>
		/// Download most recent files
		/// </summary>
		Downloading,
		/// <summary>
		/// Finished Saving
		/// </summary>
		Success,
		/// <summary>
		/// General syncing
		/// </summary>
		Syncing,

	}
}
