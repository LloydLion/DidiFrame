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
			services.AddTransient<SystemCore>();
			services.AddTransient<CommandsHandler>();

			//Settings
			services.AddTransient<ISettingsRepository, SettingsRepository>();
			services.AddTransient<IModelConverter<VoiceSettingsPM, VoiceSettings>, SettingsConverter>();

			//States
			services.AddTransient<ICreatedVoiceChannelRepository, CreatedVoiceChannelRepository>();
			services.AddTransient<IModelFactory<ICollection<CreatedVoiceChannelPM>>, DefaultFactory>();
			services.AddTransient<IModelConverter<CreatedVoiceChannelPM, CreatedVoiceChannel>, ModelConverter>();

			//Core
			services.AddSingleton<ICreatedVoiceChannelLifetimeRepository, CreatedVoiceChannelLifetimeRepository>();
			services.AddTransient<UIHelper>();
		}
	}
}
