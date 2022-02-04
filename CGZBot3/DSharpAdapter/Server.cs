using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Server : IServer
	{
		private readonly DiscordGuild guild;
		private readonly Client client;

		public string Name => guild.Name;

		public string Id => guild.ToString();

		public IClient Client => client;


		public async Task<IMember> GetMemberAsync(string id)
		{
			return new Member(await guild.GetMemberAsync(ulong.Parse(id)), this);
		}

		public async Task<IReadOnlyCollection<IMember>> GetMembersAsync()
		{
			return (await guild.GetAllMembersAsync()).Select(s => new Member(s, this)).ToArray();
		}

		public Task<IReadOnlyCollection<IChannelCategory>> GetCategoriesAsync()
		{
			var ret = guild.Channels.Where(s => s.Value.IsCategory).Select(s => new ChannelCategory(s.Value, this)).ToList();
			ret.Add(new ChannelCategory(guild, this));
			return Task.FromResult((IReadOnlyCollection<IChannelCategory>)ret);
		}

		public Task<IChannelCategory> GetCategoryAsync(string id)
		{
			if (id.StartsWith("server "))
			{
				var guid = ulong.Parse(id["server ".Length..]);
				if (guild.Id != guid) throw new FormatException("Id is invalid. Global category for this server is another");
				return Task.FromResult((IChannelCategory)new ChannelCategory(guild, this));
			}
			else
			{
				return Task.FromResult((IChannelCategory)new ChannelCategory(guild.GetChannel(ulong.Parse(id)), this));
			}
		}

		public Task<IReadOnlyCollection<IChannel>> GetChannelsAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IChannel>)guild.Channels.Values.Select(s => Channel.Construct(s, this)).ToArray());
		}

		public Task<IChannel> GetChannelAsync(string id)
		{
			return Task.FromResult((IChannel)Channel.Construct(guild.GetChannel(ulong.Parse(id)), this));
		}

		public bool Equals(IServer? other) => other is not null && other.Id == Id;


		public Server(DiscordGuild guild, Client client)
		{
			this.guild = guild;
			this.client = client;
		}
	}
}
