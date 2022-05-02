using Microsoft.Extensions.DependencyInjection;

namespace TestBot.Systems.Streaming.CommandEvironment
{
	internal class StreamExist : IUserCommandArgumentPreValidator
	{
		private readonly bool inverse;


		public StreamExist(bool inverse)
		{
			this.inverse = inverse;
		}


		public string? Validate(IServiceProvider services, UserCommandPreContext context, UserCommandArgument argument, IReadOnlyList<object> values)
		{
			var has = services.GetRequiredService<ISystemCore>().HasStream(context.Invoker.Server, (string)values[0]);

			if (inverse) return has ? "StreamExist" : null;
			else return has ? null : "StreamNotFound";
		}
	}
}
