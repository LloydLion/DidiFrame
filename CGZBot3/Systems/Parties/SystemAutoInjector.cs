using CGZBot3.AutoInjecting;
using CGZBot3.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Parties
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemCore>(s => s.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemNotifier>(s => s.GetRequiredService<SystemCore>());
			services.AddSingleton<ICommandsHandler, CommandsHandler>();
			services.AddTransient<IModelFactory<ICollection<PartyModel>>, DefaultCtorModelFactory<List<PartyModel>>>();
			services.AddTransient<UIHelper>();
		}
	}
}
