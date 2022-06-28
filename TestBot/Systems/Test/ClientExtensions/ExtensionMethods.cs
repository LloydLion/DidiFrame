using DidiFrame.ClientExtensions;

namespace TestBot.Systems.Test.ClientExtensions
{
	internal static class ExtensionMethods
	{
		public static int GetReactions(this IMessage message, string emoji)
		{
			return message.TextChannel.Server.Client.GetExtension<IReactionsExtension>().GetReactionsCount(message, emoji);
		}
	}
}
