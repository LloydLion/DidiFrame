using DidiFrame.Dependencies;
using DidiFrame.Utils.RoutedEvents;
using System.Reflection;

namespace DidiFrame.ClientExtensions.Reflection
{
	/// <summary>
	/// Reflection based implementation of DidiFrame.ClientExtensions.IServerExtensionFactory
	/// </summary>
	/// <typeparam name="TExtension">Type of extension interface</typeparam>
	/// <typeparam name="TImplementation">Type of extension implementation</typeparam>
	public class ReflectionServerExtensionFactory<TExtension, TImplementation> : IServerExtensionFactory<TExtension> where TExtension : class where TImplementation : TExtension
	{
		private readonly IServiceProvider services;
		private readonly Dictionary<IServer, TImplementation> instances = new();
		private readonly object syncRoot = new();


		/// <summary>
		/// Creates new instance of DidiFrame.ClientExtensions.Reflection.ReflectionServerExtensionFactory`2
		/// </summary>
		/// <param name="services">Services to create extension instance</param>
		/// <exception cref="InvalidOperationException">If TImplementation doesn't have valid TargetExtensionTypeAttribute</exception>
		public ReflectionServerExtensionFactory(IServiceProvider services)
		{
			var implementationType = typeof(TImplementation);
			var attr = implementationType.GetCustomAttribute<TargetExtensionTypeAttribute>();

			if (attr is null)
				throw new InvalidOperationException("Invalid implementation type! No TargetExtensionType attribute was found on type");

			var targetType = attr.TargetType;

			if (targetType.IsAssignableTo(typeof(IServer)) == false)
				throw new InvalidOperationException("Invalid implementation type! TargetExtensionType attribute contains invalid type that doesn't inherit IServer");

			if (targetType == typeof(IServer))
				throw new InvalidOperationException("Invalid implementation type! TargetExtensionType attribute contains IServer type, enable to create factory for interface");

			if (targetType.IsInterface)
				throw new InvalidOperationException("Invalid implementation type! TargetExtensionType attribute contains interface type, enable to create factory for interface");

			TargetServerType = targetType;
			this.services = services;
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
				if (server.Status.IsAfter(ServerStatus.Stopped))
					throw new InvalidServerStatusException("Enable to create extension for closed server",
						condition: new(ServerStatus.Stopped, InvalidServerStatusException.Direction.After, Inclusive: true), server);

				if (instances.ContainsKey(server) == false)
				{
					instances.Add(server, services.ResolveObjectWithDependencies<TImplementation>(new object[] { server, extensionContext }));

					server.AddListener(IServer.ServerRemovedPre, onServerRemoved);
				}

				return instances[server];
			}



			void onServerRemoved(RoutedEventSender sender, IServer.ServerEventArgs args)
			{
				lock (syncRoot)
				{
					instances.Remove(server);
					sender.UnSubscribeMyself();
				}
			}
		}
	}
}
