﻿using CGZBot3.Entities;
using CGZBot3.Interfaces;

namespace TestProject.TestAdapter
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
	}
}