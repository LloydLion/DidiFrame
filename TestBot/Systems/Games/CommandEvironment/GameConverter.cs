using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Games.CommandEvironment
{
	internal class GameConverter : IUserCommandContextSubConverter
	{
		private readonly ISystemCore core;


		public Type WorkType => typeof(GameLifetime);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public GameConverter(ISystemCore core)
		{
			this.core = core;
		}


		public ConvertationResult Convert(UserCommandSendData sendData, IReadOnlyList<object> preObjects, IServiceProvider? locals = null)
		{
			var name = (string)preObjects[0];
			if (core.HasGame(sendData.Invoker, name)) return ConvertationResult.Success(core.GetGame(sendData.Invoker, name));
			else return ConvertationResult.Failture("NoGameExist", UserCommandCode.InvalidInput);
		}

		public BackConvertationResult ConvertBack(object convertationResult)
		{
			var gl = (GameLifetime)convertationResult;
			return new BackConvertationResult(new object[] { gl.GetName() }, gl.GetCreator());
		}

		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
		{
			return null;
		}
	}
}
