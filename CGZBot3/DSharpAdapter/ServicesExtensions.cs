using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.DSharpAdapter
{
	internal static class ServicesExtensions
	{
		public static IServiceCollection AddDSharpClient(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<Client.Options>(configuration);
			services.AddSingleton<IClient, Client>();
			return services;
		}
	}
}
