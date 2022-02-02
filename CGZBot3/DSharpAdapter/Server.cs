using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Server : IServer
	{
		private readonly DiscordGuild guild;
		private readonly DiscordClient client;


		public string Name => guild.Name;

		public string Id => guild.ToString();

		public IClient Client => new Client(client);


		public async Task<IMember> GetMemberAsync(string id)
		{
			return new Member(await guild.GetMemberAsync(ulong.Parse(id)), client);
		}

		public async Task<IReadOnlyCollection<IMember>> GetMembersAsync()
		{
			return (await guild.GetAllMembersAsync()).Select(s => new Member(s, client)).ToArray();
		}

		public Task<IReadOnlyCollection<IChannelCategory>> GetCategoriesAsync()
		{
			var ret = guild.Channels.Where(s => s.Value.IsCategory).Select(s => new ChannelCategory(s.Value, client)).ToList();
			ret.Add(new ChannelCategory(guild, client));
			return Task.FromResult((IReadOnlyCollection<IChannelCategory>)ret);
		}

		public Task<IChannelCategory> GetCategoryAsync(string id)
		{
			if (id.StartsWith("server "))
			{
				var guid = ulong.Parse(id["server ".Length..]);
				if (guild.Id != guid) throw new FormatException("Id is invalid. Global category for this server is another");
				return Task.FromResult((IChannelCategory)new ChannelCategory(guild, client));
			}
			else
			{
				return Task.FromResult((IChannelCategory)new ChannelCategory(guild.GetChannel(ulong.Parse(id)), client));
			}
		}

		public Task<IReadOnlyCollection<IChannel>> GetChannelsAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IChannel>)guild.Channels.Values.Select(s => Channel.Construct(s, client)).ToArray());
		}

		public Task<IChannel> GetChannelAsync(string id)
		{
			return Task.FromResult((IChannel)Channel.Construct(guild.GetChannel(ulong.Parse(id)), client));
		}

		public bool Equals(IServer? other) => other is not null && other.Id == Id;


		public Server(DiscordGuild guild, DiscordClient client)
		{
			this.guild = guild;
			this.client = client;
		}
	}
}
