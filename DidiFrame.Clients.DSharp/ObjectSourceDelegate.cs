namespace DidiFrame.Clients.DSharp
{
	public delegate TSource? ObjectSourceDelegate<out TSource>() where TSource : notnull;
}
