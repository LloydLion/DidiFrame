﻿using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Reputation
{
	internal class SystemAutoInjector : IAutoSubInjector
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
