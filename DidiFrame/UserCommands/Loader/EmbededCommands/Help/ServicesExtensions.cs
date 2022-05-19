using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace DidiFrame.UserCommands.Loader.EmbededCommands.Help
{
	public static class ServicesExtensions
	{
		public static IServiceCollection AddHelpCommands(this IServiceCollection services)
		{
			services.AddTransient<IUserCommandsLoader, HelpCommandLoader>();
			services.AddTransient<IDefaultContextConveterSubConverter, CommandConverter>();
			return services;
		}
	}
}
