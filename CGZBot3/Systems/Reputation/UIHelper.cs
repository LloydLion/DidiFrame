using CGZBot3.Entities.Message;
using CGZBot3.Entities.Message.Embed;

namespace CGZBot3.Systems.Reputation
{
	internal class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreateDirectNotification(IMember invoker, int amount, string reason)
		{
			return new MessageSendModel(localizer["LegalLevelDecreaseNotification", invoker.Server.Name, invoker.UserName, amount, reason]);
		}

		public MessageSendModel CreateReputaionTablet(IReadOnlyDictionary<ReputationType, int> rp, IMember member)
		{
			var embedBuilder = new MessageEmbedBuilder(localizer["ReputationTabletTitle"],
				localizer["ReputationTabletDescription"], new Color("#5f1a9c"));

			embedBuilder.AddField(new EmbedField(localizer["ServerActivityReputation"], rp[ReputationType.ServerActivity].ToString()));
			embedBuilder.AddField(new EmbedField(localizer["ExperienceReputation"], rp[ReputationType.Experience].ToString()));
			embedBuilder.AddField(new EmbedField(localizer["LegalLevelReputation"], rp[ReputationType.LegalLevel].ToString()));

			embedBuilder.AddAuthor(member.UserName, null, null);

			return new MessageSendModel() { MessageEmbeds = new MessageEmbed[] { embedBuilder.Build() } };
		}
	}
}
