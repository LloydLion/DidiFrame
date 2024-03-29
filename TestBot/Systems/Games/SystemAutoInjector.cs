﻿using TestBot.Systems.Games.CommandEvironment;
using DidiFrame.AutoInjecting;
using DidiFrame.Lifetimes;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.UserCommands.PreProcessing;

namespace TestBot.Systems.Games
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddTransient<UIHelper>();
			services.AddSingleton<ISystemCore>(s => s.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemNotifier>(s => s.GetRequiredService<SystemCore>());
			services.AddTransient<ICommandsModule, CommandsHandler>();
			services.AddTransient<IModelFactory<ICollection<GameModel>>, DefaultCtorModelFactory<List<GameModel>>>();
			services.AddLifetime<GameLifetime, GameModel>(StatesKeys.GamesSystem);
			services.AddTransient<IContextSubConverterInstanceCreator, ReflectionContextSubConverterInstanceCreator<GameConverter>>();
		}
	}
}
