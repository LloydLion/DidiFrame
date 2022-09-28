namespace DidiFrame.ClientExtensions.Abstract
{
	/// <summary>
	/// Abstract class for client extension factories
	/// </summary>
	/// <typeparam name="TExtension">Type of extension interface</typeparam>
	public abstract class AbstractClientExtensionFactory<TExtension> : IClientExtensionFactory<TExtension> where TExtension : class
	{
		private readonly Dictionary<IClient, TExtension> instances = new();
		private readonly object syncRoot = new();


		/// <summary>
		/// Creates new instance of DidiFrame.ClientExtensions.Abstract.AbstractClientExtensionFactory`1
		/// </summary>
		/// <param name="targetClientType">Type of target client</param>
		/// <exception cref="ArgumentException">If given client type is invalid</exception>
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


		/// <inheritdoc/>
		public Type TargetClientType { get; }


		/// <inheritdoc/>
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

		/// <summary>
		/// Creates new instance of extension for given client and context
		/// </summary>
		/// <param name="client">Target client of TargetClientType type</param>
		/// <param name="extensionContext">Extension context</param>
		/// <returns>New TExtension instance</returns>
		protected abstract TExtension CreateInstance(IClient client, IClientExtensionContext<TExtension> extensionContext);
	}
}
