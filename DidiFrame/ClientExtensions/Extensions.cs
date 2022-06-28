using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.ClientExtensions
{
	/// <summary>
	/// Extensions for client and server to extend thier functions
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Gets extensions for client
		/// </summary>
		/// <typeparam name="TExtension">Type of extension interface</typeparam>
		/// <param name="client">Client to extend</param>
		/// <returns>Some inplementation of TExtension that binded to given client</returns>
		/// <exception cref="NoExtensionFoundException">If no extension has been found for your client type ot interface type</exception>
		public static TExtension GetExtension<TExtension>(this IClient client) where TExtension : class
		{
			var implementations = client.Services.GetServices<IClientExtensionFactory<TExtension>>();

			foreach (var factory in implementations)
				if (factory.TargetClientType == client.GetType())
					return factory.Create(client.Services, client);

			throw new NoExtensionFoundException(typeof(TExtension), client.GetType());
		}

		/// <summary>
		/// Gets extensions for server
		/// </summary>
		/// <typeparam name="TExtension">Type of extension interface</typeparam>
		/// <param name="server">server to extend</param>
		/// <returns>Some inplementation of TExtension that binded to given server</returns>
		/// <exception cref="NoExtensionFoundException">If no extension has been found for your server type ot interface type</exception>
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
