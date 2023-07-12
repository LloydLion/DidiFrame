using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
using DidiFrame.Clients.DSharp.Utils;
using DidiFrame.Utils;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	public class Member : ServerObject<DiscordMember, Member.State>, IMember
	{
		private readonly MemberRepository repository;


		public Member(DSharpServer baseServer, ulong id, MemberRepository repository)
			: base(baseServer, id, new(nameof(Member), 10007 /*Unknown member*/))
		{
			this.repository = repository;
		}


		public string Nickname => AccessState().Nickname;

		public string UserName => AccessState().UserName;

		public string AvatarURL => AccessState().AvatarURL;

		public string Mention => AccessEntity().Mention; //Can't be changed

		public bool IsBot => AccessState().IsBot;


		public IReadOnlyList<IRole> ListRoles() => AccessState().Roles;

		public ValueTask GrantRoleAsync(IRole role)
		{
			return new(DoDiscordOperation(async () =>
			{
				await AccessEntity().GrantRoleAsync(((Role)role).AccessEntity());
				return Unit.Default;
			},

			async (_) =>
			{
				await MutateStateAsync((state) =>
				{
					if (state.Roles.Contains(role))
						return state;

					var newRoles = state.Roles.Append(role).OrderByDescending(s => s.Position).Select(s => s.Id).ToArray();

					return state with { Roles = new ReferenceEntityCollection<IRole>(newRoles, repository.RoleRepository) };
				});
			}));
		}

		public ValueTask RevokeRoleAsync(IRole role)
		{
			return new(DoDiscordOperation(async () =>
			{
				await AccessEntity().RevokeRoleAsync(((Role)role).AccessEntity());
				return Unit.Default;
			},

			async (_) =>
			{
				await MutateStateAsync((state) =>
				{
					if (state.Roles.Contains(role) == false)
						return state;

					var newRoles = state.Roles.Where(s => Equals(s, role) == false).OrderByDescending(s => s.Position).Select(s => s.Id).ToArray();

					return state with { Roles = new ReferenceEntityCollection<IRole>(newRoles, repository.RoleRepository) };
				});
			}));
		}

		public override ValueTask NotifyRepositoryThatDeletedAsync()
		{
			return new(repository.DeleteAsync(this));
		}

		protected override ValueTask CallDeleteOperationAsync()
		{
			return new(AccessEntity().RemoveAsync());
		}

		protected override ValueTask CallRenameOperationAsync(string newName)
		{
			return new(AccessEntity().ModifyAsync(s => s.Nickname = UserName == newName ? null : newName));
		}

		protected override Mutation<State> CreateNameMutation(string newName)
		{
			return (state) => state with { Nickname = newName };
		}

		protected override Mutation<State> MutateWithNewObject(DiscordMember newDiscordObject)
		{
			return (state) =>
			{
				var roles = newDiscordObject.Roles.Select(s => s.Id).Append(repository.RoleRepository.EveryoneRole.Id).ToArray();

				return new State(
					newDiscordObject.Nickname ?? newDiscordObject.Username,
					newDiscordObject.Username,
					newDiscordObject.AvatarUrl,
					newDiscordObject.IsBot,
					new ReferenceEntityCollection<IRole>(roles, repository.RoleRepository)
				);
			};
		}

		public record struct State(string Nickname, string UserName, string AvatarURL, bool IsBot, ReferenceEntityCollection<IRole> Roles) : IServerObjectState
		{
			public string Name => Nickname;
		}
	}
}
