using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Embed;

namespace CGZBot3.Systems.Voice
{
	public class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreateReport(string name, IMember owner)
		{
			return new MessageSendModel(localizer["ReportContent"])
			{
				MessageEmbeds = new MessageEmbed[] { new MessageEmbed(localizer["ReportTitle"], localizer["ReportDescription"], new Color("#41D1C3"),
					new List<EmbedField>() { new EmbedField(localizer["ReportNameField"], name) }, new EmbedMeta()) }
			};
		}

		public async Task<IMessage> SendReportAsync(string name, IMember owner, ITextChannel channel)
		{
			return await channel.SendMessageAsync(CreateReport(name, owner));
		}
	}
}
