using DidiFrame.Entities;
using DidiFrame.Client;

namespace TestProject.Environment.Client
{
	internal class Role : IRole
	{
		public Role(Permissions permissions, string name, Server server)
		{
			Permissions = permissions;
			Name = name;
			Id = server.BaseClient.GenerateId();
			Server = server;
		}


		public Permissions Permissions { get; }

		public string Name { get; }

		public ulong Id { get; }

		public IServer Server { get; }


		public bool Equals(IServerEntity? other) => other is Role role && role.Id == Id;

		public bool Equals(IRole? other) => Equals((IServerEntity?)other);
	}
}