
using DSharpPlus;
using DSharpPlus.Entities;

namespace CGZBot3.DSharpAdapter
{
	internal class Member : User, IMember
	{
		private readonly DiscordMember member;


		public IServer Server { get; }

		public override string UserName => member.DisplayName;


		public Member(DiscordMember member, Server server) : base(member, (Client)server.Client)
		{
			this.member = member;
			Server = server;
		}


		public bool Equals(IServerEntity? other) => other is Member member && member.Id == Id;
	}
}