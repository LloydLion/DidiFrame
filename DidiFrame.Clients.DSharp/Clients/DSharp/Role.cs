using DidiFrame.Entities;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidiFrame.Clients.DSharp
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IRole
	/// </summary>
	public class Role : IRole
	{
		private readonly DiscordRole role;
		private readonly Server server;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Role
		/// </summary>
		/// <param name="role">Base DiscordRole from DSharp</param>
		/// <param name="server">Base server object wrap</param>
		public Role(DiscordRole role, Server server)
		{
			this.role = role;
			this.server = server;
		}


		/// <inheritdoc/>
		public Permissions Permissions => role.Permissions.GetAbstract();

		/// <inheritdoc/>
		public string Name => role.Name;

		/// <inheritdoc/>
		public ulong Id => role.Id;

		/// <inheritdoc/>
		public IServer Server => server;

		/// <summary>
		/// Base discord role from DSharp
		/// </summary>
		public DiscordRole BaseRole => role;


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as Role);

		/// <inheritdoc/>
		public bool Equals(IRole? other) => other is Role role && role.Id == Id && role.Server == Server;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Role);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		/// <summary>
		/// Checks equality between two DidiFrame.Clients.DSharp.Role objects
		/// </summary>
		/// <param name="left">First object</param>
		/// <param name="right">Second objects</param>
		/// <returns>If objects are equal</returns>
		public static bool operator ==(Role? left, Role? right)
		{
			return EqualityComparer<Role>.Default.Equals(left, right);
		}

		/// <summary>
		/// Checks equality between two DidiFrame.Clients.DSharp.Role objects
		/// </summary>
		/// <param name="left">First object</param>
		/// <param name="right">Second objects</param>
		/// <returns>If objects are not equal</returns>
		public static bool operator !=(Role? left, Role? right)
		{
			return !(left == right);
		}
	}
}
