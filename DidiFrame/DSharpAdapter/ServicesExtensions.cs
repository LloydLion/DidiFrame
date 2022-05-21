using DidiFrame.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.DSharpAdapter
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds DSharp client as discord client
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <param name="configuration">Configuration for client</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddDSharpClient(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<Client.Options>(configuration);
			services.AddSingleton(new LoggingFilterOption((category) => category.StartsWith("DSharpPlus.") ? LogLevel.Information : LogLevel.Trace));
			services.AddSingleton<IClient, Client>();
			return services;
		}
	}
}
