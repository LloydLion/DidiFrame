
using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Member : User, IMember
	{
		private readonly DiscordMember member;


		public IServer Server { get; }

		public override string UserName => member.DisplayName;


		public Member(DiscordMember member, DiscordClient client) : base(member, client)
		{
			this.member = member;
			Server = new Server(member.Guild, client);
		}


		public bool Equals(IServerEntity? other) => other is Member member && member.Id == Id;
	}
}