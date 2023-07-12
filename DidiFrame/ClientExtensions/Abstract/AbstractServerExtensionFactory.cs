using DidiFrame.Utils.RoutedEvents;

namespace DidiFrame.ClientExtensions.Abstract
{
	/// <summary>
	/// Abstract class for server extension factories
	/// </summary>
	/// <typeparam name="TExtension">Type of extension interface</typeparam>
	public abstract class AbstractServerExtensionFactory<TExtension> : IServerExtensionFactory<TExtension> where TExtension : class
	{
		private readonly Dictionary<IServer, TExtension> instances = new();
		private readonly object syncRoot = new();


		/// <summary>
		/// Creates new instance of DidiFrame.ClientExtensions.Abstract.AbstractServerExtensionFactory`1
		/// </summary>
		/// <param name="targetServerType">Type of target server</param>
		/// <exception cref="ArgumentException">If given server type is invalid</exception>
		protected AbstractServerExtensionFactory(Type targetServerType)
		{
			if (targetServerType.IsAssignableTo(typeof(IServer)) == false)
				throw new ArgumentException("Target type doesn't inherit IServer", nameof(targetServerType));

			if (targetServerType == typeof(IServer))
				throw new ArgumentException("Target type equals to IServer, enable to create factory for interface", nameof(targetServerType));

			if (targetServerType.IsInterface)
				throw new ArgumentException("Target type is interface, enable to create factory for interface", nameof(targetServerType));

			TargetServerType = targetServerType;
		}


		/// <inheritdoc/>
		public Type TargetServerType { get; }


		/// <inheritdoc/>
		public TExtension Create(IServer server, IServerExtensionContext<TExtension> extensionContext)
		{
			if (TargetServerType.IsInstanceOfType(server) == false)
				throw new InvalidOperationException($"Given client must be of type {TargetServerType.FullName}");

			lock (syncRoot)
			{
				if (instances.ContainsKey(server) == false)
				{
					instances.Add(server, CreateInstance(server, extensionContext));

					server.AddListener(IServer.ServerRemovedPost, onServerRemoved);
				}

				return instances[server];
			}



			void onServerRemoved(RoutedEventSender sender, IServer.ServerEventArgs args)
			{
				lock (syncRoot)
				{
					instances.Remove(args.Server);
					sender.UnSubscribeMyself();
				}
			}
		}

		/// <summary>
		/// Creates new instance of extension for given server and context
		/// </summary>
		/// <param name="client">Target client of TargetClientType type</param>
		/// <param name="extensionContext">Extension context</param>
		/// <returns>New TExtension instance</returns>
		public abstract TExtension CreateInstance(IServer client, IServerExtensionContext<TExtension> extensionContext);
	}
}
