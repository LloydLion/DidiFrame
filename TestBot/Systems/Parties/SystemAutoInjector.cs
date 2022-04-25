using TestBot.Systems.Parties.CommandEvironment;
using DidiFrame.AutoInjecting;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Parties
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
			services.AddTransient<IDefaultContextConveterSubConverter, PartyConverter>();
		}
	}
}
