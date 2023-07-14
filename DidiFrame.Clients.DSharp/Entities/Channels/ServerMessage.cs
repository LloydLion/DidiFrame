using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities.Channels
{
	public record ServerMessage : Message, IServerMessage
	{
		public ServerMessage(MessageRepository repository, IDSharpTextChannelBase baseChannel, DiscordMessage discordMessage) : base(repository, baseChannel, discordMessage)
		{
			if (Author is not IMember)
				throw new ArgumentException("Author of given message is not member of server, enable to create server message");
		}


		public IDSharpTextChannelBase BaseChannel => (IDSharpTextChannelBase)Container;

		public ITextChannelBase Channel => BaseChannel;

		public new IMember Author => (IMember)base.Author;
	}
}
