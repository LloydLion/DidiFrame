using DidiFrame.UserCommands;
using DidiFrame.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming.CommandEvironment
{
	internal class StreamExist : IUserCommandArgumentPreValidator
	{
		private readonly bool inverse;


		public StreamExist(bool inverse)
		{
			this.inverse = inverse;
		}


		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandInfo.Argument argument, IReadOnlyList<object> values)
		{
			var has = services.GetRequiredService<ISystemCore>().HasStream(context.Invoker.Server, (string)values[0]);

			if (inverse) return has ? "StreamExist" : null;
			else return has ? null : "StreamNotFound";
		}
	}
}
