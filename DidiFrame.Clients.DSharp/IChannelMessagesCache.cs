using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp
{
	public interface IChannelMessagesCache
	{
		public void AddMessage(DiscordMessage msg);

		public void DeleteMessage(ulong msgId, DiscordChannel textChannel);

		public IReadOnlyList<DiscordMessage> GetMessages(DiscordChannel textChannel, int quantity);

		public bool HasMessage(ulong id, DiscordChannel textChannel);

		public DiscordMessage? GetNullableMessage(ulong id, DiscordChannel textChannel);

		public DiscordMessage GetMessage(ulong id, DiscordChannel textChannel);

		public Task InitChannelAsync(DiscordChannel textChannel);

		public void DeleteChannelCache(DiscordChannel textChannel);

		public object Lock(DiscordChannel textChannel);
	}
}
