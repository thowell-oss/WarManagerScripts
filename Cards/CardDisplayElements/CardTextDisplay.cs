/* CardTextDisplay.cs
 * Author: Taylor Howell
 */

using System;
using System.Collections;
using System.Collections.Generic;

using WarManager;

namespace WarManager.Cards
{
	/// <summary>
	/// Display text
	/// </summary>
	public class CardTextDisplay : ICardDisplayable
	{
		public string ID { get; private set; }
		public (float, float) DisplayLocation { get; set; } = (.54f, .2f);
		public float Scale { get; set; } = 1;
		public bool CanDisplayData { get; set; }
		public CardDisplayType DisplayType
		{
			get
			{
				return CardDisplayType.stringDisplay;
			}
		}

		/// <summary>
		/// The text to display
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// The Font size of the text
		/// </summary>
		public int FontSize { get; set; } = 12;

		/// <summary>
		/// The font of the text
		/// </summary>
		public string Font { get; set; } = "Roboto";

		/// <summary>
		/// The color tuple (float, float, float, float) in RGBA format
		/// </summary>
		public (float, float, float, float) ColorRGBA { get; set; } = (5, 5, 5, 255);

		/// <summary>
		/// Text bold style
		/// </summary>
		public bool Bold { get; set; }

		/// <summary>
		/// Text italics style
		/// </summary>
		public bool Italic { get; set; }

		/// <summary>
		/// Text Underline style
		/// </summary>
		public bool Underline { get; set; }

		public TextAlignment Alignment { get; set; } = TextAlignment.LeftJustified;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="id">the guid of the element</param>
		public CardTextDisplay(string id)
		{
			ID = id;
		}

		public bool Equals(string other)
		{
			if (ID == other)
				return true;

			return false;
		}

		public object GetDetails()
		{
			return this;
		}

		/// <summary>
		/// Get the normalized rgba (values from 0-1 not 0 - 255).
		/// </summary>
		/// <returns>returns a tuple (float, float, float, float) of the RGBA values</returns>
		public (float, float, float, float) GetNormalizedRGBA()
		{
			return (ColorRGBA.Item1 / 255, ColorRGBA.Item2 / 255, ColorRGBA.Item3 / 255, ColorRGBA.Item4 / 255);
		}
	}
}
