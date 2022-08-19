using System.Reflection;
using DidiFrame.Dependencies;

namespace DidiFrame.ClientExtensions.Reflection
{
	/// <summary>
	/// Reflection based implementation of DidiFrame.ClientExtensions.IClientExtensionFactory
	/// </summary>
	/// <typeparam name="TExtension">Type of extension interface</typeparam>
	/// <typeparam name="TImplementation">Type of extension implementation</typeparam>
	public class ReflectionClientExtensionFactory<TExtension, TImplementation> : IClientExtensionFactory<TExtension> where TExtension : class where TImplementation : TExtension
	{
		private readonly IServiceProvider services;
		private readonly Dictionary<IClient, TImplementation> instances = new();
		private readonly object syncRoot = new();


		public ReflectionClientExtensionFactory(IServiceProvider services)
		{
			var implementationType = typeof(TImplementation);
			var attr = implementationType.GetCustomAttribute<TargetExtensionTypeAttribute>();

			if (attr is null)
				throw new InvalidOperationException("Invalid implementation type! No TargetExtensionType attribute was found on type");

			var targetType = attr.TargetType;

			if (targetType.IsAssignableTo(typeof(IClient)) == false)
				throw new InvalidOperationException("Invalid implementation type! TargetExtensionType attribute contains invalid type that doesn't inherit IClient");

			if (targetType == typeof(IClient))
				throw new InvalidOperationException("Invalid implementation type! TargetExtensionType attribute contains invalid type that equals to IClient, enable to create for interface");

			if (targetType.IsInterface)
				throw new InvalidOperationException("Invalid implementation type! TargetExtensionType attribute contains invalid type that is interface, enable to create for interface");

			TargetClientType = targetType;
			this.services = services;
		}


		/// <inheritdoc/>
		public Type TargetClientType { get; }


		public TExtension Create(IClient client, IClientExtensionContext<TExtension> extensionContext)
		{
			if (TargetClientType.IsInstanceOfType(client) == false)
				throw new InvalidOperationException($"Given client must be of type {TargetClientType.FullName}");

			lock (syncRoot)
			{
				if (instances.ContainsKey(client) == false)
					instances.Add(client, services.ResolveObjectWithDependencies<TImplementation>(new object[] { client, extensionContext }));

				return instances[client];
			}
		}
	}
}
