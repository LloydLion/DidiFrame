using TestBot.Systems.Games.CommandEvironment;
using DidiFrame.AutoInjecting;
using DidiFrame.Data.Lifetime;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.UserCommands.PreProcessing;

namespace TestBot.Systems.Games
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddTransient<UIHelper>();
			services.AddSingleton<ISystemCore>(s => s.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemNotifier>(s => s.GetRequiredService<SystemCore>());
			services.AddTransient<ICommandsHandler, CommandsHandler>();
			services.AddTransient<IModelFactory<ICollection<GameModel>>, DefaultCtorModelFactory<List<GameModel>>>();
			services.AddLifetime<GameLifetime, GameModel>(StatesKeys.GamesSystem);
			services.AddTransient<IDefaultContextConveterSubConverter, GameConverter>();
		}
	}
}
