using DSharpPlus.Entities;

namespace DidiFrame.Client.DSharp.DiscordServer
{
	/// <summary>
	/// Factory for DidiFrame.Clients.DSharp.IChannelMessagesCache implemetations
	/// </summary>
	public interface IChannelMessagesCacheFactory
	{
		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.IChannelMessagesCache implemetation
		/// </summary>
		/// <param name="server">Base server where need to create instance</param>
		/// <param name="client">Global dsharp client</param>
		/// <returns>Ready to use cache</returns>
		public IChannelMessagesCache Create(DiscordGuild server, Client client);
	}
}
