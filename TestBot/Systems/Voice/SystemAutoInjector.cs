using DidiFrame.Data.Lifetime;
using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Voice
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemNotifier, SystemCore>(services => services.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemCore, SystemCore>(services => services.GetRequiredService<SystemCore>());
			services.AddSingleton<CommandsHandler>();
			services.AddTransient<UIHelper>();

			//States
			services.AddTransient<IModelFactory<ICollection<CreatedVoiceChannel>>, DefaultCtorModelFactory<List<CreatedVoiceChannel>>>();

			//Lifetime
			services.AddLifetime<CreatedVoiceChannelLifetime, CreatedVoiceChannel>(StatesKeys.VoiceSystem);
		}
	}
}
