using DidiFrame.ClientExtensions;

namespace TestBot.Systems.Test.ClientExtensions.ReactionExtension
{
	internal static class ExtensionMethods
	{
		public static int GetReactions(this IMessage message, string emoji)
		{
			return message.TextChannel.Server.Client.CreateExtension<IReactionsExtension>().GetReactionsCount(message, emoji);
		}
	}
}
