using TestBot.Systems.Streaming.CommandEvironment;
using DidiFrame.AutoInjecting;
using DidiFrame.Data.Lifetime;
using DidiFrame.UserCommands;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Streaming
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<ISystemCore, SystemCore>(prov => prov.GetRequiredService<SystemCore>());
			services.AddSingleton<ISystemNotifier, SystemCore>(prov => prov.GetRequiredService<SystemCore>());
			services.AddTransient<UIHelper>();
			services.AddTransient<ICommandsHandler, CommandHandler>();
			services.AddTransient<IModelFactory<ICollection<StreamModel>>, DefaultCtorModelFactory<List<StreamModel>>>();
			services.AddLifetime<StreamLifetime, StreamModel>(StatesKeys.StreamingSystem);
			services.AddTransient<IDefaultContextConveterSubConverter, StreamConverter>();
		}
	}
}
