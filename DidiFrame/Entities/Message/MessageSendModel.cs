using DidiFrame.Entities.Message.Components;
using DidiFrame.Entities.Message.Embed;

namespace DidiFrame.Entities.Message
{
	public record MessageSendModel(string? Content = null)
	{
		public IReadOnlyList<MessageEmbed>? MessageEmbeds { get; init; }

		public IReadOnlyCollection<MessageFile>? Files { get; init; }

		public IReadOnlyCollection<MessageComponentsRow>? ComponentsRows { get; init; }
	}
}