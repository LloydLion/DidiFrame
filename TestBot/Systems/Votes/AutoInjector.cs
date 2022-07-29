using DidiFrame.AutoInjecting;
using DidiFrame.Lifetimes;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Votes
{
	internal class AutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddTransient<ICommandsModule, CommandHandler>();
			services.AddTransient<SystemCore>();
			services.AddLifetime<VoteLifetime, VoteModel>("vote");
			services.AddTransient<IModelFactory<ICollection<VoteModel>>, DefaultCtorModelFactory<List<VoteModel>>>();
		}
	}
}
