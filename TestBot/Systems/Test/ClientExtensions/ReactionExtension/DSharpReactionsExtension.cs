using DidiFrame.Clients.DSharp;
using DidiFrame.ClientExtensions;
using DidiFrame.ClientExtensions.Abstract;
using DidiFrame.ClientExtensions.Reflection;
using DSharpPlus.Entities;
using DSharpMessage = DidiFrame.Clients.DSharp.Entities.Message;

namespace TestBot.Systems.Test.ClientExtensions.ReactionExtension
{
	internal class DSharpReactionsExtension : IReactionsExtension
	{
		private readonly DSharpClient client;


		public DSharpReactionsExtension(DSharpClient client)
		{
			this.client = client;
		}


		public int GetReactionsCount(IMessage message, string emoji)
		{
			return ((DSharpMessage)message).BaseMessage.GetReactionsAsync(DiscordEmoji.FromName(client.BaseClient, emoji)).Result.Count;
		}


		public class Factory : AbstractClientExtensionFactory<IReactionsExtension>
		{
			public Factory() : base(typeof(DSharpClient)) { }


			public override IReactionsExtension CreateInstance(IClient client, IClientExtensionContext<IReactionsExtension> extensionContext) =>
				new DSharpReactionsExtension((DSharpClient)client);
		}
	}
}
