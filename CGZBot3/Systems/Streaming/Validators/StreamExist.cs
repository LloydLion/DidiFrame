using CGZBot3.UserCommands;
using CGZBot3.UserCommands.ArgumentsValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CGZBot3.Systems.Streaming.Validators
{
	internal class StreamExist : AbstractArgumentValidator<string>
	{
		private readonly bool inverse;


		public StreamExist(bool inverse)
		{
			this.inverse = inverse;
		}


		protected override string? Validate(UserCommandContext context, UserCommandInfo.Argument argument, string value)
		{
			var has = GetServiceProvider().GetRequiredService<ISystemCore>().HasStream(context.Invoker.Server, value);

			if (inverse) return has ? "StreamExist" : null;
			else return has ? null : "StreamNotFound";
		}
	}
}
