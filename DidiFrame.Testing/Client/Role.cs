using DidiFrame.Entities;
using DidiFrame.Clients;
using DidiFrame.Exceptions;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	/// <summary>
	/// Test IRole implmentation
	/// </summary>
	public class Role : IRole, IServerDeletable
	{
		private readonly Permissions permissions;
		private readonly string name;


		/// <summary>
		/// Creates new instance of DidiFrame.Testing.Client.Role
		/// </summary>
		/// <param name="permissions">Permissions for new role</param>
		/// <param name="name">Name for new role</param>
		/// <param name="server">Server for new role</param>
		public Role(Permissions permissions, string name, Server server)
		{
			this.permissions = permissions;
			this.name = name;
			Id = server.BaseClient.GenerateNextId();
			Server = server;
		}


		/// <inheritdoc/>
		public Permissions Permissions => GetIfExist(permissions);

		/// <inheritdoc/>
		public string Name => GetIfExist(name);

		/// <inheritdoc/>
		public ulong Id { get; }

		/// <inheritdoc/>
		public IServer Server { get; }

		/// <inheritdoc/>
		public bool IsExist { get; private set; } = true;

		/// <inheritdoc/>
		public string Mention => $"!<{Id}>";


		/// <inheritdoc/>
		public bool Equals(IServerEntity? other) => other is Role role && IsExist && role.IsExist && role.Id == Id;

		/// <inheritdoc/>
		public bool Equals(IRole? other) => Equals((IServerEntity?)other);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as IRole);

		/// <inheritdoc/>
		public override int GetHashCode() => Id.GetHashCode();

		void IServerDeletable.DeleteInternal() => IsExist = false;

		private TValue GetIfExist<TValue>(TValue value, [CallerMemberName] string nameOfCaller = "")
		{
			if (IsExist == false)
				throw new ObjectDoesNotExistException(nameOfCaller);
			else return value;
		}
	}
}