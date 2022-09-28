using TestBot.Systems.Parties.CommandEvironment;
using DidiFrame.AutoInjecting;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.UserCommands.PreProcessing;

namespace TestBot.Systems.Parties
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemCore>(s => s.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemNotifier>(s => s.GetRequiredService<SystemCore>());
			services.AddSingleton<ICommandsModule, CommandsHandler>();
			services.AddTransient<IModelFactory<ICollection<PartyModel>>, DefaultCtorModelFactory<List<PartyModel>>>();
			services.AddTransient<UIHelper>();
			services.AddTransient<IContextSubConverterInstanceCreator, ReflectionContextSubConverterInstanceCreator<PartyConverter>>();
		}
	}
}
