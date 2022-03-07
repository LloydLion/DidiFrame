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
			services.AddSingleton<IReputationDispatcher, ReputationDispatcher>();
			services.AddTransient<UIHelper>();

			//States
			services.AddTransient<IModelFactory<ICollection<MemberReputation>>, DefaultCtorModelFactory<List<MemberReputation>>>();
			services.AddTransient<IMembersReputationRepository, MembersReputationRepository>();
		}
	}
}
