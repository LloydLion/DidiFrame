﻿using CGZBot3.Systems.Voice.Lifetime;
using CGZBot3.Systems.Voice.Settings;
using CGZBot3.Systems.Voice.States;
using CGZBot3.SystemsInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Voice
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemNotifier, SystemCore>(services => services.GetRequiredService<SystemCore>());
			services.AddSingleton<CommandsHandler>();
			services.AddTransient<UIHelper>();

			//Settings
			services.AddTransient<ISettingsRepository, SettingsRepository>();

			//States
			services.AddTransient<ICreatedVoiceChannelRepository, CreatedVoiceChannelRepository>();
			services.AddTransient<IModelFactory<ICollection<CreatedVoiceChannel>>, DefaultCtorModelFactory<List<CreatedVoiceChannel>>>();

			//Lifetime
			services.AddSingleton<ICreatedVoiceChannelLifetimeRegistry, CreatedVoiceChannelLifetimeRegistry>();
			services.AddTransient<ICreatedVoiceChannelLifetimeRepository, CreatedVoiceChannelLifetimeRepository>();
			services.AddTransient<ICreatedVoiceChannelLifetimeFactory, CreatedVoiceChannelLifetimeFactory>();
		}
	}
}
