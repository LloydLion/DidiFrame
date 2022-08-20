namespace DidiFrame.ClientExtensions.Abstract
{
	public abstract class AbstractClientExtensionFactory<TExtension> : IClientExtensionFactory<TExtension> where TExtension : class
	{
		private readonly Dictionary<IClient, TExtension> instances = new();
		private readonly object syncRoot = new();


		protected AbstractClientExtensionFactory(Type targetClientType)
		{
			if (targetClientType.IsAssignableTo(typeof(IClient)) == false)
				throw new ArgumentException("Target type doesn't inherit IClient", nameof(targetClientType));

			if (targetClientType == typeof(IClient))
				throw new ArgumentException("Target type equals to IClient, enable to create for factory interface", nameof(targetClientType));

			if (targetClientType.IsInterface)
				throw new ArgumentException("Target type is interface, enable to create for factory interface", nameof(targetClientType));

			TargetClientType = targetClientType;
		}


		public Type TargetClientType { get; }


		public TExtension Create(IClient client, IClientExtensionContext<TExtension> extensionContext)
		{
			if (TargetClientType.IsInstanceOfType(client) == false)
				throw new InvalidOperationException($"Given client must be of type {TargetClientType.FullName}");

			lock (syncRoot)
			{
				if (instances.ContainsKey(client) == false)
					instances.Add(client, CreateInstance(client, extensionContext));

				return instances[client];
			}
		}

		public abstract TExtension CreateInstance(IClient client, IClientExtensionContext<TExtension> extensionContext);
	}
}
