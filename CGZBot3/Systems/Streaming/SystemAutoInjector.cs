using CGZBot3.AutoInjecting;
using CGZBot3.Data.Lifetime;
using CGZBot3.Systems.Streaming.CommandEvironment;
using CGZBot3.UserCommands;
using CGZBot3.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming
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
