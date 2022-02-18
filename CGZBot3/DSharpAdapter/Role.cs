﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.DSharpAdapter
{
	internal class Role : IRole
	{
		private readonly DiscordRole role;
		private readonly Server server;


		public Role(DiscordRole role, Server server)
		{
			this.role = role;
			this.server = server;
		}


		public Permissions Permissions => role.Permissions.GetAbstract();

		public string Name => role.Name;

		public ulong Id => role.Id;

		public IServer Server => server;

		public DiscordRole BaseRole => role;


		public bool Equals(IServerEntity? other) => other is Role role && role?.Id == Id;
	}
}