using DidiFrame.UserCommands;
using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class StreamConverter : IDefaultContextConveterSubConverter
	{
		public Type WorkType => typeof(StreamLifetime);

		public IReadOnlyList<UserCommandInfo.Argument.Type> PreObjectTypes => new[] { UserCommandInfo.Argument.Type.String };


		public object Convert(IServiceProvider services, UserCommandPreContext preCtx, IReadOnlyList<object> preObjects)
		{
			return services.GetRequiredService<ISystemCore>().GetStream(preCtx.Invoker.Server, (string)preObjects[0]);
		}
	}
}
