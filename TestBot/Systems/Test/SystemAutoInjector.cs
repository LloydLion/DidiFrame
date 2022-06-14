using DidiFrame.AutoInjecting;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Test
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<ICommandsModule, CommandsHandler>();
			services.AddSingleton<SystemCore>();
		}
	}
}
