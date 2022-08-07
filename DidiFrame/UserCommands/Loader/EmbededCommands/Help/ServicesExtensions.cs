using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	/// <summary>
	/// Extensions for DidiFrame.UserCommands.Loader.EmbededCommands.Help namespace for service collection
	/// </summary>
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
			services.AddTransient<IContextSubConverterInstanceCreator, CommandConverter.Creator>();
			return services;
		}
	}
}
