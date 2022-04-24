using DidiFrame.UserCommands;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Games.CommandEvironment
{
	internal class GameConverter : IDefaultContextConveterSubConverter
	{
		public Type WorkType => typeof(GameLifetime);

		public IReadOnlyList<UserCommandInfo.Argument.Type> PreObjectTypes => new[] { UserCommandInfo.Argument.Type.String };


		public object Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects)
		{
			return services.GetRequiredService<ISystemCore>().GetGame(preCtx.Invoker, (string)preObjects[0]);
		}
	}
}
