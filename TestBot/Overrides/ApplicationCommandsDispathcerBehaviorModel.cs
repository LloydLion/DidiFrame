using DidiFrame.Client.DSharp.DiscordServer;
using DidiFrame.Clients.DSharp.ApplicationCommands;

namespace TestBot.Overrides
{
	internal class ApplicationCommandsDispathcerBehaviorModel : ApplicationCommandDispatcher.BehaviorModel
	{
		protected override object ConvertValueUp(ServerWrap server, object raw, UserCommandArgument.Type type)
		{
			switch (type)
			{
				case not UserCommandArgument.Type.DateTime and not UserCommandArgument.Type.TimeSpan:
					return base.ConvertValueUp(server, raw, type);
				case UserCommandArgument.Type.DateTime:
					try
					{
						return new DateTime(long.Parse((string)raw));
					}
					catch (FormatException)
					{
						throw new ApplicationCommandDispatcher.ArgumentConvertationException("InvalidDateCode");
					}
				case UserCommandArgument.Type.TimeSpan:
					try
					{
						return new TimeSpan(long.Parse((string)raw));
					}
					catch (FormatException)
					{
						throw new ApplicationCommandDispatcher.ArgumentConvertationException("InvalidDateCode");
					}
				default:
					throw new NotSupportedException();
			}
		}
	}
}
