using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;
using DidiFrame.Entities.Message.Embed;

namespace TestBot.Systems.Games
{
	public class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreateWatingForPlayersReport(string name, IMember creator, IReadOnlyCollection<IMember> invited, IReadOnlyCollection<IMember> inGame, bool waitingEveryoneInvited, int startAtMembers, string description)
		{
			var embed = new MessageEmbedBuilder(localizer["EmbedTitle", name], localizer["EmbedDescription", description], new Color("#7d91b4"));

			embed.AddAuthor(creator.UserName, null, null);

			embed.AddField(new EmbedField(localizer["StartIfFieldTitle"], localizer[waitingEveryoneInvited ? "StartIfFieldContent" : "StartIfFieldContentAllInvited", startAtMembers]));
			embed.AddField(new EmbedField(localizer["InvitedFieldTitle"], invited.Count == 0 ? localizer["NoMembers"] : string.Join(", ", invited.Select(s => s.Mention))));
			embed.AddField(new EmbedField(localizer["InGameFieldTitle"], inGame.Count == 0 ? localizer["NoMembers"] : string.Join(", ", inGame.Select(s => s.Mention))));


			return new MessageSendModel()
			{
				MessageEmbeds = new MessageEmbed[] { embed.Build() },
				ComponentsRows = new MessageComponentsRow[]
				{
					new MessageComponentsRow(new IComponent[]
					{
						new MessageButton(GameLifetime.JoinGameButtonId, localizer["JoinGameButtonText"], ButtonStyle.Secondary),
						new MessageButton(GameLifetime.ExitGameButtonId, localizer["ExitGameButtonText"], ButtonStyle.Secondary)
					}),

					new MessageComponentsRow(new IComponent[]
					{
						new MessageButton(GameLifetime.StartGameButtonId, localizer["StartGameButtonText"], ButtonStyle.Primary, true)
					})
				}
			};
		}

		public MessageSendModel CreataWaitingForCreatorReport(string name, IMember creator, IReadOnlyCollection<IMember> invited, IReadOnlyCollection<IMember> inGame, bool waitingEveryoneInvited, int startAtMembers, string description)
		{
			var embed = new MessageEmbedBuilder(localizer["EmbedTitle", name], localizer["EmbedDescription", description], new Color("#43f1c3"));

			embed.AddAuthor(creator.UserName, null, null);

			embed.AddField(new EmbedField(localizer["StartIfFieldTitle"], localizer[waitingEveryoneInvited ? "StartIfFieldContent" : "StartIfFieldContentAllInvited", startAtMembers]));
			embed.AddField(new EmbedField(localizer["InvitedFieldTitle"], invited.Count == 0 ? localizer["NoMembers"] : string.Join(", ", invited.Select(s => s.Mention))));
			embed.AddField(new EmbedField(localizer["InGameFieldTitle"], inGame.Count == 0 ? localizer["NoMembers"] : string.Join(", ", inGame.Select(s => s.Mention))));


			return new MessageSendModel()
			{
				MessageEmbeds = new MessageEmbed[] { embed.Build() },
				ComponentsRows = new MessageComponentsRow[]
				{
					new MessageComponentsRow(new IComponent[]
					{
						new MessageButton(GameLifetime.JoinGameButtonId, localizer["JoinGameButtonText"], ButtonStyle.Secondary),
						new MessageButton(GameLifetime.ExitGameButtonId, localizer["ExitGameButtonText"], ButtonStyle.Secondary)
					}),

					new MessageComponentsRow(new IComponent[]
					{
						new MessageButton(GameLifetime.StartGameButtonId, localizer["StartGameButtonText"], ButtonStyle.Primary)
					})
				}
			};
		}

		public MessageSendModel CreateRunningReport(string name, IMember creator, IReadOnlyCollection<IMember> inGame, string description)
		{
			var embed = new MessageEmbedBuilder(localizer["EmbedTitle", name], localizer["EmbedDescription", description], new Color("#43f1c3"));

			embed.AddAuthor(creator.UserName, null, null);

			embed.AddField(new EmbedField(localizer["InGameFieldTitle"], inGame.Count == 0 ? localizer["NoMembers"] : string.Join(", ", inGame.Select(s => s.Mention))));


			return new MessageSendModel()
			{
				MessageEmbeds = new MessageEmbed[] { embed.Build() },
				ComponentsRows = new MessageComponentsRow[]
				{
					new MessageComponentsRow(new IComponent[]
					{
						new MessageButton(GameLifetime.FinishGameButtonId, localizer["FinishGameButtonText"], ButtonStyle.Primary),
						new MessageButton(GameLifetime.JoinGameButtonId, localizer["JoinGameButtonText"], ButtonStyle.Secondary)
					})
				}
			};
		}

		public MessageSendModel CreateInvitationTablet(IMember gameCreator, string name)
		{
			var embed = new MessageEmbedBuilder(localizer["InvitationTabletTitle"], localizer["InvitationTabletDescription"], new Color("#d341ac"));

			embed.AddField(new EmbedField(localizer["ServerFieldTitle"], gameCreator.Server.Name));
			embed.AddField(new EmbedField(localizer["CreatorFieldTitle"], gameCreator.Mention));
			embed.AddField(new EmbedField(localizer["GameNameFieldTitle"], name));

			return new MessageSendModel() { MessageEmbeds = new MessageEmbed[] { embed.Build() } };
		}
	}
}
