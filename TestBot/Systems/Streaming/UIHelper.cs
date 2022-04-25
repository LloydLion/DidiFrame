using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;
using DidiFrame.Entities.Message.Embed;

namespace TestBot.Systems.Streaming
{
	public class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreateAnnouncedReport(string name, IMember streamer, DateTime utcPlannedStartDate)
		{
			var plannedStartDate = utcPlannedStartDate.ToLocalTime();

			var embed = new MessageEmbedBuilder(localizer["EmbedTitle"], localizer["EmbedDescription"], new Color("#cabc12"));
			embed.AddAuthor(streamer.UserName, null, null);

			embed.AddField(new EmbedField(localizer["NameFieldTitle"], name));
			embed.AddField(new EmbedField(localizer["PlannedStartDateFieldTitle"], plannedStartDate.ToString("dd MMMM HH:mm" + (plannedStartDate.Second == 0 ? "" : ":ss"))));

			return new MessageSendModel()
			{
				MessageEmbeds = new List<MessageEmbed>() { embed.Build() }
			};
		}

		public MessageSendModel CreateWaitingStreamerReport(string name, IMember streamer, DateTime utcPlannedStartDate)
		{
			var plannedStartDate = utcPlannedStartDate.ToLocalTime();

			var embed = new MessageEmbedBuilder(localizer["EmbedTitle"], localizer["EmbedDescription"], new Color("#5a9b9f"));
			embed.AddAuthor(streamer.UserName, null, null);

			embed.AddField(new EmbedField(localizer["NameFieldTitle"], name));
			embed.AddField(new EmbedField(localizer["PlannedStartDateFieldTitle"], plannedStartDate.ToString("dd MMMM HH:mm" + (plannedStartDate.Second == 0 ? "" : ":ss"))));

			return new MessageSendModel()
			{
				MessageEmbeds = new List<MessageEmbed>() { embed.Build() },
				ComponentsRows = new List<MessageComponentsRow>() { new MessageComponentsRow(new List<IComponent>()
				{
					new MessageButton(StreamLifetime.StartStreamButtonId, localizer["StartStreamButtonText"], ButtonStyle.Primary)
				}) }
			};
		}

		public MessageSendModel CreateRunningReport(string name, IMember streamer, string place)
		{
			var embed = new MessageEmbedBuilder(localizer["EmbedTitle"], localizer["EmbedDescription"], new Color("#9b75ca"));
			embed.AddAuthor(streamer.UserName, null, null);

			embed.AddField(new EmbedField(localizer["NameFieldTitle"], name));
			embed.AddField(new EmbedField(localizer["PlaceFieldTitle"], place));

			return new MessageSendModel()
			{
				MessageEmbeds = new List<MessageEmbed>() { embed.Build() },
				ComponentsRows = new List<MessageComponentsRow>() { new MessageComponentsRow(new List<IComponent>()
				{
					new MessageButton(StreamLifetime.FinishStreamButtonId, localizer["FinishStreamButtonText"], ButtonStyle.Primary)
				}) }
			};
		}
	}
}