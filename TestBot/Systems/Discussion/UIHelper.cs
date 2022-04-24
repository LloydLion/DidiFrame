using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Components;
using DidiFrame.Entities.Message.Embed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGZBot3.Systems.Discussion
{
	internal class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreateAskMessageSendModel()
		{
			var embed = new MessageEmbed(localizer["AskMessageTitle"], localizer["AskMessageDescription"], new Color("#7f8ca1"), Array.Empty<EmbedField>(), new EmbedMeta());

			var components = new List<IComponent>
			{
				new MessageButton(DiscussionChannelLifetime.ConfirmButtonId, localizer["ConfirmButtonText"], ButtonStyle.Primary),
				new MessageButton(DiscussionChannelLifetime.CloseButtonId, localizer["CloseButtonText"], ButtonStyle.Secondary)
			};

			return new MessageSendModel() { MessageEmbeds = new MessageEmbed[] { embed },
				ComponentsRows = new MessageComponentsRow[] { new MessageComponentsRow(components) } };
		}
	}
}
