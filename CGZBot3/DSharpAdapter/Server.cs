using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Server : IServer
	{
		private readonly DiscordGuild guild;
		private readonly Client client;

		public string Name => guild.Name;

		public IClient Client => client;

		public Client SourceClient => client;

		public ulong Id => guild.Id;

		public async Task<IMember> GetMemberAsync(ulong id)
		{
			return new Member(await guild.GetMemberAsync(id), this);
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

		public Task<IChannelCategory> GetCategoryAsync(ulong? id)
		{
			if (id == null)
			{
				return Task.FromResult((IChannelCategory)new ChannelCategory(guild, this));
			}
			else
			{
				return Task.FromResult((IChannelCategory)new ChannelCategory(guild.GetChannel((ulong)id), this));
			}
		}

		public async Task<IReadOnlyCollection<IChannel>> GetChannelsAsync()
		{
			var channels = await guild.GetChannelsAsync();
			return channels.Select(s => Channel.Construct(s, this)).ToArray();
		}

		public async Task<IChannel> GetChannelAsync(ulong id)
		{
			var channels = await guild.GetChannelsAsync();
			var channel = channels.Single(s => s.Id == id);
			return Channel.Construct(channel, this);
		}

		public Task<IReadOnlyCollection<IRole>> GetRolesAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IRole> GetRoleAsync(ulong id)
		{
			return Task.FromResult((IRole)new Role(guild.GetRole(id), this));
		}

		public bool Equals(IServer? other) => other is Server server && server.Id == Id;


		public Server(DiscordGuild guild, Client client)
		{
			this.guild = guild;
			this.client = client;
		}
	}
}
