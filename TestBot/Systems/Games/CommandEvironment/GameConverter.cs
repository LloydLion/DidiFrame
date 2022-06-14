using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class GameConverter : IDefaultContextConveterSubConverter
	{
		public Type WorkType => typeof(GameLifetime);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public ConvertationResult Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects, IServiceProvider locals)
		{
			var sysCore = services.GetRequiredService<ISystemCore>();
			var name = (string)preObjects[0];
			if (sysCore.HasGame(preCtx.Invoker, name)) return ConvertationResult.Success(sysCore.GetGame(preCtx.Invoker, name));
			else return ConvertationResult.Failture("NoGameExist", UserCommandCode.InvalidInput);
		}
	}
}
