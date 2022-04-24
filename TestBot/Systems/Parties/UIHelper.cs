using DidiFrame.Entities.Message;
using DidiFrame.Entities.Message.Embed;

namespace CGZBot3.Systems.Parties
{
	internal class UIHelper
	{
		private readonly IStringLocalizer<UIHelper> localizer;


		public UIHelper(IStringLocalizer<UIHelper> localizer)
		{
			this.localizer = localizer;
		}


		public MessageSendModel CreatePersonalTablet(IReadOnlyCollection<PartyModel> parties, IMember member)
		{
			var embed = new MessageEmbedBuilder(localizer["PersonalTabletTitle"], localizer["PersonalTabletDescription"], new Color("#3c9af3"));

			embed.AddAuthor(member.UserName, null, null);

			embed.AddField(new EmbedField(localizer["YouAreInFieldTitle"], parties.Count == 0 ? localizer["NoParties"] : string.Join(",\n", parties.Select(s => $"{s.Name} [{s.Creator.Mention}]"))));

			return new MessageSendModel() { MessageEmbeds = new MessageEmbed[] { embed.Build() } };
		}

		public MessageSendModel CreatePartyTablet(PartyModel party)
		{
			var embed = new MessageEmbedBuilder(localizer["PersonalTabletTitle"], localizer["PersonalTabletDescription"], new Color("#4cc19f"));

			embed.AddField(new EmbedField(localizer["PartyCreatorFieldTitle"], party.Creator.Mention));
			embed.AddField(new EmbedField(localizer["PartyMembersFieldTitle"], party.Members.Count == 0 ? localizer["NoParties"] : string.Join(",\n", party.Members.Select(s => $"{s.Mention}"))));

			return new MessageSendModel() { MessageEmbeds = new MessageEmbed[] { embed.Build() } };
		}

		//public MessageSendModel CreateServerTablet(IMember member, IReadOnlyCollection<PartyModel> youAreOutParties, IReadOnlyCollection<PartyModel> youAreInParties)
		//{
		//	var embed = new MessageEmbedBuilder(localizer["PersonalServerTitle"], localizer["PersonaServerDescription"], new Color("3fc1aa"));

		//	embed.AddAuthor(member.UserName, null, null);

		//	embed.AddField(new EmbedField(localizer["YouAreInFieldTitle"], youAreInParties.Count == 0 ? localizer["NoParties"] : string.Join(",\n", youAreInParties.Select(s => $"{s.Name}[{s.Creator.UserName}]"))));
		//	embed.AddField(new EmbedField(localizer["YouAreOutFieldTitle"], youAreOutParties.Count == 0 ? localizer["NoParties"] : string.Join(",\n", youAreOutParties.Select(s => $"{s.Name}[{s.Creator.UserName}]"))));

		//	return new MessageSendModel() { MessageEmbeds = new MessageEmbed[] { embed.Build() } };
		//}
	}
}
