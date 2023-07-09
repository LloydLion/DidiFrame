using DidiFrame.Clients.DSharp.Mutations;
using DidiFrame.Clients.DSharp.Server;
using DidiFrame.Clients.DSharp.Server.VSS.EntityRepositories;
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
				return new State(newDiscordObject.Nickname ?? newDiscordObject.Username, newDiscordObject.Username, newDiscordObject.AvatarUrl, newDiscordObject.IsBot);
			};
		}

		public record struct State(string Nickname, string UserName, string AvatarURL, bool IsBot) : IServerObjectState
		{
			public string Name => Nickname;
		}
	}
}
