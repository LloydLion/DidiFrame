namespace DidiFrame.ClientExtensions.Abstract
{
	public abstract class AbstractServerExtensionFactory<TExtension> : IServerExtensionFactory<TExtension> where TExtension : class
	{
		private readonly Dictionary<IServer, TExtension> instances = new();
		private readonly object syncRoot = new();


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


		public Type TargetServerType { get; }


		public TExtension Create(IServer server, IServerExtensionContext<TExtension> extensionContext)
		{
			if (TargetServerType.IsInstanceOfType(server) == false)
				throw new InvalidOperationException($"Given client must be of type {TargetServerType.FullName}");

			lock (syncRoot)
			{
				if (instances.ContainsKey(server) == false)
				{
					instances.Add(server, CreateInstance(server, extensionContext));

					server.Client.ServerRemoved += onServerRemoved;


					void onServerRemoved(IServer removedServer)
					{
						if (server.Equals(removedServer))
						{
							lock (syncRoot)
							{
								instances.Remove(server);
								server.Client.ServerRemoved -= onServerRemoved;
							}
						}
					}
				}

				return instances[server];
			}
		}

		public abstract TExtension CreateInstance(IServer client, IServerExtensionContext<TExtension> extensionContext);
	}
}
