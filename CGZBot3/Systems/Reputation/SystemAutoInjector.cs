using CGZBot3.Systems.Reputation.Settings;
using CGZBot3.Systems.Reputation.States;
using CGZBot3.SystemsInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Reputation
{
	internal class SystemAutoInjector : IAutoSubinjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<SystemCore>();
			services.AddSingleton<CommandsHandler>();
			services.AddTransient<UIHelper>();
			services.AddSingleton<IReputationDispatcher, ReputationDispatcher>();

			//Settings
			services.AddTransient<ISettingsRepository, SettingsRepository>();

			//States
			services.AddTransient<IModelFactory<ICollection<MemberReputation>>, DefaultCtorModelFactory<List<MemberReputation>>>();
			services.AddTransient<IMembersReputationRepository, MembersReputationRepository>();
		}
	}
}
