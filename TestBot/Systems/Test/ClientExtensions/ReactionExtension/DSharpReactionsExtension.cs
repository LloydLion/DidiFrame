using DidiFrame.ClientExtensions;
using DidiFrame.ClientExtensions.Reflection;
using DSharpPlus.Entities;
using DSharpClient = DidiFrame.Client.DSharp.DSharpClient;
using DSharpMessage = DidiFrame.Client.DSharp.Entities.Message;

namespace TestBot.Systems.Test.ClientExtensions.ReactionExtension
{
	[TargetExtensionType(typeof(DSharpClient))]
	internal class DSharpReactionsExtension : IReactionsExtension
	{
		private readonly DSharpClient client;


		public DSharpReactionsExtension(DSharpClient client, IClientExtensionContext<IReactionsExtension> context)
		{
			this.client = client;
		}


		public int GetReactionsCount(IMessage message, string emoji)
		{
			return ((DSharpMessage)message).BaseMessage.GetReactionsAsync(DiscordEmoji.FromName(client.BaseClient, emoji)).Result.Count;
		}
	}
}
