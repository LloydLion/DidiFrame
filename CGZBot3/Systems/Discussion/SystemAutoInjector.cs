using CGZBot3.Data.Lifetime;
using CGZBot3.AutoInjecting;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Discussion
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
