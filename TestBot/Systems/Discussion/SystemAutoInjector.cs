using DidiFrame.Data.Lifetime;
using DidiFrame.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Discussion
{
	internal class SystemAutoInjector : IAutoSubinjector
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
