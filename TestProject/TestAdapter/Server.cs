using CGZBot3.Entities;
using CGZBot3.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.TestAdapter
{
	internal class Server : IServer
	{
		private readonly List<Role> roles = new();
		private readonly List<Member> members = new();
		private readonly List<ChannelCategory> cats = new();


		public IClient Client => BaseClient;

		public Client BaseClient { get; }

		public string Name { get; }

		public ulong Id { get; }


		public ICollection<Role> Roles => roles;

		public IReadOnlyCollection<Member> Members => members;

		public ICollection<ChannelCategory> Categories => cats;


		public Server(Client client, string name)
		{
			BaseClient = client;
			Name = name;
			AddMember(client.BaseSelfAccount, Permissions.All);
			Categories.Add(new ChannelCategory(null, this));
			Id = client.GenerateId();
		}


		public bool Equals(IServer? other) => other is Server server && server.Id == Id;

		public Task<IReadOnlyCollection<IChannelCategory>> GetCategoriesAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IChannelCategory>)cats);
		}

		public Task<IChannelCategory> GetCategoryAsync(ulong? id)
		{
			return Task.FromResult((IChannelCategory)cats.Single(s => s.Id == id));
		}

		public Task<IChannel> GetChannelAsync(ulong id)
		{
			foreach (var cat in cats)
			{
				var res = cat.Channels.SingleOrDefault(s => s.Id == id);
				if (res is not null) return Task.FromResult(res);
			}

			throw new KeyNotFoundException(nameof(id));
		}

		public Task<IReadOnlyCollection<IChannel>> GetChannelsAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IChannel>)cats.SelectMany(s => s.Channels).ToArray());
		}

		public Task<IMember> GetMemberAsync(ulong id)
		{
			return Task.FromResult((IMember)members.Single(s => s.Id == id));
		}

		public Task<IReadOnlyCollection<IMember>> GetMembersAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IMember>)members);
		}

		public Task<IRole> GetRoleAsync(ulong id)
		{
			return Task.FromResult((IRole)roles.Single(s => s.Id == Id));
		}

		public Task<IReadOnlyCollection<IRole>> GetRolesAsync()
		{
			return Task.FromResult((IReadOnlyCollection<IRole>)roles);
		}

		public Member AddMember(User user, Permissions permissions)
		{
			var member = new Member(this, user, permissions);
			members.Add(member);
			return member;
		}
	}
}