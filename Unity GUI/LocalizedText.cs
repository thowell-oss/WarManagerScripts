using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace WarManager.Unity3D
{
	[ExecuteAlways]
	/// <summary>
	/// Sets the text to the set language for the user and translates it upon request
	/// </summary>
	public class LocalizedText : MonoBehaviour
	{

		public bool Localized = true;
		TMP_Text text;

		public LocalizedLanguage Language;



		/// <summary>
		/// Get the text based on the correct language
		/// </summary>
		public void Refresh()
		{
			if (Localized)
			{
				text = GetComponent<TMP_Text>();
				text.text = Language.GetLocalizedLanguage();
			}
		}

		public void ChangeLanguage(Language l)
		{
			Refresh();
		}

		public void Awake()
		{
			Refresh();
		}

		public void OnEnable()
		{
			if(text.text != Language.GetLocalizedLanguage())
			{
				text.text = Language.GetLocalizedLanguage();
			}

			GeneralSettings.OnChangeLangauge += ChangeLanguage;
		}

		public void OnDisable()
		{
			GeneralSettings.OnChangeLangauge -= ChangeLanguage;
		}
	}
}
