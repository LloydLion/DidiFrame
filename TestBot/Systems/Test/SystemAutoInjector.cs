using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Test
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
