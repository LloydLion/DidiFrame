using CGZBot3.UserCommands;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming.CommandEvironment
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
