using DidiFrame.UserCommands.PreProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class StreamConverter : IUserCommandContextSubConverter
	{
		public Type WorkType => typeof(StreamLifetime);

		public IReadOnlyList<UserCommandArgument.Type> PreObjectTypes => new[] { UserCommandArgument.Type.String };


		public ConvertationResult Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects, IServiceProvider locals)
		{
			var sysCore = services.GetRequiredService<ISystemCore>();
			var name = (string)preObjects[0];
			if (sysCore.HasStream(preCtx.Invoker.Server, name)) return ConvertationResult.Success(sysCore.GetStream(preCtx.Invoker.Server, name));
			else return ConvertationResult.Failture("StreamNotFound", UserCommandCode.InvalidInput);
		}

		public IReadOnlyList<object> ConvertBack(IServiceProvider services, object convertationResult)
		{
			var sl = (StreamLifetime)convertationResult;
			return new object[] { sl };
		}

		public IUserCommandArgumentValuesProvider? CreatePossibleValuesProvider()
		{
			return null;
		}
	}
}
