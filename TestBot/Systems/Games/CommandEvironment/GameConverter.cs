using DidiFrame.UserCommands;
using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class GameConverter : IDefaultContextConveterSubConverter
	{
		public Type WorkType => typeof(GameLifetime);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public object Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects)
		{
			return services.GetRequiredService<ISystemCore>().GetGame(preCtx.Invoker, (string)preObjects[0]);
		}
	}
}
