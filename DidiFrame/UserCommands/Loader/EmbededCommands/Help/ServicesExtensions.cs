using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	public static class ServicesExtensions
	{
		/// <summary>
		/// Adds help and cmd commands into repository from services
		/// </summary>
		/// <param name="services">Service collection</param>
		/// <returns>Given collection to be chained</returns>
		public static IServiceCollection AddHelpCommands(this IServiceCollection services)
		{
			services.AddTransient<IUserCommandsLoader, HelpCommandLoader>();
			services.AddTransient<IDefaultContextConveterSubConverter, CommandConverter>();
			return services;
		}
	}
}
