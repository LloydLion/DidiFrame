using System.Collections.Generic;
using System.Linq;

namespace TestProject.Environment.Client
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

		public Member AddMember(User user, Permissions permissions)
		{
			var member = new Member(this, user, permissions);
			members.Add(member);
			return member;
		}

		public IReadOnlyCollection<IMember> GetMembers()
		{
			return members;
		}

		public IMember GetMember(ulong id)
		{
			return members.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IChannelCategory> GetCategories()
		{
			return cats;
		}

		public IChannelCategory GetCategory(ulong? id)
		{
			return cats.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IChannel> GetChannels()
		{
			return cats.SelectMany(s => s.Channels).ToArray();
		}

		public IChannel GetChannel(ulong id)
		{
			return cats.Single(s => s.Channels.Any(s => s.Id == id)).Channels.Single(s => s.Id == id);
		}

		public IReadOnlyCollection<IRole> GetRoles()
		{
			return roles;
		}

		public IRole GetRole(ulong id)
		{
			return roles.Single(s => s.Id == id);
		}
	}
}