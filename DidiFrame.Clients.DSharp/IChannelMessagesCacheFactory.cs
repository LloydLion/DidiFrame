using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public interface IChannelMessagesCacheFactory
	{
		public IChannelMessagesCache Create(DiscordGuild server, Client client);
	}
}
