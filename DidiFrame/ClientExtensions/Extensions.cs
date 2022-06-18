using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.ClientExtensions
{
	public static class Extensions
	{
		public static TExtension GetExtension<TExtension>(this IClient client) where TExtension : class
		{
			var implementations = client.Services.GetServices<IClientExtensionFactory<TExtension>>();

			foreach (var factory in implementations)
				if (factory.TargetClientType == client.GetType())
					return factory.Create(client.Services, client);

			throw new NoExtensionFoundException(typeof(TExtension), client.GetType());
		}

		public static TExtension GetExtension<TExtension>(this IServer server) where TExtension : class
		{
			var implementations = server.Client.Services.GetServices<IServerExtensionFactory<TExtension>>();

			foreach (var factory in implementations)
				if (factory.TargetServerType == server.GetType())
					return factory.Create(server.Client.Services, server);

			throw new NoExtensionFoundException(typeof(TExtension), server.GetType());
		}
	}
}
