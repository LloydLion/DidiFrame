using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Test
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<CommandsHandler>();
			services.AddSingleton<SystemCore>();
		}
	}
}
