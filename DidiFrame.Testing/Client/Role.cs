using DidiFrame.Entities;
using DidiFrame.Clients;
using DidiFrame.Exceptions;
using System.Runtime.CompilerServices;

namespace DidiFrame.Testing.Client
{
	public class Role : IRole, IServerDeletable
	{
		private readonly Permissions permissions;
		private readonly string name;


		public Role(Permissions permissions, string name, Server server)
		{
			this.permissions = permissions;
			this.name = name;
			Id = server.BaseClient.GenerateNextId();
			Server = server;
		}


		public Permissions Permissions => GetIfExist(permissions);

		public string Name => GetIfExist(name);

		public ulong Id { get; }

		public IServer Server { get; }

		public bool IsExist { get; private set; } = true;

		public string Mention => $"!<{Id}>";


		public bool Equals(IServerEntity? other) => other is Role role && IsExist && role.IsExist && role.Id == Id;

		public bool Equals(IRole? other) => Equals((IServerEntity?)other);

		public override bool Equals(object? obj) => Equals(obj as IRole);

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