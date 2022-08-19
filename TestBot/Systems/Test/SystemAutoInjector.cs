using DidiFrame.AutoInjecting;
using DidiFrame.UserCommands.Loader.Reflection;
using Microsoft.Extensions.DependencyInjection;
using DidiFrame.ClientExtensions;
using TestBot.Systems.Test.ClientExtensions.ReactionExtension;
using TestBot.Systems.Test.ClientExtensions.NewsChannels;

namespace TestBot.Systems.Test
{
	internal class SystemAutoInjector : IAutoSubInjector
	{
		public void InjectDependencies(IServiceCollection services)
		{
			services.AddSingleton<ICommandsModule, CommandsHandler>();
			services.AddTransient<IReflectionCommandAdditionalInfoLoader, LazyAdditionalLoader>();
			services.AddClientExtension<IReactionsExtension, DSharpReactionsExtension>();
			services.AddServerExtension<INewsChannelExtension, DSharpNewsChannelExtension>();
			services.AddSingleton<SystemCore>();
		}
	}
}
