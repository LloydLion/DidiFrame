using DidiFrame.Utils.RoutedEvents;
using System.Diagnostics.CodeAnalysis;

namespace DidiFrame.Clients
{
	/// <summary>
	/// Discord object contained by server
	/// </summary>
	public interface IServerObject : IDiscordObject
	{
		/// <summary>
		/// Public name of object
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Server containing object
		/// </summary>
		public IServer Server { get; }

		/// <summary>
		/// If object still presents in server structure
		/// </summary>
		public bool IsExists { get; }


		/// <summary>
		/// Renames object
		/// </summary>
		/// <param name="newName">New object's name</param>
		/// <returns>Wait task</returns>
		public ValueTask RenameAsync(string newName);

		/// <summary>
		/// Deletes object
		/// </summary>
		/// <returns>Wait task</returns>
		public ValueTask DeleteAsync();


		public static readonly RoutedEvent<ServerObjectEventArgs> ObjectCreated = new(typeof(IServerObject), nameof(ObjectCreated), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<ServerObjectEventArgs> ObjectModified = new(typeof(IServerObject), nameof(ObjectModified), RoutedEvent.PropagationDirection.Bubbling);

		public static readonly RoutedEvent<ServerObjectEventArgs> ObjectDeleted = new(typeof(IServerObject), nameof(ObjectDeleted), RoutedEvent.PropagationDirection.Bubbling);


		public class ServerObjectEventArgs : EventArgs
		{
			public IServerObject Object { get; }


			public ServerObjectEventArgs(IServerObject @object)
			{
				Object = @object;
			}


			public bool Is<TTarget>([NotNullWhen(true)] out TTarget? result) where TTarget : notnull, IServerObject
			{
				if (Object is TTarget tmp)
				{
					result = tmp;
					return true;
				}
				else
				{
					result = default;
					return false;
				}
			}
		}
	}
}
