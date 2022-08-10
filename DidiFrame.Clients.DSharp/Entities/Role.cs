using DidiFrame.Entities;
using DidiFrame.Exceptions;
using DidiFrame.Interfaces;
using DSharpPlus.Entities;
using System.Runtime.CompilerServices;

namespace DidiFrame.Clients.DSharp.Entities
{
	/// <summary>
	/// DSharp implementation of DidiFrame.Interfaces.IRole
	/// </summary>
	public class Role : IRole
	{
		private readonly ObjectSourceDelegate<DiscordRole> role;
		private readonly Server server;


		/// <summary>
		/// Creates new instance of DidiFrame.Clients.DSharp.Role
		/// </summary>
		/// <param name="id">Id of role</param>
		/// <param name="role">Base DiscordRole from DSharp source</param>
		/// <param name="server">Base server object wrap</param>
		public Role(ulong id, ObjectSourceDelegate<DiscordRole> role, Server server)
		{
			Id = id;
			this.role = role;
			this.server = server;
		}


		/// <inheritdoc/>
		public Permissions Permissions => AccessBase().Permissions.GetAbstract();

		/// <inheritdoc/>
		public string Name => AccessBase().Name;

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public IServer Server => server;

		/// <summary>
		/// Base discord role from DSharp
		/// </summary>
		public DiscordRole BaseRole => AccessBase();

		/// <inheritdoc/>
		public bool IsExist => role() is not null;

		/// <inheritdoc/>
		public string Mention => AccessBase().Mention;


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => Equals(other as Role);

		/// <inheritdoc/>
		public bool Equals(IRole? other) => other is Role role && role.IsExist && IsExist && role.Id == Id && role.Server == Server;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Role);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		private DiscordRole AccessBase([CallerMemberName] string nameOfCaller = "")
		{
			var obj = role();
			if (obj is null)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return obj;
		}
	}
}
