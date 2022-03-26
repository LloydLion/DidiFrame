using CGZBot3.Entities.Message.Components;
using CGZBot3.Entities.Message.Embed;

namespace CGZBot3.Entities.Message
{
	public record MessageSendModel(string? Content = null)
	{
		public MessageEmbed? MessageEmbed { get; init; }

		public IReadOnlyCollection<MessageFile>? Files { get; init; }

		public IReadOnlyCollection<IComponent>? Components { get; init; }
	}
}