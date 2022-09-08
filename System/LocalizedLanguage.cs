using System;
using System.Collections.Generic;

using UnityEngine;

namespace WarManager
{
	[Serializable]
	public class LocalizedLanguage
	{
		[SerializeField] public string LanguageTag = "new_tag";
		[SerializeField] public List<string> TextTranslations = new List<string>();

		public LocalizedLanguage(List<string> text)
		{
			TextTranslations = text;
		}

		public string GetLocalizedLanguage()
		{
			if (TextTranslations.Count > 0 && TextTranslations.Count > (int)GeneralSettings.Account_Current_Language && TextTranslations[(int)GeneralSettings.Account_Current_Language] != string.Empty)
				return TextTranslations[(int)GeneralSettings.Account_Current_Language];
			else
				return LanguageTag;
		}
	}
}
