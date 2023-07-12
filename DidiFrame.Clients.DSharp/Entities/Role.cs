using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Entities;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	public class Role : ServerObject<DiscordRole, Role.State>, IRole
	{
		private readonly RoleRepository repository;


		public Role(DSharpServer baseServer, ulong id, RoleRepository repository)
			: base(baseServer, id, new Configuration(nameof(Role), 10011 /*Unknown role*/))
		{
			this.repository = repository;
		}


		public Color Color => AccessState().Color;

		public int Position => AccessState().Position;

		public string? IconUrl => AccessState().IconUrl;

		public bool IsMentionable => AccessState().IsMentionable;

		public bool IsHoisted => AccessState().IsHoisted;


		public override ValueTask NotifyRepositoryThatDeletedAsync()
		{
			return new(repository.DeleteAsync(this));
		}

		protected override ValueTask CallDeleteOperationAsync()
		{
			return new(AccessEntity().DeleteAsync());
		}

		protected override ValueTask CallRenameOperationAsync(string newName)
		{
			return new(AccessEntity().ModifyAsync(newName));
		}

		protected override Mutation<State> CreateNameMutation(string newName)
		{
			return (state) => state with { Name = newName };
		}

		protected override Mutation<State> MutateWithNewObject(DiscordRole newDiscordObject)
		{
			return (_) => new State(
				newDiscordObject.Name,
				newDiscordObject.Color.GetAbstract(),
				newDiscordObject.Position,
				newDiscordObject.IconUrl,
				newDiscordObject.IsMentionable,
				newDiscordObject.IsHoisted
			);
		}

		public record struct State(string Name, Color Color, int Position, string? IconUrl, bool IsMentionable, bool IsHoisted) : IServerObjectState;
	}
}
