using DidiFrame.UserCommands.PreProcessing;
using DidiFrame.Utils.Collections;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class StreamConverter : IUserCommandContextSubConverter
	{
		private ISystemCore core;


		public Type WorkType => typeof(StreamLifetime);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public StreamConverter(ISystemCore core)
		{
			this.core = core;
		}


		public ConvertationResult Convert(UserCommandSendData sendData, IReadOnlyList<object> preObjects, IServiceProvider? locals = null)
		{
			var name = (string)preObjects.Single();
			if (core.HasStream(sendData.Invoker.Server, name)) return ConvertationResult.Success(core.GetStream(sendData.Invoker.Server, name));
			else return ConvertationResult.Failture("StreamNotFound", UserCommandCode.InvalidInput);
		}

		public BackConvertationResult ConvertBack(object convertationResult)
		{
			var sl = (StreamLifetime)convertationResult;
			return new BackConvertationResult(sl.StoreSingle(), sl.GetOwner());
		}

		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
		{
			return null;
		}
	}
}
