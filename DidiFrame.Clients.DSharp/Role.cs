using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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
		private DiscordRole role;
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
		public string Name
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(Name));
					return role.Name;
				}
			}
		}

		/// <inheritdoc/>
		public ulong Id => role.Id;

		/// <inheritdoc/>
		public IServer Server => server;

		/// <summary>
		/// Base discord role from DSharp
		/// </summary>
		public DiscordRole BaseRole
		{
			get
			{
				lock (this)
				{
					if (IsExist == false)
						throw new ObjectDoesNotExistException(nameof(BaseRole));
					return role;
				}
			}
		}

		/// <inheritdoc/>
		public bool IsExist => server.HasRole(this);


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as Role);

		/// <inheritdoc/>
		public bool Equals(IRole? other) => other is Role role && role.IsExist && IsExist && role.Id == Id && role.Server == Server;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Role);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		internal void ModifyInternal(DiscordRole role)
		{
			if (this.role.Id != role.Id)
				throw new InvalidOperationException("This operation can't modify id of model");

			lock (this)
			{
				this.role = role;
			}
		}
	}
}
