﻿using DidiFrame.Lifetimes;
using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.UserCommands.Loader.Reflection;

namespace TestBot.Systems.Voice
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemNotifier, SystemCore>(services => services.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemCore, SystemCore>(services => services.GetRequiredService<SystemCore>());
			services.AddSingleton<ICommandsModule, CommandsHandler>();
			services.AddTransient<UIHelper>();

			//States
			services.AddTransient<IModelFactory<ICollection<CreatedVoiceChannel>>, DefaultCtorModelFactory<List<CreatedVoiceChannel>>>();

			//Lifetime
			services.AddLifetime<CreatedVoiceChannelLifetime, CreatedVoiceChannel>(StatesKeys.VoiceSystem);
		}
	}
}
