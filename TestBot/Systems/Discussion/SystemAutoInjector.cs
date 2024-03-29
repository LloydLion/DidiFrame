﻿using DidiFrame.Lifetimes;
using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Discussion
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<CommandsHandler>();
			services.AddSingleton<SystemCore>();
			services.AddTransient<UIHelper>();
			services.AddLifetime<DiscussionChannelLifetime, DiscussionChannel>(StatesKeys.DiscussionSystem);
			services.AddTransient<IModelFactory<ICollection<DiscussionChannel>>, DefaultCtorModelFactory<List<DiscussionChannel>>>();
		}
	}
}
