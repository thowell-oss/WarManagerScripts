/* ICardDisplayable.cs
 * Author: Taylor Howell
 */
using System;
using System.Drawing;

namespace WarManager.Cards
{
	/// <summary>
	/// Handle generic display of information on a card
	/// </summary>
	/// <typeparam name="TdataType">The type of data to display</typeparam>
	public interface ICardDisplayable : IEquatable<string>
	{
		/// <summary>
		/// The ID of the card information
		/// </summary>
		public string ID { get;}

		/// <summary>
		/// The local location of the data on the card
		/// </summary>
		public (float, float) DisplayLocation { get; set; }

		/// <summary>
		/// The scale of the IDisplayElement
		/// </summary>
		public float Scale { get; set; }

		/// <summary>
		/// Can the object display the data?
		/// </summary>
		public bool CanDisplayData { get; set; }

		/// <summary>
		/// The type of data the object will display
		/// </summary>
		public CardDisplayType DisplayType { get;}

		/// <summary>
		/// The object class of the interface (you will need cast the object)
		/// </summary>
		/// <returns>returns the object</returns>
		public object GetDetails();
	}
}
