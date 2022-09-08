/*IWarManagerFile.cs
 * Author: Taylor Howell
 */

namespace WarManager.Sharing
{
	/// <summary>
	/// Handles specific information tied to loading/unloading all files
	/// </summary>
	public interface IWarManagerFile
	{
		/// <summary>
		/// The GUID of the file
		/// </summary>
		string ID { get; }

		/// <summary>
		/// The name of the file
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The full file name (path) of the file
		/// </summary>
		string FileName { get; }
	}
}
