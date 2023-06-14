using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// Extensions for DidiFrame.DSharpAdapter namespace for service collection
	/// </summary>
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DSharp client as discord client
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="configuration">Configuration for client (DidiFrame.DSharpAdapter.Client.Options)</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddDSharpClient(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<DSharpClient.Options>(configuration);
			services.Configure<LoggerFilterOptions>(options => options.AddFilter("DSharpPlus.", LogLevel.Information));
			services.AddSingleton<IClient, DSharpClient>();
			return services;
		}
	}
}
