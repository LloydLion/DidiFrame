﻿using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.DSharpAdapter
{
	internal class Role : IRole, IEquatable<Role?>
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

		public bool Equals(Role? other) => other is Role role && role?.Id == Id;

		public override bool Equals(object? obj) => obj is Role role && role?.Id == Id;

		public override int GetHashCode() => Id.GetHashCode();

		public static bool operator ==(Role? left, Role? right)
		{
			return EqualityComparer<Role>.Default.Equals(left, right);
		}

		public static bool operator !=(Role? left, Role? right)
		{
			return !(left == right);
		}
	}
}