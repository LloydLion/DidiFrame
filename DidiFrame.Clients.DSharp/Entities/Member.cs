using DidiFrame.Utils;
using DSharpPlus.Entities;

namespace DidiFrame.Clients.DSharp.Entities
{
	public class Member : ServerObject, IMember
	{
		private DiscordMember? discordMember;
		private string userName;
		private string avatarURL;
		private string mention;
		private bool isBot;


		public Member(Server server, DiscordMember discordMember) : base(server, discordMember)
		{
			this.discordMember = discordMember;

			WrapName = discordMember.DisplayName;
			userName = discordMember.Username;
			avatarURL = discordMember.AvatarUrl;
			mention = discordMember.Mention;
			isBot = discordMember.IsBot;
		}

		public Member(Server server, ulong targetId) : base(server, targetId)
		{
			discordMember = null;

			WrapName = string.Empty;
			userName = string.Empty;
			avatarURL = string.Empty;
			mention = string.Empty;
			isBot = false;
		}


		public string Nickname => Name;

		public string UserName { get => CheckAccess<string>(userName); private set => userName = value; }

		public string AvatarURL { get => CheckAccess<string>(avatarURL); private set => avatarURL = value; }

		public string Mention { get => CheckAccess<string>(mention); private set => mention = value; }

		public bool IsBot { get => CheckAccess(isBot); private set => isBot = value; }

		protected override string WrapName { get; set; }


		public override string? ToString()
		{
			return $"[Discord member ({Id})] {{Nickname={Nickname}; UserName={UserName}; Mention={Mention}; IsBot={IsBot}; CreationTimeStamp={CreationTimeStamp}; AvatarURL={AvatarURL}}}";
		}

		internal Task MutateAsync(DiscordMember discordMember)
		{
			CheckAccess();

			this.discordMember = discordMember;
			WrapName = discordMember.DisplayName;
			UserName = discordMember.Username;
			AvatarURL = discordMember.AvatarUrl;
			Mention = discordMember.Mention;
			IsBot = discordMember.IsBot;

			return NotifyModified();
		}

		protected override async ValueTask CallRenameOperationAsync(string newName)
		{
			await AccessObject(discordMember).ModifyAsync(s =>
			{
				s.Nickname = newName == UserName ? null : newName;
			});
		}

		protected override async ValueTask CallDeleteOperationAsync()
		{
			await AccessObject(discordMember).RemoveAsync();
		}
	}
}
