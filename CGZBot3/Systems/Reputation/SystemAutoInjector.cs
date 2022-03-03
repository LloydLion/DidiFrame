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
			services.AddTransient<IModelConverter<ReputationSettingsPM, ReputationSettings>, SettingsConverter>();
			services.AddTransient<ISettingsRepository, SettingsRepository>();

			//States
			services.AddTransient<IModelFactory<ICollection<MemberReputationPM>>, DefaultFactory>();
			services.AddTransient<IMembersReputationRepository, MembersReputationRepository>();
			services.AddTransient<IModelConverter<MemberReputationPM, MemberReputation>, ModelConverter>();
		}
	}
}
