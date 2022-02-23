using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Embed;

namespace CGZBot3.Systems.Voice
{
	public class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreateReport(CreatedVoiceChannel model)
		{
			return new MessageSendModel(localizer["ReportContent"])
			{
				MessageEmbed = new MessageEmbed(localizer["ReportTitle"], localizer["ReportDescription"], new Color("#41D1C3"),
					new List<EmbedField>() { new EmbedField(localizer["ReportNameField"], model.Name) }, new EmbedMeta())
			};
		}

		public async Task<IMessage> SendReportAsync(CreatedVoiceChannel model, ITextChannel channel)
		{
			return await channel.SendMessageAsync(CreateReport(model));
		}
	}
}
