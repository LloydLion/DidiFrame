﻿using DidiFrame.AutoInjecting;
using DidiFrame.Lifetimes;
using DidiFrame.UserCommands.Loader.Reflection;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;
using TestBot.Systems.Streaming.CommandEvironment;

namespace TestBot.Systems.Streaming
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemCore, SystemCore>(prov => prov.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemNotifier, SystemCore>(prov => prov.GetRequiredService<SystemCore>());
			services.AddTransient<UIHelper>();
			services.AddTransient<ICommandsModule, CommandHandler>();
			services.AddTransient<IModelFactory<ICollection<StreamModel>>, DefaultCtorModelFactory<List<StreamModel>>>();
			services.AddLifetime<StreamLifetime, StreamModel>(StatesKeys.StreamingSystem);
			services.AddTransient<IContextSubConverterInstanceCreator, ReflectionContextSubConverterInstanceCreator<StreamConverter>>();
		}
	}
}
