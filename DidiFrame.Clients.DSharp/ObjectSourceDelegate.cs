namespace DidiFrame.Client.DSharp
{
	/// <summary>
	/// Represents source of some object
	/// </summary>
	/// <typeparam name="TSource">Type of object</typeparam>
	/// <returns>Target object or null if it doesn't exist</returns>
	public delegate TSource? ObjectSourceDelegate<out TSource>() where TSource : notnull;
}
